using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace ssh_tunnel_agent {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void onStartup(object sender, StartupEventArgs e) {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            upgradeFile(@"ssh-tunnel-agent-plink.exe", @"Release 0.63");

            try {
                FindResource("MyNotifyIcon");
            }
            catch (Exception) {
                Application.Current.Shutdown();
            }
        }

        private void upgradeFile(string name, string version) {
            if (File.Exists(name) && FileVersionInfo.GetVersionInfo(name).FileVersion == version)
                return;

            using (Stream resource = getEmbedded(name)) {
                if (resource != null)
                    using (FileStream file = new FileStream(name, FileMode.Create, FileAccess.Write)) {
                        resource.CopyTo(file);
                    }
            }
        }

        public Assembly CurrentDomain_AssemblyResolve(Object sender, ResolveEventArgs args) {
            string name = args.Name.Substring(0, args.Name.IndexOf(',')) + @".dll";

            if (name != @"Hardcodet.Wpf.TaskbarNotification.dll")
                return null;

            using (Stream resource = getEmbedded(name)) {
                if (resource == null)
                    return null;

                byte[] read = new byte[(int)resource.Length];
                resource.Read(read, 0, (int)resource.Length);
                return Assembly.Load(read);
            }
        }

        private Stream getEmbedded(string name) {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(@"ssh_tunnel_agent.Embedded." + name);
        }
    }
}
