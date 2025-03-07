//using Microsoft.UI.Xaml;
//using System;
//using System.IO;
//using Microsoft.Web.WebView2.Core;
//using Windows.Win32;
//using Windows.Win32.Foundation;
//using Windows.Win32.UI.WindowsAndMessaging;
//using Vanara.PInvoke;
//using Microsoft.UI.Xaml.Controls;
//using Microsoft.UI.Xaml.Input;
//using System.ComponentModel;
//using System.Windows.Input;
//using H.NotifyIcon;
//using H.NotifyIcon.Core;
//using Windows.ApplicationModel.Chat;
//using Windows.UI.WebUI;
//using CommunityToolkit.WinUI.Notifications;
//using System.Runtime.InteropServices;
//using Microsoft.UI;
//using System.Net.Http;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using Microsoft.UI.Windowing;
//using WinRT.Interop;
//using Windows.UI.ViewManagement;
//using System.Drawing;
//using Windows.System;

//namespace TrayIconShowing
//{
//    public sealed partial class MainWindow : Window
//    {
//        public event Action? OnWindowClosing;

//        public MainWindow()
//        {
//            InitializeComponent();
//            InitializeWebView();
//            this.Closed += MainWindow_Closed;
//            ShowWindowCommand = new RelayCommand(_ => ShowChatApp());
//            TrayIcon.LeftClickCommand = ShowWindowCommand;
//            this.Closed += OnWindowClosing;
//        }

//        private async void InitializeWebView()
//        {
//            await webView.EnsureCoreWebView2Async();
//            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
//                "app.chatbot",
//                Path.Combine(AppContext.BaseDirectory, "Assets"),
//                CoreWebView2HostResourceAccessKind.Allow);
//            webView.Source = new Uri("http://localhost:3002/");
//        }

//        private void MainWindow_Closed(object sender, WindowEventArgs args)
//        {
//            // Notify App to hide instead of closing
//            args.Handled = true;
//            OnWindowClosing?.Invoke();
//        }
//        private void ShowChatApp()
//        {
//            this.Show();
//            this.Activate();
//        }
//    }
//    public class RelayCommand : ICommand
//    {
//        private readonly Action<object?> _execute;
//        private readonly Func<object?, bool>? _canExecute;

//        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
//        {
//            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
//            _canExecute = canExecute;
//        }

//        public event EventHandler? CanExecuteChanged;

//        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

//        public void Execute(object? parameter) => _execute(parameter);
//    }
//}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.ComponentModel;
using System.Windows.Input;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using Windows.ApplicationModel.Chat;
using Microsoft.Web.WebView2.Core;
using Windows.UI.WebUI;
using CommunityToolkit.WinUI.Notifications;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.UI;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using Vanara.PInvoke;
using Windows.UI.ViewManagement;
using System.Drawing;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.System;
namespace TrayIconShowing
{
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ICommand ShowWindowCommand { get; }
        private AppWindow _appWindow;
        public MainWindow()
        {
            this.InitializeComponent();
            _appWindow = GetAppWindow(this);
            ExtendsContentIntoTitleBar = true; // ✅ Remove default WinUI navbar
            SetTitleBar(null); // ✅ Prevent default title bar behavior
            CustomizeWindow();
            SetAppIcon();
            webView.CoreWebView2Initialized += WebView_CoreWebView2Initialized;
            InitializeWebView();
            //this.Closed += MainWindow_Closed;

            // Command to handle left-click on tray icon
            ShowWindowCommand = new RelayCommand(_ => ShowChatApp());
            TrayIcon.LeftClickCommand = ShowWindowCommand;
            this.Closed += OnWindowClosing;
        }
        private static AppWindow GetAppWindow(Window window)
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(windowId);
        }
        private void OnWindowClosing(object sender, WindowEventArgs args)
        {
            args.Handled = true; // Prevents window from closing
            HideWindow(); // Hides the window instead
        }

        private void HideWindow()
        {
            _appWindow.Hide();
        }

        public void ShowWindow()
        {
            _appWindow.Show();
            Activate();
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0); // Fully closes the app
        }

        private void OnOpenClick(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }
        private void WebView_CoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
        {
            sender.CoreWebView2.WebMessageReceived += async (s, e) =>
            {
                string message = e.WebMessageAsJson;
                if (message.Contains("minimize"))
                {
                    MinimizeWindow();
                }
                else if (message.Contains("close"))
                {
                    CloseWindow();
                }
            };
        }
        private void MinimizeWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            User32.ShowWindow(hWnd, Vanara.PInvoke.ShowWindowCommand.SW_MINIMIZE); // ✅ Corrected
        }
        private void CloseWindow()
        {
            this.Close();
        }

        private void CustomizeWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            // Make the title bar transparent
            appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed; // Reduce height

            // Hide system buttons (minimize, maximize, close)
            appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }
        private async void SendMessageToOllama(string userMessage)
        {
            try
            {
                var response = await FetchOllamaResponse(userMessage);
                await webView.CoreWebView2.ExecuteScriptAsync($"displayResponse({JsonSerializer.Serialize(response)})");
            }
            catch (Exception ex)
            {
                await webView.CoreWebView2.ExecuteScriptAsync($"displayError({JsonSerializer.Serialize(ex.Message)})");
            }
        }

        private async Task<string> FetchOllamaResponse(string message)
        {
            using var httpClient = new HttpClient();
            var requestData = new { prompt = message, model = "phi-4" };
            var jsonRequest = JsonSerializer.Serialize(requestData);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("http://localhost:11434/api/generate", content);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        private const uint WM_SETICON = 0x0080;


        private void SetAppIcon()
        {
            //string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "hugeicons__chat_bot_Bg0_icon.ico");
            //if (File.Exists(iconPath))
            //{
            //    var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(
            //        Win32Interop.GetWindowIdFromWindow(
            //            WinRT.Interop.WindowNative.GetWindowHandle(this)
            //        )
            //    );
            //    appWindow.SetIcon(iconPath);
            //}
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "hugeicons__chat_bot_Bg0_icon.ico");

            using (var icon = new Icon(iconPath))
            {
                SendMessage(
                    new Windows.Win32.Foundation.HWND(hwnd),
                    WM_SETICON,
                    IntPtr.Zero,
                    icon.Handle
                );
            }
        }
        private async void InitializeWebView()
        {
            await webView.EnsureCoreWebView2Async();

            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app.chatbot",
                Path.Combine(AppContext.BaseDirectory, "Assets"),
                CoreWebView2HostResourceAccessKind.Allow);

            // Navigate to your React App inside WebView2
            webView.Source = new Uri("http://localhost:3002/");
        }
        private void ShowChatApp()
        {
            this.Show();
            this.Activate();
        }

        //private void OnOpenClick(object sender, RoutedEventArgs e)
        //{
        //    ShowChatApp();
        //}

        //private void OnExitClick(object sender, RoutedEventArgs e)
        //{
        //    TrayIcon.Dispose();
        //    Application.Current.Exit();
        //}

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true; // Prevent closing, minimize to tray
            this.Hide();
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            TrayIcon.Dispose();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        //private void OnPropertyChanged(string propertyName) =>
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Handle sending messages
        //private void OnSendClick(object sender, RoutedEventArgs e)
        //{
        //    string message = MessageInput.Text.Trim();
        //    if (!string.IsNullOrEmpty(message))
        //    {
        //        TextBlock textBlock = new TextBlock
        //        {
        //            Text = "You: " + message,
        //            FontSize = 16,
        //            Margin = new Thickness(5)
        //        };

        //        ChatMessages.Children.Add(textBlock);
        //        MessageInput.Text = "";
        //    }
        //}
    }

    // RelayCommand Implementation (Allows binding commands in MVVM)
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);
    }
}


