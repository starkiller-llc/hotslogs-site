using HOTSLogsUploader.Core.Models;
using HOTSLogsUploader.Core.Properties;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using static HOTSLogsUploader.Core.Extensions.HotsLogsExtensions;


namespace HOTSLogsUploader.Core.Views
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();            
        }

        
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Only save if they acknowledge no replays are in the folder            
            if (Directory.Exists(ReplayFolder.Text))
            {
                if (!GetReplaysInDirectory(ReplayFolder.Text, Constants.ReplayExtensionFilter).Any() &&
                    MessageBox.Show("No replay files could be found in the specified folder or any of it's subfolders. Are you sure you want to use this folder?",
                                    "No Replays Found", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            else
            {
                MessageBox.Show("The selected folder does not exist.", "Folder not found", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Prevent "spamming" of update checks
            if (FrequencyType.SelectedIndex == 0 && Convert.ToInt32(FrequencyInterval.Text) < 30)
            {
                MessageBox.Show("The minimum update interval is 30 minutes.", "Update check too frequent", MessageBoxButton.OK, MessageBoxImage.Error);
                FrequencyInterval.Text = "30";
                return;
            }

            // run at startup
            var runAtStartup = new RunAtStartup(Application.ResourceAssembly.GetName().Name, $"\"{ System.Reflection.Assembly.GetExecutingAssembly().Location }\"");
            if (Settings.Default.LaunchOnStartup)
            {
                if (!runAtStartup.WillRunAtStartup)
                {
                    runAtStartup.WillRunAtStartup = true;
                }
            }
            else
            {
                if (runAtStartup.WillRunAtStartup)
                {
                    runAtStartup.WillRunAtStartup = false;
                }
            }

            Settings.Default.ShowTrayNotifications = cbShowInTray.IsChecked.Value;
            Settings.Default.ReplayDirectory = ReplayFolder.Text;
            Settings.Default.LaunchOnStartup = cbRunAtStartup.IsChecked.Value;
            Settings.Default.LaunchInTray = cbStartInTray.IsChecked.Value;
            Settings.Default.AutomaticUpload = cbAutoUpload.IsChecked.Value;
            Settings.Default.AutoUpdate = cbAutoUpdate.IsChecked.Value;
            Settings.Default.ShowMatchSummary = cbShowMatchSummary.IsChecked.Value;
            Settings.Default.AutoUpdateFrequency = Convert.ToInt32(FrequencyInterval.Text);
            Settings.Default.AutoUpdateInterval = FrequencyType.SelectedIndex;

            // save changes
            Settings.Default.Save();
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PlayerProfile profile = new PlayerProfile() { Owner = this };
            profile.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Settings.Default.ReplayDirectory = dialog.SelectedPath;
            }
        }
    }
}
