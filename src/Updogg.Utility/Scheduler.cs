using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updogg.Scheduler
{
    public class Scheduler : IDisposable
    {
        bool _disposed;
        bool _disposing;
        readonly Thread _thread;
        readonly CancellationTokenSource _cancellation;
        private readonly TimeSpan _interval;
        private readonly bool _immediate;
        private readonly Action _action;

        public Scheduler(Action action, TimeSpan interval, bool immediate = true, bool background = false, ThreadPriority priority = ThreadPriority.Normal)
        {
            _cancellation = new CancellationTokenSource();
            _thread = new Thread(new ParameterizedThreadStart(ScheduledTask)) { IsBackground = background, Priority = priority };
            _interval = interval;
            _immediate = immediate;
            _action = action;
        }

        public Scheduler Start()
        {
            _thread.Start(_cancellation.Token);
            return this;
        }

        public void Stop()
        {
            _cancellation.Cancel();
            _thread.Join();
        }

        private void ScheduledTask(object? state)
        {
            Task.Run(async () =>
            {
                var token = state as CancellationToken?;
                if (token != null)
                {
                    token.Value.ThrowIfCancellationRequested();
                    try
                    {
                        if (!_immediate)
                            await Task.Delay(_interval);
                        while (!_disposing)
                        {
                            _action?.Invoke();
                            await Task.Delay(_interval);
                        }
                    }
                    catch (OperationCanceledException) { }
                }
            }).Wait();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed || _disposing)
            {
                return;
            };
            if (disposing)
            {
                _disposing = true;
                Stop();
            }

            _disposed = true;
        }
    }
}
