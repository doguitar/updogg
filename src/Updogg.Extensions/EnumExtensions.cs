namespace Updogg.Extensions
{
    public static class EnumExtensions
    {
        public static bool Has<T>(this System.Enum type, T value)
        {
            try
            {
                if (value == null) return false;
                int all = (int)(object)type;
                int one = (int)(object)value;
                return (all & one) == one;
            }
            catch
            {
                return false;
            }
        }

        public static bool Is<T>(this System.Enum type, T value)
        {
            try
            {
                if (value == null) return false;
                int all = (int)(object)type;
                int one = (int)(object)value;
                return all == one;
            }
            catch
            {
                return false;
            }
        }


        public static T Add<T>(this Enum type, T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            try
            {
                int all = (int)(object)type;
                int one = (int)(object)value;
                return (T)(object)(all | one);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format(
                        "Could not append value from enumerated type '{0}'.",
                        typeof(T).Name
                        ), ex);
            }
        }


        public static T Remove<T>(this System.Enum type, T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            try
            {
                int all = (int)(object)type;
                int one = (int)(object)value;
                return (T)(object)(all & ~one);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format(
                        "Could not remove value from enumerated type '{0}'.",
                        typeof(T).Name
                        ), ex);
            }
        }
    }
}
