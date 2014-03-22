using System;
namespace GGWebLib
{
    /// <summary>
    /// 每次执行数据库操作 后 的事件委托
    /// </summary>
    /// <param name="context">GGDbContext对象</param>
    public delegate void GGDbContextAfterExecute(GGDbContext context);
}
