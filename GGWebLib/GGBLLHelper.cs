using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
namespace GGWebLib
{
    /// <summary>
    /// 用于方便使用GGDbContext的一些反射操作的辅助工具类
    /// </summary>
    public static class GGBLLHelper
    {
        private static Hashtable s_spParamTypeCache = Hashtable.Synchronized(new Hashtable(3072));
        /// <summary>
        /// <para>根据一个数据实体实例及其它的“补充参数”，自动设置GGDbContext的“当前命令”的参数</para>
        /// <para>注意：操作前，会清除命令的所有参数。</para>
        /// </summary>
        /// <param name="db">GGDbContext实例</param>
        /// <param name="item">数据实体实例，可为null</param>
        /// <param name="objArray">其它的“补充参数”，
        /// 每当处理到一个参数，只要数据实体中没有对应的成员或没有传入数据实体对象，
        /// 所需的参数就从这个数组中取得。</param>
        /// <returns>返回已成功设置的参数数量</returns>
        public static int AutoSetSpParameters(GGDbContext db, object item, params object[] objArray)
        {
            if (db == null || db.Connection == null || db.CurrentCommand == null)
            {
                throw new ArgumentNullException("db");
            }
            if (item != null)
            {
                GGItemHelper.EnsureIsDataItemType(item.GetType());
            }
            if (db.CurrentCommand.Parameters.Count > 0)
            {
                db.CurrentCommand.Parameters.Clear();
            }
            DbParameter[] spParameters = SpParameterCache.GetSpParameters(db);
            if (spParameters.Length == 0)
            {
                return 0;
            }
            int num = 0;
            int num2 = 0;
            int num3 = string.IsNullOrEmpty(db.ParamNamePrefix) ? -1 : spParameters[0].ParameterName.IndexOf(db.ParamNamePrefix);
            DataItemDescription dataItemDescription = null;
            if (item != null)
            {
                dataItemDescription = GGItemHelper.GetItemDescription(item.GetType());
            }
            DbParameter[] array = spParameters;
            for (int i = 0; i < array.Length; i++)
            {
                DbParameter dbParameter = array[i];
                if (dbParameter.Direction == ParameterDirection.Input || dbParameter.Direction == ParameterDirection.InputOutput)
                {
                    if (dataItemDescription != null)
                    {
                        string name = dbParameter.ParameterName.Substring(num3 + 1);
                        DataItemFieldInfo dataItemFieldInfo = null;
                        if (dataItemDescription.TryGetValue(name, out dataItemFieldInfo))
                        {
                            dbParameter.Value = dataItemFieldInfo.GetValue(item);
                        }
                        else
                        {
                            if (num2 < objArray.Length)
                            {
                                dbParameter.Value = objArray[num2++];
                            }
                        }
                    }
                    else
                    {
                        if (num2 < objArray.Length)
                        {
                            dbParameter.Value = objArray[num2++];
                        }
                    }
                }
                db.CurrentCommand.Parameters.Add(dbParameter);
                num++;
            }
            db.SpOutParamDescription = null;
            if (item != null && db.AutoGetSpOutputValuesForThisInstance)
            {
                SpOutParamDescription spOutParamDescription = new SpOutParamDescription();
                DbParameter[] array2 = spParameters;
                for (int j = 0; j < array2.Length; j++)
                {
                    DbParameter dbParameter2 = array2[j];
                    if (dbParameter2.Direction == ParameterDirection.Output || dbParameter2.Direction == ParameterDirection.InputOutput)
                    {
                        string name2 = dbParameter2.ParameterName.Substring(num3 + 1);
                        spOutParamDescription.AddParamName(name2);
                    }
                }
                if (spOutParamDescription.NamesList != null && spOutParamDescription.NamesList.Count > 0)
                {
                    spOutParamDescription.DataItem = item;
                    db.SpOutParamDescription = spOutParamDescription;
                }
            }
            return num;
        }
        /// <summary>
        /// 获取所有存储过程返回值。
        /// </summary>
        /// <param name="db">GGDbContext对象</param>
        internal static void GetSpOutputValues(GGDbContext db)
        {
            if (db == null || db.SpOutParamDescription == null)
            {
                return;
            }
            SpOutParamDescription spOutParamDescription = db.SpOutParamDescription;
            if (spOutParamDescription.DataItem == null)
            {
                throw new InvalidOperationException(SR.IE_DateItemIsNull);
            }
            DataItemDescription itemDescription = GGItemHelper.GetItemDescription(spOutParamDescription.DataItem.GetType());
            foreach (string current in spOutParamDescription.NamesList)
            {
                DataItemFieldInfo info = null;
                if (!itemDescription.TryGetValue(current, out info))
                {
                    throw new ArgumentOutOfRangeException(string.Format(SR.SetSpOutError_MemberNotFound, current));
                }
                DbParameter parameterFromCommand = GGBLLHelper.GetParameterFromCommand(db, current);
                if (parameterFromCommand == null)
                {
                    throw new InvalidOperationException(string.Format(SR.SetSpOutError_CmdParamNotFound, current));
                }
                if (!GGItemHelper.SafeSetMemberValue(spOutParamDescription.DataItem, info, parameterFromCommand.Value))
                {
                    throw new InvalidOperationException(string.Format(SR.SetSpOutError_CanNotSet, current));
                }
            }
        }
        /// <summary>
        /// <para>在一个命令的参数集合中，根据（实体对象的属性）名称，获取对应的命令参数。</para>
        /// <para>注意：此方法在查找时，已补入“命令前缀”部分。</para>
        /// <para>如果没有找到合适的参数，则抛出异常。</para>
        /// </summary>
        /// <param name="db">GGDbContext对象</param>
        /// <param name="parameterName">参数名</param>
        /// <returns>存在的参数对象</returns>
        public static DbParameter GetParameterFromCommand(GGDbContext db, string parameterName)
        {
            if (db == null || db.CurrentCommand == null)
            {
                throw new ArgumentNullException("db");
            }
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentNullException("parameterName");
            }
            parameterName = db.ParamNamePrefix + parameterName;
            DbParameterCollection parameters = db.CurrentCommand.Parameters;
            int num = parameters.IndexOf(parameterName);
            if (num >= 0)
            {
                return parameters[num];
            }
            foreach (DbParameter dbParameter in parameters)
            {
                if (dbParameter.ParameterName.EndsWith(parameterName))
                {
                    return dbParameter;
                }
            }
            throw new ArgumentOutOfRangeException(parameterName);
        }
        /// <summary>
        /// （用默认的连接字符串）直接调用一个存储过程 ExecuteNonQuery()
        /// </summary>
        /// <param name="spName">存储过程名称</param>
        /// <param name="item">参数实体实例，可为null</param>
        /// <param name="objArray">其它的“补充参数”，
        /// 每当处理到一个参数，只要参数实体中没有对应的成员或没有传入参数实体对象，
        /// 所需的参数就从这个数组中取得。</param>
        /// <returns>返回“受影响的记录数”。</returns>
        public static int CallSpExecuteNonQuery(string spName, object item, params object[] objArray)
        {
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            int result;
            using (GGDbContext GGDbContext = new GGDbContext(false))
            {
                result = GGBLLHelper.CallSpExecuteNonQuery(GGDbContext, spName, item, objArray);
            }
            return result;
        }
        /// <summary>
        /// 直接调用一个存储过程 ExecuteNonQuery()
        /// </summary>
        /// <param name="db">GGDbContext实例</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="item">参数实体实例，可为null</param>
        /// <param name="objArray">其它的“补充参数”，
        /// 每当处理到一个参数，只要参数实体中没有对应的成员或没有传入参数实体对象，
        /// 所需的参数就从这个数组中取得。</param>
        /// <returns>返回“受影响的记录数”。</returns>
        public static int CallSpExecuteNonQuery(GGDbContext db, string spName, object item, params object[] objArray)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            db.CreateCommand(spName);
            GGBLLHelper.AutoSetSpParameters(db, item, objArray);
            return db.ExecuteNonQuery();
        }
        /// <summary>
        /// （用默认的连接字符串）直接调用一个存储过程 ExecuteScalar()
        /// </summary>
        /// <param name="spName">存储过程名称</param>
        /// <param name="item">参数实体实例，可为null</param>
        /// <param name="objArray">其它的“补充参数”，
        /// 每当处理到一个参数，只要参数实体中没有对应的成员或没有传入参数实体对象，
        /// 所需的参数就从这个数组中取得。</param>
        /// <returns>从存储过程中返回的结果</returns>
        public static object CallSpExecuteScalar(string spName, object item, params object[] objArray)
        {
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            object result;
            using (GGDbContext GGDbContext = new GGDbContext(false))
            {
                result = GGBLLHelper.CallSpExecuteScalar(GGDbContext, spName, item, objArray);
            }
            return result;
        }
        /// <summary>
        /// 直接调用一个存储过程 ExecuteScalar()
        /// </summary>
        /// <param name="db">GGDbContext实例</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="item">参数实体实例，可为null</param>
        /// <param name="objArray">其它的“补充参数”，
        /// 每当处理到一个参数，只要参数实体中没有对应的成员或没有传入参数实体对象，
        /// 所需的参数就从这个数组中取得。</param>
        /// <returns>从存储过程中返回的结果</returns>
        public static object CallSpExecuteScalar(GGDbContext db, string spName, object item, params object[] objArray)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            db.CreateCommand(spName);
            GGBLLHelper.AutoSetSpParameters(db, item, objArray);
            return db.ExecuteScalar();
        }
        /// <summary>
        /// （用默认的连接字符串）直接调用一个存储过程返回一个业务实体对象
        /// </summary>
        /// <typeparam name="T">结果实体类型</typeparam>
        /// <param name="spName">存储过程名称</param>
        /// <param name="item">参数实体实例，可为null</param>
        /// <param name="objArray">其它的“补充参数”，
        /// 每当处理到一个参数，只要参数实体中没有对应的成员或没有传入参数实体对象，
        /// 所需的参数就从这个数组中取得。</param>
        /// <returns>将查询语句的结果转成指定的结果实体对象</returns>
        public static T CallSpGetDataItem<T>(string spName, object item, params object[] objArray) where T : class, new()
        {
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            T result;
            using (GGDbContext GGDbContext = new GGDbContext(false))
            {
                result = GGBLLHelper.CallSpGetDataItem<T>(GGDbContext, spName, item, objArray);
            }
            return result;
        }
        /// <summary>
        /// 直接调用一个存储过程返回一个业务实体对象
        /// </summary>
        /// <typeparam name="T">结果实体类型</typeparam>
        /// <param name="db">GGDbContext实例</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="item">参数实体实例，可为null</param>
        /// <param name="objArray">其它的“补充参数”，
        /// 每当处理到一个参数，只要参数实体中没有对应的成员或没有传入参数实体对象，
        /// 所需的参数就从这个数组中取得。</param>
        /// <returns>将查询语句的结果转成指定的结果实体对象</returns>
        public static T CallSpGetDataItem<T>(GGDbContext db, string spName, object item, params object[] objArray) where T : class, new()
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            db.CreateCommand(spName);
            GGBLLHelper.AutoSetSpParameters(db, item, objArray);
            return db.ExecuteSelectCommandGetBllObject<T>();
        }
        /// <summary>
        /// （用默认的连接字符串）直接调用一个存储过程返回一个业务实体对象列表
        /// </summary>
        /// <typeparam name="T">结果实体类型</typeparam>
        /// <param name="spName">存储过程名称</param>
        /// <param name="item">参数实体实例，可为null</param>
        /// <param name="objArray">其它的“补充参数”，
        /// 每当处理到一个参数，只要参数实体中没有对应的成员或没有传入参数实体对象，
        /// 所需的参数就从这个数组中取得。</param>
        /// <returns>将查询语句的结果转成指定的结果实体对象列表</returns>
        public static List<T> CallSpGetDataItemList<T>(string spName, object item, params object[] objArray) where T : class, new()
        {
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            List<T> result;
            using (GGDbContext GGDbContext = new GGDbContext(false))
            {
                result = GGBLLHelper.CallSpGetDataItemList<T>(GGDbContext, spName, item, objArray);
            }
            return result;
        }
        /// <summary>
        /// 直接调用一个存储过程返回一个业务实体对象列表
        /// </summary>
        /// <typeparam name="T">结果实体类型</typeparam>
        /// <param name="db">GGDbContext实例</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="item">参数实体实例，可为null</param>
        /// <param name="objArray">其它的“补充参数”，
        /// 每当处理到一个参数，只要参数实体中没有对应的成员或没有传入参数实体对象，
        /// 所需的参数就从这个数组中取得。</param>
        /// <returns>将查询语句的结果转成指定的结果实体对象列表</returns>
        public static List<T> CallSpGetDataItemList<T>(GGDbContext db, string spName, object item, params object[] objArray) where T : class, new()
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            db.CreateCommand(spName);
            GGBLLHelper.AutoSetSpParameters(db, item, objArray);
            return db.ExecuteSelectCommandToList<T>();
        }
        /// <summary>
        /// <para>直接调用一个存储过程返回一个业务实体对象列表的一个分页</para>
        /// <para> 说明：存储过程最后的三个参数一定要是用于分页的参数，</para>
        /// <para> 且参数名为(前缀部分请自行添加)：in PageIndex int, in PageSize int, out TotalRecords int</para>
        /// <para> 注意：pageIndex从零开始计数。</para>
        /// </summary>
        /// <typeparam name="T">结果实体类型</typeparam>
        /// <param name="spName">存储过程名称</param>
        /// <param name="pagingInfo">分页参数信息（会包含一些输出参数）</param>
        /// <param name="objArray">其它的“补充参数”</param>
        /// <returns>将查询语句的结果转成指定的结果实体对象列表</returns>
        public static List<T> CallSpGetDataItemListPaged<T>(string spName, PagingInfo pagingInfo, params object[] objArray) where T : class, new()
        {
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            List<T> result;
            using (GGDbContext GGDbContext = new GGDbContext(false))
            {
                result = GGBLLHelper.CallSpGetDataItemListPaged<T>(GGDbContext, spName, pagingInfo, objArray);
            }
            return result;
        }
        /// <summary>
        /// <para>直接调用一个存储过程返回一个业务实体对象列表的一个分页</para>
        /// <para> 说明：存储过程最后的三个参数一定要是用于分页的参数，</para>
        /// <para> 且参数名为(前缀部分请自行添加)：in PageIndex int, in PageSize int, out TotalRecords int</para>
        /// <para> 注意：pageIndex从零开始计数。</para>
        /// </summary>
        /// <typeparam name="T">结果实体类型</typeparam>
        /// <param name="db">GGDbContext实例</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="pagingInfo">分页参数信息（会包含一些输出参数）</param>
        /// <param name="objArray">其它的“补充参数”</param>
        /// <returns>将查询语句的结果转成指定的结果实体对象列表</returns>
        public static List<T> CallSpGetDataItemListPaged<T>(GGDbContext db, string spName, PagingInfo pagingInfo, params object[] objArray) where T : class, new()
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            db.CreateCommand(spName);
            GGBLLHelper.AutoSetSpParameters(db, null, objArray);
            DbParameter parameterFromCommand = GGBLLHelper.GetParameterFromCommand(db, GGDbContext.STR_PageIndex);
            DbParameter parameterFromCommand2 = GGBLLHelper.GetParameterFromCommand(db, GGDbContext.STR_PageSize);
            DbParameter parameterFromCommand3 = GGBLLHelper.GetParameterFromCommand(db, GGDbContext.STR_TotalRecords);
            parameterFromCommand.Value = pagingInfo.PageIndex;
            parameterFromCommand2.Value = pagingInfo.PageSize;
            parameterFromCommand3.Value = 0;
            List<T> list = db.ExecuteSelectCommandToList<T>();
            pagingInfo.RecCount = (int)parameterFromCommand3.Value;
            if (list.Count == 0 && pagingInfo.PageIndex > 0 && pagingInfo.RecCount > 0)
            {
                pagingInfo.PageIndex = 0;
                parameterFromCommand.Value = 0;
                list = db.ExecuteSelectCommandToList<T>();
                parameterFromCommand3 = GGBLLHelper.GetParameterFromCommand(db, GGDbContext.STR_TotalRecords);
                pagingInfo.RecCount = (int)parameterFromCommand3.Value;
            }
            return list;
        }
        /// <summary>
        /// 根据一个“匿名”类型对象自动填充一个命令对象的参数集合
        /// </summary>
        /// <param name="command">命令对象</param>
        /// <param name="paramsObj">包含所有参数值的“复杂对象”</param>
        /// <param name="cmdParamNamePrefix">命令参数名称的前缀部分</param>
        /// <returns>返回已成功设置的参数数量</returns>
        public static int AutoSetSpParameters(DbCommand command, object paramsObj, string cmdParamNamePrefix)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (paramsObj == null)
            {
                throw new ArgumentNullException("paramsObj");
            }
            if (cmdParamNamePrefix == null)
            {
                cmdParamNamePrefix = string.Empty;
            }
            int result;
            try
            {
                result = GGBLLHelper.InternalAutoSetSpParameters(command, paramsObj, cmdParamNamePrefix);
            }
            catch (TargetException)
            {
                string key = paramsObj.GetType().ToString();
                GGBLLHelper.s_spParamTypeCache.Remove(key);
                result = GGBLLHelper.InternalAutoSetSpParameters(command, paramsObj, cmdParamNamePrefix);
            }
            return result;
        }
        /// <summary>
        /// 为存储过程命令设置所有的参数
        /// </summary>
        /// <param name="command">命令对象</param>
        /// <param name="paramsObj">包含所有参数的“匿名”类型对象</param>
        /// <param name="cmdParamNamePrefix">命令参数名称的前缀部分</param>
        /// <returns>返回已成功设置的参数数量</returns>
        private static int InternalAutoSetSpParameters(DbCommand command, object paramsObj, string cmdParamNamePrefix)
        {
            if (command == null || string.IsNullOrEmpty(command.CommandText))
            {
                throw new ArgumentNullException("command");
            }
            if (command.Connection == null)
            {
                throw new ArgumentNullException("command.Connection");
            }
            if (paramsObj == null)
            {
                throw new ArgumentNullException("paramsObj");
            }
            DbParameter[] spParameters = SpParameterCache.GetSpParameters(command.Connection, command.CommandText);
            if (spParameters.Length == 0)
            {
                return 0;
            }
            if (command.Parameters.Count > 0)
            {
                command.Parameters.Clear();
            }
            string key = paramsObj.GetType().ToString();
            Dictionary<string, MemberInfo> dictionary = GGBLLHelper.s_spParamTypeCache[key] as Dictionary<string, MemberInfo>;
            if (dictionary == null)
            {
                PropertyInfo[] properties = paramsObj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                FieldInfo[] fields = paramsObj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
                dictionary = new Dictionary<string, MemberInfo>(properties.Length + fields.Length, StringComparer.OrdinalIgnoreCase);
                PropertyInfo[] array = properties;
                for (int i = 0; i < array.Length; i++)
                {
                    PropertyInfo propertyInfo = array[i];
                    dictionary.Add(propertyInfo.Name, propertyInfo);
                }
                FieldInfo[] array2 = fields;
                for (int j = 0; j < array2.Length; j++)
                {
                    FieldInfo fieldInfo = array2[j];
                    dictionary.Add(fieldInfo.Name, fieldInfo);
                }
                GGBLLHelper.s_spParamTypeCache[key] = dictionary;
            }
            int num = 0;
            int num2 = string.IsNullOrEmpty(cmdParamNamePrefix) ? -1 : spParameters[0].ParameterName.IndexOf(cmdParamNamePrefix);
            DbParameter[] array3 = spParameters;
            for (int k = 0; k < array3.Length; k++)
            {
                DbParameter dbParameter = array3[k];
                if (dbParameter.Direction == ParameterDirection.Input || dbParameter.Direction == ParameterDirection.InputOutput)
                {
                    string key2 = (num2 < 0) ? dbParameter.ParameterName : dbParameter.ParameterName.Substring(num2 + 1);
                    MemberInfo memberInfo;
                    if (dictionary.TryGetValue(key2, out memberInfo))
                    {
                        if (memberInfo is PropertyInfo)
                        {
                            dbParameter.Value = ((PropertyInfo)memberInfo).GetValue(paramsObj, null);
                        }
                        else
                        {
                            dbParameter.Value = ((FieldInfo)memberInfo).GetValue(paramsObj);
                        }
                        num++;
                    }
                }
                command.Parameters.Add(dbParameter);
            }
            return num;
        }
        /// <summary>
        /// <para>（用默认的连接字符串）直接调用一个存储过程返回一个业务实体对象列表的一个分页，所有的参数可以放在一个“匿名”类型的对象中</para>
        /// <para>注意：存储过程必须包含三个参数（前缀部分请自行添加）：in PageIndex int, in PageSize, out TotalRecords int</para>
        /// <para>注意：pageIndex从零开始计数。</para>
        /// </summary>
        /// <typeparam name="T">结果实体类型</typeparam>
        /// <param name="spName">存储过程名称</param>
        /// <param name="pagingInfo">分页参数信息（会包含一些输出参数）</param>
        /// <param name="paramsObj">包含所有参数的“匿名”类型对象</param>
        /// <returns>将查询语句的结果转成指定的结果实体对象列表</returns>
        public static List<T> CallSpGetDataItemListPaged2<T>(string spName, PagingInfo pagingInfo, object paramsObj) where T : class, new()
        {
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            List<T> result;
            using (GGDbContext GGDbContext = new GGDbContext(false))
            {
                result = GGBLLHelper.CallSpGetDataItemListPaged2<T>(GGDbContext, spName, pagingInfo, paramsObj);
            }
            return result;
        }
        /// <summary>
        /// <para>直接调用一个存储过程返回一个业务实体对象列表的一个分页，所有的参数可以放在一个“匿名”类型的对象中</para>
        /// <para>注意：存储过程必须包含三个参数（前缀部分请自行添加）：in PageIndex int, in PageSize, out TotalRecords int</para>
        /// <para>注意：pageIndex从零开始计数。</para>
        /// </summary>
        /// <typeparam name="T">结果实体类型</typeparam>
        /// <param name="db">GGDbContext实例</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="pagingInfo">分页参数信息（会包含一些输出参数）</param>
        /// <param name="paramsObj">包含所有参数的“匿名”类型对象</param>
        /// <returns>将查询语句的结果转成指定的结果实体对象列表</returns>
        public static List<T> CallSpGetDataItemListPaged2<T>(GGDbContext db, string spName, PagingInfo pagingInfo, object paramsObj) where T : class, new()
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            db.CreateCommand(spName);
            GGBLLHelper.AutoSetSpParameters(db.CurrentCommand, paramsObj, db.ParamNamePrefix);
            DbParameter parameterFromCommand = GGBLLHelper.GetParameterFromCommand(db, GGDbContext.STR_PageIndex);
            DbParameter parameterFromCommand2 = GGBLLHelper.GetParameterFromCommand(db, GGDbContext.STR_PageSize);
            DbParameter parameterFromCommand3 = GGBLLHelper.GetParameterFromCommand(db, GGDbContext.STR_TotalRecords);
            parameterFromCommand.Value = pagingInfo.PageIndex;
            parameterFromCommand2.Value = pagingInfo.PageSize;
            parameterFromCommand3.Value = 0;
            List<T> list = db.ExecuteSelectCommandToList<T>();
            pagingInfo.RecCount = (int)parameterFromCommand3.Value;
            if (list.Count == 0 && pagingInfo.PageIndex > 0 && pagingInfo.RecCount > 0)
            {
                pagingInfo.PageIndex = 0;
                parameterFromCommand.Value = 0;
                list = db.ExecuteSelectCommandToList<T>();
                parameterFromCommand3 = GGBLLHelper.GetParameterFromCommand(db, GGDbContext.STR_TotalRecords);
                pagingInfo.RecCount = (int)parameterFromCommand3.Value;
            }
            return list;
        }
    }
}
