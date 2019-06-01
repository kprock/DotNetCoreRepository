using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNetCoreRepository.Extensions
{
    public static class StringExtensions
    {
        private static readonly CultureInfo _cultureInfo = CultureInfo.InvariantCulture;

        public static class EnumUtil
        {
            public static IEnumerable<T> GetValues<T>()
            {
                return Enum.GetValues(typeof(T)).Cast<T>();
            }
        }

        public static bool Contains(this String str, String substring,
                               StringComparison comp)
        {
            if (substring == null)
                throw new ArgumentNullException("substring",
                                                "substring cannot be null.");
            else if (!Enum.IsDefined(typeof(StringComparison), comp))
                throw new ArgumentException("comp is not a member of StringComparison",
                                            "comp");

            return str.IndexOf(substring, comp) >= 0;
        }

        public static string Truncate(this string value, int maxChars)
        {
            if (value != null)
            {
                return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
            }
            else
            {
                return string.Empty;
            }
        }

        public static string FriendlyAddressName(this string value)
        {
            // remove number from AddressName
            if (value != null)
            {
                int index = value.IndexOf("[");
                return (index > 0 ? value.Substring(0, index - 1) : value);
            }
            return value;
        }

        public static string SplitToLines(string text, char[] splitOnCharacters, int maxStringLength)
        {
            var sb = new StringBuilder();
            var index = 0;

            while (text.Length > index)
            {
                // start a new line, unless we've just started
                if (index != 0)
                    sb.AppendLine();

                // get the next substring, else the rest of the string if remainder is shorter than `maxStringLength`
                var splitAt = index + maxStringLength <= text.Length
                    ? text.Substring(index, maxStringLength).LastIndexOfAny(splitOnCharacters)
                    : text.Length - index;

                // if can't find split location, take `maxStringLength` characters
                splitAt = (splitAt == -1) ? maxStringLength : splitAt;

                // add result to collection & increment index
                sb.Append(text.Substring(index, splitAt).Trim());
                index += splitAt;
            }

            return sb.ToString();
        }

        public static string BreakLine(string text, int maxCharsInLine)
        {
            int charsInLine = 0;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsWhiteSpace(c) || charsInLine >= maxCharsInLine)
                {
                    builder.AppendLine();
                    charsInLine = 0;
                }
                else
                {
                    builder.Append(c);
                    charsInLine++;
                }
            }
            return builder.ToString();
        }

        public static string Titleize(this string text)
        {
            //return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text).ToSentenceCase();
            return text.ToTitleCase().ToSentenceCase();
        }

        public static string ToSentenceCase(this string str)
        {
            return Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1]));
        }

        public static string ToTitleCase(this string str)
        {
            var tokens = Regex.Split(_cultureInfo.TextInfo.ToLower(str), "([ -])");

            for (var i = 0; i < tokens.Length; i++)
            {
                if (!Regex.IsMatch(tokens[i], "^[ -]$"))
                {
                    tokens[i] = $"{_cultureInfo.TextInfo.ToUpper(tokens[i].Substring(0, 1))}{tokens[i].Substring(1)}";
                }
            }

            return string.Join("", tokens);
        }

        public static string ReplaceLineBreaks(this string str)
        {
            return str.Replace("\r\r", "<br />").Replace("\r\n", "<br />").Replace("\r", "<br />").Replace("\n", "<br />");
        }

        public static string[] SplitCartItemDescription(this string str)
        {
            string[] s = str.Split(new char[] { ' ' }, 2);
            return s;
        }

        public static string OrdinalSuffixOf(this string str)
        {
            int j = Convert.ToInt32(str) % 10,
                k = Convert.ToInt32(str) % 100;
            if (j == 1 && k != 11)
            {
                return str + "st";
            }
            if (j == 2 && k != 12)
            {
                return str + "nd";
            }
            if (j == 3 && k != 13)
            {
                return str + "rd";
            }
            return str + "th";
        }

        public static string Right(this string param, int length)
        {
            if (param.Length >= length)
            {
                string result = param.Substring(param.Length - length, length);
                return result;
            }
            else
                return param;
        }

        public static string Left(this string param, int length)
        {
            if (param == null) { return null; }
            if (param.Length >= length)
            {
                string result = param.Substring(0, length);
                return result;
            }
            else
                return param;
        }

        public static string Mid(this string param, int startIndex)
        {
            if (param.Length >= startIndex)
            {
                string result = param.Substring(startIndex);
                return result;
            }
            else
                return param;
        }

        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNullOrWhitespace(this string s)
        {
            return String.IsNullOrWhiteSpace(s);
        }
    }
}
