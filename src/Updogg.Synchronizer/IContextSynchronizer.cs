namespace Updogg.Synchronizer
{
    public interface IContextSynchronizer<T> : IDisposable
    {
        Task<R?> SynchronizeAsync<R>(Func<T?, Task<R>> action);
        Task SynchronizeAsync(Func<T?, Task> action);
        void Synchronize(Action<T?> action);
        R? Synchronize<R>(Func<T?, R> action);
        void ChangePriority(ThreadPriority priority);
        void Stop(bool completeQueue = true);
    }
}
