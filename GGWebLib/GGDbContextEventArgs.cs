using System;
namespace GGWebLib
{
    /// <summary>
    /// 委托GGDbContextGetDefaultConnectionString的事件参数
    /// </summary>
    public sealed class GGDbContextEventArgs : EventArgs
    {
        private string m_connectionString;
        private string m_configName;
        /// <summary>
        /// 新的默认连接字符串
        /// </summary>
        public string NewConnectionString
        {
            get
            {
                return this.m_connectionString;
            }
            set
            {
                this.m_connectionString = value;
            }
        }
        /// <summary>
        /// 配置名称
        /// </summary>
        public string ConfigName
        {
            get
            {
                return this.m_configName;
            }
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="configName">配置名称</param>
        public GGDbContextEventArgs(string configName)
        {
            this.m_configName = configName;
        }
    }
}
