using Microsoft.EntityFrameworkCore;

namespace Updogg.Extensions.EntityFramework
{
    public static class EntityFrameworkExtension
    {
        public static async Task<(bool Success, TSource? Item)> TryFirstAsync<TSource>(this IQueryable<TSource> source)
        {
            IQueryable<TSource>? items = source.Take(1);
            bool success = items.Any();
            TSource? item = await items.FirstOrDefaultAsync();
            return (success, item);
        }
        public static async Task<(bool Success, TSource? Item)> TryFirstAsync<TSource>(this IQueryable<TSource> source, System.Linq.Expressions.Expression<Func<TSource, bool>> predicate)
        {
            IQueryable<TSource>? items = source.Where(predicate).Take(1);
            bool success = items.Any();
            TSource? item = await items.FirstOrDefaultAsync();
            return (success, item);
        }
    }
}
