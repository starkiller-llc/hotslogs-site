using HelperCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HotsAdminConsole;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : IDisposable
{
    private readonly IServiceProvider _svcp;
    private readonly SynchronizationContext _sync;
    private readonly CancellationTokenSource _tokenSource = new();
    private bool _closing;

    // private List<HOTSTalentBuilderTalent> _currentlySelectedTalentSet;
    private Dictionary<string, HotsServiceInfo> _serviceDic;

    public MainWindow(IServiceProvider svcp)
    {
        _svcp = svcp;
        InitializeComponent();
        DataContext = this;
        Config.IsInitialized = true;
        // _currentlySelectedTalentSet = new List<HOTSTalentBuilderTalent>();
        //FillHeroes();
        //FillBuilds();
        //FillTalentSets();
        FillServiceNames();

        var targetMode = !IsApplicationOnline();
        BtnToggleOfflineMode.Content = $"Toggle site to <{(targetMode ? "Online" : "Offline")}> mode";

        _sync = SynchronizationContext.Current;

        var tf = new TaskFactory();
        tf.StartNew(HourlyRestartStoppedServices, default, default, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public static FileConfigurationManager Config { get; } = new();

    public ObservableCollection<ServiceForm> ServiceCollection { get; set; } = new();

    public void Dispose() { }

    private static bool IsApplicationOnline()
    {
        return DataHelper.RedisCacheGet<string>("HOTSLogs:OfflineMode") != "1";
    }

    private void BtnArrangeAll_Click(object sender, EventArgs e)
    {
        var openWindows = _serviceDic.Values.Where(x => x.ServiceForm != null).ToList();
        var top = 20;
        var left = 20;
        var cnt = 0;

        const int stackSize = 5;
        const int hStep = 100;
        const int vStep = 100;

        foreach (var svc in openWindows)
        {
            var wnd = svc.ServiceForm;
            wnd.WindowState = WindowState.Normal;
            wnd.Top = top;
            wnd.Left = left;
            wnd.Width = 700;
            wnd.Height = 350;
            wnd.Activate();
            wnd.AutoScroll();

            cnt++;
            top += vStep;
            left += hStep;
            if (cnt >= stackSize)
            {
                top -= (vStep * stackSize) - (vStep / 2);
                left -= stackSize * hStep / 2;
                cnt = 0;
            }
        }
    }

    private void BtnDismissStopped_Click(object sender, EventArgs e)
    {
        var runningServices = _serviceDic.Values.Where(x => x.ServiceForm?.IsStopped == true).ToList();
        runningServices.ForEach(x => x.ServiceForm.Close());
    }

    private void BtnMinimizeAll_Click(object sender, EventArgs e)
    {
        var openWindows = _serviceDic.Values.Where(x => x.ServiceForm != null).ToList();
        openWindows.ForEach(x => x.ServiceForm.WindowState = WindowState.Minimized);
    }

    private async void BtnRunService_Click(object sender, EventArgs e)
    {
        if (ComboService.SelectedItem == null)
        {
            return;
        }

        var service = (HotsServiceInfo)ComboService.SelectedItem;
        if (_serviceDic.ContainsKey(service.Name))
        {
            await RunService(service);
        }
        else
        {
            MessageBox.Show("Please select an item...");
        }
    }

    private void BtnToggleOfflineMode_Click(object sender, EventArgs e)
    {
        var targetMode = !IsApplicationOnline();

        using var scope = _svcp.CreateScope();
        var redisClient = MyDbWrapper.Create(scope);
        redisClient["HOTSLogs:OfflineMode"] = targetMode ? "0" : "1";

        BtnToggleOfflineMode.Content = $"Toggle site to <{(!targetMode ? "Online" : "Offline")}> mode";
        MessageBox.Show($"Site changed to {(targetMode ? "Online" : "Offline")} mode");
    }

    private void Button8_Click(object sender, EventArgs e)
    {
        var iRegion = int.Parse(TxtRegion.Text);
        var iGame = int.Parse(TxtGameMode.Text);
        var dtStart = DateTime.Parse(TxtOldDate.Text);

        var sfMMR = new ServiceForm(_svcp);
        sfMMR.Show();

        sfMMR.CalculateOldMMR(iRegion, iGame, dtStart);
    }

    private async void ButtonRunAll_Click(object sender, EventArgs e)
    {
        await RunAllServices();
    }

    private void FillServiceNames()
    {
        var services = from x in Assembly.GetExecutingAssembly().GetTypes()
                       where typeof(ServiceBase).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract
                       let attr = (HotsServiceAttribute)x.GetCustomAttribute(typeof(HotsServiceAttribute))
                       where attr != null
                       let text = attr.Title
                       let autoStart = attr.AutoStart
                       let comboText = autoStart ? $"(autostart) {text}" : text
                       select new HotsServiceInfo
                       {
                           Name = x.Name,
                           ServiceType = x,
                           Title = text,
                           ComboText = comboText,
                           KeepRunning = attr.KeepRunning,
                           Sort = attr.Sort,
                           AutoStart = attr.AutoStart,
                           Port = attr.Port,
                       };

        _serviceDic = services.ToDictionary(x => x.Name);

        ComboService.Items.Clear();
        foreach (var service in _serviceDic.Values.OrderBy(x => x.Sort))
        {
            ComboService.Items.Add(service);
        }
    }

    private async Task HourlyRestartStoppedServices()
    {
        var token = _tokenSource.Token;
        while (!token.IsCancellationRequested)
        {
            var oldStoppedServices = _serviceDic.Values
                .Where(x => x.ServiceForm?.IsStopped == true && x.ServiceForm?.StopTime < DateTime.Now.AddHours(-1))
                .ToList();
            if (oldStoppedServices.Any())
            {
                oldStoppedServices.ForEach(x => x.ServiceForm.Close());
                foreach (var serv in oldStoppedServices.Select(x => x.Name))
                {
                    await RunService(_serviceDic[serv]);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1), token);
        }
    }

    private void MainWindow_OnClosing(object sender, CancelEventArgs e)
    {
        _closing = true;
        _tokenSource.Cancel();
        Title = "HOTSLogs Admin Console - Stopping Services...";

        e.Cancel = StopAllServices();
    }

    private async Task RunAllServices()
    {
        foreach (var service in _serviceDic.Values.Where(x => x.AutoStart))
        {
            await RunService(service);
            await Task.Delay(400);
        }
    }

    private async Task RunService(HotsServiceInfo service)
    {
        if (service.ServiceForm?.IsVisible ?? _closing)
        {
            return;
        }

        var sf = new ServiceForm(_svcp, service.Port);
        ServiceCollection.Add(sf);
        sf.SetLabel($"{service.Title}");
        sf.SetCheck(true, service.KeepRunning);
        sf.Show();

        SpawnService(service);

        sf.CreateHotsSvcClient();

        sf.Closed += (_, _) =>
        {
            ServiceCollection.Remove(sf);
            service.ServiceForm = null;
            if (_serviceDic.Values.All(x => x.ServiceForm == null))
            {
                Title = "HOTSLogs Admin Console Stopped";
                if (_closing)
                {
                    Close();
                }
            }
        };

        await sf.StartService();

        service.ServiceForm = sf;
    }

    private void Service_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        var d = (FrameworkElement)sender;
        var frm = (ServiceForm)d.DataContext;
        SynchronizationContext.Current?.Post(
            _ =>
            {
                frm.WindowState = WindowState.Normal;
                frm.Activate();
                frm.AutoScroll();
            },
            null);
    }

    private void ServiceClose_Click(object sender, RoutedEventArgs e)
    {
        var d = (FrameworkElement)sender;
        var frm = (ServiceForm)d.DataContext;
        frm.Close();
    }

    private async void ServiceStop_Click(object sender, RoutedEventArgs e)
    {
        var d = (FrameworkElement)sender;
        var frm = (ServiceForm)d.DataContext;
        await frm.CancelService();
    }

    private void SpawnService(HotsServiceInfo service)
    {
        var t = _serviceDic[service.Name].ServiceType;
        var ctor = t.GetConstructor(new Type[] { typeof(IServiceProvider) });
        ctor?.Invoke(new object[]{_svcp});
    }

    private bool StopAllServices()
    {
        var runningServices = _serviceDic.Values.Where(x => x.ServiceForm != null).ToList();
        if (runningServices.Any())
        {
            _sync.Post(
                _ =>
                {
                    runningServices.ForEach(x => x.ServiceForm.Close());
                },
                null);
            return true;
        }

        return false;
    }
}
