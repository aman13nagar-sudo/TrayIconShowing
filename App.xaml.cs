//using Microsoft.UI;
//using Microsoft.UI.Windowing;
//using Microsoft.UI.Xaml;
//using System;
//using System.Diagnostics;
//using Windows.Win32;
//using Windows.Win32.Foundation;
//using Windows.Win32.UI.WindowsAndMessaging;

//namespace TrayIconShowing
//{
//    public partial class App : Application
//    {
//        private MainWindow? _mainWindow;
//        private SystemTrayIcon? _trayIcon;
//        private AppWindow? _appWindow;
//        private HWND _windowHandle;

//        public App()
//        {
//            InitializeComponent();
//        }

//        protected override void OnLaunched(LaunchActivatedEventArgs args)
//        {
//            _mainWindow = new MainWindow();
//            _mainWindow.Activate();

//            // Get window handle & AppWindow
//            _windowHandle = (HWND)WinRT.Interop.WindowNative.GetWindowHandle(_mainWindow);
//            _appWindow = GetAppWindow(_mainWindow);

//            // Initialize System Tray Icon
//            InitializeTrayIcon();

//            // Minimize the window initially
//            _mainWindow.DispatcherQueue.TryEnqueue(() =>
//            {
//                _appWindow?.Hide();
//            });

//            // Handle closing event
//            _mainWindow.OnWindowClosing += HandleWindowClosing;
//        }

//        private void InitializeTrayIcon()
//        {
//            _trayIcon = new SystemTrayIcon(_windowHandle);

//            // Left-click to restore app
//            _trayIcon.LeftClick += () =>
//            {
//                Debug.WriteLine("Tray icon left-clicked!");
//                _mainWindow?.DispatcherQueue.TryEnqueue(() =>
//                {
//                    ShowMainWindow();
//                });
//            };

//            // Right-click context menu
//            _trayIcon.RightClick += () =>
//            {
//                Debug.WriteLine("Tray icon right-clicked!");
//                _trayIcon.ShowContextMenu();
//            };
//        }

//        public void ShowMainWindow()
//        {
//            // Refresh window handle
//            _windowHandle = (HWND)WinRT.Interop.WindowNative.GetWindowHandle(_mainWindow);

//            if (_appWindow != null)
//            {
//                _appWindow.Show(); // Show app
//                PInvoke.ShowWindow(_windowHandle, SHOW_WINDOW_CMD.SW_RESTORE); // Restore if minimized
//                PInvoke.SetForegroundWindow(_windowHandle); // Bring to front
//                _mainWindow!.Activate();
//                PositionWindowBottomRight();
//            }
//        }

//        private void HandleWindowClosing()
//        {
//            // Instead of closing, hide the window & keep tray icon visible
//            _mainWindow?.DispatcherQueue.TryEnqueue(() =>
//            {
//                _appWindow?.Hide();
//            });
//        }

//        private static AppWindow GetAppWindow(MainWindow window)
//        {
//            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
//            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
//            return AppWindow.GetFromWindowId(windowId);
//        }

//        private void PositionWindowBottomRight()
//        {
//            if (_appWindow == null) return;

//            var displayArea = DisplayArea.GetFromWindowId(_appWindow.Id, DisplayAreaFallback.Primary);
//            if (displayArea != null)
//            {
//                var bounds = displayArea.WorkArea;
//                int windowWidth = 400;
//                int windowHeight = 600;
//                int x = bounds.Width - windowWidth - 10;
//                int y = bounds.Height - windowHeight - 10;

//                _appWindow.MoveAndResize(new Windows.Graphics.RectInt32(x, y, windowWidth, windowHeight));
//            }
//        }
//    }
//}


using Microsoft.UI.Xaml;
using System;
using H.NotifyIcon;
using TrayIconShowing;
using Microsoft.UI.Windowing;
using Vanara.PInvoke;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using TrayIconApp;


namespace TrayIconShowing
{
    public partial class App : Application
    {
        private MainWindow? _mainWindow;
        private NetworkWatcher? _networkWatcher;
        private AppWindow? _appWindow;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _mainWindow = new MainWindow();
            _appWindow = GetAppWindow(_mainWindow);
            PositionWindowBottomRight(); // ✅ Position before showing
            _mainWindow.Hide(); // Start minimized to tray
            _appWindow.Hide();  // Hide initially

            // Set the window icon
            SetWindowIcon(_appWindow, "Assets/hugeicons__chat_bot_Bg0_icon.ico");

            _networkWatcher = new NetworkWatcher(DispatcherQueue.GetForCurrentThread());
            _networkWatcher.WiFiDisconnected += ShowAppOnWiFiOff;
        }

        private void SetWindowIcon(AppWindow appWindow, string iconPath)
        {
            try
            {
                appWindow.SetIcon(iconPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set window icon: {ex.Message}");
            }
        }
        private void ShowAppOnWiFiOff()
        {
            ShowMainWindow();
        }
        private void PositionWindowBottomRight()
        {
            if (_appWindow == null) return;

            var displayArea = DisplayArea.GetFromWindowId(_appWindow.Id, DisplayAreaFallback.Primary);
            if (displayArea != null)
            {
                var bounds = displayArea.WorkArea;
                int windowWidth = 400;
                int windowHeight = 600;
                int x = bounds.X + bounds.Width - windowWidth - 10;
                int y = bounds.Y + bounds.Height - windowHeight - 10;

                // Ensure we move & resize before showing
                _appWindow.MoveAndResize(new Windows.Graphics.RectInt32(x, y, windowWidth, windowHeight));
            }
        }

        public void ShowMainWindow()
        {
            if (_mainWindow == null)
            {
                _mainWindow = new MainWindow();
                _appWindow = GetAppWindow(_mainWindow);
            }

            PositionWindowBottomRight(); // ✅ Ensure positioning happens before showing
            _appWindow?.Show();
            _mainWindow.Activate();
        }
        private static AppWindow GetAppWindow(MainWindow window)
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(windowId);
        }
    }
}
