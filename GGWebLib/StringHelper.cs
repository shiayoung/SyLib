using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
namespace GGWebLib
{
    /// <summary>
    /// 字符串操作的工具类
    /// </summary>
    public static class StringHelper
    {
        private static readonly char[] charArray_semicolon = new char[]
		{
			';'
		};
        private static readonly char[] charArray_comma = new char[]
		{
			','
		};
        private static readonly char[] charArray_enter = new char[]
		{
			'\r',
			'\n'
		};
        private static StringIsBool s_StringIsBoolFunc = new StringIsBool(StringHelper.StringIsBool_MyFunc);
        /// <summary>
        /// GGWebLib中“判断一个字符串是否可以表示一个布尔型的 true”的测试函数的引用
        /// </summary>
        public static StringIsBool StringIsBoolFunc
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                StringHelper.s_StringIsBoolFunc = value;
            }
        }
        /// <summary>
        /// 尝试将一个字符串转换成一个整形数字
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <param name="defaultVal">默认值</param>
        /// <returns>转换后的数字</returns>
        public static int TryToInt(string str, int defaultVal)
        {
            if (string.IsNullOrEmpty(str))
            {
                return defaultVal;
            }
            int result = defaultVal;
            int.TryParse(str, out result);
            return result;
        }
        /// <summary>
        /// 尝试将一个字符串转换成一个整形数字
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns>转换后的数字</returns>
        public static int TryToInt(string str)
        {
            return StringHelper.TryToInt(str, 0);
        }
        /// <summary>
        /// 尝试将一个字符串转换成一个金额数字
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <param name="defaultVal">默认值</param>
        /// <returns>转换后的数字</returns>
        public static decimal TryToDecimal(string str, decimal defaultVal)
        {
            if (string.IsNullOrEmpty(str))
            {
                return defaultVal;
            }
            decimal result = defaultVal;
            decimal.TryParse(str, NumberStyles.Currency, null, out result);
            return result;
        }
        /// <summary>
        /// 尝试将一个字符串转换成一个金额数字
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns>转换后的数字</returns>
        public static decimal TryToDecimal(string str)
        {
            return StringHelper.TryToDecimal(str, 0m);
        }
        /// <summary>
        /// 尝试将一个字符串转换成一个DateTime
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <param name="defaultVal">默认值</param>
        /// <returns>转换后的DateTime</returns>
        public static DateTime TryToDateTime(string str, DateTime defaultVal)
        {
            if (string.IsNullOrEmpty(str))
            {
                return defaultVal;
            }
            DateTime result = defaultVal;
            DateTime.TryParse(str, out result);
            return result;
        }
        /// <summary>
        /// 尝试将一个字符串转换成一个DateTime
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns>转换后的DateTime</returns>
        public static DateTime TryToDateTime(string str)
        {
            return StringHelper.TryToDateTime(str, DateTime.MinValue);
        }
        /// <summary>
        /// 计算一个字符串的MD5值，编码方式：Encoding.Default
        /// </summary>
        /// <param name="input">要计算的字符串</param>
        /// <returns>MD5值</returns>
        public static string GetMd5String(string input)
        {
            if (input == null)
            {
                input = string.Empty;
            }
            byte[] value = new MD5CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(input));
            return BitConverter.ToString(value).Replace("-", "");
        }
        /// <summary>
        /// 计算一个字符串的SHA1值，编码方式：Encoding.Default
        /// </summary>
        /// <param name="input">要计算的字符串</param>
        /// <returns>SHA1值</returns>
        public static string GetSha1String(string input)
        {
            if (input == null)
            {
                input = string.Empty;
            }
            byte[] value = new SHA1CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(input));
            return BitConverter.ToString(value).Replace("-", "");
        }
        /// <summary>
        /// 得到一个字符串的Base64编码格式的字符串
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns>转换后的Base64编码格式的字符串</returns>
        public static string StringToBase64(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }
        /// <summary>
        /// 从Base64编码格式的字符串中还原原始的字符串
        /// </summary>
        /// <param name="base64str">Base64编码格式的字符串</param>
        /// <returns>解码后的原字符串</returns>
        public static string Base64ToString(string base64str)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64str));
        }
        /// <summary>
        /// （安全地）从Base64编码格式的字符串中还原原始的字符串，如果失败，返回空字符串
        /// </summary>
        /// <param name="base64str">Base64编码格式的字符串</param>
        /// <returns>解码后的原字符串</returns>
        public static string SafeBase64ToString(string base64str)
        {
            string result;
            try
            {
                result = Encoding.UTF8.GetString(Convert.FromBase64String(base64str));
            }
            catch
            {
                result = string.Empty;
            }
            return result;
        }
        /// <summary>
        /// 判断一个字符串是不是一个GUID的字符串
        /// </summary>
        /// <param name="str">要判断的字符串</param>
        /// <returns>是不是一个GUID的字符串</returns>
        public static bool StringIsGuid(string str)
        {
            if (str == null || str.Length != 36)
            {
                return false;
            }
            bool result;
            try
            {
                new Guid(str);
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 拆分由“分号”组成的字符串。拆分时将会去掉所有空白行。
        /// </summary>
        /// <param name="line">字符串行</param>
        /// <returns>拆分后的结果</returns>
        public static string[] SplitBySemicolon(string line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }
            return line.Split(StringHelper.charArray_semicolon, StringSplitOptions.RemoveEmptyEntries);
        }
        /// <summary>
        /// 拆分由“逗号”组成的字符串。拆分时将会去掉所有空白行。
        /// </summary>
        /// <param name="line">字符串行</param>
        /// <returns>拆分后的结果</returns>
        public static string[] SplitByComma(string line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }
            return line.Split(StringHelper.charArray_comma, StringSplitOptions.RemoveEmptyEntries);
        }
        /// <summary>
        /// 拆分由“回车符，换行符”组成的字符串。拆分时将会去掉所有空白行。
        /// </summary>
        /// <param name="text">字符串行</param>
        /// <returns>拆分后的结果</returns>
        public static string[] SplitByEnter(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            return text.Split(StringHelper.charArray_enter, StringSplitOptions.RemoveEmptyEntries);
        }
        /// <summary>
        /// <para>一个委托StringIsBool的实现</para>
        /// <para>判断原则：明确指出是 "false" or "0" ，则返回 false , 否则，只要长度大于零就算 true</para>
        /// </summary>
        /// <param name="str">要判断的字符串</param>
        /// <returns>字符串是否表示 true</returns>
        public static bool StringIsBool_MyFunc(string str)
        {
            return string.Compare(str, "false", true) != 0 && !(str == "0") && str.Length > 0;
        }
        /// <summary>
        /// （安全地）将一个字符串的内容转换成任意数据类型的值。
        /// </summary>
        /// <typeparam name="T">要转换后的类型</typeparam>
        /// <param name="str">将要转换的字符串</param>
        /// <returns>转换后的结果</returns>
        public static T SafeConvertString<T>(string str)
        {
            if (str == null)
            {
                return default(T);
            }
            T result;
            try
            {
                result = (T)Convert.ChangeType(str, typeof(T));
            }
            catch
            {
                result = default(T);
            }
            return result;
        }
        /// <summary>
        /// （尝试）将一个字符串的内容转换成任意数据类型的值，如果无法转换失败，抛出异常
        /// </summary>
        /// <param name="str">一个字符串</param>
        /// <param name="conversionType">将要转换到的新类型</param>
        /// <returns>转换后的结果</returns>
        public static object ConvertString(string str, Type conversionType)
        {
            if (conversionType == null)
            {
                throw new ArgumentNullException("conversionType");
            }
            if (conversionType == TypeList._string)
            {
                return str;
            }
            if (!string.IsNullOrEmpty(str))
            {
                if (conversionType.IsEnum)
                {
                    return int.Parse(str);
                }
                if (conversionType.IsGenericType)
                {
                    if (conversionType.GetGenericTypeDefinition() != TypeList._nullable)
                    {
                        throw new InvalidCastException();
                    }
                    conversionType = conversionType.GetGenericArguments()[0];
                }
                if (conversionType == TypeList._int)
                {
                    return int.Parse(str);
                }
                if (conversionType == TypeList._long)
                {
                    return long.Parse(str);
                }
                if (conversionType == TypeList._short)
                {
                    return short.Parse(str);
                }
                if (conversionType == TypeList._DateTime)
                {
                    return DateTime.Parse(str);
                }
                if (conversionType == TypeList._bool)
                {
                    return StringHelper.s_StringIsBoolFunc(str);
                }
                if (conversionType == TypeList._double)
                {
                    return double.Parse(str);
                }
                if (conversionType == TypeList._decimal)
                {
                    return decimal.Parse(str);
                }
                if (conversionType == TypeList._float)
                {
                    return float.Parse(str);
                }
                if (conversionType == TypeList._Guid)
                {
                    return new Guid(str);
                }
                if (conversionType == TypeList._ulong)
                {
                    return ulong.Parse(str);
                }
                if (conversionType == TypeList._uint)
                {
                    return uint.Parse(str);
                }
                if (conversionType == TypeList._ushort)
                {
                    return ushort.Parse(str);
                }
                if (conversionType == TypeList._char)
                {
                    return str[0];
                }
                if (conversionType == TypeList._byte)
                {
                    return byte.Parse(str);
                }
                if (conversionType == TypeList._sbyte)
                {
                    return sbyte.Parse(str);
                }
                throw new InvalidCastException();
            }
            else
            {
                if (conversionType == TypeList._DateTime)
                {
                    return DateTime.MinValue;
                }
                if (conversionType == TypeList._Guid)
                {
                    return Guid.Empty;
                }
                if (conversionType.IsValueType && !conversionType.IsGenericType)
                {
                    try
                    {
                        return Convert.ChangeType(0, conversionType);
                    }
                    catch
                    {
                        throw new InvalidCastException();
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// 任意值如果转成bool类型，是否为true
        /// </summary>
        /// <param name="obj">要判断的对象</param>
        /// <returns>对象是否为true</returns>
        public static bool ObjectIsTrue(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Type type = obj.GetType();
            bool result;
            try
            {
                if (type == TypeList._bool)
                {
                    result = (bool)obj;
                }
                else
                {
                    if (type == TypeList._string)
                    {
                        string text = obj.ToString();
                        if (string.IsNullOrEmpty(text))
                        {
                            result = false;
                        }
                        else
                        {
                            result = StringHelper.s_StringIsBoolFunc(text);
                        }
                    }
                    else
                    {
                        result = Convert.ToBoolean(obj);
                    }
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 将一个十六进制的字符串转成byte[]
        /// </summary>
        /// <param name="hex">十六进制的字符串</param>
        /// <returns>转换后的byte[]</returns>
        public static byte[] HexToBin(string hex)
        {
            if (hex == null)
            {
                throw new ArgumentNullException("hex");
            }
            if (hex.Length % 2 != 0)
            {
                throw new InvalidOperationException(SR.HexLenIsWrong);
            }
            byte[] array = new byte[hex.Length / 2];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber);
            }
            return array;
        }
        /// <summary>
        /// <para>拆分一个字符串行。如：a=1;b=2;c=3;d=4;</para>
        /// <para>此时可以调用: SplitString("a=1;b=2;c=3;d=4;", ';', '=');</para>
        /// <para>说明：对于空字符串，方法也会返回一个空的列表。</para>
        /// </summary>
        /// <param name="line">包含所有项目组成的字符串行</param>
        /// <param name="separator1">每个项目之间的分隔符</param>
        /// <param name="separator2">每个项目内的分隔符</param>
        /// <returns>拆分后的结果列表</returns>
        public static List<KeyValuePair<string, string>> SplitString(string line, char separator1, char separator2)
        {
            if (string.IsNullOrEmpty(line))
            {
                return new List<KeyValuePair<string, string>>();
            }
            string[] array = line.Split(new char[]
			{
				separator1
			}, StringSplitOptions.RemoveEmptyEntries);
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>(array.Length);
            char[] separator3 = new char[]
			{
				separator2
			};
            string[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                string text = array2[i];
                string[] array3 = text.Split(separator3, StringSplitOptions.RemoveEmptyEntries);
                if (array3.Length != 2)
                {
                    throw new ArgumentException(SR.StringFormatInvalid);
                }
                list.Add(new KeyValuePair<string, string>(array3[0], array3[1]));
            }
            return list;
        }
        /// <summary>
        /// 根据一个字符串返回标准的日期格式，如果字符串不是有效的日期，则返回null
        /// </summary>
        /// <param name="date">包含日期的字符串</param>
        /// <returns>转换后的标准的日期格式("yyyy-MM-dd")</returns>
        public static string GetStdDateFormatString(string date)
        {
            DateTime dateTime;
            if (DateTime.TryParse(date, out dateTime))
            {
                return dateTime.ToString("yyyy-MM-dd");
            }
            return null;
        }
        /// <summary>
        /// 根据一个字符串返回标准的日期时间格式，如果字符串不是有效的日期时间，则返回null
        /// </summary>
        /// <param name="time">包含日期时间的字符串</param>
        /// <returns>转换后的标准的日期时间格式("yyyy-MM-dd HH:mm:ss")</returns>
        public static string GetStdDateTimeFormatString(string time)
        {
            DateTime dateTime;
            if (DateTime.TryParse(time, out dateTime))
            {
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            return null;
        }
        /// <summary>
        /// <para>将一个字符串拆分成一个整数的列表。要求：字符串中每个整数由指定的分隔符区分开。</para>
        /// <para>若包含非数字内容或空白项，将会忽略。此方法始终会返回一个列表。</para>
        /// </summary>
        /// <param name="str">要拆分的字符串</param>
        /// <param name="flag">数字之间的分隔符</param>
        /// <returns>整数列表</returns>
        public static List<int> StringToIntList(string str, char flag)
        {
            if (string.IsNullOrEmpty(str))
            {
                return new List<int>();
            }
            string[] array = str.Split(new char[]
			{
				flag
			}, StringSplitOptions.RemoveEmptyEntries);
            List<int> list = new List<int>(array.Length);
            string[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                string s = array2[i];
                int item;
                if (int.TryParse(s, out item))
                {
                    list.Add(item);
                }
            }
            return list;
        }
    }
}
