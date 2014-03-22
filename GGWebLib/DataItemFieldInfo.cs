using System;
using System.Reflection;
namespace GGWebLib
{
    /// <summary>
    /// 用于标识实体的每个数据成员的完整描述信息
    /// </summary>
    internal sealed class DataItemFieldInfo
    {
        private ItemFieldAttribute _attr;
        private MemberInfo _info;
        /// <summary>
        /// ItemFieldAttribute形式的描述信息
        /// </summary>
        public ItemFieldAttribute MemberAttr
        {
            get
            {
                return this._attr;
            }
        }
        /// <summary>
        /// 反射信息
        /// </summary>
        public MemberInfo MemberInfo
        {
            get
            {
                return this._info;
            }
        }
        /// <summary>
        /// 此成员是否为“属性”，否则为“字段”，
        /// </summary>
        public bool IsProperty
        {
            get
            {
                return this._info.MemberType == MemberTypes.Property;
            }
        }
        /// <summary>
        /// 此成员（字段或属性）的类型
        /// </summary>
        public Type MemberType
        {
            get
            {
                if (!this.IsProperty)
                {
                    return ((FieldInfo)this.MemberInfo).FieldType;
                }
                return ((PropertyInfo)this.MemberInfo).PropertyType;
            }
        }
        public DataItemFieldInfo(ItemFieldAttribute attr, MemberInfo info)
        {
            if (attr == null)
            {
                throw new ArgumentNullException("attr");
            }
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this._attr = attr;
            this._info = info;
        }
        public object GetValue(object item)
        {
            if (this.IsProperty)
            {
                return ((PropertyInfo)this.MemberInfo).GetValue(item, null);
            }
            return ((FieldInfo)this.MemberInfo).GetValue(item);
        }
        public void SetValue(object item, object val)
        {
            if (this.IsProperty)
            {
                ((PropertyInfo)this.MemberInfo).SetValue(item, val, null);
                return;
            }
            ((FieldInfo)this.MemberInfo).SetValue(item, val);
        }
    }
}
