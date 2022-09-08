namespace Updogg.Extensions
{
    public static class StreamReaderExtensions
    {
        public static bool TryReadLine(this StreamReader reader, out string? line)
        {
            line = reader?.ReadLine();
            return line != null;
        }
    }
}
