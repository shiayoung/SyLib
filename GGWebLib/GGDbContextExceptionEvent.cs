using System;
namespace GGWebLib
{
    /// <summary>
    /// 如果GGDbContext在执行数据库操作时发生异常，将会调用这个委托的事件
    /// </summary>
    /// <param name="context">GGDbContext对象</param>
    /// <param name="ex">Exception对象</param>
    public delegate void GGDbContextExceptionEvent(GGDbContext context, Exception ex);
}
