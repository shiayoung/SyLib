using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Model;
using Core.BLL;
using System.Web.Security;
using System.Web;
using Core.IBLL;

namespace Core.Common
{
    public sealed class LoginManager
    {
        private static ICusBLL CusBLL;

        static LoginManager()
        {
            CusBLL = new CusBLL();
        }

        /// <summary>
        /// 如果用戶已通過登錄驗證，則返回該用戶的登錄名
        /// </summary>
        public static string UserName
        {
            get
            {
                if (HttpContext.Current.Request.IsAuthenticated)
                {
                    return HttpContext.Current.User.Identity.Name;
                }
                return "";
            }
        }

        public static int CustomerId
        {
            get { return Customer==null ? 0 : Customer.Id; }
        }

        /// <summary>
        /// 客户实体，由已登录的验证信息从后台获取
        /// </summary>
        public static Customer Customer
        {
            get
            {
                if (String.IsNullOrEmpty(UserName))
                    return null;
                return CusBLL.Get(UserName);
            }
        }

        /// <summary>
        /// 获取 检查用户是否已登录
        /// </summary>
        public static bool IsLogin()
        {
            return HttpContext.Current.Request.IsAuthenticated;
        }

        /// <summary>
        /// 登录操作
        /// </summary>
        /// <param name="userName">登录名</param>
        /// <param name="password">登录密码</param>
        /// <returns>成功与否</returns>
        public static bool Login(string userName, string password)
        {
            if (CusBLL.Login(userName, password))
            {
                
                FormsAuthentication.SetAuthCookie(userName, false);

                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查用户登陆状态并实现跳转
        /// </summary>
        public static void CheckAndRedirect()
        {
            CheckAndRedirect(null);
        }

        /// <summary>
        /// 检查用户登陆状态并实现跳转
        /// </summary>
        public static void CheckAndRedirect(string backUrl)
        {
            var response = HttpContext.Current.Response;
            var request = HttpContext.Current.Request;
            if (!IsLogin())
            {
                response.Redirect("/Login.aspx");
            }
        }

        /// <summary>
        /// 登出
        /// </summary>
        public static void LoginOut()
        {
            FormsAuthentication.SignOut();
        }
    }
}
