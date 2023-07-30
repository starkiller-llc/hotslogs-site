using Grpc.Core;
using HelperCore;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace HotsAdminConsole;

/// <summary>
///     Interaction logic for ServiceForm.xaml
/// </summary>
public partial class ServiceForm : INotifyPropertyChanged
{
    internal const int MMRMaxDegrees = 4;
    internal const int DaysOfStatisticsToQuery = 15;

    private readonly Subject<string> _logSubject = new();

    private readonly IServiceProvider _svcp;
    private readonly int? _port;

    private readonly Server _srvr;
    private readonly IDisposable _sub;
    private readonly SynchronizationContext _sync;
    private bool _clientShutdown;
    private HotsSvcService.HotsSvcServiceClient _clnt;
    private bool _closing;
    private double? _elapsed;
    private bool _isError;
    private bool _isRunning;
    private bool _isStopped;
    private double _lastPassDuration;
    private int _passesComplete;
    private DateTime _passStart;
    private string _serviceName;
    private DateTime _stopTime;
    private readonly string _connectionString;

    public ServiceForm(IServiceProvider svcp, int? port = null)
    {
        InitializeComponent();
        DataContext = this;
        _svcp = svcp;
        _port = port;
        _sync = SynchronizationContext.Current;
        var config = _svcp.GetRequiredService<IConfiguration>();
        _connectionString = config.GetConnectionString("DefaultConnection");

        if (port.HasValue)
        {
            _srvr = new Server
            {
                Services = { HotsSvcHost.BindService(new HotsSvcHostImpl2(this)) },
                Ports = { new ServerPort("localhost", port.Value & ~1, ServerCredentials.Insecure) },
            };

            _srvr.Start();
        }

        _sub = _logSubject
            .Select(x => (Date: NowString(), Message: x))
            .Buffer(TimeSpan.FromMilliseconds(1000))
            .ObserveOn(_sync!)
            .Subscribe(
                r =>
                {
                    if (IsRunning)
                    {
                        Elapsed = (DateTime.UtcNow - PassStart).TotalMinutes;
                    }
                    else
                    {
                        Elapsed = null;
                    }

                    if (r.Count == 0)
                    {
                        return;
                    }

                    var msgs = r.Select(x => $"{x.Date}:{x.Message}").ToList();

                    // This was filling the C: drive, and was really excessive -- Aviad, 13-Dec-2021
                    // DataHelper.LogApplicationEvents(msgs, $"Service {_serviceName}.txt");

                    var msg = string.Join(Environment.NewLine, msgs);
                    if (txtOutput.Text.Length == 0)
                    {
                        txtOutput.CaretIndex = 0;
                    }

                    var scrollToEnd = txtOutput.CaretIndex == txtOutput.Text.Length;
                    txtOutput.AppendText(msg);
                    txtOutput.AppendText(Environment.NewLine);
                    if (scrollToEnd)
                    {
                        txtOutput.ScrollToEnd();
                    }
                });

        chkRun.Checked += chkRun_CheckedChanged;
        chkRun.Unchecked += chkRun_CheckedChanged;

        chkNotifyDiscord.Checked += chkNotifyDiscord_CheckedChanged;
        chkNotifyDiscord.Unchecked += chkNotifyDiscord_CheckedChanged;
    }

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (value == _isRunning)
            {
                return;
            }

            _isRunning = value;
            OnPropertyChanged();
        }
    }

    public bool IsStopped
    {
        get => _isStopped;
        set
        {
            if (value == _isStopped)
            {
                return;
            }

            _isStopped = value;
            OnPropertyChanged();
        }
    }

    public DateTime StopTime
    {
        get => _stopTime;
        set
        {
            if (value.Equals(_stopTime))
            {
                return;
            }

            _stopTime = value;
            OnPropertyChanged();
        }
    }

    public bool IsError
    {
        get => _isError;
        set
        {
            if (value == _isError)
            {
                return;
            }

            _isError = value;
            OnPropertyChanged();
        }
    }

    public int PassesComplete
    {
        get => _passesComplete;
        set
        {
            if (value == _passesComplete)
            {
                return;
            }

            _passesComplete = value;
            OnPropertyChanged();
        }
    }

    public DateTime PassStart
    {
        get => _passStart;
        set
        {
            if (value.Equals(_passStart))
            {
                return;
            }

            _passStart = value;
            OnPropertyChanged();
        }
    }

    public double LastPassDuration
    {
        get => _lastPassDuration;
        set
        {
            if (value.Equals(_lastPassDuration))
            {
                return;
            }

            _lastPassDuration = value;
            OnPropertyChanged();
        }
    }

    public double? Elapsed
    {
        get => _elapsed;
        set
        {
            if (value.Equals(_elapsed))
            {
                return;
            }

            _elapsed = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    #region Output

    public void AddServiceOutput(string text)
    {
        _logSubject.OnNext(text);
    }

    #endregion

    public void AutoScroll()
    {
        txtOutput.CaretIndex = txtOutput.Text.Length;
        txtOutput.ScrollToEnd();
    }

    public void CalculateOldMMR(int regionId, int gameModeId, DateTime startDate)
    {
        Task.Run(() => RunSpecificMMR(regionId, gameModeId, startDate));
    }

    public async Task CancelService()
    {
        if (_clnt == null)
        {
            return;
        }

        await _clnt.ShutdownAsync(new VoidMsg());
    }

    public void CreateHotsSvcClient()
    {
        if (!_port.HasValue)
        {
            return;
        }

        var chnl = new Channel($"127.0.0.1:{_port.Value | 1}", ChannelCredentials.Insecure);
        _clnt = new HotsSvcService.HotsSvcServiceClient(chnl);

        _clnt.SetNotifyDiscord(new BoolMsg { V = chkNotifyDiscord.IsChecked ?? false });
        _clnt.SetKeepRunning(new BoolMsg { V = chkRun.IsChecked ?? false });

        _clnt.SetConnectionString(new StringMsg { Str = _connectionString });
    }

    public void SetCheck(bool bEnable = true, bool bCheck = true)
    {
        chkRun.IsEnabled = bEnable;
        chkRun.IsChecked = bCheck;
    }

    public void SetLabel(string txt)
    {
        _serviceName = txt;
        Title = $"{txt}";
    }

    public async Task StartService()
    {
        if (_clnt == null)
        {
            return;
        }

        await _clnt.StartAsync(new VoidMsg());
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void BtnAutoScroll_OnClick(object sender, RoutedEventArgs e)
    {
        AutoScroll();
    }

    private void chkNotifyDiscord_CheckedChanged(object sender, EventArgs e)
    {
        _clnt?.SetNotifyDiscord(new BoolMsg { V = chkNotifyDiscord.IsChecked ?? false });
    }

    private void chkRun_CheckedChanged(object sender, EventArgs e)
    {
        _clnt?.SetKeepRunning(new BoolMsg { V = chkRun.IsChecked ?? false });
    }

    private void ClientError()
    {
        _sync.Post(
            s =>
            {
                Title = $"{_serviceName} - Error";
            },
            null);
    }

    private void ClientShutdown()
    {
        IsStopped = true;
        StopTime = DateTime.Now;
        IsRunning = false;
        _clientShutdown = true;
        _sync.Post(
            s =>
            {
                GridOverlay.Visibility = Visibility.Visible;
                if (!_closing)
                {
                    Title = $"{_serviceName} - Stopped";
                    return;
                }

                Close();
            },
            null);
    }

    private string NowString()
    {
        return DateTime.Now.ToShortDateString() + ":" + DateTime.Now.ToLongTimeString();
    }

    private void PassComplete()
    {
        IsRunning = false;
        LastPassDuration = (DateTime.UtcNow - PassStart).TotalMinutes;
        PassesComplete++;
        _sync.Post(
            s =>
            {
                Title = $"{_serviceName} - Waiting";
            },
            null);
    }

    private void PassStarted()
    {
        PassStart = DateTime.UtcNow;
        IsRunning = true;
        _sync.Post(
            s =>
            {
                TruncateOutput();
                Title = $"{_serviceName} - Running";
            },
            null);
    }

    private void RunSpecificMMR(int regionId, int gameModeId, DateTime startDate)
    {
        try
        {
            var replayDateToProcess = startDate;
            while (replayDateToProcess < DateTime.UtcNow.AddDays(-4))
            {
                AddServiceOutput("Running AggregateMMR for date " + replayDateToProcess.ToShortDateString());
                using var scope = _svcp.CreateScope();
                var mmr = scope.ServiceProvider.GetRequiredService<MMR>();
                mmr.AggregateReplayCharacterMMR(regionId, gameModeId, replayDateToProcess);
                replayDateToProcess = replayDateToProcess.AddDays(1);
            }
        }
        catch (Exception ex)
        {
            AddServiceOutput("Error: " + ex);
        }
    }

    private void ServiceForm_OnActivated(object sender, EventArgs e)
    {
        InactiveOverlay.IsHitTestVisible = false;
    }

    private async void ServiceForm_OnClosing(object sender, CancelEventArgs e)
    {
        _closing = true;
        if (!_clientShutdown)
        {
            Title = $"{_serviceName} - Closing";
            e.Cancel = true;
            await _clnt.ShutdownAsync(new VoidMsg());
        }
        else
        {
            await _srvr.ShutdownAsync();
            _sub.Dispose();
            _logSubject.Dispose();
        }
    }

    private void ServiceForm_OnDeactivated(object sender, EventArgs e)
    {
        InactiveOverlay.IsHitTestVisible = true;
    }

    private void TruncateOutput()
    {
        var t = txtOutput.Text.Split("\n");
        if (t.Length > 1000)
        {
            var newT = string.Join("\n", t.Reverse().Take(500).Reverse());
            txtOutput.Text = newT;
            AutoScroll();
        }
    }

    private class HotsSvcHostImpl2 : HotsSvcHost.HotsSvcHostBase
    {
        private readonly ServiceForm _svcb;

        public HotsSvcHostImpl2(ServiceForm svcb)
        {
            _svcb = svcb;
        }

        public override Task<VoidMsg> AddServiceOutput(LogMessage request, ServerCallContext context)
        {
            var msg = request.Msg;
            _svcb.AddServiceOutput(msg);
            return Task.FromResult(new VoidMsg());
        }

        public override Task<VoidMsg> ClientError(VoidMsg request, ServerCallContext context)
        {
            _svcb.IsStopped = true;
            _svcb.IsError = true;
            _svcb.ClientError();
            return Task.FromResult(new VoidMsg());
        }

        public override Task<VoidMsg> ClientShutdown(VoidMsg request, ServerCallContext context)
        {
            _svcb.ClientShutdown();
            return Task.FromResult(new VoidMsg());
        }

        public override Task<VoidMsg> PassComplete(VoidMsg request, ServerCallContext context)
        {
            _svcb.PassComplete();
            return Task.FromResult(new VoidMsg());
        }

        public override Task<VoidMsg> PassStarted(VoidMsg request, ServerCallContext context)
        {
            _svcb.PassStarted();
            return Task.FromResult(new VoidMsg());
        }
    }
}
