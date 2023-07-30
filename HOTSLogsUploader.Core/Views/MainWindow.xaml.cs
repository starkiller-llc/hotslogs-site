using HOTSLogsUploader.Core;
using HOTSLogsUploader.Core.Extensions;
using HOTSLogsUploader.Core.Models;
using HOTSLogsUploader.Core.Models.Db.Entity;
using HOTSLogsUploader.Core.Properties;
using HOTSLogsUploader.Core.Utilities.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace HOTSLogsUploader.Core.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ICommonUploadService uploadService;
        private readonly IMatchSummaryService matchSummaryService;
        private readonly System.Windows.Forms.NotifyIcon trayIcon;

        public MainWindow(IServiceProvider serviceProvider, IReplayScannerService replayScannerService, ICommonUploadService uploadService, IMatchSummaryService matchSummaryService)
        {
            InitializeComponent();
            this.serviceProvider = serviceProvider;
            this.replayScannerService = replayScannerService;
            this.uploadService = uploadService;
            this.matchSummaryService = matchSummaryService;
            Properties.Settings.Default.Upgrade();
            ReplayListView.ItemsSource = replayScannerService.Data;

            trayIcon = SetupNotificationIcon();
        }

        public IReplayScannerService replayScannerService { get; init; }

        private System.Windows.Forms.NotifyIcon SetupNotificationIcon()
        {
            using var icon = Application.GetResourceStream(new Uri("pack://application:,,,/Images/favicon-light.ico")).Stream;
            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon()
            {
                Icon = new System.Drawing.Icon(icon),
                Visible = false
            };

            ni.DoubleClick += delegate (object sender, EventArgs e)
            {
                trayIcon.Visible = false;
                Show();
                WindowState = WindowState.Normal;
            };

            if (Properties.Settings.Default.LaunchInTray)
            {
                WindowState = WindowState.Minimized;
            }

            return ni;
        }

        private void SetDefaultReplayFolder()
        {
            // Get the "Heroes of the Storm" base folder in Documents
            var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Heroes of the Storm\\Accounts");
            if (Directory.Exists(baseDir))
            {
                Properties.Settings.Default.ReplayDirectory = baseDir;
                Properties.Settings.Default.Save();
            }
        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            SetDefaultReplayFolder();
            replayScannerService.ReplayDirectory = Properties.Settings.Default.ReplayDirectory;
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {
            // show about
        }

        private void options_Click(object sender, RoutedEventArgs e)
        {
            Options optionsView = new Options() { Owner = this };
            optionsView.ShowDialog();

            Properties.Settings.Default.Reload();
            replayScannerService.ReplayDirectory = Properties.Settings.Default.ReplayDirectory;
        }

        private void viewProfile_Click(object sender, RoutedEventArgs e)
        {
            //StatusPanel.Visibility = (StatusPanel.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void uploadReplays_Click(object sender, RoutedEventArgs e)
        {
            var selectedReplays = ReplayListView.SelectedCells.Select(i => (Replay)i.Item).Distinct().ToList();
            var single = selectedReplays.Count == 1;
            foreach (var replay in selectedReplays)
            {
                var job = new UploadJob { Replay = replay };
                if (Settings.Default.ShowMatchSummary && single)
                {
                    job.OnUploadSuccess = matchSummaryService.ShowMatchSummaryAction;
                }
                uploadService.AddToQueue(job);                
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                trayIcon.Visible = true;
                Hide();
            }
            base.OnStateChanged(e);
        }
    }
}
