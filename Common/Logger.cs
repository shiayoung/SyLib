using System;
using System.Configuration;
using System.Web;
using Core.BLL;
using Core.Model;
using PostSharp.Aspects;

namespace Core.Common
{
    public class Logger
    {
        public static void WriteLoggerToDB(MethodExecutionArgs args, string message)
        {
            HttpRequest currentRequest = HttpContext.Current.Request;
            var isClient = string.IsNullOrEmpty(ConfigurationManager.AppSettings["clientRoot"]);
            var systemLog = new SystemLog();

            systemLog.Event = message;
            systemLog.Method = args != null ? args.Method.Name : "";
            systemLog.EventData = args != null ? Tools.GetJson(args.Arguments) : "";
            systemLog.LogTime = DateTime.Now;
            systemLog.Url = currentRequest.Url.AbsolutePath;
            systemLog.IP = currentRequest.UserHostAddress;
            if (!isClient)
            {
                var admin = AdminLoginManager.AdminUsersInfo;
                systemLog.WhoType = (int)admin.RoleKind;
                systemLog.Who = admin.RealName;
            }
            else
            {
                systemLog.WhoType = 8;
                if (LoginManager.Customer == null)
                {
                    systemLog.Who = "新用户";
                }
                else
                {
                    var user = LoginManager.Customer;
                    systemLog.Who = user.LoginName;
                }
            }
            var systemLogBll = new SystemLogBLL();
            systemLogBll.Add(systemLog);
        }

        public static void WriteLoggerToDB(string message)
        {
            WriteLoggerToDB(null, message);
        }
    }
}
