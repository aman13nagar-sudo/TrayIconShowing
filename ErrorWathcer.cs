using System;
using System.Diagnostics.Eventing.Reader;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;

namespace TrayIconShowing
{
    public class ErrorWatcher
    {
        private EventLogWatcher _watcher;

        public event Action? ErrorDetected;

        public ErrorWatcher()
        {
            // Define query to listen for application errors
            string query = @"
                <QueryList>
                    <Query Id='0' Path='Application'>
                        <Select Path='Application'>*[System[(Level=2)]]</Select>
                    </Query>
                </QueryList>";

            EventLogQuery eventQuery = new EventLogQuery("Application", PathType.LogName, query);
            _watcher = new EventLogWatcher(eventQuery);
            _watcher.EventRecordWritten += OnEventRecordWritten;
            _watcher.Enabled = true;
        }

        private void OnEventRecordWritten(object sender, EventRecordWrittenEventArgs e)
        {
            if (e.EventRecord != null)
            {
                Console.WriteLine($"Error detected: {e.EventRecord.FormatDescription()}");
                ErrorDetected?.Invoke(); // Trigger the event
            }
        }
    }
}
