using System.Globalization;

namespace Updogg.Extensions
{
    public static class StringExtensions
    {
        public static int ToCupSize(this string self)
        {
            int cupSize = 0;
            if (self.ToUpperInvariant().TryFirst(out char cup))
            {
                int cupInt = (int)cup;
                if (cupInt is >= 65 and <= 90)
                {
                    cupSize += cupInt * 100;

                    cupSize += self.Length - 1;
                }
            }
            return cupSize;
        }
        public static string FromCupSize(this int self)
        {
            return new string((char)(self / 100), (self % 100) + 1);
        }
        public static string ToTitleCase(this string s)
        {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLower());
        }

        private static IEnumerable<char> GetCharsInRange(this string s, int min, int max)
        {
            return s.ToCharArray().Where(e => e >= min && e <= max);
        }

        public static bool ContainsJapanese(this string s)
        {
            return GetCharsInRange(s, 0x0020, 0x007E).Any() ||
                GetCharsInRange(s, 0x3040, 0x309F).Any() ||
                GetCharsInRange(s, 0x30A0, 0x30FF).Any() ||
                GetCharsInRange(s, 0x4E00, 0x9FBF).Any();
        }


    }
}
