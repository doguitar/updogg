using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Updogg.Extensions;

namespace Updogg.Synchronizer.EntityFramework
{
    public class DbContextSynchronizer<T> : ContextSynchronizer<T>, IDbContextSynchronizer<T> where T : DbContext
    {
        readonly IDbContextFactory<T> _factory;
        readonly bool _persist;

        protected override T? Context
        {
            get
            {
                if (!_persist) return _factory.CreateDbContext();
                return base.Context;
            }
        }

        bool _disposed;
        bool _disposing;
        public DbContextSynchronizer(IDbContextFactory<T> factory, bool persist = false) : this(factory, null, persist)
        {
        }
        public DbContextSynchronizer(IDbContextFactory<T> factory, ILogger? logger, bool persist = false) : this(factory, ThreadPriority.Normal, logger, persist)
        {
        }
        public DbContextSynchronizer(IDbContextFactory<T> factory, ThreadPriority priority, ILogger? logger, bool persist = false) : base(persist ? factory.CreateDbContext() : null, priority, logger)
        {
            _factory = factory;
            _persist = persist;
        }
        ~DbContextSynchronizer()
        {
            //_logger?.LogTrace($"Synchronizer-{_id}-Destructed");
            Dispose(false);
        }

        public override void Synchronize(Action<T?> action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            base.Synchronize((basec) =>
            {
                var c = basec;

                if (!_persist)
                {
                    c = _factory.CreateDbContext();
                }

                try
                {
                    action(c);
                }
                finally
                {
                    if (!_persist)
                        c?.Dispose();
                }
            });
        }
        public override R? Synchronize<R>(Func<T?, R> action) where R : default
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            return base.Synchronize((basec) =>
            {
                var c = basec;

                if (!_persist)
                {
                    c = _factory.CreateDbContext();
                }
                R? ret = default;
                try
                {
                    ret = action(c);
                }
                finally
                {
                    if (!_persist)
                        c?.Dispose();
                }
                return ret;
            });
        }
        public override async Task SynchronizeAsync(Func<T?, Task> action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            await base.SynchronizeAsync(async (basec) =>
            {
                var c = basec;

                if (!_persist)
                {
                    c = _factory.CreateDbContext();
                }
                    
                try
                {
                    await action(c);
                }
                finally
                {
                    if (!_persist)
                        c?.Dispose();
                }
            });
        }
        public override async Task<R?> SynchronizeAsync<R>(Func<T?, Task<R>> action) where R : default
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            return await base.SynchronizeAsync(async (basec) =>
            {
                var c = basec;

                if (!_persist)
                {
                    c = _factory.CreateDbContext();
                }
                R? ret = default;
                try
                {
                    ret = await action(c);
                }
                finally
                {
                    if (!_persist)
                        c?.Dispose();
                }
                return ret;
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed || _disposing)
            {
                return;
            };
            if (disposing)
            {
                _disposing = true;
                if (_persist)
                    Context?.Dispose();
            }

            _disposed = true;
        }

    }
}
