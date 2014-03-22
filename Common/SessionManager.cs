using System;
using System.Linq;
using System.Web.SessionState;
using System.Web;

namespace Core.Common
{
    public class SessionManager
    {
        #region 字段

        /// <summary>
        /// 验证码 Key
        /// </summary>
        private const string VerificationCodeKey = "VerificationCode";

        /// <summary>
        /// 用户登陆 Key
        /// </summary>
        private const string UserLoginStateKey = "UserLoginState";

        /// <summary>
        /// 该网站所属的商户ID
        /// </summary>
        private const string FromAgentIdKey = "FromAgentId";

        /// <summary>
        /// 登陆前的Url地址 Key
        /// </summary>
        private const string UserLoginBackUrlKey = "UserLoginBackUrl";

        /// <summary>
        /// 管理员用户登陆 Key
        /// </summary>
        private const string AdminLoginStateKey = "AdminLoginState";

        /// <summary>
        /// 管理员登陆前的Url地址 Key
        /// </summary>
        private const string AdminLoginBackUrlKey = "AdminLoginBackUrl";

        /// <summary>
        /// 当前会话的商户信息
        /// </summary>
        private const string CurrentAgent = "CurrentAgent";

        /// <summary>
        /// 当前会话的商户信息
        /// </summary>
        private const string RechargeInfo = "RechargeInfo";

        /// <summary>
        /// Session Key 数组
        /// <para>存在的是 Session 会使用到的所有 Key 值</para>
        /// </summary>
        private static readonly
            string[] Keys =
                {
                    VerificationCodeKey,
                    UserLoginStateKey,
                    FromAgentIdKey,
                    UserLoginBackUrlKey,
                    AdminLoginStateKey,
                    AdminLoginBackUrlKey,
                    CurrentAgent,
                    RechargeInfo
                };

        #endregion

        #region 内部类/枚举

        /// <summary>
        /// 会话键枚举
        /// </summary>
        public enum SessionKeys : byte
        {
            /// <summary>
            /// 验证码
            /// </summary>
            VerificationCode = 0,

            /// <summary>
            /// 用户登陆
            /// </summary>
            UserLoginState = 1,

            /// <summary>
            /// 用户登陆前的Url地址
            /// </summary>
            UserLoginBackUrl = 2,

            /// <summary>
            /// 管理员用户登陆 Key
            /// </summary>
            AdminLoginStateKey = 3,

            /// <summary>
            /// 管理员登陆前的Url地址 Key
            /// </summary>
            AdminLoginBackUrlKey = 4,

            /// <summary>
            /// 
            /// </summary>
            FromAgentIdKey = 5,

            /// <summary>
            /// 当前会话的商户信息
            /// </summary>
            CurrentAgent = 6,

            /// <summary>
            /// 支付信息
            /// </summary>
            RechargeInfo = 7
        }

        #endregion

        #region 属性访问器

        /// <summary>
        /// HttpSessionState 的一个引用实例
        /// <para>直接引用于 HttpContext.Current.Session</para>
        /// </summary>
        private static HttpSessionState Session
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Session == null)
                {
                    throw new NullReferenceException("Invalid Operation, HttpContext or HttpSessionState is null!");
                }

                return HttpContext.Current.Session;
            }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 设置 Session 值
        /// </summary>
        /// <param name="key">存储于 Session 中的键枚举</param>
        /// <param name="value">存储于 Session 中的值，该值允许为null</param>
        /// <exception cref="ArgumentOutOfRangeException">当 SessionKeys 不包含枚举项 key 时</exception>
        public static void Set(SessionKeys key, object value)
        {
            Session[GetKey(key)] = value;
        }

        /// <summary>
        /// 根据键枚举获取值
        /// </summary>
        /// <typeparam name="T">欲获取的值的类型</typeparam>
        /// <param name="key">存储于 Session 中的键枚举</param>
        /// <returns>与key相对应的值</returns>
        public static T Get<T>(SessionKeys key)
        {
            return (T)Session[GetKey(key)];
        }

        /// <summary>
        /// 尝试从Session获取会话值
        /// </summary>
        /// <typeparam name="T">欲获取的值的类型</typeparam>
        /// <param name="key">存储于 Session 中的键枚举</param>
        /// <param name="value">获取成功时，该值为存储于Session中的值，否则为类型T的默认值。
        /// 但获取成功时，不代表value不为null，当你存储于Session中的值有可能为null时，那么取出来也有可能是null。</param>
        /// <returns>指示是否获取成功</returns>
        public static bool TryGet<T>(SessionKeys key, out T value)
        {
            var keyString = GetKey(key);
            var v = Session[keyString];

            if (v == null)
            {
                value = default(T);
                return Session.Keys.OfType<string>().Contains(keyString);
            }

            if (v.GetType() != typeof(T))
            {
                value = default(T);
                return false;
            }

            value = (T)v;
            return true;
        }

        /// <summary>
        /// 根据键枚举移除相应的会话值
        /// </summary>
        /// <param name="key">存储于 Session 中的键枚举</param>
        public static void Remove(SessionKeys key)
        {
            Session.Remove(GetKey(key));
        }

        /// <summary>
        /// 根据键枚举获取相应的键值
        /// </summary>
        /// <param name="key">键枚举</param>
        /// <returns>与键枚举相对应的键值</returns>
        private static string GetKey(SessionKeys key)
        {
            if (!Shine.Utility.EnumHelper.Contains(key))
            {
                throw new ArgumentOutOfRangeException("key");
            }

            return Keys[(byte)key];
        }

        #endregion
    }
}
