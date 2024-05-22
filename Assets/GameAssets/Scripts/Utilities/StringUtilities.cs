using System;
using System.Text;

namespace com.brg.Utilities
{
    public static class StringUtilities
    {
        public static string Repeat(this string input, int count)
        {
            if (string.IsNullOrEmpty(input) || count <= 1)
                return input;

            var builder = new StringBuilder(input.Length * count);

            for (var i = 0; i < count; i++) builder.Append(input);

            return builder.ToString();
        }

        public static string FormatHourTime(this int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);

            //here backslash is must to tell that colon is
            //not the part of format, it just a character that we want in output
            string str = time.ToString(@"hh\:mm\:ss");

            return str;
        }
    }
}
