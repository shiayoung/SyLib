using System;
namespace GGWebLib
{
    /// <summary>
    /// 加载成员失败时的异常类型
    /// </summary>
    [Serializable]
    public sealed class LoadMemberFaildException : MyMessageException
    {
        /// <summary>
        /// 构造方法：加载成员失败时的异常类型
        /// </summary>
        /// <param name="t">加载失败的类型名称</param>
        /// <param name="fieldNames">加载失败的成员名称</param>
        public LoadMemberFaildException(Type t, string fieldNames)
            : base(string.Concat(new string[]
		{
			"在加载类型[",
			t.ToString(),
			"]的实例时，有些成员在加载时失败了，名称： [",
			fieldNames,
			"]"
		}))
        {
        }
    }
}
