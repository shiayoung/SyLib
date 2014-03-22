using System;
namespace GGWebLib
{
    /// <summary>
    /// 用于标识实体的每个数据成员的一些加载信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class ItemFieldAttribute : Attribute
    {
        /// <summary>
        /// 是否仅在加载“所有成员”时才加载，即：仅当在调用LoadALLValues()时加载
        /// </summary>
        public bool OnlyLoadAll;
        /// <summary>
        /// 在加载数据时，不加载这个成员
        /// </summary>
        public bool IgnoreLoad;
        /// <summary>
        /// 数据库中对应的字段名，如不指定，则与成员的名称相同。
        /// </summary>
        public string DbFieldName;
        /// <summary>
        /// 从查询字符串或者从FROM加载时的Key
        /// </summary>
        public string KeyName;
        /// <summary>
        /// 允许在加载时，找不到相应的匹配数据来源。（只用于值类型），此设置对于从数据库加载时无效。
        /// </summary>
        public bool AllowNotFoundOnLoad;
        /// <summary>
        /// 在加载时，如果找不到相应的匹配数据来源，对于“字符串”类型来说，就设置为 String.Empty ，此设置对于从数据库加载时无效。
        /// </summary>
        public bool SetEmptyIfNotFoundOnLoad;
        /// <summary>
        /// 指示是否是一个子实体对象
        /// </summary>
        internal bool IsSubItem;
    }
}
