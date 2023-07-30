using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace HOTSLogsUploader
{
    public partial class Options : Form
    {
        private const string ApplicationName = "HOTS Logs Uploader";

        public Options()
        {
            InitializeComponent();
        }        


        private void buttonReplayFolderBrowser_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxReplayFolder.Text = folderBrowserDialog1.SelectedPath;
            }                
        }


        private void buttonSaveSettings_Click(object sender, EventArgs e)
        {
            // Only save if they acknowledge no replays are in the folder
            textBoxReplayFolder.Text = textBoxReplayFolder.Text.Trim();
            if (Directory.Exists(textBoxReplayFolder.Text))
            {
                if (Directory.GetFiles(textBoxReplayFolder.Text, Constants.ReplayExtensionFilter, SearchOption.AllDirectories).Length == 0 &&
                    MessageBox.Show("No replay files could be found in the specified folder or any of it's subfolders. Are you sure you want to use this folder?", 
                                    "No Replays Found", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
                {                    
                    return;
                }                
            }
            else
            {
                MessageBox.Show("The selected folder does not exist.", "Folder not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Prevent "spamming" of update checks
            if (comboBoxUpdateInterval.SelectedIndex == 0 && Convert.ToInt32(textBoxUpdateFrequency.Text) < 30)
            {
                MessageBox.Show("The minimum update interval is 30 minutes.", "Update check too frequent", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxUpdateFrequency.Text = "30";
                return;
            }
            
            // run at startup
            var runAtStartup = new RunAtStartup(Application.ProductName, $"\"{ System.Reflection.Assembly.GetExecutingAssembly().Location }\"");
            if (checkBoxIsRunOnStartup.Checked)
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

            // save settings
            Properties.Settings.Default.ShowTrayNotifications = checkBoxEnableTrayNotifications.Checked;
            Properties.Settings.Default.ReplayDirectory = textBoxReplayFolder.Text;
            Properties.Settings.Default.LaunchOnStartup = checkBoxIsRunOnStartup.Checked;
            Properties.Settings.Default.LaunchInTray = checkBoxLaunchInTray.Checked;
            Properties.Settings.Default.AutomaticUpload = checkBoxAutomaticUpload.Checked;
            Properties.Settings.Default.AutoUpdate = checkBoxAutoUpdate.Checked;
            Properties.Settings.Default.SendtoHP = checkBoxSendToHP.Checked;
            Properties.Settings.Default.ShowMatchSummary = checkBoxEnableMatchSummary.Checked;
            Properties.Settings.Default.AutoUpdateFrequency = Convert.ToInt32(textBoxUpdateFrequency.Text);
            Properties.Settings.Default.AutoUpdateInterval = comboBoxUpdateInterval.SelectedIndex;

            // save changes
            Properties.Settings.Default.Save();
            Close();
        }

        private void Options_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void buttonResetAllUploads_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This will attempt to re-upload all of your replay files.\r\n\r\nAre you sure you want to continue?", "Reset All Uploads", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {              
                (Owner as Main).ResetUploads();
                Hide();
            }
        }

        private void buttonSetPlayerProfile_Click(object sender, EventArgs e)
        {
            using (var playerIdForm = new SetPlayerID())
            {
                playerIdForm.ShowDialog();
            }            
        }


        private void Options_Load(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();

            checkBoxEnableTrayNotifications.Checked = Properties.Settings.Default.ShowTrayNotifications;
            checkBoxEnableMatchSummary.Checked = Properties.Settings.Default.ShowMatchSummary;
            checkBoxIsRunOnStartup.Checked = Properties.Settings.Default.LaunchOnStartup;
            checkBoxLaunchInTray.Checked = Properties.Settings.Default.LaunchInTray;
            checkBoxAutomaticUpload.Checked = Properties.Settings.Default.AutomaticUpload;
            checkBoxAutoUpdate.Checked = Properties.Settings.Default.AutoUpdate;
            checkBoxSendToHP.Checked = Properties.Settings.Default.SendtoHP;
            textBoxReplayFolder.Text = Properties.Settings.Default.ReplayDirectory;
            textBoxUpdateFrequency.Text = Properties.Settings.Default.AutoUpdateFrequency.ToString();
            comboBoxUpdateInterval.SelectedIndex = Properties.Settings.Default.AutoUpdateInterval;
        }


        private void textBoxUpdateFrequency_TextChanged(object sender, EventArgs e)
        {
            var r = Regex.Replace(((TextBox)sender).Text, "[^0-9]", "");
            if (((TextBox)sender).Text != r)
            {
                ((TextBox)sender).Text = r;
            }
        }

        private void comboBoxUpdateInterval_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex == 0 && Convert.ToInt32(textBoxUpdateFrequency.Text) < 30)
            {
                textBoxUpdateFrequency.Text = "30";
            }
        }
    }
}
