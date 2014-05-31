using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using ssh_tunnel_agent.Tray;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace ssh_tunnel_agent {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    // app icon https://www.iconfinder.com/icons/15459/network_receive_icon
    // Public Domain by Various - http://tango.freedesktop.org/The_People
    //
    // configure image https://www.iconfinder.com/icons/274895/
    // Free for commercial use by Popcic
    //
    // connection status images, color replaced of http://openiconlibrary.sourceforge.net/gallery2/?./Icons/status/user-online.png
    // no source or license listed

    public partial class App : Application {
        private static Mutex _mutex;

        public static string Plink { get; set; }

        private static TaskbarIcon _trayIcon;
        public static TaskbarIcon TrayIcon {
            get {
                if (_trayIcon == null)
                    _trayIcon = (TaskbarIcon)App.Current.FindResource("TrayIcon");

                return _trayIcon;
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e) {
            try {
                closeViewModel(false);
            }
            catch (Exception) { }

            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
        }

        private void closeViewModel(bool kill) {
            if (TrayIcon == null)
                return;

            TrayViewModel viewModel = TrayIcon.DataContext as TrayViewModel;
            if (viewModel == null)
                return;

            viewModel.DisconnectSessions(kill);
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
            bool createdNew;
            _mutex = new Mutex(true, "{BBCB93D1-0DF2-4BB9-8825-D111A44D33FA}", out createdNew);
            if (!createdNew) {
                //MessageBox.Show("This program is accessed through its tray icon next to the clock.", "SSH Tunnel Agent", MessageBoxButton.OK, MessageBoxImage.Information);
                App.Current.Shutdown();
                return;
            }

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            try {
                FindResource("TrayIcon");
            }
            catch (Exception) {
                App.showErrorMessage("Unable to load a required embedded extension.");
                App.Current.Shutdown();
                return;
            }

            Plink = FindFile("plink.exe", new Version(0, 63));
            if (Plink == null) { // not worth continuing from here :(
                App.showErrorMessage("Failed to find up-to-date plink.exe.");
                App.Current.Shutdown();
            }

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e) {
            if (e.Mode == PowerModes.Resume)
                closeViewModel(true);
        }

        public static string FindFile(string file, Version version) {
            string myFile = "ssh-tunnel-agent-" + file;
            if (matchFileVersion(file, version)) { // utilize user's existing copy if same version or newer than ours
                try { File.Delete(myFile); } // attempt cleanup
                catch (Exception) { }

                return file;
            }

            if (upgradeFile(file, myFile, version)) // try to write our copy to current directory
                return myFile;

            myFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ssh-tunnel-agent", file);
            if (upgradeFile(file, myFile, version)) // last resort, write to user's roaming appdata
                return myFile;

            return null;
        }

        private static bool upgradeFile(string file, string myFile, Version version) {
            if (matchFileVersion(myFile, version))
                return true;

            using (Stream resource = getEmbedded(file)) {
                if (resource != null)
                    try {
                        string path = Path.GetDirectoryName(myFile);
                        if (path != String.Empty)
                            Directory.CreateDirectory(path);

                        using (FileStream fileStream = new FileStream(myFile, FileMode.Create, FileAccess.Write)) {
                            resource.CopyTo(fileStream);
                            return true;
                        }
                    }
                    catch (Exception) { }
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

        public Assembly CurrentDomain_AssemblyResolve(Object sender, ResolveEventArgs args) {
            string name = args.Name.Substring(0, args.Name.IndexOf(',')) + @".dll";

            if (name == "ssh-tunnel-agent.resources.dll")
                return null;

            Debug.WriteLine("attempt to find assembly: " + name);

            using (Stream resource = getEmbedded(name)) {
                if (resource == null)
                    return null;

                byte[] read = new byte[(int)resource.Length];
                resource.Read(read, 0, (int)resource.Length);
                return Assembly.Load(read);
            }
        }

        public static void showErrorMessage(string message) {
            MessageBox.Show(message, "SSH Tunnel Agent", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
