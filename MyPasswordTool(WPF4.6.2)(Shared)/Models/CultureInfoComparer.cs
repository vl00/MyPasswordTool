using System;
using System.Collections.Generic;
using System.Globalization;

namespace MyPasswordTool.Models
{
    public class CultureInfoComparer : IComparer<string>
    {
        //readonly StringComparer _comparer = StringComparer.Create(CultureInfo.GetCultureInfo("zh-cn"), true);

        public int Compare(string s1, string s2)
        {
            if (s1 == null) throw new ArgumentNullException(nameof(s1));
            if (s2 == null) throw new ArgumentNullException(nameof(s2));

            return string.Compare(ChineseConvertToLetter.UtilIndexCode(s1), ChineseConvertToLetter.UtilIndexCode(s2), true);
        }

        static class ChineseConvertToLetter
        {
            // 返回字符串的首写字母字符串 
            public static string UtilIndexCode(string str)
            {
                string _Temp = null;
                for (var i = 0; i < str.Length; i++)
                    _Temp += GetOneIndex(str.Substring(i, 1));
                return _Temp;
            }

            //得到单个字符的首字母 
            private static string GetOneIndex(string c)
            {
                if (Convert.ToChar(c) >= 0 && Convert.ToChar(c) < 256)
                    return c;
                else
                    return GetGbkX(c);
            }

            //根据汉字拼音排序得到首字母 
            private static string GetGbkX(string c)
			{
				if (c.CompareTo("啊") < 0)//吖
				{
					return c;
				}
				if (c.CompareTo("八") < 0)
				{
					return "A";
				}
				if (c.CompareTo("擦") < 0)//嚓
				{
					return "B";
				}

				if (c.CompareTo("咑") < 0)
				{
					return "C";
				}
				if (c.CompareTo("妸") < 0)
				{
					return "D";
				}
				if (c.CompareTo("冹") < 0)//发
				{
					return "E";
				}
				if (c.CompareTo("旮") < 0)
				{
					return "F";
				}
				if (c.CompareTo("铪") < 0)
				{
					return "G";
				}
				if (c.CompareTo("讥") < 0)
				{
					return "H";
				}
				if (c.CompareTo("喀") < 0)//咔
				{
					return "J";
				}
				if (c.CompareTo("垃") < 0)
				{
					return "K";
				}
				if (c.CompareTo("呒") < 0)
				{
					return "L";
				}
				if (c.CompareTo("拏") < 0)
				{
					return "M";
				}
				if (c.CompareTo("噢") < 0)
				{
					return "N";
				}
				if (c.CompareTo("妑") < 0)
				{
					return "O";
				}
				if (c.CompareTo("七") < 0)
				{
					return "P";
				}
				if (c.CompareTo("亽") < 0)
				{
					return "Q";
				}
				if (c.CompareTo("仨") < 0)
				{
					return "R";
				}
				if (c.CompareTo("他") < 0)
				{
					return "S";
				}
				if (c.CompareTo("哇") < 0)
				{
					return "T";
				}
				if (c.CompareTo("夕") < 0)
				{
					return "W";
				}
				if (c.CompareTo("丫") < 0)
				{
					return "X";
				}
				if (c.CompareTo("帀") < 0)
				{
					return "Y";
				}
				if (c.CompareTo("糳") < 0)//咗
				{
					return "Z";
				}
				return c;
			}
		}
    }
}