using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Runtime.InteropServices;
using WinRT;
namespace TrayIconShowing
{
    public class TrayIconManager
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct NOTIFYICONDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uID;
            public int uFlags;
            public int uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            public int dwState;
            public int dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public int uTimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public int dwInfoFlags;
        }
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA lpData);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr LoadImage(IntPtr hInst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);
        public const int NIM_ADD = 0x00;
        public const int NIM_DELETE = 0x02;
        public const int NIF_MESSAGE = 0x01;
        public const int NIF_ICON = 0x02;
        public const int NIF_TIP = 0x04;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int IMAGE_ICON = 1;
        public const int LR_LOADFROMFILE = 0x00000010;
        private NOTIFYICONDATA _notifyIconData;
        private Window _mainWindow;
        public TrayIconManager(Window mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeTrayIcon();
        }
        private void InitializeTrayIcon()
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(_mainWindow);
            _notifyIconData = new NOTIFYICONDATA();
            _notifyIconData.cbSize = Marshal.SizeOf(_notifyIconData);
            _notifyIconData.hWnd = hWnd;
            _notifyIconData.uID = 1;
            _notifyIconData.uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP;
            _notifyIconData.uCallbackMessage = WM_RBUTTONDOWN;
            _notifyIconData.hIcon = LoadImage(IntPtr.Zero, "icon.ico", IMAGE_ICON, 0, 0, LR_LOADFROMFILE);
            _notifyIconData.szTip = "My App";
            Shell_NotifyIcon(NIM_ADD, ref _notifyIconData);
        }
        public void ShowContextMenu()
        {
            var flyout = new MenuFlyout();
            var openItem = new MenuFlyoutItem { Text = "Open" };
            openItem.Click += (s, e) => _mainWindow.Activate();
            var exitItem = new MenuFlyoutItem { Text = "Exit" };
            exitItem.Click += (s, e) => Application.Current.Exit();
            flyout.Items.Add(openItem);
            flyout.Items.Add(exitItem);
            flyout.ShowAt(null);
        }
    }
}