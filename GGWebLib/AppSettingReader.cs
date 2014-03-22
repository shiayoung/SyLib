using System;
using System.Configuration;
namespace GGWebLib
{
    /// <summary>
    /// 读取配置文件中AppSetting配置节的工具类
    /// </summary>
    public static class AppSettingReader
    {
        /// <summary>
        /// 从AppSetting配置节读取一个整数。
        /// </summary>
        /// <param name="keyName">配置项的键名</param>
        /// <param name="mustExist">是否要求配置项必须存在</param>
        /// <param name="defaultVal">默认值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>配置项的值</returns>
        public static int GetInt(string keyName, bool mustExist, int defaultVal, int? min, int? max)
        {
            if (string.IsNullOrEmpty(keyName))
            {
                throw new ArgumentNullException("keyName");
            }
            string text = ConfigurationManager.AppSettings[keyName];
            if (text == null && mustExist)
            {
                throw new ConfigurationErrorsException(string.Format(SR.ConfigItemNotSet, keyName));
            }
            int num = StringHelper.TryToInt(text, defaultVal);
            return AppSettingReader.CheckValueRange(num, min, max, defaultVal);
        }
        /// <summary>
        /// 检查一个数值是否在有效范围内，如果是有效的数值，则直接返回，否则，返回默认值
        /// </summary>
        /// <param name="num">要测试的数值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <param name="defaultVal">默认值</param>
        /// <returns>配置项的值</returns>
        internal static int CheckValueRange(int num, int? min, int? max, int defaultVal)
        {
            if (min.HasValue && num < min.Value)
            {
                return defaultVal;
            }
            if (max.HasValue && num > max.Value)
            {
                return defaultVal;
            }
            return num;
        }
        /// <summary>
        /// 从AppSetting配置节读取一个整数，如果找不到设置项，就返回默认值。
        /// </summary>
        /// <param name="keyName">配置项的键名</param>
        /// <param name="defaultVal">默认值</param>
        /// <returns>配置项的值</returns>
        public static int GetInt(string keyName, int defaultVal)
        {
            return AppSettingReader.GetInt(keyName, false, defaultVal, null, null);
        }
        /// <summary>
        /// 从AppSetting配置节读取一个整数，如果找不到设置项，就返回 0
        /// </summary>
        /// <param name="keyName">配置项的键名</param>
        /// <returns>配置项的值</returns>
        public static int GetInt(string keyName)
        {
            return AppSettingReader.GetInt(keyName, false, 0, null, null);
        }
        /// <summary>
        /// 从AppSetting配置节读取一个字符串。
        /// </summary>
        /// <param name="keyName">配置项的键名</param>
        /// <param name="mustExist">是否要求配置项必须存在</param>
        /// <param name="defaultVal">默认值</param>
        /// <returns>配置项的值</returns>
        public static string GetString(string keyName, bool mustExist, string defaultVal)
        {
            if (string.IsNullOrEmpty(keyName))
            {
                throw new ArgumentNullException("keyName");
            }
            string text = ConfigurationManager.AppSettings[keyName];
            if (text != null)
            {
                return text;
            }
            if (mustExist)
            {
                throw new ConfigurationException(string.Format(SR.ConfigItemNotSet, keyName));
            }
            return defaultVal;
        }
        /// <summary>
        /// 从AppSetting配置节读取一个字符串，如果找不到设置项，就返回 ""
        /// </summary>
        /// <param name="keyName">配置项的键名</param>
        /// <returns>配置项的值</returns>
        public static string GetString(string keyName)
        {
            return AppSettingReader.GetString(keyName, false, string.Empty);
        }
    }
}
