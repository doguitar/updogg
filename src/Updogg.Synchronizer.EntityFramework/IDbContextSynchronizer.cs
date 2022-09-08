using Microsoft.EntityFrameworkCore;

namespace Updogg.Synchronizer.EntityFramework
{
    public interface IDbContextSynchronizer<T> : IContextSynchronizer<T> where T : DbContext
    {
    }
}
