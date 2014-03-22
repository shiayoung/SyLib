using System;
using System.Collections.Specialized;
namespace GGWebLib
{
    /// <summary>
    /// 定义用于从一个Request.Form集合读取某个指定键的键值的方法委托
    /// </summary>
    /// <param name="form">参数值集合</param>
    /// <param name="key">要读取的键名</param>
    /// <returns>读取到的键值</returns>
    public delegate string GetValueFromCollection(NameValueCollection form, string key);
}
