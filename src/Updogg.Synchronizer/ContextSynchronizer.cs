using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Updogg.Common;
using Updogg.Extensions;

namespace Updogg.Synchronizer
{

    public class ContextSynchronizer<T> : IContextSynchronizer<T>
    {
        readonly Guid _id;
        readonly ITaskExecutor _executor;

        public void Stop(bool completeQueue = true)
        {
            _executor.Stop(completeQueue);
        }

        readonly ILogger? _logger;
        readonly bool _traceLoggingEnabled;

        bool _disposed;
        bool _disposing;
        Exception? _exception = null;

        protected virtual T? Context { get; }
        public ContextSynchronizer(T? context) : this(context, null)
        {
        }
        public ContextSynchronizer(T? context, ILogger? logger) : this(context, ThreadPriority.Normal, logger)
        {
        }
        public ContextSynchronizer(T? context, ThreadPriority priority, ILogger? logger)
        {
            _logger = logger;
            _executor = new TaskExecutor(1, priority, logger);
            _executor.ExceptionThrown += ExceptionThrown;
            _executor.Start();
            _id = Guid.NewGuid();
            //_logger?.LogTrace($"Synchronizer-{_id}-Constructed");
            _traceLoggingEnabled = (_logger?.IsEnabled(LogLevel.Trace)).GetValueOrDefault();
            Context = context;
        }

        private void ExceptionThrown(object sender, ITaskExecutor.ExceptionThrownEventArgs exception)
        {
            _exception = exception.Exception;
        }

        ~ContextSynchronizer()
        {
            //_logger?.LogTrace($"Synchronizer-{_id}-Destructed");
            Dispose(false);
        }

        public void ChangePriority(ThreadPriority priority)
        {
            _executor.ChangePriority(priority);
        }

        public virtual async Task<R?> SynchronizeAsync<R>(Func<T?, Task<R>> action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            if (typeof(R) == typeof(Task)) throw new ArgumentException($"{nameof(R)} must not be of type Task");

            SemaphoreSlim? semaphore = new(0);
            R? result = default;

            Guid id = Guid.Empty;
            if (_traceLoggingEnabled)
            {
                id = Guid.NewGuid();
                StackFrame? frame = new(4);
                System.Reflection.MethodBase? method = frame.GetMethod();
                Type? type = method?.DeclaringType;
                string? name = method?.Name;
                //_logger?.LogTrace($"Synchronizer-{_id}-{type}-{name}-Queued Asynchonous Action-{id}");
            }
            T? c = Context;
            Exception? exception = null;
            _executor.AddTask(() =>
            {
                //_logger?.LogTrace($"Synchronizer-{_id}-Executing Asynchonous Action-{id}");
                try
                {
                    result = action(c).RunSync();
                    //_logger?.LogTrace($"Synchronizer-{_id}-Executed  Asynchonous Action-{id}");
                    if (_exception != null)
                    {
                        exception = _exception;
                        _exception = null;
                    }
                }
                catch (Exception e)
                {
                    exception = e;
                    _logger?.LogError("Exception execting task. {e}", e);
                }
                finally
                {
                    _ = semaphore.Release();
                }
            });
            await semaphore.WaitAsync();
            return exception != null ? throw exception : result;
        }
        public virtual Task SynchronizeAsync(Func<T?, Task> action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            SemaphoreSlim? semaphore = new(0);

            Guid id = Guid.Empty;
            if (_traceLoggingEnabled)
            {
                id = Guid.NewGuid();
                StackFrame? frame = new(4);
                System.Reflection.MethodBase? method = frame.GetMethod();
                Type? type = method?.DeclaringType;
                string? name = method?.Name;
                // _logger?.LogTrace($"Synchronizer-{_id}-{type}-{name}-Queued Asynchonous Action-{id}");
            }

            _executor.AddTask(() =>
            {
                T? c = Context;
                //_logger?.LogTrace($"Synchronizer-{_id}-Executing Asynchonous Action-{id}");
                try
                {
                    action(c).RunSync();
                    //_logger?.LogTrace($"Synchronizer-{_id}-Executed  Asynchonous Action-{id}");
                }
                catch (Exception e)
                {
                    _logger?.LogError("Exception execting task. {e}", e);
                }
                finally
                {
                    _ = semaphore.Release();
                }
            });
            return semaphore.WaitAsync();
        }

        public virtual void Synchronize(Action<T?> action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            SemaphoreSlim? semaphore = new(0);

            Guid id = Guid.Empty;
            if (_traceLoggingEnabled)
            {
                id = Guid.NewGuid();
                StackFrame? frame = new(1);
                System.Reflection.MethodBase? method = frame.GetMethod();
                Type? type = method?.DeclaringType;
                string? name = method?.Name;
                //_logger?.LogTrace($"Synchronizer-{_id}-{type}-{name}-Queued Synchonous Action-{id}");
            }

            _executor.AddTask(() =>
            {
                T? c = Context;
                //_logger?.LogTrace($"Synchronizer-{_id}-Executing Synchonous Action-{id}");
                try
                {
                    action(c);
                    // _logger?.LogTrace($"Synchronizer-{_id}-Executed  Synchonous Action-{id}");
                }
                catch (Exception e)
                {
                    _logger?.LogError("Exception execting task. {e}", e);
                }
                finally
                {
                    _ = semaphore.Release();
                }
            });
            semaphore.Wait();
        }
        public virtual R? Synchronize<R>(Func<T?, R> action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            if (typeof(R) == typeof(Task)) throw new ArgumentException($"{nameof(R)} must not be of type Task");

            SemaphoreSlim? semaphore = new(0);
            R? result = default;

            Guid id = Guid.Empty;
            if (_traceLoggingEnabled)
            {
                id = Guid.NewGuid();
                StackFrame? frame = new(1);
                System.Reflection.MethodBase? method = frame.GetMethod();
                Type? type = method?.DeclaringType;
                string? name = method?.Name;
                //_logger?.LogTrace($"Synchronizer-{_id}-{type}-{name}-Queued Synchonous Action-{id}");
            }

            _executor.AddTask(() =>
            {
                T? c = Context;
                //_logger?.LogTrace($"Synchronizer-{_id}-Executing Synchonous Action-{id}");
                try
                {
                    result = action(c);
                    //_logger?.LogTrace($"Synchronizer-{_id}-Executed  Synchonous Action-{id}");
                }
                catch (Exception e)
                {
                    _logger?.LogError("Exception execting task. {e}", e);
                }
                finally
                {
                    _ = semaphore.Release();
                }
            });
            semaphore.Wait();
            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            //_logger?.LogTrace($"Synchronizer-{_id}-Dispose Request");
            if (_disposed || _disposing)
            {
                return;
            };
            if (disposing)
            {
                //_logger?.LogTrace($"Synchronizer-{_id}-Disposing");
                _disposing = true;
                _executor.Dispose();
                if (Context is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                //_logger?.LogTrace($"Synchronizer-{_id}-Disposed");
            }

            _disposed = true;
        }

    }
}
