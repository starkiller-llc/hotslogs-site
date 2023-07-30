using HOTSLogsUploader.Core.Properties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HOTSLogsUploader.Core.Views
{
    /// <summary>
    /// Interaction logic for PlayerProfile.xaml
    /// </summary>
    public partial class PlayerProfile : Window
    {
        public PlayerProfile()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var match = new Regex(".*PlayerID=(\\d+)", RegexOptions.IgnoreCase).Match(ProfileEdit.Text);

            if (match.Success && int.TryParse(match.Groups[0].Value, out int playerId))
            {
                Settings.Default.PlayerID = playerId;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.PlayerID > 0)
            {
                ProfileEdit.Text = $"https://www.hotslogs.com/Player/Profile?PlayerID={Settings.Default.PlayerID}";
            }            
        }
    }
}
