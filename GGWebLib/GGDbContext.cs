using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web;
namespace GGWebLib
{
    /// <summary>
    /// <para>对 Connection, Transaction, Command 的包装类，支持多种数据库处理，如：MySql, MsSql</para>
    /// <para>每当执行数据库的操作时，如果发生异常，会自动回滚事务，关闭连接。</para>
    /// <para>注意：当给命令添加参数时，不要传入命令名称的前缀部分。</para>
    /// </summary>
    public class GGDbContext : IDisposable
    {
        private sealed class DataBaseConnectionInfo
        {
            public DbProviderFactory Factory;
            public string ProviderName;
            public string ParamNamePrefix;
            public string ConnectionString;
        }
        private static DbProviderFactory s_factory;
        /// <summary>
        /// 默认的连接字符串
        /// </summary>
        private static string s_defaultConnectionString;
        /// <summary>
        /// 默认的命令参数的前缀。
        /// </summary>
        private static string s_defaultParamNamePrefix;
        private static string s_applicationName = "MyWebSite";
        /// <summary>
        /// 用于保存多种数据库的连接信息
        /// </summary>
        private static Dictionary<string, GGDbContext.DataBaseConnectionInfo> s_dbInfo = new Dictionary<string, GGDbContext.DataBaseConnectionInfo>(20);
        /// <summary>
        /// 在从数据库加载实体对象时，是否采用“严格”模式，此时会检查成功的加载数量。默认值：true
        /// </summary>
        public static bool StrictModeLoadItem = true;
        /// <summary>
        /// （默认设置）在调用存储过程完成后，是否自动获取输出的参数值。默认值：false
        /// </summary>
        public static bool AutoGetSpOutputValues;
        private static int s_ResultListCapacity = 50;
        private DbConnection m_conn;
        private DbTransaction m_trans;
        private DbCommand m_command;
        private bool m_autoOpenClose;
        private GGBaseBLL m_ownerBll;
        /// <summary>
        /// 在异常发生时，不关闭连接，也不回滚事务。默认值：false
        /// </summary>
        public bool KeepConnectionOnException;
        /// <summary>
        /// 附加的属性，用于在监测时可以保存一些额外的信息。
        /// </summary>
        public object Tag;
        /// <summary>
        /// 是否发生了异常
        /// </summary>
        private bool m_occurExecption;
        private string m_ParamNamePrefix;
        /// <summary>
        /// 对于当前实例，是否要在调用存储过程完成后，自动获取输出的参数值。默认值：false
        /// </summary>
        public bool AutoGetSpOutputValuesForThisInstance;
        internal SpOutParamDescription SpOutParamDescription;
        /// <summary>
        /// 本次操作时是否忽略所有发生的异常（可以避免在写日志时产生的循环调用）
        /// </summary>
        public bool IgnoreErrorEvent;
        /// <summary>
        /// 字符串: "PageIndex"
        /// </summary>
        public static readonly string STR_PageIndex = "PageIndex";
        /// <summary>
        /// 字符串: "PageSize"
        /// </summary>
        public static readonly string STR_PageSize = "PageSize";
        /// <summary>
        /// 字符串: "TotalRecords"
        /// </summary>
        public static readonly string STR_TotalRecords = "TotalRecords";
        /// <summary>
        /// <para>GGDbContext在执行数据库操作时发生异常时引发的事件，供记录日志</para>
        /// <para>应该在程序初始化时订阅这个事件</para>
        /// </summary>
        public static event GGDbContextExceptionEvent OnGGDbContextExceptionEvent;
        /// <summary>
        /// 每次在执行数据库操作前会触发的事件。可用此事件记录程序执行了哪些操作。
        /// </summary>
        public static event GGDbContextBeforeExecute OnGGDbContextBeforeExecute;
        /// <summary>
        /// 每次在执行数据库操作完成时会触发的事件。
        /// </summary>
        public static event GGDbContextAfterExecute OnGGDbContextAfterExecute;
        /// <summary>
        /// 每次打开数据库连接时会触发的事件
        /// </summary>
        public static event GGDbContextOpenConnection OnGGDbContextOpenConnection;
        /// <summary>
        /// <para>如果在构造方法中没有提供连接字符串，同时也没有提供默认的连接字符串，那么，将会引发这个事件</para>
        /// <para>说明：对于“根据不同的登录用户需要连接不同的数据库“这样的使用场景，就应该注册并处理这个事件。</para>
        /// <para>推荐做法：在登录时，根据用户登录的选择，写一个标记到cookie，然后，在这个事件处理器中读取cookie，设置相应的连接字符串</para>
        /// </summary>
        public static event GGDbContextGetDefaultConnectionString OnGetDefaultConnectionString;
        /// <summary>
        /// 当前应用程序名称，它将显示在GGSQLProfiler中，用于区别不同的程序产生的消息，默认值："GGDemoApp"
        /// </summary>
        public static string ApplicationName
        {
            get
            {
                return GGDbContext.s_applicationName;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    GGDbContext.s_applicationName = value;
                }
            }
        }
        /// <summary>
        /// <para>当从数据库中返回一个实体列表时，为列表的初始化长度是多少。默认值：50;</para>
        /// <para>对于有分页的应用程序，请根据程序的分页大小来合理地设置此参数。</para>
        /// </summary>
        public static int ResultListCapacity
        {
            get
            {
                return GGDbContext.s_ResultListCapacity;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("ResultListCapacity must more than zero.");
                }
                GGDbContext.s_ResultListCapacity = value;
            }
        }
        /// <summary>
        /// 当前连接对象
        /// </summary>
        public DbConnection Connection
        {
            get
            {
                return this.m_conn;
            }
        }
        /// <summary>
        /// 当前事务对象
        /// </summary>
        public DbTransaction Transaction
        {
            get
            {
                return this.m_trans;
            }
        }
        /// <summary>
        /// 当前的命令对象，每当执行ExecteuXXXXXXXXX时，都会在这个对象上执行。
        /// </summary>
        public DbCommand CurrentCommand
        {
            get
            {
                return this.m_command;
            }
        }
        /// <summary>
        /// <para>是否会自动打开并关闭连接。如果调用无参的构造函数，此属性为true</para>
        /// <para>注意：如果为true，则表示每次操作前打开连接，并在操作后关闭连接。</para>
        /// </summary>
        public bool IsAutoOpenClose
        {
            get
            {
                return this.m_autoOpenClose;
            }
        }
        /// <summary>
        /// 当前连接对象由哪个BLL实例创建的（例如：在访问GGBaseBLL.DbContext时自动创建）。
        /// </summary>
        public GGBaseBLL OwnerBLL
        {
            get
            {
                return this.m_ownerBll;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_ownerBll = value;
            }
        }
        /// <summary>
        /// 命令参数的前缀。如: @
        /// </summary>
        public string ParamNamePrefix
        {
            get
            {
                return this.m_ParamNamePrefix;
            }
            set
            {
                this.m_ParamNamePrefix = value;
            }
        }
        /// <summary>
        /// <para>初始化连接工厂（仅供连接一种数据库）。</para>
        /// <para>设置GGDbContext默认连接数据库的方式，一般用于只访问一个数据库。</para>
        /// <para>此时GGDbContext仍可以连接多个数据库，但仅限一种数据库类型了。</para>
        /// </summary>
        /// <param name="providerName">数据提供者名称</param>
        /// <param name="cmdParamNamePrefix">存储过程参数的名称前缀。
        /// 对于MSSQL来说，一定是"@"，
        /// MySql虽然对这个没有要求，但建议设置为"_"，用来区分其它变量。（当然也可能设置为""）</param>
        /// <param name="defaultConnString">缺省的连接字符串</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Init(string providerName, string cmdParamNamePrefix, string defaultConnString)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                throw new ArgumentNullException("providerName");
            }
            if (GGDbContext.s_factory != null)
            {
                throw new InvalidOperationException(SR.MethodCalled);
            }
            GGDbContext.s_factory = GGDbContext.GetDbProviderFactory(providerName);
            GGDbContext.s_defaultParamNamePrefix = (cmdParamNamePrefix ?? "@");
            GGDbContext.s_defaultConnectionString = defaultConnString;
        }
        /// <summary>
        /// <para>设置GGDbContext为“多种连接配置”方式。</para>
        /// <para>注册数据库的连接信息。用于需要同时连接多种类型的数据库，调用本方法和调用Init()并不冲突。</para>
        /// <para>如果程序要访问二种不同类型的数据库，如：MSSqlServer和MySql，那么至少需要调用本方法二次。</para>
        /// <para>每种类型的数据库如果有多个“数据库的连接”，可以在构造方法中指定。这里的连接字符串只是做为默认的连接字符串</para>
        /// </summary>
        /// <param name="configName">配置名称：不同种类的数据库的配置名称，如：MSSQL, MySql。这个参数用于后续调用时传入构造方法中</param>
        /// <param name="providerName">数据提供者名称</param>
        /// <param name="cmdParamNamePrefix">存储过程参数的名称前缀。</param>
        /// <param name="defaultConnString">缺省的连接字符串</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void RegisterDataBaseConnectionInfo(string configName, string providerName, string cmdParamNamePrefix, string defaultConnString)
        {
            if (string.IsNullOrEmpty(configName))
            {
                throw new ArgumentNullException("configName");
            }
            if (string.IsNullOrEmpty(providerName))
            {
                throw new ArgumentNullException("providerName");
            }
            if (GGDbContext.s_dbInfo.ContainsKey(configName))
            {
                throw new InvalidOperationException(string.Format(SR.ConfigItemRegistered, configName));
            }
            GGDbContext.DataBaseConnectionInfo dataBaseConnectionInfo = new GGDbContext.DataBaseConnectionInfo();
            dataBaseConnectionInfo.Factory = GGDbContext.GetDbProviderFactory(providerName);
            dataBaseConnectionInfo.ProviderName = providerName;
            dataBaseConnectionInfo.ParamNamePrefix = (cmdParamNamePrefix ?? "@");
            dataBaseConnectionInfo.ConnectionString = defaultConnString;
            GGDbContext.s_dbInfo.Add(configName, dataBaseConnectionInfo);
        }
        /// <summary>
        /// 根据一个提供者程序，获取对应的DbProviderFactory对象，如果获取失败，则抛出异常。
        /// </summary>
        /// <param name="providerName">提供者程序名称</param>
        /// <returns>DbProviderFactory对象</returns>
        private static DbProviderFactory GetDbProviderFactory(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                throw new ArgumentNullException("providerName");
            }
            DbProviderFactory result;
            try
            {
                result = DbProviderFactories.GetFactory(providerName);
            }
            catch (Exception ex)
            {
                if (providerName == "MySql.Data.MySqlClient")
                {
                    try
                    {
                        result = GGDbContext.GetMySqlClientFactory();
                        return result;
                    }
                    catch
                    {
                    }
                }
                throw ex;
            }
            return result;
        }
        /// <summary>
        /// <para>其实，直接访问静态属性就可以了： MySql.Data.MySqlClient.MySqlClientFactory.Instance;</para>
        /// <para>但是，这样做就要求项目要直接引用MySql.Data.dll这个文件。</para>
        /// <para>如果应用程序项目根本不使用MySql，但仍然需要引用这个文件。这样就没意思了。</para>
        /// <para>所以，下面的写法，可以不需要强制引用MySql.Data.dll这个文件。</para>
        /// </summary>
        /// <returns></returns>
        private static DbProviderFactory GetMySqlClientFactory()
        {
            Type type = Type.GetType("MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data");
            if (type == null)
            {
                throw new InvalidOperationException(SR.MySqlClientFactoryNotFound);
            }
            return (DbProviderFactory)type.InvokeMember("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField, null, null, null);
        }
        /// <summary>
        /// 获取MySql的DbProviderFactory，如果获取失败，则抛出异常。
        /// </summary>
        /// <returns></returns>
        internal static DbProviderFactory GetMySqlClientFactoryFromCache()
        {
            if (GGDbContext.s_factory != null && GGDbContext.s_factory.GetType().ToString().StartsWith("MySql.Data.MySqlClient"))
            {
                return GGDbContext.s_factory;
            }
            if (GGDbContext.s_dbInfo != null)
            {
                foreach (KeyValuePair<string, GGDbContext.DataBaseConnectionInfo> current in GGDbContext.s_dbInfo)
                {
                    if (current.Value.ProviderName == "MySql.Data.MySqlClient")
                    {
                        return current.Value.Factory;
                    }
                }
            }
            DbProviderFactory dbProviderFactory = GGDbContext.GetDbProviderFactory("MySql.Data.MySqlClient");
            if (dbProviderFactory == null)
            {
                throw new InvalidOperationException(SR.MySqlClientFactoryNotFound);
            }
            return dbProviderFactory;
        }
        private GGDbContext()
        {
        }
        /// <summary>
        /// <para>构造方法，使用默认连接字符串，不使用事务</para>
        /// <para>注意：1.这个构造方法仅供每次操作前打开连接，并在操作后关闭连接的情形下使用。</para>
        /// <para>       2.此构造方法在调用前需要调用Init()</para>
        /// </summary>
        internal GGDbContext(GGBaseBLL owner)
            : this(null, false)
        {
            this.m_autoOpenClose = true;
            this.OwnerBLL = owner;
        }
        /// <summary>
        /// <para>构造方法，使用默认连接字符串，可指定是否使用事务</para>
        /// <para>注意：此构造方法在调用前需要调用Init()</para>
        /// </summary>
        /// <param name="useTransaction">是否使用事务</param>
        public GGDbContext(bool useTransaction)
            : this(null, useTransaction)
        {
        }
        /// <summary>
        /// <para>构造方法，可以指定连接字符串和是否使用事务。</para>
        /// <para>注意：此构造方法在调用前需要调用Init()</para>
        /// </summary>
        /// <param name="connectionString">连接字符串，如果为空，则使用默认的连接字符串</param>
        /// <param name="useTransaction">是否使用事务</param>
        public GGDbContext(string connectionString, bool useTransaction)
        {
            if (GGDbContext.s_factory == null)
            {
                throw new InvalidOperationException(SR.NeedInitGGDbContext);
            }
            this.m_conn = GGDbContext.s_factory.CreateConnection();
            this.m_ParamNamePrefix = GGDbContext.s_defaultParamNamePrefix;
            if (string.IsNullOrEmpty(connectionString))
            {
                if (string.IsNullOrEmpty(GGDbContext.s_defaultConnectionString))
                {
                    this.m_conn.ConnectionString = this.GetDefaultConnectionStringFromEventHandler(null);
                }
                else
                {
                    this.m_conn.ConnectionString = GGDbContext.s_defaultConnectionString;
                }
            }
            else
            {
                this.m_conn.ConnectionString = connectionString;
            }
            if (string.IsNullOrEmpty(this.m_conn.ConnectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            this.m_conn.Open();
            this.RaiseOpenConnectionEvent();
            if (useTransaction)
            {
                this.m_trans = this.m_conn.BeginTransaction();
            }
            this.InnerInit();
        }
        /// <summary>
        /// <para>构造方法，可以指定连接哪种数据库，以及连接字符串和是否使用事务。</para>
        /// <para>用于程序需要支持多种类型的数据库时。</para>
        /// <para>注意：此构造方法在调用前需要调用RegisterDataBaseConnectionInfo()</para>
        /// </summary>
        /// <param name="configName">调用RegisterDataBaseConnectionInfo()时指定的配置名称</param>
        /// <param name="connectionString">连接字符串，如果为空，则使用默认的连接字符串</param>
        /// <param name="useTransaction">是否使用事务</param>
        public GGDbContext(string configName, string connectionString, bool useTransaction)
        {
            if (string.IsNullOrEmpty(configName))
            {
                throw new ArgumentNullException("configName");
            }
            GGDbContext.DataBaseConnectionInfo dataBaseConnectionInfo;
            if (!GGDbContext.s_dbInfo.TryGetValue(configName, out dataBaseConnectionInfo))
            {
                throw new InvalidOperationException(string.Format(SR.ConfigItemNotFound, configName));
            }
            this.m_conn = dataBaseConnectionInfo.Factory.CreateConnection();
            this.m_ParamNamePrefix = dataBaseConnectionInfo.ParamNamePrefix;
            if (string.IsNullOrEmpty(connectionString))
            {
                if (string.IsNullOrEmpty(dataBaseConnectionInfo.ConnectionString))
                {
                    this.m_conn.ConnectionString = this.GetDefaultConnectionStringFromEventHandler(configName);
                }
                else
                {
                    this.m_conn.ConnectionString = dataBaseConnectionInfo.ConnectionString;
                }
            }
            else
            {
                this.m_conn.ConnectionString = connectionString;
            }
            if (string.IsNullOrEmpty(this.m_conn.ConnectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            this.m_conn.Open();
            this.RaiseOpenConnectionEvent();
            if (useTransaction)
            {
                this.m_trans = this.m_conn.BeginTransaction();
            }
            this.InnerInit();
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="providerName">数据提供者名称</param>
        /// <param name="cmdParamNamePrefix">命令中的参数前缀</param>
        public GGDbContext(bool useTransaction, string connectionString, string providerName, string cmdParamNamePrefix)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(providerName))
            {
                throw new ArgumentNullException("providerName");
            }
            DbProviderFactory dbProviderFactory = GGDbContext.GetDbProviderFactory(providerName);
            this.m_conn = dbProviderFactory.CreateConnection();
            this.m_conn.ConnectionString = connectionString;
            this.m_ParamNamePrefix = (cmdParamNamePrefix ?? "@");
            this.m_conn.Open();
            this.RaiseOpenConnectionEvent();
            if (useTransaction)
            {
                this.m_trans = this.m_conn.BeginTransaction();
            }
            this.InnerInit();
        }
        /// <summary>
        /// 根据存储过程名称，在当前连接上下文中创建命令对象，并可能会添加到事务中
        /// </summary>
        /// <param name="spName">存储过程名称</param>
        /// <returns>创建的命令对象</returns>
        public DbCommand CreateCommand(string spName)
        {
            this.MakeSureConnectionNotNull();
            DbCommand dbCommand = this.m_conn.CreateCommand();
            dbCommand.CommandText = spName;
            dbCommand.CommandType = CommandType.StoredProcedure;
            if (this.m_trans != null)
            {
                dbCommand.Transaction = this.m_trans;
            }
            this.m_command = dbCommand;
            return dbCommand;
        }
        /// <summary>
        /// 根据存储过程名称或SQL语句，在当前连接上下文中创建命令对象，并可能会添加到事务中
        /// </summary>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>创建的命令对象</returns>
        public DbCommand CreateCommand(string commandText, CommandType commandType)
        {
            DbCommand dbCommand = this.CreateCommand(commandText);
            dbCommand.CommandType = commandType;
            return dbCommand;
        }
        /// <summary>
        /// 提交当前事务。如果没有事务，将会引发异常。
        /// </summary>
        public void CommitTransaction()
        {
            this.MakeSureTranscationExist();
            this.m_trans.Commit();
        }
        /// <summary>
        /// 关闭并释放连接（将不能再访问连接）
        /// </summary>
        public void CloseConnection()
        {
            if (this.m_conn != null)
            {
                try
                {
                    this.m_conn.Close();
                    this.m_conn.Dispose();
                }
                catch
                {
                }
                finally
                {
                    this.m_conn = null;
                }
            }
        }
        /// <summary>
        /// 简单的关闭连接，不清除连接对象。（可以重新打开）
        /// </summary>
        public void SimpleCloseConnection()
        {
            if (this.m_conn != null)
            {
                try
                {
                    this.m_conn.Close();
                }
                catch
                {
                }
            }
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        private void RollbackTransaction()
        {
            if (this.m_trans == null)
            {
                return;
            }
            try
            {
                this.m_trans.Rollback();
            }
            catch
            {
            }
            finally
            {
                this.m_trans = null;
            }
        }
        /// <summary>
        /// 清除对象，关闭并释放连接
        /// </summary>
        public void Dispose()
        {
            this.CloseConnection();
            if (this.m_trans != null)
            {
                try
                {
                    this.m_trans.Dispose();
                }
                catch
                {
                }
                finally
                {
                    this.m_trans = null;
                }
            }
        }
        /// <summary>
        /// 清理当前命令对象，处理方式：重置CommandText, Parameters.Clear()
        /// </summary>
        public void ClearCommandStatus()
        {
            this.MakeSureCommandNotNull();
            this.m_command.CommandText = null;
            this.m_command.Parameters.Clear();
        }
        /// <summary>
        /// 确认连接对象存在。
        /// </summary>
        private void MakeSureConnectionNotNull()
        {
            if (this.m_conn == null)
            {
                throw new InvalidOperationException("connection is null.");
            }
        }
        /// <summary>
        /// 确认连接为打开状态，如果连接没有打开，则打开连接。
        /// </summary>
        public void MakeSureConnectionOpen()
        {
            this.MakeSureConnectionNotNull();
            if (this.m_conn.State != ConnectionState.Open)
            {
                this.m_conn.Open();
                this.RaiseOpenConnectionEvent();
            }
        }
        /// <summary>
        /// 确认命令对象不为空。
        /// </summary>
        private void MakeSureCommandNotNull()
        {
            if (this.m_command == null)
            {
                throw new InvalidOperationException("command is null.");
            }
        }
        /// <summary>
        /// 确认本次连接已经打开了事务
        /// </summary>
        public void MakeSureTranscationExist()
        {
            if (this.m_trans == null)
            {
                throw new InvalidOperationException("transcation is null.");
            }
        }
        private void ProcessException(Exception ex)
        {
            if (!this.KeepConnectionOnException)
            {
                this.RollbackTransaction();
                this.CloseConnection();
            }
            this.m_occurExecption = true;
            if (!this.IgnoreErrorEvent)
            {
                GGDbContextExceptionEvent onGGDbContextExceptionEvent = GGDbContext.OnGGDbContextExceptionEvent;
                if (onGGDbContextExceptionEvent != null)
                {
                    onGGDbContextExceptionEvent(this, ex);
                }
            }
            throw ex;
        }
        private void InnerInit()
        {
            this.AutoGetSpOutputValuesForThisInstance = GGDbContext.AutoGetSpOutputValues;
        }
        private void RaiseOpenConnectionEvent()
        {
            GGDbContextOpenConnection onGGDbContextOpenConnection = GGDbContext.OnGGDbContextOpenConnection;
            if (onGGDbContextOpenConnection != null)
            {
                onGGDbContextOpenConnection(this);
            }
        }
        private void RaiseBeforeExecuteEvent()
        {
            if (this.m_autoOpenClose)
            {
                this.MakeSureConnectionOpen();
            }
            GGDbContextBeforeExecute onGGDbContextBeforeExecute = GGDbContext.OnGGDbContextBeforeExecute;
            if (onGGDbContextBeforeExecute != null)
            {
                onGGDbContextBeforeExecute(this);
            }
        }
        private void RaiseAfterExecuteEvent()
        {
            if (this.m_autoOpenClose)
            {
                this.SimpleCloseConnection();
            }
            if (this.m_occurExecption)
            {
                return;
            }
            GGDbContextAfterExecute onGGDbContextAfterExecute = GGDbContext.OnGGDbContextAfterExecute;
            if (onGGDbContextAfterExecute != null)
            {
                onGGDbContextAfterExecute(this);
            }
            if (this.SpOutParamDescription != null)
            {
                GGBLLHelper.GetSpOutputValues(this);
            }
        }
        /// <summary>
        /// <para>如果在构造方法中没有提供连接字符串，同时也没有提供默认的连接字符串，</para>
        /// <para>这里将引发事件，并从事件处理器中返回一个可用的连接字符串。</para>
        /// </summary>
        /// <returns></returns>
        private string GetDefaultConnectionStringFromEventHandler(string configName)
        {
            GGDbContextGetDefaultConnectionString onGetDefaultConnectionString = GGDbContext.OnGetDefaultConnectionString;
            if (onGetDefaultConnectionString == null)
            {
                return null;
            }
            GGDbContextEventArgs GGDbContextEventArgs = new GGDbContextEventArgs(configName);
            onGetDefaultConnectionString(GGDbContextEventArgs);
            return GGDbContextEventArgs.NewConnectionString;
        }
        /// <summary>
        /// 执行select命令并返回结果到一个DataTalbe
        /// </summary>
        /// <returns>查询结果</returns>
        public DataTable ExecuteSelectCommand()
        {
            this.MakeSureCommandNotNull();
            this.RaiseBeforeExecuteEvent();
            DataTable dataTable = null;
            try
            {
                using (DbDataReader dbDataReader = this.m_command.ExecuteReader())
                {
                    dataTable = new DataTable();
                    dataTable.Load(dbDataReader);
                    dbDataReader.Close();
                }
            }
            catch (Exception ex)
            {
                this.ProcessException(ex);
            }
            finally
            {
                this.RaiseAfterExecuteEvent();
            }
            return dataTable;
        }
        /// <summary>
        /// 执行select存储过程并返回结果到一个DataTalbe，仅适用于调用无参的存储过程
        /// </summary>
        /// <param name="spName">存储过程名称</param>
        /// <returns>查询结果</returns>
        public DataTable ExecuteSelectCommand(string spName)
        {
            this.CreateCommand(spName);
            return this.ExecuteSelectCommand();
        }
        /// <summary>
        /// 执行当前命令并返回多个DataTalbe保存到一个DataSet
        /// </summary>
        /// <param name="tableNames">结果表的表名称</param>
        /// <returns>查询结果</returns>
        public DataSet ExecuteSelectCommand(string[] tableNames)
        {
            this.MakeSureCommandNotNull();
            this.RaiseBeforeExecuteEvent();
            DataSet dataSet = null;
            try
            {
                using (DbDataReader dbDataReader = this.m_command.ExecuteReader(CommandBehavior.Default))
                {
                    dataSet = new DataSet();
                    dataSet.Load(dbDataReader, LoadOption.PreserveChanges, tableNames);
                    dbDataReader.Close();
                }
            }
            catch (Exception ex)
            {
                this.ProcessException(ex);
            }
            finally
            {
                this.RaiseAfterExecuteEvent();
            }
            return dataSet;
        }
        /// <summary>
        /// 执行当前命令并返回多个DataTalbe保存到一个DataSet
        /// </summary>
        /// <param name="tableCount">查询结果将包含多少个表</param>
        /// <returns>查询结果</returns>
        public DataSet ExecuteSelectCommand(int tableCount)
        {
            string[] array = new string[tableCount];
            for (int i = 1; i <= tableCount; i++)
            {
                array[i - 1] = "Table" + i.ToString();
            }
            return this.ExecuteSelectCommand(array);
        }
        /// <summary>
        /// 执行当前命令并返回一个泛型的Dictionary，表中关键字为ID整型
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="fieldName">做为字典KEY的字段名，它应该包含一个int的值</param>
        /// <param name="loadAllField">是否加载业务实体全部字段</param>
        /// <param name="checkSucessCount">加载完成后，是否要检查成功的数量，确保每次的加载都是成功的</param>
        /// <param name="loadFunc">从数据行加载字段的方法</param>
        /// <returns>查询结果对应的泛型的Dictionary</returns>
        public Dictionary<int, T> ExecuteSelectCommandToDictionary<T>(string fieldName, bool loadAllField, bool checkSucessCount, LoadItemValuesFormRowFunc loadFunc) where T : class, new()
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            if (loadFunc == null)
            {
                throw new ArgumentNullException("loadFunc");
            }
            GGItemHelper.EnsureIsDataItemType(typeof(T));
            this.MakeSureCommandNotNull();
            this.RaiseBeforeExecuteEvent();
            Dictionary<int, T> dictionary = new Dictionary<int, T>(GGDbContext.s_ResultListCapacity);
            try
            {
                using (DbDataReader dbDataReader = this.m_command.ExecuteReader())
                {
                    MyDataAdapter row = new MyDataAdapter(dbDataReader);
                    while (dbDataReader.Read())
                    {
                        T t = Activator.CreateInstance(typeof(T)) as T;
                        loadFunc(row, t, loadAllField, checkSucessCount);
                        dictionary.Add(Convert.ToInt32(dbDataReader[fieldName]), t);
                    }
                    dbDataReader.Close();
                }
            }
            catch (Exception ex)
            {
                this.ProcessException(ex);
            }
            finally
            {
                this.RaiseAfterExecuteEvent();
            }
            return dictionary;
        }
        /// <summary>
        /// 执行当前命令并返回一个泛型的Dictionary，表中关键字为ID整型
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="fieldName">做为字典KEY的字段名，它应该包含一个int的值</param>
        /// <param name="loadAllField">是否加载业务实体全部字段</param>
        /// <param name="checkSucessCount">加载完成后，是否要检查成功的数量，确保每次的加载都是成功的</param>
        /// <returns>查询结果对应的泛型的Dictionary</returns>
        public Dictionary<int, T> ExecuteSelectCommandToDictionary<T>(string fieldName, bool loadAllField, bool checkSucessCount) where T : class, new()
        {
            return this.ExecuteSelectCommandToDictionary<T>(fieldName, loadAllField, checkSucessCount, new LoadItemValuesFormRowFunc(GGItemHelper.LoadItemValuesFormDbRow));
        }
        /// <summary>
        /// 执行当前命令并返回一个泛型的Dictionary，表中关键字为ID整型
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="fieldName">做为字典KEY的字段名，它应该包含一个int的值</param>
        /// <returns>查询结果对应的泛型的Dictionary</returns>
        public Dictionary<int, T> ExecuteSelectCommandToDictionary<T>(string fieldName) where T : class, new()
        {
            return this.ExecuteSelectCommandToDictionary<T>(fieldName, false, GGDbContext.StrictModeLoadItem, new LoadItemValuesFormRowFunc(GGItemHelper.LoadItemValuesFormDbRow));
        }
        /// <summary>
        /// 执行当前命令并返回一个泛型的Dictionary，表中关键字为GUID字符型
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="fieldName">做为字典KEY的字段名，它应该包含一个string的值</param>
        /// <param name="loadAllField">是否加载业务实体全部字段</param>
        /// <param name="checkSucessCount">加载完成后，是否要检查成功的数量，确保每次的加载都是成功的</param>
        /// <param name="loadFunc">从数据行加载字段的方法</param>
        /// <returns>查询结果对应的泛型的Dictionary</returns>
        public Dictionary<string, T> ExecuteSelectCommandToDictionary2<T>(string fieldName, bool loadAllField, bool checkSucessCount, LoadItemValuesFormRowFunc loadFunc) where T : class, new()
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            if (loadFunc == null)
            {
                throw new ArgumentNullException("loadFunc");
            }
            GGItemHelper.EnsureIsDataItemType(typeof(T));
            this.MakeSureCommandNotNull();
            this.RaiseBeforeExecuteEvent();
            Dictionary<string, T> dictionary = new Dictionary<string, T>(GGDbContext.s_ResultListCapacity);
            try
            {
                using (DbDataReader dbDataReader = this.m_command.ExecuteReader())
                {
                    MyDataAdapter row = new MyDataAdapter(dbDataReader);
                    while (dbDataReader.Read())
                    {
                        T t = Activator.CreateInstance(typeof(T)) as T;
                        loadFunc(row, t, loadAllField, checkSucessCount);
                        dictionary.Add(dbDataReader[fieldName].ToString(), t);
                    }
                    dbDataReader.Close();
                }
            }
            catch (Exception ex)
            {
                this.ProcessException(ex);
            }
            finally
            {
                this.RaiseAfterExecuteEvent();
            }
            return dictionary;
        }
        /// <summary>
        /// 执行当前命令并返回一个泛型的Dictionary，表中关键字为GUID字符型
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="fieldName">做为字典KEY的字段名，它应该包含一个string的值</param>
        /// <param name="loadAllField">是否加载业务实体全部字段</param>
        /// <param name="checkSucessCount">加载完成后，是否要检查成功的数量，确保每次的加载都是成功的</param>
        /// <returns>查询结果对应的泛型的Dictionary</returns>
        public Dictionary<string, T> ExecuteSelectCommandToDictionary2<T>(string fieldName, bool loadAllField, bool checkSucessCount) where T : class, new()
        {
            return this.ExecuteSelectCommandToDictionary2<T>(fieldName, loadAllField, checkSucessCount, new LoadItemValuesFormRowFunc(GGItemHelper.LoadItemValuesFormDbRow));
        }
        /// <summary>
        ///  执行当前命令并返回一个泛型的Dictionary，表中关键字为GUID字符型
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="fieldName">做为字典KEY的字段名，它应该包含一个string的值</param>
        /// <returns>查询结果对应的泛型的Dictionary</returns>
        public Dictionary<string, T> ExecuteSelectCommandToDictionary2<T>(string fieldName) where T : class, new()
        {
            return this.ExecuteSelectCommandToDictionary2<T>(fieldName, false, GGDbContext.StrictModeLoadItem, new LoadItemValuesFormRowFunc(GGItemHelper.LoadItemValuesFormDbRow));
        }
        /// <summary>
        /// 执行当前命令并返回一个泛型的List
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="loadAllField">是否加载业务实体全部字段</param>
        /// <param name="checkSucessCount">加载完成后，是否要检查成功的数量，确保每次的加载都是成功的</param>
        /// <param name="loadFunc">从数据行加载字段的方法</param>
        /// <returns>查询结果对应的泛型的List</returns>
        public List<T> ExecuteSelectCommandToList<T>(bool loadAllField, bool checkSucessCount, LoadItemValuesFormRowFunc loadFunc) where T : class, new()
        {
            if (loadFunc == null)
            {
                throw new ArgumentNullException("loadFunc");
            }
            GGItemHelper.EnsureIsDataItemType(typeof(T));
            this.MakeSureCommandNotNull();
            this.RaiseBeforeExecuteEvent();
            List<T> list = new List<T>(GGDbContext.s_ResultListCapacity);
            try
            {
                using (DbDataReader dbDataReader = this.m_command.ExecuteReader())
                {
                    MyDataAdapter row = new MyDataAdapter(dbDataReader);
                    while (dbDataReader.Read())
                    {
                        T t = Activator.CreateInstance(typeof(T)) as T;
                        loadFunc(row, t, loadAllField, checkSucessCount);
                        list.Add(t);
                    }
                    dbDataReader.Close();
                }
            }
            catch (Exception ex)
            {
                this.ProcessException(ex);
            }
            finally
            {
                this.RaiseAfterExecuteEvent();
            }
            return list;
        }
        /// <summary>
        /// 执行当前命令并返回一个泛型的List
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="loadAllField">是否加载业务实体全部字段</param>
        /// <param name="checkSucessCount">加载完成后，是否要检查成功的数量，确保每次的加载都是成功的</param>
        /// <returns>查询结果对应的泛型的List</returns>
        public List<T> ExecuteSelectCommandToList<T>(bool loadAllField, bool checkSucessCount) where T : class, new()
        {
            return this.ExecuteSelectCommandToList<T>(loadAllField, checkSucessCount, new LoadItemValuesFormRowFunc(GGItemHelper.LoadItemValuesFormDbRow));
        }
        /// <summary>
        /// 执行当前命令并返回一个泛型的List
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <returns>查询结果对应的泛型的List</returns>
        public List<T> ExecuteSelectCommandToList<T>() where T : class, new()
        {
            return this.ExecuteSelectCommandToList<T>(false, GGDbContext.StrictModeLoadItem, new LoadItemValuesFormRowFunc(GGItemHelper.LoadItemValuesFormDbRow));
        }
        /// <summary>
        /// 执行当前命令，获取一个业务实体对象
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="loadAllField">是否加载业务实体全部字段</param>
        /// <param name="checkSucessCount">加载完成后，是否要检查成功的数量，确保每次的加载都是成功的</param>
        /// <param name="loadFunc">从数据行加载字段的方法</param>
        /// <returns>数据实体类型</returns>
        public T ExecuteSelectCommandGetBllObject<T>(bool loadAllField, bool checkSucessCount, LoadItemValuesFormRowFunc loadFunc) where T : class, new()
        {
            if (loadFunc == null)
            {
                throw new ArgumentNullException("loadFunc");
            }
            GGItemHelper.EnsureIsDataItemType(typeof(T));
            this.MakeSureCommandNotNull();
            this.RaiseBeforeExecuteEvent();
            T t = default(T);
            try
            {
                using (DbDataReader dbDataReader = this.m_command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    MyDataAdapter row = new MyDataAdapter(dbDataReader);
                    if (dbDataReader.Read())
                    {
                        t = (Activator.CreateInstance(typeof(T)) as T);
                        loadFunc(row, t, loadAllField, checkSucessCount);
                    }
                    dbDataReader.Close();
                }
            }
            catch (Exception ex)
            {
                this.ProcessException(ex);
            }
            finally
            {
                this.RaiseAfterExecuteEvent();
            }
            return t;
        }
        /// <summary>
        /// 执行当前命令，获取一个业务实体对象
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="loadAllField">是否加载业务实体全部字段</param>
        /// <param name="checkSucessCount">加载完成后，是否要检查成功的数量，确保每次的加载都是成功的</param>
        /// <returns>数据实体类型</returns>
        public T ExecuteSelectCommandGetBllObject<T>(bool loadAllField, bool checkSucessCount) where T : class, new()
        {
            return this.ExecuteSelectCommandGetBllObject<T>(loadAllField, checkSucessCount, new LoadItemValuesFormRowFunc(GGItemHelper.LoadItemValuesFormDbRow));
        }
        /// <summary>
        /// 执行当前命令，获取一个业务实体对象
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <returns>数据实体类型</returns>
        public T ExecuteSelectCommandGetBllObject<T>() where T : class, new()
        {
            return this.ExecuteSelectCommandGetBllObject<T>(true, GGDbContext.StrictModeLoadItem, new LoadItemValuesFormRowFunc(GGItemHelper.LoadItemValuesFormDbRow));
        }
        /// <summary>
        /// <para>执行一个简单的操作，直接调用DbCommand.ExecuteNonQuery()</para>
        /// <para>说明：如果执行一个存储过程，则将返回存储过程内 “所有 UPDATE、INSERT 或 DELETE语句” “影响”的行数累加之和。</para>
        /// </summary>
        /// <returns>命令所影响的行数</returns>
        public int ExecuteNonQuery()
        {
            this.MakeSureCommandNotNull();
            this.RaiseBeforeExecuteEvent();
            int result = -1;
            try
            {
                result = this.m_command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                this.ProcessException(ex);
            }
            finally
            {
                this.RaiseAfterExecuteEvent();
            }
            return result;
        }
        /// <summary>
        /// 执行一个简单的操作，直接调用DbCommand.ExecuteScalar()
        /// </summary>
        /// <returns>执行结果</returns>
        public object ExecuteScalar()
        {
            this.MakeSureCommandNotNull();
            this.RaiseBeforeExecuteEvent();
            object result = null;
            try
            {
                result = this.m_command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                this.ProcessException(ex);
            }
            finally
            {
                this.RaiseAfterExecuteEvent();
            }
            return result;
        }
        /// <summary>
        /// <para>执行一个简单的操作，直接调用DbCommand.ExecuteNonQuery()</para>
        /// <para>执行完调用后，根据数据库返回的“受影响的记录数 &gt; 0“ 判断操作是否成功。</para>
        /// </summary>
        /// <returns>如果命令的影响行数大于零，则返回 true ，否则返回 false </returns>
        public bool ExecuteCommandReturnBoolResult()
        {
            int num = this.ExecuteNonQuery();
            return num > 0;
        }
        /// <summary>
        /// 执行查询，将结果集的第一列以一个List《string》的形式返回
        /// </summary>
        /// <returns>查询结果</returns>
        public List<string> ExecuteSelectCommandToStringList()
        {
            this.MakeSureCommandNotNull();
            this.RaiseBeforeExecuteEvent();
            List<string> list = new List<string>(100);
            try
            {
                using (DbDataReader dbDataReader = this.m_command.ExecuteReader())
                {
                    while (dbDataReader.Read())
                    {
                        list.Add(dbDataReader.GetString(0));
                    }
                    dbDataReader.Close();
                }
            }
            catch (Exception ex)
            {
                this.ProcessException(ex);
            }
            finally
            {
                this.RaiseAfterExecuteEvent();
            }
            return list;
        }
        /// <summary>
        /// 往当前命令中添加一个命令参数
        /// </summary>
        /// <param name="paraName">参数名（不包含参数名前缀）</param>
        /// <param name="paraValue">参数值</param>
        /// <returns>新增后的命令参数对象</returns>
        public DbParameter AddParameterToCommand(string paraName, object paraValue)
        {
            this.MakeSureCommandNotNull();
            DbParameter dbParameter = this.m_command.CreateParameter();
            dbParameter.ParameterName = this.m_ParamNamePrefix + paraName;
            dbParameter.Value = ((paraValue == null) ? DBNull.Value : paraValue);
            this.m_command.Parameters.Add(dbParameter);
            return dbParameter;
        }
        /// <summary>
        /// 往当前命令中添加一个命令参数
        /// </summary>
        /// <param name="paraName">参数名（不包含参数名前缀）</param>
        /// <param name="paraValue">参数值</param>
        /// <param name="paraType">参数类型</param>
        /// <returns>新增后的命令参数对象</returns>
        public DbParameter AddParameterToCommand(string paraName, object paraValue, DbType paraType)
        {
            return this.AddParameterToCommand(paraName, paraValue, paraType, null, ParameterDirection.Input);
        }
        /// <summary>
        /// 往当前命令中添加一个命令参数
        /// </summary>
        /// <param name="paraName">参数名（不包含参数名前缀）</param>
        /// <param name="paraValue">参数值</param>
        /// <param name="paraType">参数类型</param>
        /// <param name="size">参数值的数据长度</param>
        /// <returns>新增后的命令参数对象</returns>
        public DbParameter AddParameterToCommand(string paraName, object paraValue, DbType paraType, int size)
        {
            return this.AddParameterToCommand(paraName, paraValue, paraType, new int?(size), ParameterDirection.Input);
        }
        /// <summary>
        /// 往当前命令中添加一个命令参数
        /// </summary>
        /// <param name="paraName">参数名（不包含参数名前缀）</param>
        /// <param name="paraValue">参数值</param>
        /// <param name="paraType">参数类型</param>
        /// <param name="size">参数值的数据长度</param>
        /// <param name="inout">输入，输出类型</param>
        /// <returns>新增后的命令参数对象</returns>
        public DbParameter AddParameterToCommand(string paraName, object paraValue, DbType paraType, int? size, ParameterDirection inout)
        {
            this.MakeSureCommandNotNull();
            DbParameter dbParameter = this.m_command.CreateParameter();
            dbParameter.ParameterName = this.m_ParamNamePrefix + paraName;
            dbParameter.DbType = paraType;
            dbParameter.Direction = inout;
            if (size.HasValue)
            {
                dbParameter.Size = size.Value;
            }
            if (paraValue != null)
            {
                dbParameter.Value = paraValue;
            }
            this.m_command.Parameters.Add(dbParameter);
            return dbParameter;
        }
        /// <summary>
        /// 往当前命令中加入“分页”相关的三个命令参数：in PageIndex int, in PageSize int, out TotalRecords int
        /// </summary>
        /// <param name="pageIndex">分页数，从0开始计算</param>
        /// <param name="pageSize">分页大小</param>
        public void AddPagingParameters(int pageIndex, int pageSize)
        {
            this.AddParameterToCommand(GGDbContext.STR_PageIndex, pageIndex, DbType.Int32);
            this.AddParameterToCommand(GGDbContext.STR_PageSize, pageSize, DbType.Int32);
            this.AddParameterToCommand(GGDbContext.STR_TotalRecords, null, DbType.Int32, null, ParameterDirection.Output);
        }
        /// <summary>
        /// 根据最后一次查询，计算有多少个分页数量
        /// </summary>
        /// <returns>最后一次查询有多少个分页</returns>
        public int GetPageCountFromLastQuery()
        {
            this.MakeSureCommandNotNull();
            int result;
            try
            {
                int num = int.Parse(this.m_command.Parameters[this.m_ParamNamePrefix + GGDbContext.STR_PageSize].Value.ToString());
                int num2 = int.Parse(this.m_command.Parameters[this.m_ParamNamePrefix + GGDbContext.STR_TotalRecords].Value.ToString());
                result = (int)Math.Ceiling((double)num2 / (double)num);
            }
            catch
            {
                result = 0;
            }
            return result;
        }
        /// <summary>
        /// 根据最后一次查询，获取命令输入参数"TotalRecords"的值。
        /// </summary>
        /// <returns>最后一次查询能查到多少条记录</returns>
        public int GetTotalRecordsFromLastQuery()
        {
            this.MakeSureCommandNotNull();
            int result;
            try
            {
                result = int.Parse(this.m_command.Parameters[this.m_ParamNamePrefix + GGDbContext.STR_TotalRecords].Value.ToString());
            }
            catch
            {
                result = 0;
            }
            return result;
        }
        /// <summary>
        /// 从连接池中清除所有SqlServer连接
        /// </summary>
        public static void ClearMsSqlConnectionPools()
        {
            SqlConnection.ClearAllPools();
        }
        /// <summary>
        /// 运行一段 SQL Server TSQL脚本（不按事务方式执行）
        /// </summary>
        /// <param name="connection">SqlConnection对象</param>
        /// <param name="SqlText">SQL Server TSQL脚本</param>
        public static void ExecuteTsqlScript(SqlConnection connection, string SqlText)
        {
            Regex regex = new Regex("^\\s*GO\\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string[] array = regex.Split(SqlText);
            using (SqlCommand sqlCommand = connection.CreateCommand())
            {
                sqlCommand.Connection = connection;
                string[] array2 = array;
                for (int i = 0; i < array2.Length; i++)
                {
                    string text = array2[i];
                    if (text.Length > 0)
                    {
                        sqlCommand.CommandText = text;
                        sqlCommand.CommandType = CommandType.Text;
                        try
                        {
                            sqlCommand.ExecuteNonQuery();
                        }
                        catch (SqlException)
                        {
                            throw;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 创建一个业务对象，并设置它的DbContext为“当前对象”
        /// </summary>
        /// <typeparam name="T">业务类的类型</typeparam>
        /// <returns>业务类的实例</returns>
        public T CreateBll<T>() where T : GGBaseBLL, new()
        {
            T result = Activator.CreateInstance(typeof(T)) as T;
            result.DbContext = this;
            return result;
        }
        /// <summary>
        /// <para>检查网站的BIN目录是否包含GGSQLProfilerLibrary.dll，</para>
        /// <para>如果存在，则调用GGSQLProfilerLibrary.dll中的SubscribeNotify()方法，</para>
        /// <para>如果不存在，则忽略。</para>
        /// <para>说明：这个方法是“安全”的，即使调用有异常，也不会抛出。</para>
        /// </summary>
        public static void TryInvokeSubscribeNotifyInGGSQLProfilerLibraryDll()
        {
            try
            {
                if (File.Exists(HttpContext.Current.Server.MapPath("~/Bin/GGSQLProfilerLibrary.dll")))
                {
                    Type type = Type.GetType("GGSQLProfilerLibrary.EventHelper, GGSQLProfilerLibrary");
                    if (type != null)
                    {
                        type.InvokeMember("SubscribeNotify", BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, null, null);
                    }
                }
            }
            catch
            {
            }
        }
    }
}
