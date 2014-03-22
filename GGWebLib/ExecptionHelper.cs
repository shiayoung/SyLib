using System;
using System.Data.SqlClient;
using System.Text;
namespace GGWebLib
{
    /// <summary>
    /// 处理异常消息文本的工具类
    /// </summary>
    public static class ExecptionHelper
    {
        private static readonly string s_messageSeparator = "\r\n\r\n -> ";
        /// <summary>
        /// 返回异常的全部信息，包括：Message, Type, InnerException, Source, Method, Stack Trace
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <returns>返回异常的全部信息，包括：Message, Type, InnerException, Source, Method, Stack Trace</returns>
        public static string GetExecptionDetailInfo(Exception ex)
        {
            if (ex == null)
            {
                return string.Empty;
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Format("{0} ({1})", ex.Message, ex.GetType().Name));
            for (Exception innerException = ex.InnerException; innerException != null; innerException = innerException.InnerException)
            {
                stringBuilder.Append(ExecptionHelper.s_messageSeparator);
                stringBuilder.Append(string.Format("{0} ({1})", innerException.Message, innerException.GetType().Name));
            }
            StringBuilder stringBuilder2 = new StringBuilder();
            stringBuilder2.AppendFormat("Exception generated at: {0}\r\n", DateTime.Now.ToString("u"));
            stringBuilder2.AppendLine("Message: " + stringBuilder.ToString());
            stringBuilder2.AppendLine("Source: " + ex.Source);
            stringBuilder2.AppendLine("Method: " + ex.TargetSite);
            stringBuilder2.AppendLine("Stack Trace: ");
            stringBuilder2.AppendLine(ex.StackTrace);
            return stringBuilder2.ToString();
        }
        /// <summary>
        /// 返回异常的可供显示信息
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <returns>返回异常的可供显示信息</returns>
        public static string GetExceptionMessage(Exception ex)
        {
            if (ex == null)
            {
                return string.Empty;
            }
            string text = ExecptionHelper.TryGetSpecicalSqlExceptionMessage(ex);
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(ex.Message);
            for (Exception innerException = ex.InnerException; innerException != null; innerException = innerException.InnerException)
            {
                stringBuilder.Append(ExecptionHelper.s_messageSeparator);
                stringBuilder.Append(string.Format("{0} ({1})", innerException.Message, innerException.GetType().Name));
            }
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 尝试判断是否为数据库的“约束冲突”异常，如果是，则返回固定的消息，否则返回 null
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <returns>尝试判断是否为数据库的“约束冲突”异常，如果是，则返回固定的消息，否则返回 null</returns>
        public static string TryGetSpecicalSqlExceptionMessage(Exception ex)
        {
            if (ex == null)
            {
                return string.Empty;
            }
            if (ex is SqlException)
            {
                SqlException ex2 = ex as SqlException;
                if (ex2.Number == 547 && ex2.Class == 16)
                {
                    return "您执行的这个操作与数据库结构的约束有冲突，为了保证数据的完整性，您当前的操作将被取消。\r\n\r\n如果您在删除记录，那么极有可能是您将要删除的记录被其它的数据行在引用。\r\n\r\n如果是新增则可能是有些内容没有填写。";
                }
                if (ex2.Number == 2601 && ex2.Class == 14)
                {
                    return "您执行的这个操作与数据库结构的约束有冲突，为了保证数据的完整性，您当前的操作将被取消。\r\n\r\n请检查您所输入的数据的某些字段是否已经存在。";
                }
            }
            return null;
        }
    }
}
