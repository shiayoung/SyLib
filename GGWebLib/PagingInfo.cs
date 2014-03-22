using System;
namespace GGWebLib
{
    /// <summary>
    /// 分页信息
    /// </summary>
    public class PagingInfo
    {
        /// <summary>
        /// 分页序号，从0开始计数
        /// </summary>
        public int PageIndex;
        /// <summary>
        /// 分页大小
        /// </summary>
        public int PageSize;
        /// <summary>
        /// 从相关查询中获取到的符合条件的总记录数
        /// </summary>
        public int RecCount;
        /// <summary>
        /// 计算总页数
        /// </summary>
        /// <returns></returns>
        public int CalcPageCount()
        {
            if (this.PageSize == 0 || this.RecCount == 0)
            {
                return 0;
            }
            return (int)Math.Ceiling((double)this.RecCount / (double)this.PageSize);
        }
    }
}
