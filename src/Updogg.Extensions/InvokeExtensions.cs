namespace Updogg.Extensions
{
    public static class InvokeExtensions
    {
        public static async Task InvokeAsync(this Action self)
        {
            SemaphoreSlim? s = new(0);
            await Task.Run(() => { self(); _ = s.Release(); });
            await s.WaitAsync();
        }

        public static async Task InvokeAsync<T>(this EventHandler<T> self, object sender, T args)
        {
            if (self == null) return;
            await InvokeAsync(() => self(sender, args));
        }
        public static async Task InvokeAsync<T>(this Action<T> self, T obj)
        {
            if (self == null) return;
            await InvokeAsync(() => self(obj));
        }
        public static async Task InvokeAsync<T1, T2>(this Action<T1, T2> self, T1 obj1, T2 obj2)
        {
            if (self == null) return;
            await InvokeAsync(() => self(obj1, obj2));
        }
        public static async Task<R?> InvokeAsync<T, R>(this Func<T, R> self, T obj)
        {
            R? result = default;
            if (self == null) return result;
            await InvokeAsync(() => result = self(obj));
            return result;
        }

    }
}
