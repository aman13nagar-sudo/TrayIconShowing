using System;
using System.Net.NetworkInformation;
using Microsoft.UI.Dispatching;

namespace TrayIconApp
{
    public class NetworkWatcher
    {
        public event Action? WiFiDisconnected;
        private readonly DispatcherQueue _dispatcherQueue;

        public NetworkWatcher(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue;
            NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
        }

        private void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (!e.IsAvailable) // If network is unavailable (WiFi turned off)
            {
                Console.WriteLine("WiFi Disconnected!");

                // Invoke the event on the UI thread
                _dispatcherQueue.TryEnqueue(() =>
                {
                    WiFiDisconnected?.Invoke();
                });
            }
        }
    }
}
