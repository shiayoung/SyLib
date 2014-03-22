using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace System
{
    public static class Extensions
    {
        public static bool IsNumber(this string strNumber)
        {
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"^\d+$");
            return r.IsMatch(strNumber);
        }

        public static string MD5Encrypt(this string str)
        {
            string encryptStr = str + "101ac962ac50755b964b07152d123b70";
            string first = encryptStr.Substring(0, 32);
            string last = encryptStr.Substring(32);
            return (FormsAuthentication.HashPasswordForStoringInConfigFile(first, "MD5") + FormsAuthentication.HashPasswordForStoringInConfigFile(last, "MD5")).ToLower();
        }

        public static Int32 ToInt32(this string str)
        {
            Int32 i = 0;
            Int32.TryParse(str, out i);
            return i;
        }

        public static DateTime ToDateTime(this string str, DateTime d)
        {
            if (str.IsEmpty())
            {
                return d;
            }
            DateTime.TryParse(str, out d);
            return d;
        }

        public static decimal ToDecimal(this string str)
        {
            decimal i = 0;
            decimal.TryParse(str, out i);
            return i;
        }

        public static bool IsEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
    }
}
