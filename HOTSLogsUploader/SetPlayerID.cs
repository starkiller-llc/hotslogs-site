using System;
using System.Linq;
using System.Windows.Forms;

namespace HOTSLogsUploader
{
    public partial class SetPlayerID : Form
    {
        public SetPlayerID()
        {
            InitializeComponent();
        }


        private void buttonSaveSettings_Click(object sender, EventArgs e)
        {
            textBoxPlayerProfileURL.Text = textBoxPlayerProfileURL.Text.Trim();
            if (textBoxPlayerProfileURL.Text.Contains('?') && int.TryParse(textBoxPlayerProfileURL.Text.Split('?')[1].Replace("PlayerID=", ""), out int playerID))
            {
                Properties.Settings.Default.PlayerID = playerID;
                Properties.Settings.Default.Save();
                Close();
            }
            else
            {
                MessageBox.Show("The URL you entered was invalid.  Your HOTS Logs Profile URL should be similar to this: https://www.hotslogs.com/Player/Profile?PlayerID=###", "Error: Invalid HOTS Logs Profile URL");
            }
        }


        private void SetPlayerID_Load(object sender, EventArgs e)
        {
            textBoxPlayerProfileURL.Text = Properties.Settings.Default.PlayerID == -1 ? "" : "https://www.hotslogs.com/Player/Profile?PlayerID=" + Properties.Settings.Default.PlayerID;
        }
    }
}
