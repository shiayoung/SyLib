using System;
namespace GGWebLib
{
    /// <summary>
    /// 调用参数无效异常：无参数的类型不是有效的数据实体类型。
    /// </summary>
    [Serializable]
    public sealed class InvalidDataItemTypeException : MyMessageException
    {
        /// <summary>
        /// 调用参数无效异常：无参数的类型不是有效的数据实体类型。
        /// </summary>
        public InvalidDataItemTypeException()
            : base("参数的类型不是有效的数据实体类型。。")
        {
        }
    }
}
