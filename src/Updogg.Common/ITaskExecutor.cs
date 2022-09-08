namespace Updogg.Common
{
    public interface ITaskExecutor : IDisposable
    {
        delegate void OnExceptionThrown(object sender, ExceptionThrownEventArgs exception);
        event OnExceptionThrown ExceptionThrown;
        bool Started { get; }
        bool Starting { get; }
        bool Stopped { get; }
        bool Stopping { get; }

        void AddTask(Action action);
        void AddTaskAndWait(Action action);
        void Start();
        void Stop(bool completeQueue = false);
        void ChangePriority(ThreadPriority priority);

        public class ExceptionThrownEventArgs : EventArgs { public Exception? Exception; }
    }
}