namespace Updogg.Extensions
{
    public static class NumericExtensions
    {
        #region int

        public static bool InRange<T>(this T source, T min, T max, bool inclusive = true) where T : IComparable
        {
            int tomin = source.CompareTo(min);
            int tomax = source.CompareTo(max);

            return inclusive && (tomin == 0 || tomax == 0) ? true : tomax < 0 && tomin > 0;
        }

        public static bool Between<T>(this T source, T min, T max) where T : IComparable
        {
            return source.InRange(min, max, false);
        }

        #endregion
    }
}
