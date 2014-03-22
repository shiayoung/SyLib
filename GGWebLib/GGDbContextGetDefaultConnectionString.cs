using System;
namespace GGWebLib
{
    /// <summary>
    /// 如果在构造方法中没有提供连接字符串并且也没有提供默认的连接字符串，将会引发这个事件的委托
    /// </summary>
    /// <param name="e">事件参数</param>
    public delegate void GGDbContextGetDefaultConnectionString(GGDbContextEventArgs e);
}
