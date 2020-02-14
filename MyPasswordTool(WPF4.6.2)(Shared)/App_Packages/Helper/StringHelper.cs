using System.Text;
using System.Text.RegularExpressions;

namespace Common
{
    public static class StringHelper
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str);
        }

        public static StringBuilder AppendLine(this StringBuilder sb, string format, params object[] args)
        {
            return sb.AppendLine(string.Format(format, args));
        }

        public static string Trim(this string str, string trimStr)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return string.IsNullOrEmpty(trimStr) ? str.Trim() : str.Trim(trimStr.ToCharArray());
        }

        public static string TrimStart(this string str, string trimStr)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return string.IsNullOrEmpty(trimStr) ? str.TrimStart() : str.TrimStart(trimStr.ToCharArray());
        }

        public static string TrimEnd(this string str, string trimStr)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return string.IsNullOrEmpty(trimStr) ? str.TrimEnd() : str.TrimEnd(trimStr.ToCharArray());
        }

        public static string ReplaceEx(this string str, string oldVal, string newVal)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(oldVal)) return str;
            return str.Replace(oldVal, newVal ?? "");
        }

        public static string ReplaceWith(this string str, string regexStr, string replacement)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return Regex.Replace(str, regexStr, replacement);
        }
    }
}