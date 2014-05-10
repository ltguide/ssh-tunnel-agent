using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace ssh_tunnel_agent.Classes {
    public static class WinApi {
        /// <summary>
        /// Gives focus to a given window.
        /// </summary>
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void ActivatePopup(DependencyObject Parent) {
            Popup popup = Parent as Popup;
            if (popup == null || popup.Child == null)
                return;

            //try to get a handle on the popup itself (via its child)
            HwndSource source = (HwndSource)PresentationSource.FromVisual(popup.Child);
            if (source == null)
                return;

            //activate the popup
            SetForegroundWindow(source.Handle);
        }
    }
}
