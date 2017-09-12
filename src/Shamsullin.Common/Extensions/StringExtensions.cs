using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Shamsullin.Common.Extensions
{
    /// <summary>
    /// Extensions for System.String.
    /// </summary>
    public static class StringExtensions
    {
        private static readonly Regex RegexUnicodeSymbol = new Regex(@"&#x\d{4,};", RegexOptions.Compiled);
        private static readonly Regex RegexBadChars = new Regex(@"&#x.;", RegexOptions.Compiled);

        private static readonly ConcurrentDictionary<string, string> Locks = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Creates an interned string to be used in synchronization context.
        /// </summary>
        public static string SyncRoot(this string str)
        {
            return Locks.GetOrAdd(str, string.Copy);
        }

        public static IEnumerable<XElement> DeserializeXml(this string str)
        {
            if (string.IsNullOrEmpty(str)) return new XElement[0];
            var result = XDocument.Parse(str).Elements().First().Nodes().Cast<XElement>();
            return result;
        }

        public static List<int> AllIndexesOf(this string str, string value)
        {
            var indexes = new List<int>();
            for (var i = 0;; i += value.Length)
            {
                i = str.IndexOf(value, i);
                if (i == -1)
                    return indexes;
                indexes.Add(i);
            }
        }

        public static string ReplaceEx(this string source, string oldString, string newString)
        {
            if (string.IsNullOrEmpty(source)) return source;
            var result = source.Replace(oldString, newString);
            return result;
        }

        public static string ReplaceEx(this string source, string oldString, string newString, StringComparison comp)
        {
            if (string.IsNullOrEmpty(source)) return source;
            var index = source.IndexOf(oldString, comp);
            var matchFound = index >= 0;
            if (matchFound)
            {
                source = source.Remove(index, oldString.Length);
                source = source.Insert(index, newString);
            }

            return source;
        }

        public static string RemoveDoubleSpaces(this string text)
        {
            while (text.Replace("  ", " ") != text) text = text.Replace("  ", " ");
            return text;
        }

        public static string RemoveUnicodeSymbols(this string text)
        {
            var s = RegexBadChars.Replace(text, string.Empty);
            return RegexUnicodeSymbol.Replace(s, string.Empty);
        }

        public static bool EqualsNoCase(this string thisArg, string otherString)
        {
            return String.Compare(thisArg, otherString, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static string JoinBySeparator(this IEnumerable<string> enumerable, string separator)
        {
            return string.Join(separator, enumerable.ToArray());
        }

        public static string JoinBySeparator(this IEnumerable<int> enumerable, char separator, string fallbackValue)
        {
            if (!enumerable.Any()) return fallbackValue;

            var str = new StringBuilder();
            foreach (var value in enumerable)
            {
                str.Append(value);
                str.Append(separator);
            }
            str.Remove(str.Length - 1, 1);
            return str.ToString();
        }

        public static string JoinBySeparator(this IEnumerable<int> enumerable, string separator, string fallbackValue)
        {
            if (!enumerable.Any()) return fallbackValue;

            var str = new StringBuilder();
            foreach (var value in enumerable)
            {
                str.Append(value);
                str.Append(separator);
            }
            str.Remove(str.Length - 1, separator.Length);
            return str.ToString();
        }

        public static string WrapTag(this string thisArg, string tagName)
        {
            return string.Format("<{0}>{1}</{0}>", tagName, thisArg);
        }

        public static string StripTags(this string source)
        {
            return Regex.Replace(source, "<.*?>", string.Empty);
        }

        public static string Limit(this string thisArg, int length)
        {
            if (string.IsNullOrEmpty(thisArg)) return thisArg;
            if (thisArg.Length <= length) return thisArg;
            return thisArg.Substring(0, length);
        }

        public static string LimitRight(this string thisArg, int length)
        {
            if (string.IsNullOrEmpty(thisArg)) return thisArg;
            if (thisArg.Length <= length) return thisArg;
            return thisArg.Substring(thisArg.Length - length, length);
        }

        public static bool HasValue(this string thisArg)
        {
            return string.IsNullOrEmpty(thisArg) == false;
        }

        public static TEnum ToEnum<TEnum>(this string strEnumValue, TEnum defaultValue)
        {
            if (Enum.IsDefined(typeof (TEnum), strEnumValue) == false)
                return defaultValue;

            return (TEnum) Enum.Parse(typeof (TEnum), strEnumValue);
        }

        public static bool IsNullOrEmpty(this string @string)
        {
            return string.IsNullOrEmpty(@string);
        }

        public static string UnescapeUri(this string uri)
        {
            var result = Uri.UnescapeDataString(uri);
            if (result != uri) result = UnescapeUri(result);
            return result;
        }

        public static XmlDocument ToXmlDocument(this string text)
        {
            var result = new XmlDocument();
            result.LoadXml(text);
            return result;
        }

        public static string Md5(this string @this)
        {
            var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(@this));
            var result = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            return result;
        }

        /// <summary>
        /// Calculates the number of occurrences in the specified string.
        /// </summary>
        /// <param name="this">The source string.</param>
        /// <param name="value">The string to seek.</param>
        /// <returns></returns>
        public static int OccurrencesCount(this string @this, string value)
        {
            var result = Regex.Matches(@this, Regex.Escape(value)).Count;
            return result;
        }
    }
}