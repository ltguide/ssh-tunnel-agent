using Hardcodet.Wpf.TaskbarNotification;
using ssh_tunnel_agent.Tray;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace ssh_tunnel_agent {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static string Plink { get; set; }

        private static TaskbarIcon _trayIcon;
        public static TaskbarIcon TrayIcon {
            get {
                if (_trayIcon == null)
                    _trayIcon = (TaskbarIcon)Application.Current.FindResource("TrayIcon");

                return _trayIcon;
            }
        }
        // todo program icons
        private void Application_Exit(object sender, ExitEventArgs e) {
            try {
                closeViewModel();
            }
            catch (Exception) { }
        }

        private void closeViewModel() {
            if (TrayIcon == null)
                return;

            TrayViewModel viewModel = TrayIcon.DataContext as TrayViewModel;
            if (viewModel == null)
                return;

            viewModel.DisconnectSessions();
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            try {
                FindResource("TrayIcon");
            }
            catch (Exception) {
                shutdown("Unable to load required embedded extensions.");
                return;
            }

            string myPlink = "ssh-tunnel-agent-plink.exe";
            if (matchFileVersion("plink.exe", new Version(0, 63))) {
                Plink = "plink.exe";
                try { File.Delete(myPlink); }
                catch (Exception) { }
            }
            else if (upgradeFile(myPlink, new Version(0, 63)))
                Plink = myPlink;
            else {
                shutdown("Failed to find up to date plink.exe or create " + myPlink);
                return;
            }
        }

        private bool upgradeFile(string name, Version version) {
            if (matchFileVersion(name, version))
                return true;

            using (Stream resource = getEmbedded(name)) {
                if (resource != null)
                    using (FileStream file = new FileStream(name, FileMode.Create, FileAccess.Write)) {
                        try {
                            resource.CopyTo(file);
                            return true;
                        }
                        catch (IOException) { }
                    }
            }

            return false;
        }

        private bool matchFileVersion(string name, Version version) {
            if (File.Exists(name))
                try {
                    return new Version(new string(
                            Array.FindAll<char>(
                                FileVersionInfo.GetVersionInfo(name).FileVersion.ToCharArray(),
                                c => char.IsDigit(c) || c == '.'
                            )
                        )) >= version;
                }
                catch (FormatException) { }

            return false;
        }

        private void shutdown(string message) {
            MessageBox.Show(message, "SSH Tunnel Agent", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }

        public Assembly CurrentDomain_AssemblyResolve(Object sender, ResolveEventArgs args) {
            string name = args.Name.Substring(0, args.Name.IndexOf(',')) + @".dll";

            if (name != "Hardcodet.Wpf.TaskbarNotification.dll" && name != "Newtonsoft.Json.dll")
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
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("ssh_tunnel_agent.Embedded." + name);
        }
    }
}
