//using System;
//using System.Runtime.InteropServices;
//using Windows.Win32;
//using Windows.Win32.Foundation;

//using Windows.Win32.UI.Shell;
//using Windows.Win32.UI.WindowsAndMessaging;
//using static Windows.Win32.PInvoke;


//namespace TrayIconShowing
//{

//    public class SystemTrayIcon : IDisposable
//    {
//        private const int WM_USER = 0x0400;
//        private const int WM_TRAYMOUSEMESSAGE = WM_USER + 1;
//        private readonly HWND _windowHandle;
//        private NOTIFYICONDATAW _notifyIconData;
//        private bool _disposed;
//        public event Action? LeftClick;
//        public event Action? RightClick;
//        public SystemTrayIcon(nint windowHandle)
//        {
//            _windowHandle = new HWND(windowHandle);
//            InitializeTrayIcon();
//        }
//        private unsafe void InitializeTrayIcon()
//        {
//            _notifyIconData = new NOTIFYICONDATAW
//            {
//                hWnd = _windowHandle,
//                uID = 1,
//                uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE |
//            NOTIFY_ICON_DATA_FLAGS.NIF_ICON |
//            NOTIFY_ICON_DATA_FLAGS.NIF_TIP,
//                uCallbackMessage = WM_TRAYMOUSEMESSAGE
//            };
//            // Load icon (replace with your own icon)
//            _notifyIconData.hIcon = LoadIcon(
//            default,IDI_APPLICATION); // Default application icon
//                                      // Set tooltip
//            var tipSpan = MemoryMarshal.CreateSpan(ref _notifyIconData.szTip[0], _notifyIconData.szTip.Length);
//            "Chat Bot".AsSpan().CopyTo(tipSpan);
//            _notifyIconData.cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>();
//            Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_ADD, in _notifyIconData);
//        }
//        public static extern bool AppendMenu(
//    IntPtr hMenu,
//    uint uFlags,
//    UIntPtr uIDNewItem,
//    string lpNewItem);
//        public void ShowContextMenu()
//        {
//            var hMenu = CreatePopupMenu();

//            if (hMenu == IntPtr.Zero)
//            {
//                // Handle error
//                return;
//            }

//            const uint MF_STRING = 0x00000000;
//            bool result = AppendMenu(hMenu, MF_STRING, new UIntPtr(1), "Exit");

//            if (!result)
//            {
//                // Handle error
//                return;
//            }


//            GetCursorPos(out var point);
//            SetForegroundWindow(_windowHandle);
//            unsafe
//            {
//                TrackPopupMenu(
//            hMenu,
//            TRACK_POPUP_MENU_FLAGS.TPM_BOTTOMALIGN,
//            point.X,
//            point.Y,
//            0,
//            _windowHandle,
//            null);
//            }
            
//            DestroyMenu(hMenu);
//        }
//        public void HandleWindowMessage(uint message, nint wParam, nint lParam)
//        {
//            if (message == WM_TRAYMOUSEMESSAGE)
//            {
//                switch ((uint)lParam)
//                {
//                    case 0x0204: // WM_RBUTTONUP
//                        RightClick?.Invoke();
//                        break;
//                    case 0x0202: // WM_LBUTTONUP
//                        LeftClick?.Invoke();
//                        break;
//                }
//            }
//        }

//        public void Dispose()
//        {
//            if (_disposed) return;
//            Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, in _notifyIconData);
//            _disposed = true;
//            GC.SuppressFinalize(this);
//        }
//    }
//}

using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.Shell;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace TrayIconShowing
{
    public class SystemTrayIcon : IDisposable
    {
        private const int WM_USER = 0x0400;
        private const int WM_TRAYMOUSEMESSAGE = WM_USER + 1;
        private readonly HWND _windowHandle;
        private NOTIFYICONDATAW _notifyIconData;
        private bool _disposed;

        public event Action? LeftClick;
        public event Action? RightClick;

        public SystemTrayIcon(nint windowHandle)
        {
            _windowHandle = new HWND(windowHandle);
            InitializeTrayIcon();
        }

        private unsafe void InitializeTrayIcon()
        {
            _notifyIconData = new NOTIFYICONDATAW
            {
                cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>(),
                hWnd = _windowHandle,
                uID = 1,
                uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE |
                         NOTIFY_ICON_DATA_FLAGS.NIF_ICON |
                         NOTIFY_ICON_DATA_FLAGS.NIF_TIP,
                uCallbackMessage = WM_TRAYMOUSEMESSAGE
            };

            // Load icon (replace with your own icon)
            _notifyIconData.hIcon = LoadIcon(default, IDI_APPLICATION); // Default application icon

            // Set tooltip
            "Chat Bot".AsSpan().CopyTo(MemoryMarshal.CreateSpan(ref _notifyIconData.szTip[0], _notifyIconData.szTip.Length));

            // Add icon to system tray
            Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_ADD, in _notifyIconData);
        }

        public void ShowContextMenu()
        {
            var hMenu = CreatePopupMenu();
            if (hMenu == IntPtr.Zero)
            {
                return; // Error handling
            }

            const uint MF_STRING = 0x00000000;
            //AppendMenu(hMenu, MF_STRING, new UIntPtr(1), "Exit");

            GetCursorPos(out var point);
            SetForegroundWindow(_windowHandle);

            unsafe
            {
                TrackPopupMenu(hMenu, TRACK_POPUP_MENU_FLAGS.TPM_BOTTOMALIGN | TRACK_POPUP_MENU_FLAGS.TPM_LEFTALIGN,
                               point.X, point.Y, 0, _windowHandle, null);
            }

            DestroyMenu(hMenu);
        }

        public void HandleWindowMessage(uint message, nint wParam, nint lParam)
        {
            if (message == WM_TRAYMOUSEMESSAGE)
            {
                switch ((uint)lParam)
                {
                    case 0x0204: // WM_RBUTTONUP - Right Click
                        RightClick?.Invoke();
                        break;
                    case 0x0201: // WM_LBUTTONDOWN - Left Click
                        LeftClick?.Invoke();
                        break;
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, in _notifyIconData);
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
