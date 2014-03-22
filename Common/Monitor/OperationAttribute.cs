using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using PostSharp.Aspects;

namespace Core.Common.Monitor
{
    /// <summary>
    /// 操作日志记录类，构造函数中的 message 表示对正在操作的方法的说明
    /// </summary>
    [Serializable]
    [global::System.AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class OperationAttribute : OnMethodBoundaryAspect
    {
        private string _msg = string.Empty;

        private bool _writeToDb = true;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="message">对正在操作的方法的说明</param>
        public OperationAttribute(string message) : this(message, true)
        {
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="message">对正在操作的方法的说明</param>
        /// <param name="writeToDb">是否寫入日志</param>
        public OperationAttribute(string message, bool writeToDb)
        {
            _msg = message;
            _writeToDb = writeToDb;
        }

        public override void OnEntry(MethodExecutionArgs args)
        {
            if (_writeToDb)
            {
                Logger.WriteLoggerToDB(args, _msg);
            }
            base.OnEntry(args);
        }

        //public override void OnExit(MethodExecutionArgs args)
        //{
        //    Logger.WriteToDB(args, _msg, 0);
        //    base.OnExit(args);
        //}
    }
}
