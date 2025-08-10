using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace BruSoftware.SharedServices.ExtensionMethods;

public static class StringExtensions
{
    public static int ToInt32Leading(this string str)
    {
        var sb = new StringBuilder();
        {
            foreach (var ch in str)
            {
                if (!char.IsDigit(ch))
                {
                    break;
                }
                sb.Append(ch);
            }
        }
        if (int.TryParse(sb.ToString(), out var result))
        {
            return result;
        }
        return 0;
    }

    public static int ToInt32(this string str)
    {
        if (int.TryParse(str, out var result))
        {
            return result;
        }
        return 0;
    }

    public static int ToInt32StripCommas(this string str)
    {
        var strNoCommas = str.Replace(",", "");
        if (int.TryParse(strNoCommas, out var result))
        {
            return result;
        }
        return 0;
    }

    public static long ToInt64(this string str)
    {
        if (long.TryParse(str, out var result))
        {
            return result;
        }
        return 0;
    }

    public static bool ToBoolean(this string str)
    {
        if (bool.TryParse(str, out var result))
        {
            return result;
        }
        return false;
    }

    public static double ToDouble(this string str)
    {
        if (double.TryParse(str, out var result))
        {
            return result;
        }
        return 0;
    }

    /// <summary>
    /// Get a type from myString using a TypeConverter
    /// Thanks to http://www.hanselman.com/blog/TypeConvertersTheresNotEnoughTypeDescripterGetConverterInTheWorld.aspx
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="myString"></param>
    /// <returns></returns>
    public static T GetTfromString<T>(this string myString)
    {
        var foo = TypeDescriptor.GetConverter(typeof(T));
        return (T)foo.ConvertFromInvariantString(myString);
    }

    public static string RemoveSurroundingParens(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return "";
        }
        if (str[0] == '(' && str[^1] == ')')
        {
            return str.Substring(1, str.Length - 2);
        }
        return str;
    }

    public static string RemoveCharacters(this string str, char input)
    {
        var sb = new StringBuilder();
        foreach (var ch in str)
        {
            if (ch != input)
            {
                sb.Append(ch);
            }
        }
        return sb.ToString();
    }

    public static string RemoveSpaces(this string str)
    {
        return str.RemoveCharacters(' ');
    }

    /// <summary>
    /// "IBM" goes to "I B M" for speaking
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToSeparateCharacters(this string str)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < str.Length; i++)
        {
            sb.Append(str[i]);
            sb.Append(" ");
        }
        return sb.ToString();
    }

    /// <summary>
    /// "10Sv" goes to "10 Sd" for speaking
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToSpaceAfterDigits(this string str)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < str.Length; i++)
        {
            var ch = str[i];
            sb.Append(ch);
            if (i < str.Length - 1)
            {
                var nextCh = str[i + 1];
                if (char.IsDigit(ch) && !char.IsDigit(nextCh))
                {
                    sb.Append(" ");
                }
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Returns the first double-quoted section, or null if there is none. Double-quoted string returning string.Empty is not the same as null.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string GetFirstDoubleQuoted(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return null;
        }
        var index1 = str.IndexOf('\"');
        if (index1 < 0)
        {
            return null;
        }
        var index2 = str.IndexOf('\"', index1 + 1);
        if (index2 < 0)
        {
            return null;
        }
        var result = str.Substring(index1 + 1, index2 - index1 - 1);
        return result;
    }

    public static List<string> GetWordsSortedByDescendingLength(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return new List<string> { str };
        }
        var result = str.Split(' ').OrderByDescending(x => x.Length).ToList();
        return result;
    }

    /// <summary>
    /// Abbreviate
    /// </summary>
    /// <param name="str">the input string</param>
    /// <param name="exemptWordsLowerCase">must be lower-case</param>
    /// <param name="replaceTuples"></param>
    /// <returns></returns>
    public static string Abbreviate(this string str, List<string> exemptWordsLowerCase, List<(string, string)> replaceTuples)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }
        var sb = new StringBuilder();

        // Handle special cases
        var words = str.Split(' ');
        foreach (var word in words)
        {
            if (exemptWordsLowerCase.Contains(word.ToLower()))
            {
                sb.Append(word);
                sb.Append(' ');
                continue;
            }
            var isReplaced = false;
            foreach (var tuple in replaceTuples)
            {
                if (word == tuple.Item1)
                {
                    sb.Append(tuple.Item2);
                    sb.Append(' ');
                    isReplaced = true;
                    break;
                }
            }
            if (!isReplaced)
            {
                sb.Append(word.Abbreviate());
                sb.Append(' ');
            }
        }
        sb.Length--;
        return sb.ToString();
    }

    public static string Abbreviate(this string str)
    {
        var sb = new StringBuilder();

        // Remove repeated consonants
        // Remove vowels that don't begin a word and are not uppercase.
        var isStartOfWord = true;
        for (var i = 0; i < str.Length; i++)
        {
            var ch = str[i];
            if (ch == ' ')
            {
                sb.Append(ch);
                isStartOfWord = true;
                continue;
            }
            if (isStartOfWord)
            {
                // Never remove the first ch of a word
                sb.Append(ch);
                isStartOfWord = false;
                continue;
            }
            if (ch.IsVowel() && !char.IsUpper(ch))
            {
                // leave out lower-case vowels except when they are the first ch in a word
                continue;
            }
            var prevCh = str[i - 1];
            if (!char.IsDigit(ch) && ch == prevCh)
            {
                // leave out repeated consonants
                continue;
            }
            sb.Append(ch);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Return <c>true</c> if every character is a digit
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNumericInteger(this string str)
    {
        foreach (var ch in str)
        {
            if (!char.IsDigit(ch))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Thanks to https://stackoverflow.com/questions/6373315/how-to-replace-a-char-in-string-with-an-empty-character-in-c-net
    /// </summary>
    /// <param name="input"></param>
    /// <param name="charItem"></param>
    /// <returns></returns>
    public static string RemoveCharFromString(this string input, char charItem)
    {
        while (true)
        {
            var indexOfChar = input.IndexOf(charItem);
            if (indexOfChar < 0)
            {
                return input;
            }
            input = input.Remove(indexOfChar, 1);
        }
    }

    public static string PullEndingLetters(this string input)
    {
        for (var i = input.Length - 1; i >= 0; i--)
        {
            var ch = input[i];
            if (!char.IsLetter(ch))
            {
                var result = input.Substring(i + 1, input.Length - i - 1);
                return result;
            }
        }
        return null;
    }

    public static string PullBeginningLetters(this string input)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < input.Length; i++)
        {
            var ch = input[i];
            if (char.IsLetter(ch))
            {
                sb.Append(ch);
            }
            else
            {
                break;
            }
        }
        return sb.ToString();
    }

    public static string PullEndingNonDigits(this string input)
    {
        for (var i = input.Length - 1; i >= 0; i--)
        {
            var ch = input[i];
            if (char.IsDigit(ch))
            {
                var result = input.Substring(i + 1, input.Length - i - 1);
                return result;
            }
        }
        return null;
    }

    /// <summary>
    /// Get the digit characters at the beginning of input, without changing input
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string PullBeginningDigits(this string input)
    {
        input = input.Trim();
        for (var i = 0; i < input.Length; i++)
        {
            var ch = input[i];
            if (!char.IsDigit(ch))
            {
                var result = input.Substring(0, i);
                return result;
            }
        }
        return null;
    }

    /// <summary>
    /// Get the digit characters at the beginning of input, removing them from input
    /// </summary>
    /// <param name="input"></param>
    /// <param name="inputWithoutBeginningDigits"></param>
    /// <returns></returns>
    public static string PullOutBeginningDigits(this string input, ref string inputWithoutBeginningDigits)
    {
        input = input.Trim();
        for (var i = 0; i < input.Length; i++)
        {
            var ch = input[i];
            if (!char.IsDigit(ch))
            {
                var result = input.Substring(0, i);
                inputWithoutBeginningDigits = input.Substring(i, input.Length - i);
                return result;
            }
        }
        return null;
    }

    /// <summary>
    /// Replace characters invalid in a Path with underscores
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string ReplaceInvalidCharactersWithUnderscores(this string path)
    {
        var sb = new StringBuilder(path);
        return sb.ReplaceInvalidCharactersWithUnderscores();
    }

    /// <summary>
    /// Replace characters invalid in a StringBuilder with underscores
    /// Uses a comprehensive set of characters that are problematic across different platforms
    /// </summary>
    /// <param name="sb"></param>
    /// <returns></returns>
    public static string ReplaceInvalidCharactersWithUnderscores(this StringBuilder sb)
    {
        // Get platform-specific invalid characters
        var invalidInFileName = Path.GetInvalidFileNameChars();

        // Add additional characters that are problematic across platforms
        // but might not be included in Path.GetInvalidFileNameChars() on all systems
        var additionalInvalidChars = new[] { '\\', '<', '>', '|', '\t' };

        for (var i = 0; i < sb.Length; i++)
        {
            var ch = sb[i];
            if (invalidInFileName.Contains(ch) || additionalInvalidChars.Contains(ch))
            {
                sb[i] = '_';
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Replace characters in charsToReplace with spaces
    /// </summary>
    /// <param name="str">The string to process</param>
    /// <param name="charsToReplace">The characters to replace with spaces</param>
    /// <returns>A new string with specified characters replaced by spaces</returns>
    public static string ReplaceCharactersWithSpaces(this string str, string charsToReplace)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < str.Length; i++)
        {
            var ch = str[i];
            if (charsToReplace.Contains(ch))
            {
                sb.Append(' ');
            }
            else
            {
                sb.Append(ch);
            }
        }
        var result = sb.ToString();
        return result;
    }

    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    /// <summary>
    /// Thanks to https://stackoverflow.com/questions/1266674/how-can-one-get-an-absolute-or-normalized-file-path-in-net
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string NormalizePath(this string path)
    {
        return Path.GetFullPath(new Uri(path).LocalPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        //.ToUpperInvariant();
    }

    /// <summary>
    /// Return a list of text segments where digit strings are grouped separately
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static List<string> SplitOnDigits(this string text)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        var isDigitValue = false;
        foreach (var ch in text)
        {
            if (char.IsDigit(ch))
            {
                if (isDigitValue)
                {
                    sb.Append(ch);
                }
                else
                {
                    if (sb.Length > 0)
                    {
                        // save the previous non-digit string
                        result.Add(sb.ToString());
                    }
                    isDigitValue = true;
                    sb.Clear();
                    sb.Append(ch);
                }
                continue;
            }
            if (!isDigitValue)
            {
                sb.Append(ch);
            }
            else
            {
                if (sb.Length > 0)
                {
                    // save the previous digit string
                    result.Add(sb.ToString());
                }
                isDigitValue = false;
                sb.Clear();
                sb.Append(ch);
            }
        }
        if (sb.Length > 0)
        {
            result.Add(sb.ToString());
        }
        return result;
    }

    /// <summary>
    /// Return the first parenthesized string in str, or string.Empty if there is none
    /// The string returned may include nested parenthetic strings
    /// </summary>
    /// <param name="str"></param>
    /// <param name="trimEnd"></param>
    /// <param name="startChar">like [, (, {</param>
    /// <param name="endChar">like ], ), }</param>
    /// <returns></returns>
    public static (string bracketed, string remainder) PullEnclosedString(this string str, bool trimEnd, char startChar, char endChar)
    {
        var sbIn = new StringBuilder(str);
        var sbResult = new StringBuilder();
        var sbOut = new StringBuilder();
        var isInsideParens = false;
        var leftParenCount = 0;
        for (var index = 0; index < sbIn.Length; index++)
        {
            var ch = sbIn[index];
            if (ch == startChar)
            {
                isInsideParens = true;
                leftParenCount++;
                if (leftParenCount > 1)
                {
                    sbResult.Append(ch);
                }
                continue;
            }
            if (ch == endChar)
            {
                leftParenCount--;
                if (leftParenCount > 0)
                {
                    sbResult.Append(ch);
                    continue;
                }

                // put the remaining string into sbOut
                for (var i = index + 1; i < sbIn.Length; i++)
                {
                    sbOut.Append(sbIn[i]);
                }
                var result1 = sbOut.ToString();
                if (trimEnd)
                {
                    result1 = result1.TrimEnd();
                }
                return (sbResult.ToString(), result1);
            }
            if (isInsideParens)
            {
                sbResult.Append(ch);
            }
            else
            {
                sbOut.Append(ch);
            }
        }
        var result = sbOut.ToString();
        if (trimEnd)
        {
            result = result.TrimEnd();
        }
        return (string.Empty, result);
    }

    /// <summary>
    /// Return the first parenthesized string in str, or string.Empty if there is none
    /// The string returned may include nested parenthetic strings
    /// </summary>
    /// <param name="str"></param>
    /// <param name="trimEnd"></param>
    /// <returns></returns>
    public static (string bracketed, string remainder) PullParenthesizedString(this string str, bool trimEnd)
    {
        return str.PullEnclosedString(trimEnd, '(', ')');
    }

    /// <summary>
    /// Return the first bracketed string in str, or string.Empty if there is none
    /// The string returned may include nested bracketed strings
    /// </summary>
    /// <param name="str"></param>
    /// <param name="trimEnd"></param>
    /// <returns>The bracketed string and the input string with the bracketed string removed</returns>
    public static (string bracketed, string remainder) PullBracketedString(this string str, bool trimEnd)
    {
        return str.PullEnclosedString(trimEnd, '[', ']');
    }

    /// <summary>
    /// Return the first braced string in str, or string.Empty if there is none
    /// The string returned may include nested braced strings
    /// </summary>
    /// <param name="str"></param>
    /// <param name="trimEnd"></param>
    /// <returns>The braced string and the input string with the braced string removed</returns>
    public static (string braced, string remainder) PullBracedString(this string str, bool trimEnd)
    {
        return str.PullEnclosedString(trimEnd, '{', '}');
    }

    /// <summary>
    /// Return the first double=quoted string in str, or string.Empty if there is none
    /// </summary>
    /// <param name="str"></param>
    /// <param name="trimEnd"></param>
    /// <returns>The quoted string and the input string with the quoted string removed</returns>
    public static (string quoted, string remainder) PullDoubleQuotedString(this string str, bool trimEnd)
    {
        var sbIn = new StringBuilder(str);
        var sbResult = new StringBuilder();
        var sbOut = new StringBuilder();
        var isInsideQuotes = false;
        for (var index = 0; index < sbIn.Length; index++)
        {
            var ch = sbIn[index];
            if (ch == '"')
            {
                isInsideQuotes = !isInsideQuotes;
                if (isInsideQuotes)
                {
                    continue;
                }
                // put the remaining string into sbOut
                for (var i = index + 1; i < sbIn.Length; i++)
                {
                    sbOut.Append(sbIn[i]);
                }
                return (sbResult.ToString(), sbOut.ToString());
            }
            if (isInsideQuotes)
            {
                sbResult.Append(ch);
            }
            else
            {
                sbOut.Append(ch);
            }
        }
        var result = sbOut.ToString();
        if (trimEnd)
        {
            result = result.TrimEnd();
        }
        return (string.Empty, result);
    }

    public static string RestOfLine(this string str)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < str.Length; i++)
        {
            var ch = str[i];
            if (ch == '\n')
            {
                return sb.ToString();
            }
            sb.Append(ch);
        }
        return sb.ToString();
    }
}