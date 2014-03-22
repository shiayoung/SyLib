using System;
using System.Runtime.Serialization;
namespace GGWebLib
{
    /// <summary>
    /// 简单的异常，一般只是为了方便从嵌套比较深的地方快速跳出，并带有一个错误消息。
    /// </summary>
    [Serializable]
    public class MyMessageException : Exception
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="info">SerializationInfo对象</param>
        /// <param name="context">StreamingContext对象</param>
        public MyMessageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="message">异常消息</param>
        public MyMessageException(string message)
            : base(message)
        {
        }
    }
}
