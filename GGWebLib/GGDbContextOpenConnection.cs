using System;
namespace GGWebLib
{
    /// <summary>
    /// 每次打开数据库连接时的事件委托
    /// </summary>
    /// <param name="context">GGDbContext对象</param>
    public delegate void GGDbContextOpenConnection(GGDbContext context);
}
