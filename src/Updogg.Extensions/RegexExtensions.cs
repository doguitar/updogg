using System.Text.RegularExpressions;

namespace Updogg.Extensions
{
    public static class RegexExtensions
    {
        public static bool TryMatch(this Regex re, string input, out Match match)
        {
            match = re.Match(input);
            return match.Success;
        }
    }
}
