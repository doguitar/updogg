using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Updogg.Extensions;

namespace Updogg.Common
{
    public class TaskExecutor : ITaskExecutor
    {
        private readonly ConcurrentQueue<Action> _taskQueue;
        private readonly BlockingCollection<Action> _tasks;
        private readonly SemaphoreSlim _semaphore;
        private readonly uint _threadCount;
        private readonly ILogger? _logger;
        private readonly ThreadPriority _priority;

        Thread[]? _threads = null;

        private CancellationTokenSource _cancel = new();
        bool _disposed;

        public event ITaskExecutor.OnExceptionThrown? ExceptionThrown;

        public TaskExecutor(uint executorCount, ThreadPriority priority, ILogger? logger)
        {
            _taskQueue = new ConcurrentQueue<Action>();
            _tasks = new BlockingCollection<Action>(_taskQueue);
            _threadCount = executorCount;
            _logger = logger;
            _priority = priority;
            _semaphore = new SemaphoreSlim(0);
        }
        public TaskExecutor(uint executorCount, ThreadPriority priority) : this(executorCount, priority, null)
        {

        }
        public TaskExecutor(uint executorCount) : this(executorCount, ThreadPriority.Normal, null)
        {
        }
        public TaskExecutor(ThreadPriority priority, ILogger logger) : this(1, priority, logger)
        {
        }
        public TaskExecutor(ILogger logger) : this(1, ThreadPriority.Normal, logger)
        {
        }
        public TaskExecutor() : this(1, ThreadPriority.Normal, null)
        {
        }

        private void Loop()
        {
            try
            {
                foreach (Action? action in _tasks.GetConsumingEnumerable(_cancel.Token))
                {
                    try
                    {
                        action();
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception e)
                    {
                        ExceptionThrown?.Invoke(this, new ITaskExecutor.ExceptionThrownEventArgs { Exception = e });
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { _logger?.LogError(e, $"Exception in ${GetType().Name}"); }
            _ = _semaphore.Release();
        }

        #region ITaskExecutor
        public bool Started { get; protected set; }
        public bool Starting { get; protected set; }
        public bool Stopped { get; protected set; }
        public bool Stopping { get; protected set; }

        public void AddTask(Action action)
        {
            if (!(Stopped || Stopping))
            {
                _ = _tasks.TryAdd(action);
            }
        }

        public void AddTaskAndWait(Action action)
        {
            SemaphoreSlim? reset = new(0);
            AddTask(() =>
            {
                action();
                _ = reset.Release();
            });
            reset.Wait();
        }


        public void Start()
        {
            if (!(Stopped || Stopping))
            {
                Stopping = false;
                Starting = true;

                if (_cancel.IsCancellationRequested)
                {
                    _cancel = new CancellationTokenSource();
                }
                _threads = new Thread[_threadCount];

                for (int i = 0; i < _threadCount; i++)
                {
                    _threads[i] = new Thread(Loop);
                    _threads[i].Start();
                    _threads[i].Priority = _priority;
                }

                Stopped = false;
                Started = true;
            }
        }

        public void Stop(bool completeQueue = false)
        {
            if (!(Stopping || Stopped || _disposed))
            {
                Stopping = true;
                Starting = false;
                if (completeQueue)
                {
                    SemaphoreSlim? end = new(0);
                    _ = _tasks.TryAdd(() => end.Release());
                    end.Wait();
                }
                _tasks.CompleteAdding();

                _semaphore.Wait();

                Stopped = true;
                Started = false;
            }
        }

        ~TaskExecutor()
        {
            Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            };

            if (disposing)
            {
                Stop();
            }

            _disposed = true;
        }

        public void ChangePriority(ThreadPriority priority)
        {
            _ = (_threads?
                .Where(t => t != null && t.ThreadState != ThreadState.Running)
                .ForEach(t => t.Priority = priority));
        }
        #endregion
    }
}
