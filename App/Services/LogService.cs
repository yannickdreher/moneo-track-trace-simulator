using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;

namespace App.Services
{
    public partial class LogService(int maxLogEntries = 50) : IDisposable
    {
        private readonly object _lock = new();
        private readonly int _maxLogEntries = maxLogEntries;
        private DispatcherQueue? _dispatcherQueue;
        
        public ObservableCollection<string> LogEntries { get; } = [];

        public void SetDispatcherQueue(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue;
        }

        private void Add(string message)
        {
            var formattedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";

            if (_dispatcherQueue == null)
            {
                // Thread-safe fallback
                lock (_lock)
                {
                    LogEntries.Insert(0, formattedMessage);
                    TrimLogEntries();
                }
                return;
            }

            var enqueued = _dispatcherQueue.TryEnqueue(() =>
            {
                LogEntries.Insert(0, formattedMessage);
                TrimLogEntries();
            });

            if (!enqueued)
            {
                // Fallback wenn Dispatcher Queue voll ist
                lock (_lock)
                {
                    LogEntries.Insert(0, formattedMessage);
                    TrimLogEntries();
                }
            }
        }

        public void LogInfo(string message) => Add($"[INFO] {message}");
        public void LogWarn(string message) => Add($"[WARN] {message}");
        public void LogError(string message) => Add($"[ERROR] {message}");

        public void Clear()
        {
            if (_dispatcherQueue == null)
            {
                lock (_lock)
                {
                    LogEntries.Clear();
                }
                return;
            }

            _dispatcherQueue.TryEnqueue(() => LogEntries.Clear());
        }

        private void TrimLogEntries()
        {
            // Limit log entries to prevent memory issues
            while (LogEntries.Count > _maxLogEntries)
            {
                LogEntries.RemoveAt(LogEntries.Count - 1);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                LogEntries.Clear();
            }
            GC.SuppressFinalize(this);
        }
    }
}