using Serilog;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace HOTSLogsUploader
{
    public static class Program
    {
        private const string appGuid = "17514efb-3483-4902-b992-9e65430a6912";
        public static Mutex _mutex = new Mutex(false, $"Global\\{appGuid}");


        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            Application.ApplicationExit += Application_ApplicationExit;

            // Make sure only one instance of this uploader runs
            if (!_mutex.WaitOne(100, false))
            { 
                MessageBox.Show("HOTS Logs Uploader is already running", "HOTS Logs Uploader"); 
            }
            else
            {
                // Normal user - run the windows form
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Main());
            }            
        }


        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            _mutex?.ReleaseMutex();
        }
        

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(args.Name);

            var path = assemblyName.Name + ".dll";
            if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
            {
                path = $"{assemblyName.CultureInfo}\\{path}";
            }

            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                {
                    return null;
                }

                var assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }
    }
}
