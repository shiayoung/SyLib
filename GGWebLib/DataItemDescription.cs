using System;
using System.Collections.Generic;
namespace GGWebLib
{
    /// <summary>
    /// 每个实体类型的描述
    /// </summary>
    internal sealed class DataItemDescription
    {
        private Dictionary<string, DataItemFieldInfo> _dict;
        private Type _type;
        private int _p_count;
        private int _a_count;
        public Dictionary<string, DataItemFieldInfo> Dict
        {
            get
            {
                return this._dict;
            }
        }
        public Type ItemType
        {
            get
            {
                return this._type;
            }
        }
        /// <summary>
        /// 调用LoadPartialValues()时，期望的成功加载数据成员数量
        /// </summary>
        public int LoadPartialValues_ExpectSuccessCount
        {
            get
            {
                return this._p_count;
            }
        }
        /// <summary>
        /// 调用LoadALLValues()时，期望的成功加载数据成员数量
        /// </summary>
        public int LoadALLValues_ExpectSuccessCount
        {
            get
            {
                return this._a_count;
            }
        }
        public DataItemDescription(Type type, int memberCapacity)
        {
            this._type = type;
            this._dict = new Dictionary<string, DataItemFieldInfo>(memberCapacity, StringComparer.OrdinalIgnoreCase);
        }
        public void SetTwoCount(int p_coount, int a_count)
        {
            this._p_count = p_coount;
            this._a_count = a_count;
        }
        /// <summary>
        /// 根据一个成员的名称，获取相应的GGDataItemFieldInfo的实例描述信息。
        /// </summary>
        /// <param name="name">成员的属性名或映射的数据库的字段名</param>
        /// <param name="info">DataItemFieldInfo对象</param>
        /// <returns>是否成功获取到指定的成员信息</returns>
        public bool TryGetValue(string name, out DataItemFieldInfo info)
        {
            if (this._dict.TryGetValue(name, out info))
            {
                return true;
            }
            foreach (KeyValuePair<string, DataItemFieldInfo> current in this._dict)
            {
                if (current.Value.MemberAttr.DbFieldName == name)
                {
                    info = current.Value;
                    return true;
                }
            }
            return false;
        }
    }
}
