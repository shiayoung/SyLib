using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Core.Common
{
    public sealed class CookieManager
    {
        /// <summary>
        /// 向客户端添加Cookie
        /// </summary>
        public static void SetCookie(string name, string value)
        {
            HttpCookie cookie = new HttpCookie(name);
            cookie.Value = value;
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// 向客户端添加是否加密的Cookie
        /// </summary>
        public static void SetCookie(string name, string value, bool encrypt)
        {
            HttpCookie cookie = new HttpCookie(name);
            if (encrypt)
            {
                value = Encrypt.MD5Encrypt(value);
            }
            cookie.Value = value;
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// 向客户端获取Cookie
        /// </summary>
        public static string GetCookie(string name)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[name];
            if (null != cookie)
            {
                return cookie.Value;
            }
            return string.Empty;
        }

        /// <summary>
        /// 向客户端获取是否加密的Cookie
        /// </summary>
        public static string GetCookie(string name, bool encrypt)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[name];
            if (null != cookie && !cookie.Value.IsEmpty())
            {
                return Encrypt.MD5Decrypt(cookie.Value);
            }
            return string.Empty;
        }
    }
}
