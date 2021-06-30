using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Datacom.IRIS.Common.Utils
{
    public static class Strings
    {
        public static string JoinNonEmptyStrings(Delimitter delimitter, params string[] words)
        {
            string returnString;
            if (delimitter == Delimitter.NewLine)
                return words.Where(w => !string.IsNullOrEmpty(w)).Aggregate("", (current, word) => current + (word + Environment.NewLine));

            string delimitterValue = " ";
            if (delimitter == Delimitter.Comma)
                delimitterValue = ", ";
            else if (delimitter == Delimitter.Pipe)
                delimitterValue = " | ";

            returnString = words.Where(w => !string.IsNullOrEmpty(w)).Aggregate("", (current, word) => current + (word + delimitterValue));
            return (returnString.EndsWith(delimitterValue)) ? 
                returnString.Substring(0, returnString.Length - delimitterValue.Length) : 
                returnString;
        }

        public static void FormatNonEmptyString(string format, string word, ref string output)
        {
            if (!string.IsNullOrEmpty(word))
            {
                output += string.Format(format, word);
            }
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static List<string> SplitAndTrim(this string source, params char[] separator)
        {
            if (source == null)
            {
                return new List<string>();  // Initial string is null, return empty list (no values split)
            }

            IEnumerable<string> enumerable = from s in source.Split(separator)
                                             where !String.IsNullOrEmpty(s.Trim())
                                             select s.Trim();

            return enumerable.ToList();
        }
        
        public static string Truncate(this string s, int maxLength)
        {
            return s != null && s.Length > maxLength ? s.Substring(0, maxLength) : s;
        }

        public static int NthIndexOf(this string target, string value, int n)
        {
            Match m = Regex.Match(target, "((" + value + ").*?){" + n + "}");

            if (m.Success)
                return m.Groups[2].Captures[n - 1].Index;
            else
                return -1;
        }

        public static string ToExternalURL(this string link)
        {
            // http:// required or the current domain e.g. http://localhost:82/ is prefixed when the link is rendered.
            return string.IsNullOrEmpty(link)
                ? string.Empty
                : link.ToLower().Contains("://") ? link : "http://" + link;
        }
    }

    public enum Delimitter
    {
        Comma,
        NewLine,
        WhiteSpace,
        Pipe
    }
}
