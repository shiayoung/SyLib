using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Core.Common
{
     /// <summary>
    /// 数据库连接字符串配置工厂类
    /// </summary>
    public struct ConnSettingsFactory
    {
        /// <summary>
        /// 获取报价数据, 来源于MySQL
        /// </summary>
        public static ConnectionStringSettings Chancellor
        {
            get { return ConfigurationManager.ConnectionStrings["Chancellor"]; }
        }

        /// <summary>
        /// 主库, 来源于MSSQL
        /// </summary>
        public static ConnectionStringSettings Main
        {
            get { return ConfigurationManager.ConnectionStrings["Main"]; }
        }
    }
}
