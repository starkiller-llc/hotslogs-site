using DiscordWebhooks;
using Grpc.Core;
using HelperCore;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace HotsAdminConsole;

[PublicAPI]
public abstract class ServiceBase
{
    protected IServiceProvider Svcp { get; }

    private readonly HotsSvcHost.HotsSvcHostClient _clnt;
    private readonly Server _srvr;
    private readonly Stopwatch _stopwatch = new();
    private readonly CancellationTokenSource _tokenSource = new();
    private int _started;

    protected ServiceBase(IServiceProvider svcp)
    {
        Svcp = svcp;
        var tp = (HotsServiceAttribute)GetType().GetCustomAttribute(typeof(HotsServiceAttribute));

        if (tp is null)
        {
            throw new InvalidOperationException("Class inherits ServiceBase but doesn't have [HotsService] attribute");
        }

        var port = tp.Port;
        ServiceName = tp.Title;

        _srvr = new Server
        {
            Services = { HotsSvcService.BindService(new HotsSvcServiceImpl2(this)) },
            Ports = { new ServerPort("localhost", port | 1, ServerCredentials.Insecure) },
        };

        _srvr.Start();

        var chnl = new Channel($"127.0.0.1:{port & ~1}", ChannelCredentials.Insecure);
        _clnt = new HotsSvcHost.HotsSvcHostClient(chnl);
    }

    // If a run completes faster than this time, wait for the remainder.
    public TimeSpan FillRunDuration { get; set; } = TimeSpan.FromMinutes(20);
    public bool DryRun { get; set; }
    public bool NotifyDiscord { get; set; }
    public bool KeepRunning { get; set; }
    public string ServiceName { get; set; }
    public string ConnectionString { get; set; }

    public void AddServiceOutput(string text)
    {
        try
        {
            _clnt.AddServiceOutput(
                new LogMessage
                {
                    Msg = text,
                });
        }
        catch
        {
            // ignored
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public async Task Run(CancellationToken token = default)
    {
        AddServiceOutput($"{ServiceName} start...");
        await OnBeforeStart(token);
        do
        {
            _stopwatch.Restart();
            AddServiceOutput($"{ServiceName} pass start...");
            await _clnt.PassStartedAsync(new VoidMsg(), cancellationToken: token);
            await OnBeforeRunOnce(token);

            await RunOnce(token);

            await OnAfterRunOnce(token);
            var msg = $"Run complete after {_stopwatch.Elapsed.TotalMinutes:N1} minutes";
            await _clnt.PassCompleteAsync(new VoidMsg(), cancellationToken: token);
            AddServiceOutput(msg);
            await OnRunComplete(token);
            var first = true;
            while (_stopwatch.Elapsed < FillRunDuration && Running(token))
            {
                if (first)
                {
                    first = false;
                    AddServiceOutput($"Waiting until {FillRunDuration.TotalMinutes:N1} minutes have elapsed.");
                }

                try
                {
                    await Task.Delay(1000, token);
                }
                catch (OperationCanceledException) { }
            }
        } while (Running(token));

        if (!token.IsCancellationRequested)
        {
            await OnAfterStop(token);
        }

        AddServiceOutput($"{ServiceName} stop...");
    }

    public async Task TryRun(CancellationToken token = default)
    {
        var atoken = _tokenSource.Token;
        try
        {
            await Run(atoken);
        }
        catch (OperationCanceledException)
        {
            AddServiceOutput("Cancelled by user");
        }
        catch (Exception x)
        {
            AddServiceOutput(x.ToString());
            await NotifyDiscordWithFault(x);
            await _clnt.ClientErrorAsync(new VoidMsg(), cancellationToken: token);
        }

        await _clnt.ClientShutdownAsync(new VoidMsg(), cancellationToken: token);
        await _srvr.ShutdownAsync();
    }

    protected void ClearState(IServiceScope scope)
    {
        using var redis = MyDbWrapper.Create(scope);
        var fn = $"Service:{ServiceName}:State";
        redis.Remove(fn);
    }

    protected virtual Task OnAfterRunOnce(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnAfterStop(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnBeforeRunOnce(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnBeforeStart(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected virtual async Task OnRunComplete(CancellationToken token = default)
    {
        var msg = $"Run complete after {_stopwatch.Elapsed.TotalMinutes:N1} minutes";
        if (!NotifyDiscord)
        {
            return;
        }
#if !LOCALDEBUG
        if (DryRun)
        {
            msg = $"(dryrun) {msg}";
        }

        using var scope = Svcp.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<Sender>();
        await sender.SendServiceMessage(new ServiceMessage
        {
            ServiceName = ServiceName,
            Message = msg,
        });
#endif
    }

    protected T RestoreState<T>(IServiceScope scope)
    {
        using var redis = MyDbWrapper.Create(scope);
        var fn = $"Service:{ServiceName}:State";
        var rc = redis.Get<T>(fn);
        return rc != null ? rc : throw new KeyNotFoundException();
    }

    protected abstract Task RunOnce(CancellationToken token = default);

    protected void SaveState<T>(IServiceScope scope, T state)
    {
        using var redis = MyDbWrapper.Create(scope);
        var fn = $"Service:{ServiceName}:State";
        redis.TrySet(fn, state);
    }

    private async Task NotifyDiscordWithFault(Exception x)
    {
#if !LOCALDEBUG
        try
        {
            string msg = $"Run failed with error after {_stopwatch.Elapsed.TotalMinutes:N1} minutes, error: {x}";
            using var scope = Svcp.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<Sender>();
            await sender.SendServiceMessage(new ServiceMessage
            {
                ServiceName = ServiceName,
                Message = msg,
            });
        }
        catch { /* ignored */ }
#endif
    }

    private bool Running(CancellationToken token)
    {
        return KeepRunning && !token.IsCancellationRequested;
    }

    private void Start()
    {
        if (Interlocked.CompareExchange(ref _started, 1, 0) == 1)
        {
            return;
        }

        var t = new Thread(
            () =>
            {
                var res = TryRun().GetAwaiter();
                res.GetResult();
            })
        {
            IsBackground = true,
        };
        t.Start();
    }

    private class HotsSvcServiceImpl2 : HotsSvcService.HotsSvcServiceBase
    {
        private readonly ServiceBase _svcb;

        public HotsSvcServiceImpl2(ServiceBase svcb)
        {
            _svcb = svcb;
        }

        public override Task<VoidMsg> SetConnectionString(StringMsg request, ServerCallContext context)
        {
            var str = request.Str;
            _svcb.ConnectionString = str;
            return Task.FromResult(new VoidMsg());
        }

        public override Task<VoidMsg> SetKeepRunning(BoolMsg request, ServerCallContext context)
        {
            var b = request.V;
            _svcb.KeepRunning = b;
            return Task.FromResult(new VoidMsg());
        }

        public override Task<VoidMsg> SetNotifyDiscord(BoolMsg request, ServerCallContext context)
        {
            var b = request.V;
            _svcb.NotifyDiscord = b;
            return Task.FromResult(new VoidMsg());
        }

        public override Task<VoidMsg> Shutdown(VoidMsg request, ServerCallContext context)
        {
            _svcb._tokenSource.Cancel();
            return Task.FromResult(new VoidMsg());
        }

        public override Task<VoidMsg> Start(VoidMsg request, ServerCallContext context)
        {
            _svcb.Start();
            return Task.FromResult(new VoidMsg());
        }
    }
}
