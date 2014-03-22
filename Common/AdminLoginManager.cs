using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Common.Monitor;
using Core.Model;
using Core.BLL;
using System.Web;
using System.Web.Security;
using System.Data.SqlClient;
using System.Data;

namespace Core.Common
{
    public sealed class AdminLoginManager
    {
        #region 属性访问器
        /// <summary>
        /// 获取 检查用户是否已登录
        /// </summary>
        public static bool IsLogin
        {
            get
            {
                return AdminUsersInfo != null;
            }
        }

        /// <summary>
        /// 获取 登陆的管理员信息
        /// <para>若用户未登录则返回 null</para>
        /// </summary>
        public static AdminUsersInfo AdminUsersInfo
        {
            get
            {
                AdminUsersInfo adminInfo;
                if (!SessionManager.TryGet<AdminUsersInfo>(SessionManager.SessionKeys.AdminLoginStateKey, out adminInfo))
                {
                    adminInfo = null;
                }

                return adminInfo;
            }
            private set
            {
                SessionManager.Set(SessionManager.SessionKeys.AdminLoginStateKey, value);
            }
        }

        /// <summary>
        /// 获取 回跳地址
        /// <para>若用户未登录则返回 null</para>
        /// </summary>
        public static string BackUrl
        {
            get
            {
                string backurl;
                if (!SessionManager.TryGet<string>(SessionManager.SessionKeys.AdminLoginBackUrlKey, out backurl))
                {
                    backurl = "/Admin/Admin_Center.aspx";
                }

                return backurl;
            }
            private set
            {
                SessionManager.Set(SessionManager.SessionKeys.AdminLoginBackUrlKey, value);
            }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 刷新管理员信息
        /// </summary>
        public static void Refresh()
        {
            var adminInfo = AdminUsersInfo;
            if (adminInfo != null)
            {
                var newInfo = AdminUsersBLL.Get(adminInfo.AdminId);
                if (newInfo != null && adminInfo.AdminId == newInfo.AdminId)
                {
                    AdminUsersInfo = newInfo;
                }
            }
        }

        /// <summary>
        /// 管理员登陆
        /// </summary>
        /// <param name="username">登陆用户名</param>
        /// <param name="password">密码</param>
        /// <returns>管理员信息</returns>
        public static AdminUsersInfo AdminLogin(string username, string password)
        {
            var adminInfo = AdminUsersBLL.AdminLogin(username, password);

            if (adminInfo != null && !adminInfo.IsLock)
            {
                SetLoginState(adminInfo);
                //if (adminInfo.RoleKind.Equals(RoleKind.系统超级管理员)
                //    || adminInfo.RoleKind.Equals(RoleKind.系统一般管理员))
                //{
                adminInfo.UploadTime = DateTime.Now;
                AdminUsersBLL.Update(adminInfo);
                //}
            }

            return adminInfo;
        }

        /// <summary>
        /// 代理管理员登陆
        /// </summary>
        /// <param name="username">登陆用户名</param>
        /// <param name="password">密码</param>
        /// <returns>管理员信息</returns>
        public static AdminUsersInfo AgentAdminLogin(string username, string password)
        {
            var adminInfo = AdminUsersBLL.AdminLogin(username, password);

            if (adminInfo != null && !adminInfo.IsLock)
            {
                if (adminInfo.RoleKind.Equals(RoleKind.AgentSuperAdmin)
                    || adminInfo.RoleKind.Equals(RoleKind.AgentAdmin))
                {
                    adminInfo.UploadTime = DateTime.Now;
                    AdminUsersBLL.Update(adminInfo);
                }

                SetLoginState(adminInfo);
            }

            return adminInfo;
        }

        /// <summary>
        /// 获取回跳地址
        /// </summary>
        /// <param name="rawUrl">回跳地址</param>
        /// <returns>是否有回跳地址</returns>
        public static bool GetBackUrl(out string rawUrl)
        {
            var result = SessionManager.TryGet<string>(SessionManager.SessionKeys.AdminLoginBackUrlKey, out rawUrl);
            if (!result)
            {
                rawUrl = "/Admin_Center.aspx";
            }

            return result;
        }

        /// <summary>
        /// 清除回跳地址
        /// </summary>
        public static void ClearBackUrl()
        {
            SessionManager.Remove(SessionManager.SessionKeys.AdminLoginBackUrlKey);
        }

        /// <summary>
        /// 退出登陆
        /// </summary>
        public static void Logout()
        {
            SetLoginState(null);
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
            if (!IsLogin)
            {
                SessionManager.Set(SessionManager.SessionKeys.AdminLoginBackUrlKey, backUrl ?? request.RawUrl);
                string WebServUrl = request.ApplicationPath;
                if (WebServUrl != "/")
                {
                    WebServUrl += "/";
                }
                response.Write("<script>top.location.href='" + WebServUrl + "login.aspx'</script>");
                response.End();
            }
            else
            {
                SessionManager.Set(SessionManager.SessionKeys.AdminLoginBackUrlKey, backUrl ?? request.RawUrl);
            }
        }

        /// <summary>
        /// 检查用户登陆状态并实现跳转
        /// </summary>
        public static void CheckAndParentRedirect(string backUrl)
        {


            var response = HttpContext.Current.Response;
            var request = HttpContext.Current.Request;
            if (!IsLogin)
            {
                SessionManager.Set(SessionManager.SessionKeys.AdminLoginBackUrlKey, backUrl ?? request.RawUrl);
                response.Write("<script>top.parent.location.href='../../login.aspx'</script>");
                response.End();
            }
        }

        /// <summary>
        /// 设置管理员的登录状态
        /// </summary>
        /// <param name="userInfo">需要被设置为登录的管理员信息</param>
        private static void SetLoginState(AdminUsersInfo adminInfo)
        {
            AdminUsersInfo = adminInfo;
        }

        #endregion
    }
}
