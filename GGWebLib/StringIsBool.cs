using System;
namespace GGWebLib
{
    /// <summary>
    /// 判断一个字符串是否可以表示一个布尔型的 true
    /// </summary>
    /// <param name="str">要判断的字符串</param>
    /// <returns>是否可以表示一个布尔型的 true</returns>
    public delegate bool StringIsBool(string str);
}
