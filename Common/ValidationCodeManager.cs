using System;

namespace Core.Common
{
    /// <summary>
    /// 验证码管理类
    /// </summary>
    public class VerificationCodeManager
    {
        /// <summary>
        /// 验证码字符源
        /// </summary>
        private static readonly string[] source = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };

        /// <summary>
        /// 获取或设置 验证码
        /// </summary>
        public static string VerificationCode
        {
            get
            {
                string verificationCode;
                if (!SessionManager.TryGet<string>(SessionManager.SessionKeys.VerificationCode, out verificationCode))
                {
                    verificationCode = null;
                }

                return verificationCode;
            }
            private set
            {
                SessionManager.Set(SessionManager.SessionKeys.VerificationCode, value);
            }
        }

        /// <summary>
        /// 生成验证码并设置验证码
        /// </summary>
        /// <param name="codeLength">生成验证码的长度</param>
        /// <returns>验证码</returns>
        public static string BuildCode(int codeLength)
        {
            var code = CreateCode(codeLength);

            VerificationCode = code;

            return code;
        }

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="codeLength">生成验证码的长度</param>
        /// <returns>验证码</returns>
        private static string CreateCode(int codeLength)
        {
            string[] strArr = source;
            var rand = new Random();
            var codeArray = new string[codeLength];
            for (int i = 0; i < codeLength; i++)
            {
                codeArray[i] = strArr[rand.Next(0, strArr.Length)];
            }

            return string.Concat(codeArray);
        }


    }
}