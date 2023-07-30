using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace HOTSLogsUploader
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
        

        private void About_Shown(object sender, EventArgs e)
        {
            this.Text = $"About (version {Application.ProductVersion})";
        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start($"mailto:{linkLabel1.Text}");
        }
    }
}
