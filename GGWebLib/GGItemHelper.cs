using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Reflection;
using System.Web;
namespace GGWebLib
{
    /// <summary>
    /// 用于数据实体的一些反射操作的辅助工具类，主要实现了从数据库加载，和从Request中加载实体的功能。
    /// </summary>
    public static class GGItemHelper
    {
        private sealed class LatestLoadDataItemInfo
        {
            public DataItemDescription ItemDescription;
            public string[] ColumnNames;
        }
        /// <summary>
        /// 从Request中加载一个字符串时，是否自动删除首尾空格。默认值：true
        /// </summary>
        public static bool AutoTrimStringOnLoad = true;
        private static CheckTypeIsDataItem s_CheckTypeIsDataItemFunc = new CheckTypeIsDataItem(GGItemHelper.CheckItemIsNormalClass);
        private static GetValueFromCollection s_GetValueFromCollectionFunc = new GetValueFromCollection(GGWebFormHelper.GetValueFromRequestFormUseFullKeyNameMatch);
        private static Hashtable s_hashtbl = Hashtable.Synchronized(new Hashtable(3072));
        /// <summary>
        /// <para>用于检查对象是否是一个数据实体类型的判断函数，默认方法： CheckItemIsNormalClass</para>
        /// <para>建议的做法是：定义一个实体基类，所有的实体类从那个类继承，然后写个简单的判断方式给当前属性赋值</para>
        /// </summary>
        public static CheckTypeIsDataItem CheckTypeIsDataItemFunc
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                GGItemHelper.s_CheckTypeIsDataItemFunc = value;
            }
        }
        /// <summary>
        /// <para>用于从一个Request.Form集合读取某个指定键的键值的方法，默认方法： GGWebFormHelper.GetValueFromRequestFormUseFullKeyNameMatch</para>
        /// <para>GGWebLib也定义了另一个可选的方法：GGWebFormHelper.GetValueFromRequestForm</para>
        /// </summary>
        public static GetValueFromCollection GetValueFromCollectionFunc
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                GGItemHelper.s_GetValueFromCollectionFunc = value;
            }
        }
        /// <summary>
        /// 检查一个类型是否是一个“普通”的实体类（建议使用自定义基类的做法）
        /// 【警告】这个方法不检查参数是否为 null 。
        /// </summary>
        /// <param name="itemType">要检查的类型</param>
        /// <returns>yes or no</returns>
        public static bool CheckItemIsNormalClass(Type itemType)
        {
            return !itemType.IsPrimitive && !itemType.IsGenericType && itemType != TypeList._string && itemType != TypeList._object && itemType.IsClass && !itemType.IsArray;
        }
        /// <summary>
        /// 判断指定的类型是否可以“称得上”是一个数据实体类型，调用CheckTypeIsDataItemFunc来决定
        /// </summary>
        /// <param name="itemType">要检查的类型</param>
        /// <returns>yes or no</returns>
        internal static bool IsDataItemType(Type itemType)
        {
            return GGItemHelper.s_CheckTypeIsDataItemFunc(itemType);
        }
        /// <summary>
        /// 确认类型是一个数据实体类型，否则会抛出异常。
        /// </summary>
        /// <param name="itemType">要检查的类型</param>
        internal static void EnsureIsDataItemType(Type itemType)
        {
            if (!GGItemHelper.s_CheckTypeIsDataItemFunc(itemType))
            {
                throw new InvalidDataItemTypeException();
            }
        }
        /// <summary>
        /// 返回一个类型的真实的类型，用于获取【可空类型】的参数类型。
        /// </summary>
        /// <param name="testType"></param>
        /// <returns></returns>
        internal static Type GetRealType(Type testType)
        {
            if (testType.IsGenericType && testType.GetGenericTypeDefinition() == TypeList._nullable)
            {
                return testType.GetGenericArguments()[0];
            }
            return testType;
        }
        /// <summary>
        /// 测试一个类型是不是可枚举类型，但不包括 string 类型
        /// </summary>
        /// <param name="testType"></param>
        /// <returns></returns>
        internal static bool IsEnumerableType(Type testType)
        {
            return testType != TypeList._string && (testType.IsArray || TypeList._IEnumerable.IsAssignableFrom(testType));
        }
        /// <summary>
        /// 获取一个类型的具体描述。
        /// </summary>
        /// <param name="itemType">要加载的类型</param>
        /// <returns>类型的具体描述</returns>
        internal static DataItemDescription GetItemDescription(Type itemType)
        {
            if (itemType == null)
            {
                throw new ArgumentNullException("itemType");
            }
            DataItemDescription dataItemDescription = GGItemHelper.s_hashtbl[itemType] as DataItemDescription;
            if (dataItemDescription != null)
            {
                return dataItemDescription;
            }
            dataItemDescription = GGItemHelper.InternalGetItemDescription(itemType);
            GGItemHelper.s_hashtbl[itemType] = dataItemDescription;
            return dataItemDescription;
        }
        private static DataItemDescription InternalGetItemDescription(Type itemType)
        {
            MemberInfo[] members = itemType.GetMembers(BindingFlags.Instance | BindingFlags.Public);
            int num = 0;
            MemberInfo[] array = members;
            for (int i = 0; i < array.Length; i++)
            {
                MemberInfo memberInfo = array[i];
                if (memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property)
                {
                    num++;
                }
            }
            int num2 = 0;
            int num3 = 0;
            DataItemDescription dataItemDescription = new DataItemDescription(itemType, num);
            MemberInfo[] array2 = members;
            for (int j = 0; j < array2.Length; j++)
            {
                MemberInfo memberInfo2 = array2[j];
                if (memberInfo2.MemberType == MemberTypes.Field || memberInfo2.MemberType == MemberTypes.Property)
                {
                    ItemFieldAttribute[] array3 = (ItemFieldAttribute[])memberInfo2.GetCustomAttributes(TypeList._ItemFieldAttribute, false);
                    ItemFieldAttribute itemFieldAttribute;
                    if (array3.Length == 1)
                    {
                        itemFieldAttribute = array3[0];
                    }
                    else
                    {
                        itemFieldAttribute = new ItemFieldAttribute();
                    }
                    if (!itemFieldAttribute.IgnoreLoad && memberInfo2.MemberType == MemberTypes.Property && !((PropertyInfo)memberInfo2).CanWrite)
                    {
                        itemFieldAttribute.IgnoreLoad = true;
                    }
                    if (!itemFieldAttribute.IgnoreLoad)
                    {
                        Type type = (memberInfo2.MemberType == MemberTypes.Property) ? ((PropertyInfo)memberInfo2).PropertyType : ((FieldInfo)memberInfo2).FieldType;
                        if (GGItemHelper.s_CheckTypeIsDataItemFunc(type))
                        {
                            itemFieldAttribute.IsSubItem = true;
                            itemFieldAttribute.IgnoreLoad = true;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(itemFieldAttribute.DbFieldName))
                            {
                                itemFieldAttribute.DbFieldName = memberInfo2.Name;
                            }
                            if (!itemFieldAttribute.IgnoreLoad && GGItemHelper.IsEnumerableType(type))
                            {
                                itemFieldAttribute.IgnoreLoad = true;
                            }
                        }
                    }
                    if (!itemFieldAttribute.IgnoreLoad)
                    {
                        num2++;
                        if (!itemFieldAttribute.OnlyLoadAll)
                        {
                            num3++;
                        }
                    }
                    DataItemFieldInfo value = new DataItemFieldInfo(itemFieldAttribute, memberInfo2);
                    dataItemDescription.Dict[memberInfo2.Name] = value;
                }
            }
            dataItemDescription.SetTwoCount(num3, num2);
            return dataItemDescription;
        }
        /// <summary>
        /// 从一个XML文件中加载实体列表。
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="xmlPath">要加载的xml文件路径</param>
        /// <returns>实体列表</returns>
        public static List<T> LoadItemsFromXmlFile<T>(string xmlPath) where T : class, new()
        {
            if (!File.Exists(xmlPath))
            {
                return null;
            }
            DataSet dataSet = new DataSet();
            dataSet.ReadXml(xmlPath);
            if (dataSet.Tables.Count == 0)
            {
                return null;
            }
            return GGItemHelper.LoadItemsFromDataRows<T>(dataSet.Tables[0].Select());
        }
        /// <summary>
        /// 从一个数据表中加载实体列表。
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="table">要加载的数据表</param>
        /// <returns>实体列表</returns>
        public static List<T> LoadItemsFromDataTable<T>(DataTable table) where T : class, new()
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            return GGItemHelper.LoadItemsFromDataRows<T>(table.Select());
        }
        /// <summary>
        /// 从一个DataRow数组中加载实体列表。
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="rows">要加载的数据行数组</param>
        /// <returns>实体列表</returns>
        public static List<T> LoadItemsFromDataRows<T>(DataRow[] rows) where T : class, new()
        {
            if (rows == null)
            {
                throw new ArgumentNullException("rows");
            }
            if (rows.Length == 0)
            {
                return new List<T>();
            }
            List<T> list = new List<T>(rows.Length);
            MyDataAdapter myDataAdapter = new MyDataAdapter(rows[0]);
            for (int i = 0; i < rows.Length; i++)
            {
                DataRow currentRow = rows[i];
                myDataAdapter.SetCurrentRow(currentRow);
                T t = Activator.CreateInstance(typeof(T)) as T;
                GGItemHelper.LoadItemValuesFormDbRow(myDataAdapter, t, true, false);
                list.Add(t);
            }
            return list;
        }
        /// <summary>
        /// 尝试从一个MyDataAdapter中加载实休对象的成员（一次加载一行信息）
        /// </summary>
        /// <param name="row">MyDataAdapter对象</param>
        /// <param name="item">实休对象的实例</param>
        /// <param name="loadAllField">是否要加载全部成员，可由ItemFieldAttribute.OnlyLoadAll控制</param>
        /// <param name="checkSucessCount">加载完成后，是否要检查成功的数量，确保每次的加载都是成功的</param>
        /// <returns>返回成功加载的成员数量</returns>
        public static int LoadItemValuesFormDbRow(MyDataAdapter row, object item, bool loadAllField, bool checkSucessCount)
        {
            if (row == null)
            {
                throw new ArgumentNullException("row");
            }
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (!(row.UserData is GGItemHelper.LatestLoadDataItemInfo))
            {
                row.UserData = new GGItemHelper.LatestLoadDataItemInfo
                {
                    ColumnNames = row.GetColumnNames(true),
                    ItemDescription = GGItemHelper.GetItemDescription(item.GetType())
                };
            }
            return GGItemHelper.Internal_LoadItemValuesFormDbRow(row, item, loadAllField, checkSucessCount, null);
        }
        private static int Internal_LoadItemValuesFormDbRow(MyDataAdapter row, object item, bool loadAllField, bool checkSucessCount, string prefix)
        {
            Type type = item.GetType();
            GGItemHelper.LatestLoadDataItemInfo latestLoadDataItemInfo = (GGItemHelper.LatestLoadDataItemInfo)row.UserData;
            DataItemDescription itemDescription = latestLoadDataItemInfo.ItemDescription;
            if (latestLoadDataItemInfo.ItemDescription.ItemType != type)
            {
                latestLoadDataItemInfo.ItemDescription = GGItemHelper.GetItemDescription(type);
            }
            int num = 0;
            string text = string.Empty;
            foreach (KeyValuePair<string, DataItemFieldInfo> current in latestLoadDataItemInfo.ItemDescription.Dict)
            {
                if (current.Value.MemberAttr.IgnoreLoad)
                {
                    if (current.Value.MemberAttr.IsSubItem)
                    {
                        object obj = Activator.CreateInstance(current.Value.MemberType);
                        current.Value.SetValue(item, obj);
                        string prefix2 = prefix + current.Value.MemberAttr.DbFieldName;
                        GGItemHelper.Internal_LoadItemValuesFormDbRow(row, obj, loadAllField, checkSucessCount, prefix2);
                        latestLoadDataItemInfo.ItemDescription = itemDescription;
                    }
                }
                else
                {
                    if (loadAllField || !current.Value.MemberAttr.OnlyLoadAll)
                    {
                        if (GGItemHelper.TrySetDataItemMemberValue(row, item, current.Value, prefix))
                        {
                            num++;
                        }
                        else
                        {
                            if (checkSucessCount)
                            {
                                text = text + current.Key + ";";
                            }
                        }
                    }
                }
            }
            if (checkSucessCount)
            {
                if (loadAllField)
                {
                    if (num != latestLoadDataItemInfo.ItemDescription.LoadALLValues_ExpectSuccessCount)
                    {
                        throw new LoadMemberFaildException(type, text);
                    }
                }
                else
                {
                    if (num != latestLoadDataItemInfo.ItemDescription.LoadPartialValues_ExpectSuccessCount)
                    {
                        throw new LoadMemberFaildException(type, text);
                    }
                }
            }
            return num;
        }
        private static bool TrySetDataItemMemberValue(MyDataAdapter row, object item, DataItemFieldInfo info, string prefix)
        {
            string[] columnNames = ((GGItemHelper.LatestLoadDataItemInfo)row.UserData).ColumnNames;
            string text = prefix + info.MemberAttr.DbFieldName;
            if (Array.IndexOf<string>(columnNames, text.ToUpper()) < 0)
            {
                return false;
            }
            object obj = row[text];
            return DBNull.Value.Equals(obj) || GGItemHelper.SafeSetMemberValue(item, info, obj);
        }
        internal static bool SafeSetMemberValue(object item, DataItemFieldInfo info, object val)
        {
            bool result;
            try
            {
                Type realType = GGItemHelper.GetRealType(info.MemberType);
                if (realType.IsEnum)
                {
                    info.SetValue(item, Convert.ToInt32(val));
                }
                else
                {
                    if (val.GetType() == realType)
                    {
                        info.SetValue(item, val);
                    }
                    else
                    {
                        info.SetValue(item, Convert.ChangeType(val, realType));
                    }
                }
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 将一个实体的所有字符串成员的值为null的成员设置为string.Empty，注意：只设置顶层成员。
        /// </summary>
        /// <param name="item">实体对象</param>
        public static void SetItemMemberStringToEmpty(object item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            DataItemDescription itemDescription = GGItemHelper.GetItemDescription(item.GetType());
            foreach (KeyValuePair<string, DataItemFieldInfo> current in itemDescription.Dict)
            {
                if (!current.Value.MemberAttr.IgnoreLoad && current.Value.MemberType == TypeList._string && current.Value.GetValue(item) == null)
                {
                    current.Value.SetValue(item, string.Empty);
                }
            }
        }
        /// <summary>
        /// 尝试从当前请求的FORM中加载实体成员
        /// </summary>
        /// <param name="item">实体对象，它的成员值将会从FROM中加载</param>		
        public static void TryGetItemValuesFromRequestForm(object item)
        {
            GGItemHelper.TryGetItemValuesFromRequestForm(item, string.Empty);
        }
        /// <summary>
        /// 尝试从当前请求的FORM中加载实体成员
        /// </summary>
        /// <param name="item">实体对象，它的成员值将会从FROM中加载</param>
        /// <param name="namePrefix">实体成员名称的“前缀”，可用于在回传时区分不同的组。</param>
        public static void TryGetItemValuesFromRequestForm(object item, string namePrefix)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            DataItemDescription itemDescription = GGItemHelper.GetItemDescription(item.GetType());
            NameValueCollection form = HttpContext.Current.Request.Form;
            foreach (KeyValuePair<string, DataItemFieldInfo> current in itemDescription.Dict)
            {
                if (!current.Value.MemberAttr.IgnoreLoad)
                {
                    string text = GGItemHelper.s_GetValueFromCollectionFunc(form, current.Value.MemberAttr.KeyName ?? (namePrefix + current.Key));
                    if (text != null)
                    {
                        if (GGItemHelper.AutoTrimStringOnLoad)
                        {
                            text = text.Trim();
                        }
                        try
                        {
                            current.Value.SetValue(item, StringHelper.ConvertString(text, current.Value.MemberType));
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 尝试从指定请求的QueryString或FORM中加载实体成员
        /// </summary>
        /// <param name="request">请求对象</param>
        /// <param name="item">实体对象，它的成员值将会从FROM中加载</param>
        /// <param name="ifNotFoundThrowException">如果没有找到匹配项，是否需要抛出异常</param>
        public static void TryGetItemValuesFromRequest(HttpRequest request, object item, bool ifNotFoundThrowException)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            DataItemDescription itemDescription = GGItemHelper.GetItemDescription(item.GetType());
            foreach (KeyValuePair<string, DataItemFieldInfo> current in itemDescription.Dict)
            {
                if (!current.Value.MemberAttr.IgnoreLoad)
                {
                    string text = current.Value.MemberAttr.KeyName ?? current.Key;
                    string text2 = GGItemHelper.TryGetValueFromRequest(request, text, false);
                    if (text2 == null)
                    {
                        if (current.Value.MemberType.IsValueType && current.Value.MemberAttr.AllowNotFoundOnLoad)
                        {
                            continue;
                        }
                        if (current.Value.MemberType == TypeList._string && current.Value.MemberAttr.SetEmptyIfNotFoundOnLoad)
                        {
                            text2 = string.Empty;
                        }
                        else
                        {
                            if (current.Value.MemberType == TypeList._bool)
                            {
                                text2 = string.Empty;
                            }
                            else
                            {
                                if (ifNotFoundThrowException)
                                {
                                    throw new InvalidOperationException(string.Format(SR.KeyNotFoundInRequest, text));
                                }
                                continue;
                            }
                        }
                    }
                    try
                    {
                        current.Value.SetValue(item, StringHelper.ConvertString(text2, current.Value.MemberType));
                    }
                    catch
                    {
                    }
                }
            }
        }
        /// <summary>
        /// 尝试从指定请求的QueryString或FORM中加载指定的参数值
        /// </summary>
        /// <param name="request">请求对象</param>
        /// <param name="key">要查找的键名</param>
        /// <param name="ifNotFoundThrowException">如果没有找到匹配项，是否需要抛出异常</param>
        /// <returns>查找到的结果</returns>
        internal static string TryGetValueFromRequest(HttpRequest request, string key, bool ifNotFoundThrowException)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            string text = GGUrlHelper.GetStringFromQueryStirng(request.QueryString, key, null);
            if (text == null)
            {
                text = GGItemHelper.s_GetValueFromCollectionFunc(request.Form, key);
            }
            if (text == null)
            {
                if (ifNotFoundThrowException)
                {
                    throw new InvalidOperationException(string.Format(SR.KeyNotFoundInRequest, key));
                }
            }
            else
            {
                if (GGItemHelper.AutoTrimStringOnLoad)
                {
                    text = text.Trim();
                }
            }
            return text;
        }
    }
}
