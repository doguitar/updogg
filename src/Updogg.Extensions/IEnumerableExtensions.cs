using System.Collections.Concurrent;

namespace Updogg.Extensions
{
    public static class IEnumerableExtensions
    {
        public static async Task<IEnumerable<T>> WhenAll<T>(this IEnumerable<T> self, Action<T> action)
        {
            IEnumerable<Task>? tasks = self.Select(i => Task.Run(() => action(i)));
            await Task.WhenAll(tasks);
            return self;
        }
        public static async Task<IEnumerable<TResult>> WhenAll<T, TResult>(this IEnumerable<T> self, Func<T, TResult> action)
        {
            ConcurrentBag<TResult> bag = new();
            IEnumerable<Task>? tasks = self.Select(i => Task.Run(() => bag.Add(action(i))));
            await Task.WhenAll(tasks);
            return bag.ToArray();
        }
        public static async Task WhenAll<T>(this IEnumerable<T> self, Func<T, Task> action)
        {
            IEnumerable<Task>? tasks = self.Select(i => action(i));
            await Task.WhenAll(tasks);
        }

        public static void WaitAll<T>(this IEnumerable<T> self, Action<T> action)
        {
            IEnumerable<Task>? tasks = self.Select(i => Task.Run(() => action(i)));
            Task.WaitAll(tasks.ToArray());
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (T? item in self)
            {
                action(item);
            }
            return self;
        }

        public static bool TryFirst<TSource>(this IEnumerable<TSource> source, out TSource? item)
        {
            item = source.FirstOrDefault();
            return item != null;
        }
        public static bool TryFirst<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, out TSource? item)
        {
            item = source.FirstOrDefault(predicate);
            return item != null;
        }
        public static IEnumerable<IEnumerable<TSource>> Split<TSource>(this IEnumerable<TSource> source, int amount)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / amount)
                .Select(x => x.Select(v => v.Value));
        }
        public static Queue<TSource> ToQueue<TSource>(this IEnumerable<TSource> source)
        {
            return new Queue<TSource>(source);
        }
        public static ConcurrentQueue<TSource> ToConcurrentQueue<TSource>(this IEnumerable<TSource> source)
        {
            return new ConcurrentQueue<TSource>(source);
        }

        public static IEnumerable<R> SelectParrallel<T, R>(this IEnumerable<T> source, Func<T, R> action, ushort parallelism = 0)
        {
            return source.SelectParrallelWithContext((t, c) => action(t), () => 0, parallelism);
        }

        public static IEnumerable<R> SelectParrallelWithContext<T, C, R>(this IEnumerable<T> source, Func<T, C, R> action, Func<C> context, ushort parallelism = 0)
        {
            if (parallelism == 0) parallelism = (ushort)Environment.ProcessorCount;
            ConcurrentQueue<T>? q = source.ToConcurrentQueue();
            ConcurrentBag<R>? b = new();

            Task.WaitAll(Enumerable.Range(0, parallelism).Select(
                i =>
                {
                    return Task.Run(() =>
                    {
                        C? c = context();
                        while (q.TryDequeue(out T? t))
                        {
                            b.Add(action(t, c));
                        }
                    });
                }).ToArray());
            return b;
        }

        public static IEnumerable<R> SelectTaskWaitAll<T, R>(this IEnumerable<T> self, Func<T, R> action)
        {
            ConcurrentBag<R>? bag = new();
            self.WaitAll(i => bag.Add(action(i)));
            return bag;
        }
        public static Task<IEnumerable<R>> SelectTaskWhenAll<T, R>(this IEnumerable<T> self, Func<T, Task<R>> action)
        {
            return Task.Run(async () =>
            {
                ConcurrentBag<R>? bag = new();
                await self.WhenAll(async i => bag.Add(await action(i)));
                return (IEnumerable<R>)bag;
            });
        }
    }
}
