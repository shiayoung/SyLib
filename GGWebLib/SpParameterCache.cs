using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Reflection;
namespace GGWebLib
{
    /// <summary>
    /// 存储过程的参数缓存类
    /// </summary>
    public static class SpParameterCache
    {
        private static Hashtable s_paramCache = Hashtable.Synchronized(new Hashtable(3072));
        /// <summary>
        /// 用指定的连接信息，获取一个存储过程的参数数组。（不会使用参数中连接）
        /// </summary>
        /// <param name="dbConn">DbConnection对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <returns></returns>
        public static DbParameter[] DiscoverSpParameters(DbConnection dbConn, string spName)
        {
            if (dbConn == null)
            {
                throw new ArgumentNullException("dbConn");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            DbCommand dbCommand = null;
            using (DbConnection dbConnection = (DbConnection)((ICloneable)dbConn).Clone())
            {
                dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = spName;
                dbCommand.CommandType = CommandType.StoredProcedure;
                dbConnection.Open();
                if (dbCommand is SqlCommand)
                {
                    SqlCommandBuilder.DeriveParameters((SqlCommand)dbCommand);
                }
                else
                {
                    if (dbCommand is OleDbCommand)
                    {
                        OleDbCommandBuilder.DeriveParameters((OleDbCommand)dbCommand);
                    }
                    else
                    {
                        if (dbCommand is OdbcCommand)
                        {
                            OdbcCommandBuilder.DeriveParameters((OdbcCommand)dbCommand);
                        }
                        else
                        {
                            if (!(dbCommand.GetType().ToString() == "MySql.Data.MySqlClient.MySqlCommand"))
                            {
                                throw new NotImplementedException();
                            }
                            DbProviderFactory mySqlClientFactoryFromCache = GGDbContext.GetMySqlClientFactoryFromCache();
                            DbCommandBuilder dbCommandBuilder = mySqlClientFactoryFromCache.CreateCommandBuilder();
                            dbCommandBuilder.GetType().InvokeMember("DeriveParameters", BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, null, new object[]
							{
								dbCommand
							});
                        }
                    }
                }
                dbConnection.Close();
            }
            if (dbCommand.Parameters.Count > 0 && dbCommand.Parameters[0].Direction == ParameterDirection.ReturnValue)
            {
                dbCommand.Parameters.RemoveAt(0);
            }
            DbParameter[] array = new DbParameter[dbCommand.Parameters.Count];
            dbCommand.Parameters.CopyTo(array, 0);
            return array;
        }
        /// <summary>
        /// 清除所有缓存项。
        /// </summary>
        public static void ClearCache()
        {
            SpParameterCache.s_paramCache.Clear();
        }
        private static DbParameter[] CloneParameters(DbParameter[] originalParameters)
        {
            int num = originalParameters.Length;
            DbParameter[] array = new DbParameter[num];
            for (int i = 0; i < num; i++)
            {
                array[i] = (DbParameter)((ICloneable)originalParameters[i]).Clone();
            }
            return array;
        }
        /// <summary>
        /// 根据一个GGDbContext的连接信息，获取“当前命令”的存储过程参数数组
        /// </summary>
        /// <param name="db">GGDbContext对象</param>
        /// <returns>获取“当前命令”的存储过程参数数组</returns>
        public static DbParameter[] GetSpParameters(GGDbContext db)
        {
            if (db == null || db.Connection == null || db.CurrentCommand == null)
            {
                throw new ArgumentNullException("db");
            }
            if (db.CurrentCommand.CommandType != CommandType.StoredProcedure || string.IsNullOrEmpty(db.CurrentCommand.CommandText))
            {
                throw new InvalidOperationException(SR.CommandTextIsNotSP);
            }
            return SpParameterCache.GetSpParameters(db.Connection, db.CurrentCommand.CommandText);
        }
        /// <summary>
        /// 根据一个数据库的连接，获取存储过程的参数数组
        /// </summary>
        /// <param name="dbConn">DbConnection对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <returns>存储过程的参数数组</returns>
        public static DbParameter[] GetSpParameters(DbConnection dbConn, string spName)
        {
            if (dbConn == null)
            {
                throw new ArgumentNullException("dbConn");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            string key = string.Concat(new string[]
			{
				spName,
				"###",
				dbConn.ConnectionString,
				"###",
				dbConn.GetType().ToString()
			});
            DbParameter[] array = SpParameterCache.s_paramCache[key] as DbParameter[];
            if (array == null)
            {
                array = SpParameterCache.DiscoverSpParameters(dbConn, spName);
                SpParameterCache.s_paramCache[key] = array;
            }
            return SpParameterCache.CloneParameters(array);
        }
    }
}
