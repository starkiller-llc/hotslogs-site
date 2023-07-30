namespace HOTSLogsUploader
{
    partial class Options
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Options));
            this.buttonSaveSettings = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBoxUpdateInterval = new System.Windows.Forms.ComboBox();
            this.textBoxUpdateFrequency = new System.Windows.Forms.TextBox();
            this.checkBoxSendToHP = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoUpdate = new System.Windows.Forms.CheckBox();
            this.checkBoxAutomaticUpload = new System.Windows.Forms.CheckBox();
            this.checkBoxLaunchInTray = new System.Windows.Forms.CheckBox();
            this.checkBoxEnableMatchSummary = new System.Windows.Forms.CheckBox();
            this.checkBoxIsRunOnStartup = new System.Windows.Forms.CheckBox();
            this.buttonSetPlayerProfile = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBoxReplayFolder = new System.Windows.Forms.TextBox();
            this.buttonReplayFolderBrowser = new System.Windows.Forms.Button();
            this.checkBoxEnableTrayNotifications = new System.Windows.Forms.CheckBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.buttonResetAllUploads = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonSaveSettings
            // 
            this.buttonSaveSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSaveSettings.Location = new System.Drawing.Point(9, 252);
            this.buttonSaveSettings.Name = "buttonSaveSettings";
            this.buttonSaveSettings.Size = new System.Drawing.Size(170, 23);
            this.buttonSaveSettings.TabIndex = 0;
            this.buttonSaveSettings.Text = "Save";
            this.buttonSaveSettings.UseVisualStyleBackColor = true;
            this.buttonSaveSettings.Click += new System.EventHandler(this.buttonSaveSettings_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.comboBoxUpdateInterval);
            this.groupBox1.Controls.Add(this.textBoxUpdateFrequency);
            this.groupBox1.Controls.Add(this.checkBoxSendToHP);
            this.groupBox1.Controls.Add(this.checkBoxAutoUpdate);
            this.groupBox1.Controls.Add(this.checkBoxAutomaticUpload);
            this.groupBox1.Controls.Add(this.checkBoxLaunchInTray);
            this.groupBox1.Controls.Add(this.checkBoxEnableMatchSummary);
            this.groupBox1.Controls.Add(this.checkBoxIsRunOnStartup);
            this.groupBox1.Controls.Add(this.buttonSetPlayerProfile);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.checkBoxEnableTrayNotifications);
            this.groupBox1.Location = new System.Drawing.Point(13, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(468, 234);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // comboBoxUpdateInterval
            // 
            this.comboBoxUpdateInterval.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxUpdateInterval.FormattingEnabled = true;
            this.comboBoxUpdateInterval.Items.AddRange(new object[] {
            "Minutes",
            "Hours",
            "Days",
            "Months"});
            this.comboBoxUpdateInterval.Location = new System.Drawing.Point(255, 131);
            this.comboBoxUpdateInterval.Name = "comboBoxUpdateInterval";
            this.comboBoxUpdateInterval.Size = new System.Drawing.Size(74, 21);
            this.comboBoxUpdateInterval.TabIndex = 15;
            this.comboBoxUpdateInterval.SelectedIndexChanged += new System.EventHandler(this.comboBoxUpdateInterval_SelectedIndexChanged);
            // 
            // textBoxUpdateFrequency
            // 
            this.textBoxUpdateFrequency.Location = new System.Drawing.Point(216, 131);
            this.textBoxUpdateFrequency.Name = "textBoxUpdateFrequency";
            this.textBoxUpdateFrequency.Size = new System.Drawing.Size(33, 20);
            this.textBoxUpdateFrequency.TabIndex = 14;
            this.textBoxUpdateFrequency.Text = "4";
            this.textBoxUpdateFrequency.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxUpdateFrequency.TextChanged += new System.EventHandler(this.textBoxUpdateFrequency_TextChanged);
            // 
            // checkBoxSendToHP
            // 
            this.checkBoxSendToHP.AutoSize = true;
            this.checkBoxSendToHP.Location = new System.Drawing.Point(5, 156);
            this.checkBoxSendToHP.Name = "checkBoxSendToHP";
            this.checkBoxSendToHP.Size = new System.Drawing.Size(290, 17);
            this.checkBoxSendToHP.TabIndex = 13;
            this.checkBoxSendToHP.Text = "Send a Copy of Uploaded Replays to HeroesProfile.com";
            this.checkBoxSendToHP.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoUpdate
            // 
            this.checkBoxAutoUpdate.AutoSize = true;
            this.checkBoxAutoUpdate.Location = new System.Drawing.Point(5, 133);
            this.checkBoxAutoUpdate.Name = "checkBoxAutoUpdate";
            this.checkBoxAutoUpdate.Size = new System.Drawing.Size(210, 17);
            this.checkBoxAutoUpdate.TabIndex = 12;
            this.checkBoxAutoUpdate.Text = "Automatically Check for Updates Every";
            this.checkBoxAutoUpdate.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutomaticUpload
            // 
            this.checkBoxAutomaticUpload.AutoSize = true;
            this.checkBoxAutomaticUpload.Location = new System.Drawing.Point(5, 110);
            this.checkBoxAutomaticUpload.Name = "checkBoxAutomaticUpload";
            this.checkBoxAutomaticUpload.Size = new System.Drawing.Size(166, 17);
            this.checkBoxAutomaticUpload.TabIndex = 11;
            this.checkBoxAutomaticUpload.Text = "Upload Replays Automatically";
            this.checkBoxAutomaticUpload.UseVisualStyleBackColor = true;
            // 
            // checkBoxLaunchInTray
            // 
            this.checkBoxLaunchInTray.AutoSize = true;
            this.checkBoxLaunchInTray.Location = new System.Drawing.Point(5, 64);
            this.checkBoxLaunchInTray.Name = "checkBoxLaunchInTray";
            this.checkBoxLaunchInTray.Size = new System.Drawing.Size(132, 17);
            this.checkBoxLaunchInTray.TabIndex = 10;
            this.checkBoxLaunchInTray.Text = "Start Minimized in Tray";
            this.checkBoxLaunchInTray.UseVisualStyleBackColor = true;
            // 
            // checkBoxEnableMatchSummary
            // 
            this.checkBoxEnableMatchSummary.AutoSize = true;
            this.checkBoxEnableMatchSummary.Location = new System.Drawing.Point(5, 87);
            this.checkBoxEnableMatchSummary.Name = "checkBoxEnableMatchSummary";
            this.checkBoxEnableMatchSummary.Size = new System.Drawing.Size(259, 17);
            this.checkBoxEnableMatchSummary.TabIndex = 9;
            this.checkBoxEnableMatchSummary.Text = "Launch Match Summary Webpage on Game End";
            this.checkBoxEnableMatchSummary.UseVisualStyleBackColor = true;
            // 
            // checkBoxIsRunOnStartup
            // 
            this.checkBoxIsRunOnStartup.AutoSize = true;
            this.checkBoxIsRunOnStartup.Location = new System.Drawing.Point(6, 42);
            this.checkBoxIsRunOnStartup.Name = "checkBoxIsRunOnStartup";
            this.checkBoxIsRunOnStartup.Size = new System.Drawing.Size(98, 17);
            this.checkBoxIsRunOnStartup.TabIndex = 7;
            this.checkBoxIsRunOnStartup.Text = "Run on Startup";
            this.checkBoxIsRunOnStartup.UseVisualStyleBackColor = true;
            // 
            // buttonSetPlayerProfile
            // 
            this.buttonSetPlayerProfile.Location = new System.Drawing.Point(335, 18);
            this.buttonSetPlayerProfile.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSetPlayerProfile.Name = "buttonSetPlayerProfile";
            this.buttonSetPlayerProfile.Size = new System.Drawing.Size(122, 28);
            this.buttonSetPlayerProfile.TabIndex = 6;
            this.buttonSetPlayerProfile.Text = "Set Player Profile";
            this.buttonSetPlayerProfile.UseVisualStyleBackColor = true;
            this.buttonSetPlayerProfile.Click += new System.EventHandler(this.buttonSetPlayerProfile_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.textBoxReplayFolder);
            this.groupBox2.Controls.Add(this.buttonReplayFolderBrowser);
            this.groupBox2.Location = new System.Drawing.Point(5, 180);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(457, 48);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Heroes of the Storm folder (e.g. Documents\\Heroes of the Storm)";
            // 
            // textBoxReplayFolder
            // 
            this.textBoxReplayFolder.Location = new System.Drawing.Point(6, 18);
            this.textBoxReplayFolder.Name = "textBoxReplayFolder";
            this.textBoxReplayFolder.Size = new System.Drawing.Size(417, 20);
            this.textBoxReplayFolder.TabIndex = 4;
            // 
            // buttonReplayFolderBrowser
            // 
            this.buttonReplayFolderBrowser.Location = new System.Drawing.Point(427, 16);
            this.buttonReplayFolderBrowser.Name = "buttonReplayFolderBrowser";
            this.buttonReplayFolderBrowser.Size = new System.Drawing.Size(24, 23);
            this.buttonReplayFolderBrowser.TabIndex = 2;
            this.buttonReplayFolderBrowser.Text = "...";
            this.buttonReplayFolderBrowser.UseVisualStyleBackColor = true;
            this.buttonReplayFolderBrowser.Click += new System.EventHandler(this.buttonReplayFolderBrowser_Click);
            // 
            // checkBoxEnableTrayNotifications
            // 
            this.checkBoxEnableTrayNotifications.AutoSize = true;
            this.checkBoxEnableTrayNotifications.Location = new System.Drawing.Point(6, 19);
            this.checkBoxEnableTrayNotifications.Name = "checkBoxEnableTrayNotifications";
            this.checkBoxEnableTrayNotifications.Size = new System.Drawing.Size(144, 17);
            this.checkBoxEnableTrayNotifications.TabIndex = 1;
            this.checkBoxEnableTrayNotifications.Text = "Enable Tray Notifications";
            this.checkBoxEnableTrayNotifications.UseVisualStyleBackColor = true;
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // buttonResetAllUploads
            // 
            this.buttonResetAllUploads.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonResetAllUploads.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonResetAllUploads.ForeColor = System.Drawing.Color.Red;
            this.buttonResetAllUploads.Location = new System.Drawing.Point(185, 252);
            this.buttonResetAllUploads.Name = "buttonResetAllUploads";
            this.buttonResetAllUploads.Size = new System.Drawing.Size(112, 23);
            this.buttonResetAllUploads.TabIndex = 2;
            this.buttonResetAllUploads.Text = "Reset All Uploads";
            this.buttonResetAllUploads.UseVisualStyleBackColor = true;
            this.buttonResetAllUploads.Click += new System.EventHandler(this.buttonResetAllUploads_Click);
            // 
            // Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.ClientSize = new System.Drawing.Size(490, 283);
            this.Controls.Add(this.buttonResetAllUploads);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonSaveSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Options";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Options_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonSaveSettings;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxEnableTrayNotifications;
        private System.Windows.Forms.TextBox textBoxReplayFolder;
        private System.Windows.Forms.Button buttonReplayFolderBrowser;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonResetAllUploads;
        private System.Windows.Forms.Button buttonSetPlayerProfile;
        private System.Windows.Forms.CheckBox checkBoxIsRunOnStartup;
        private System.Windows.Forms.CheckBox checkBoxEnableMatchSummary;
        private System.Windows.Forms.CheckBox checkBoxLaunchInTray;
        private System.Windows.Forms.CheckBox checkBoxAutomaticUpload;
        private System.Windows.Forms.CheckBox checkBoxAutoUpdate;
        private System.Windows.Forms.CheckBox checkBoxSendToHP;
        private System.Windows.Forms.TextBox textBoxUpdateFrequency;
        private System.Windows.Forms.ComboBox comboBoxUpdateInterval;
    }
}