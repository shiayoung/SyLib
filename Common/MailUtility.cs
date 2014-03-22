using System;
using System.ComponentModel;
using System.Text;
using System.Net.Mail;
using System.Net;

namespace Core.Common
{
    public static class MailUtility
    {
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="to">目标邮箱</param>
        /// <param name="subject">邮件主题</param>
        /// <param name="body">邮件内容，支持HTML</param>
        /// <param name="error">发送失败时的错误信息</param>
        /// <returns>返回发送是否成功</returns>
        public static bool Send(string to, string subject, string body, out string error)
        {
            var mailAccount = GetMailAccount();

            try
            {
                using (var client = new SmtpClient(mailAccount.Host, 25))
                {
                    client.Credentials = new NetworkCredential(mailAccount.Mail, mailAccount.Password);
                    
                    var msg = new MailMessage(mailAccount.Mail, to, subject, body)
                    {
                        SubjectEncoding = Encoding.UTF8,
                        BodyEncoding = Encoding.UTF8,
                        IsBodyHtml = true
                    };
                    client.Send(msg);
                }

                error = string.Empty;
                return true;
            }
            catch (SmtpException e)
            {
                error = "StatusCode:" + e.StatusCode + "," + e.ToString();
                return false;
            }
            catch (Exception e)
            {
                error = e.ToString();
                return false;
            }
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mail">邮件实体</param>
        /// <param name="error">发送失败时的错误信息</param>
        /// <returns>返回发送是否成功</returns>
        public static bool Send(MailMessage mail, out string error)
        {
            var mailAccount = GetMailAccount();

            try
            {
                using (var client = new SmtpClient(mailAccount.Host, 25))
                {
                    client.Credentials = new NetworkCredential(mailAccount.Mail, mailAccount.Password);
                    client.Send(mail);
                }

                error = string.Empty;
                return true;
            }
            catch (SmtpException e)
            {
                error = "StatusCode:" + e.StatusCode + "," + e.ToString();
                return false;
            }
            catch (Exception e)
            {
                error = e.ToString();
                return false;
            }
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mail">邮件实体</param>
        /// <param name="error">发送失败时的错误信息</param>
        /// <returns>返回发送是否成功</returns>
        public static bool SendASync(MailMessage mail, out string error)
        {
            var mailAccount = GetMailAccount();

            try
            {
                using (var client = new SmtpClient(mailAccount.Host, 25))
                {
                    client.Credentials = new NetworkCredential(mailAccount.Mail, mailAccount.Password);
                    client.SendCompleted += new
            SendCompletedEventHandler(SendCompletedCallback);
                    client.SendAsync(mail, mail.Subject);
                }
                error = string.Empty;
                return true;
            }
            catch (SmtpException e)
            {
                error = "StatusCode:" + e.StatusCode + "," + e.ToString();
                Tools.SafeLogMsg(error, typeof(MailUtility));
                return false;
            }
            catch (Exception e)
            {
                error = e.ToString();
                return false;
            }
        }

        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            String token = (string)e.UserState;

            if (e.Cancelled)
            {
                // 取消發送引發
            }
            if (e.Error != null)
            {
                // 發送錯誤
                Tools.SafeLogMsg("發送錯誤", typeof(MailUtility));
            }
            else
            {
                // 發送成功
                Tools.SafeLogMsg("發送成功", typeof(MailUtility));
            }
        }

        public struct MailAccount
        {
            public string Host { get; set; }

            public string Mail { get; set; }

            public string Password { get; set; }
        }

        public static MailAccount GetMailAccount()
        {
            var account = System.Configuration.ConfigurationManager.AppSettings["MailAccount"].ToString();
            var parts = account.Split(',');
            if (parts.Length != 3)
                throw new FormatException("配置项 MailAccount 格式不正确，应该为 “主机地址,邮箱地址,密码”。");

            return new MailAccount
            {
                Host = parts[0],
                Mail = parts[1],
                Password = parts[2]
            };
        }
    }
}
