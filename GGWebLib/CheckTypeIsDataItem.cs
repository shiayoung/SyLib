using System;
namespace GGWebLib
{
    /// <summary>
    /// 定义判断一个类型是否可以“称得上”是一个数据实体类型的委托
    /// </summary>
    /// <param name="itemType">要判断的类型</param>
    /// <returns>yes or no</returns>
    public delegate bool CheckTypeIsDataItem(Type itemType);
}
