using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOTSLogsUploader
{
    // Thanks to 'cmd' for the code: https://gist.github.com/nitz/bf0a5d226e194380dea1

    /// <summary>
    /// A tiny class to manage an application running at startup.
    /// </summary>
    class RunAtStartup
    {
        private static readonly string currentVersionRun = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        public string ApplicationName { get; private set; }
        public string StartupCommand { get; private set; }

        public bool WillRunAtStartup
        {
            get { return CheckKeyExists(); }
            set { SetStartupKeyValue(value); }
        }

        /// <summary>
        /// Construct a RunAtStartup instance with the given application name and command.
        /// </summary>
        /// <param name="application_name">The name of your application. Will be used as the registry key name.</param>
        /// <param name="startup_command">The command to execute on startup.</param>
        public RunAtStartup(string application_name, string startup_command)
        {
            if (string.IsNullOrEmpty(application_name))
            {
                throw new ArgumentException("application_name must be a valid, non-empty string!");
            }

            if (string.IsNullOrEmpty(startup_command))
            {
                throw new ArgumentException("startup_command must be a valid, non-empty string!");
            }

            ApplicationName = application_name;
            StartupCommand = startup_command;
        }

        /// <summary>
        /// Checks to see if the key with the application name exists in the registry,
        /// and if it does, if it matches the startup command.
        /// </summary>
        /// <returns>True if the key exists and startup command matches. Otherwise, false.</returns>
        private bool CheckKeyExists()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(currentVersionRun, true);
            string value = (string) registryKey.GetValue(ApplicationName, string.Empty);
            registryKey.Close();

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return value == StartupCommand;
        }

        /// <summary>
        /// Creates or deletes the key to start up an application based on the name and
        /// startup command provided to the constructor.
        /// </summary>
        /// <param name="run_at_startup">If true, create the key. Otherwise, remove it.</param>
        private void SetStartupKeyValue(bool run_at_startup)
        {
            // if the key doesn't exist, we're done.
            if (run_at_startup == false && CheckKeyExists() == false)
            {
                return;
            }

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(currentVersionRun, true);

            if (run_at_startup)
            {
                registryKey.SetValue(ApplicationName, StartupCommand, RegistryValueKind.String);
            }
            else
            {
                registryKey.DeleteValue(ApplicationName);
            }

            registryKey.Close();
        }
    }
}
