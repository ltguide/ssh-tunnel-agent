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
    // app icon comes from https://www.iconfinder.com/icons/15459/network_receive_icon
    // Public Domain by Various - http://tango.freedesktop.org/The_People

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

            Plink = FindFile("plink.exe", new Version(0, 63));
            if (Plink == null)
                shutdown("Failed to find up-to-date plink.exe.");
        }

        public static string FindFile(string file, Version version) {
            string myFile = "ssh-tunnel-agent-" + file;
            if (matchFileVersion(file, version)) {
                try { File.Delete(myFile); }
                catch (Exception) { }

                return file;
            }
            else if (upgradeFile(file, myFile, version))
                return myFile;
            else
                return null;
        }

        private static bool upgradeFile(string file, string myFile, Version version) {
            if (matchFileVersion(myFile, version))
                return true;

            using (Stream resource = getEmbedded(file)) {
                if (resource != null)
                    using (FileStream fileStream = new FileStream(myFile, FileMode.Create, FileAccess.Write)) {
                        try {
                            resource.CopyTo(fileStream);
                            return true;
                        }
                        catch (IOException) { }
                    }
            }

            return false;
        }

        private static bool matchFileVersion(string name, Version version) {
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

        private static Stream getEmbedded(string name) {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("ssh_tunnel_agent.Embedded." + name);
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
    }
}
