namespace Updogg.Extensions
{
    public static class AsyncHelpers
    {

        /// <summary>
        /// Execute's an async Task<T> method which has a void return value synchronously
        /// </summary>
        /// <param name="task">Task<T> method to execute</param>
        public static void RunSync(this Task task)
        {
            SynchronizationContext? oldContext = SynchronizationContext.Current;
            ExclusiveSynchronizationContext? synch = new();
            SynchronizationContext.SetSynchronizationContext(synch);
            synch.Post(async _ =>
            {
                try
                {
                    if (task.Status == TaskStatus.Created)
                    {
                        task.Start();
                    }
                    await task;
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, new object());
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            if (synch.InnerException != null) { throw synch.InnerException; }
        }

        /// <summary>
        /// Execute's an async Task<T> method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">Task<T> method to execute</param>
        /// <returns></returns>
        public static T? RunSync<T>(this Task<T> task)
        {
            SynchronizationContext? oldContext = SynchronizationContext.Current;
            ExclusiveSynchronizationContext? synch = new();
            SynchronizationContext.SetSynchronizationContext(synch);
            T? ret = default;
            synch.Post(async _ =>
            {
                try
                {
                    if (task.Status == TaskStatus.Created)
                    {
                        task.Start();
                    }
                    ret = await task;
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, new object());
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return synch.InnerException != null ? throw synch.InnerException : ret;
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext
        {
            private bool done;
            public Exception? InnerException { get; set; }
            readonly AutoResetEvent workItemsWaiting = new(false);
            readonly Queue<Tuple<SendOrPostCallback, object>> items = new();

            public override void Send(SendOrPostCallback d, object? state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object? state)
            {
                lock (items)
                {
                    items.Enqueue(Tuple.Create(d, state ?? new()));
                }
                _ = workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => done = true, new object());
            }

            public void BeginMessageLoop()
            {
                while (!done)
                {
                    Tuple<SendOrPostCallback, object>? task = null;
                    lock (items)
                    {
                        if (items.Count > 0)
                        {
                            task = items.Dequeue();
                        }
                    }
                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                        }
                    }
                    else
                    {
                        _ = workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }
}
