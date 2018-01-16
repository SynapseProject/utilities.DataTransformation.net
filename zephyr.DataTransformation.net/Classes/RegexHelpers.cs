using System;
using System.Text.RegularExpressions;

namespace Zephyr.DataTransformation
{
    public class RegexHelpers
    {
        public static Match Match(string input, string pattern, RegexOptions options, TimeSpan matchTimeout)
        {
            return Regex.Match( input, pattern, options, matchTimeout );
        }

        public static MatchCollection Matches(string input, string pattern, RegexOptions options, TimeSpan matchTimeout)
        {
            return Regex.Matches( input, pattern, options, matchTimeout );
        }
    }
}