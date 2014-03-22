using System;
namespace GGWebLib
{
    /// <summary>
    /// 所有业务逻辑对象的基类
    /// </summary>
    public abstract class GGBaseBLL : IDisposable
    {
        private GGDbContext _dbContext;
        /// <summary>
        /// GGDbContext 实例，BLL使用它来访问或操作数据库，如果直接读取它，将会创建一个实例。
        /// </summary>
        public GGDbContext DbContext
        {
            get
            {
                if (this._dbContext == null)
                {
                    this._dbContext = new GGDbContext(this);
                }
                return this._dbContext;
            }
            set
            {
                this._dbContext = value;
            }
        }
        /// <summary>
        /// 创建一个业务对象，并设置它的DbContext为“当前对象”的GGDbContext，这样可以共用连接。
        /// </summary>
        /// <typeparam name="T">业务逻辑类型</typeparam>
        /// <returns>业务逻辑类的实例</returns>
        public T CreateBll<T>() where T : GGBaseBLL, new()
        {
            if (this._dbContext == null)
            {
                throw new InvalidOperationException(SR.CanNotShareConnection);
            }
            T result = Activator.CreateInstance(typeof(T)) as T;
            result.DbContext = this._dbContext;
            return result;
        }
        /// <summary>
        /// 如果实例的GGDbContext对象不为空，则清除对象，关闭并释放连接
        /// </summary>
        public virtual void Dispose()
        {
            if (this._dbContext != null && this._dbContext.OwnerBLL == this)
            {
                this._dbContext.Dispose();
                this._dbContext = null;
            }
        }
        private void EnsureDbContextIsNull()
        {
            if (this._dbContext != null)
            {
                throw new MyMessageException(SR.DbContextNotNull);
            }
        }
        /// <summary>
        /// 创建GGDbContext实例，并赋值给属性this.DbContext
        /// </summary>
        /// <param name="useTransaction">是否使用事务</param>
        public void CreateDbContext(bool useTransaction)
        {
            this.EnsureDbContextIsNull();
            this._dbContext = new GGDbContext(useTransaction);
            this._dbContext.OwnerBLL = this;
        }
        /// <summary>
        /// 创建GGDbContext实例，并赋值给属性this.DbContext
        /// </summary>
        /// <param name="connectionString">连接字符串，如果为空，则使用默认的连接字符串</param>
        /// <param name="useTransaction">是否使用事务</param>
        public void CreateDbContext(string connectionString, bool useTransaction)
        {
            this.EnsureDbContextIsNull();
            this._dbContext = new GGDbContext(connectionString, useTransaction);
            this._dbContext.OwnerBLL = this;
        }
        /// <summary>
        /// 创建GGDbContext实例，并赋值给属性this.DbContext
        /// </summary>
        /// <param name="configName">调用RegisterDataBaseConnectionInfo()时指定的配置名称</param>
        /// <param name="connectionString">连接字符串，如果为空，则使用默认的连接字符串</param>
        /// <param name="useTransaction">是否使用事务</param>
        public void CreateDbContext(string configName, string connectionString, bool useTransaction)
        {
            this.EnsureDbContextIsNull();
            this._dbContext = new GGDbContext(configName, connectionString, useTransaction);
            this._dbContext.OwnerBLL = this;
        }
        /// <summary>
        /// 创建GGDbContext实例，并赋值给属性this.DbContext
        /// </summary>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="providerName">数据提供者名称</param>
        /// <param name="cmdParamNamePrefix">命令中的参数前缀</param>
        public void CreateDbContext(bool useTransaction, string connectionString, string providerName, string cmdParamNamePrefix)
        {
            this.EnsureDbContextIsNull();
            this._dbContext = new GGDbContext(useTransaction, connectionString, providerName, cmdParamNamePrefix);
            this._dbContext.OwnerBLL = this;
        }
    }
}
