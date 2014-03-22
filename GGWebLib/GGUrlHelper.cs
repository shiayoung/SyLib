using System;
using System.Collections.Specialized;
using System.Threading;
using System.Web;
namespace GGWebLib
{
    /// <summary>
    /// 从Url中读取参数的工具类
    /// </summary>
    public static class GGUrlHelper
    {
        /// <summary>
        /// 从URL查询字符串中获取一个参数的整数值
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>URL参数值</returns>
        public static int GetIntegerFromQueryString(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            return StringHelper.TryToInt(HttpContext.Current.Request.QueryString[name], 0);
        }
        /// <summary>
        /// 从URL查询字符串中获取一个参数的整数值
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="defaultVal">默认值</param>
        /// <returns>URL参数值</returns>
        public static int GetIntegerFromQueryString(string name, int defaultVal)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            return StringHelper.TryToInt(HttpContext.Current.Request.QueryString[name], defaultVal);
        }
        /// <summary>
        /// 从URL查询字符串中获取一个参数的整数值，可指定取值范围
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="defaultVal">默认值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>URL参数值</returns>
        public static int GetIntegerFromQueryString(string name, int defaultVal, int? min, int? max)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            int num = StringHelper.TryToInt(HttpContext.Current.Request.QueryString[name], defaultVal);
            return AppSettingReader.CheckValueRange(num, min, max, defaultVal);
        }
        /// <summary>
        /// 从URL查询字符串中获取一个参数的整数值
        /// </summary>
        /// <param name="queryStringCollection">包含键／值的集合</param>
        /// <param name="name">参数名称</param>
        /// <param name="defaultVal">默认值</param>
        /// <returns>参数值</returns>
        public static int GetIntegerFromQueryString(NameValueCollection queryStringCollection, string name, int defaultVal)
        {
            if (queryStringCollection == null)
            {
                throw new ArgumentNullException("queryStringCollection");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            return StringHelper.TryToInt(queryStringCollection[name], defaultVal);
        }
        /// <summary>
        /// 从URL查询字符串中获取一个参数的整数值，可指定取值范围
        /// </summary>
        /// <param name="queryStringCollection">包含键／值的集合</param>
        /// <param name="name">参数名称</param>
        /// <param name="defaultVal">默认值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>参数值</returns>
        public static int GetIntegerFromQueryString(NameValueCollection queryStringCollection, string name, int defaultVal, int? min, int? max)
        {
            if (queryStringCollection == null)
            {
                throw new ArgumentNullException("queryStringCollection");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            int num = StringHelper.TryToInt(queryStringCollection[name], defaultVal);
            return AppSettingReader.CheckValueRange(num, min, max, defaultVal);
        }
        /// <summary>
        /// 从URL查询字符串中获取一个参数的字符串值
        /// </summary>
        /// <param name="queryStringCollection">包含键／值的集合</param>
        /// <param name="name">参数名称</param>
        /// <param name="defaultStr">默认值</param>
        /// <returns>参数值</returns>
        public static string GetStringFromQueryStirng(NameValueCollection queryStringCollection, string name, string defaultStr)
        {
            if (queryStringCollection == null)
            {
                throw new ArgumentNullException("queryStringCollection");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            string text = queryStringCollection[name];
            string text2 = text ?? defaultStr;
            if (text2 != null)
            {
                return text2.Trim();
            }
            return null;
        }
        /// <summary>
        /// 从URL查询字符串中获取一个参数的字符串值
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="defaultStr">默认值</param>
        /// <returns>参数值</returns>
        public static string GetStringFromQueryStirng(string name, string defaultStr)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            string text = HttpContext.Current.Request.QueryString[name];
            string text2 = text ?? defaultStr;
            if (text2 != null)
            {
                return text2.Trim();
            }
            return null;
        }
        /// <summary>
        /// 从URL查询字符串中获取一个参数的字符串值
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>参数值</returns>
        public static string GetStringFromQueryStirng(string name)
        {
            return GGUrlHelper.GetStringFromQueryStirng(name, string.Empty);
        }
        /// <summary>
        /// 检查查询字符串中是否包含参数"_sleep"，如有，则调用System.Threading.Thread.Sleep(sleep)，一般用于模拟网速过慢的情形。
        /// </summary>
        public static void CheckSleep()
        {
            int integerFromQueryString = GGUrlHelper.GetIntegerFromQueryString("_sleep");
            if (integerFromQueryString > 0)
            {
                Thread.Sleep(integerFromQueryString);
            }
        }
    }
}
