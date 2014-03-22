using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using Core.BLL;
using Core.Model;
using GGWebLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Core.Common
{
    public class Tools
    {

        public static string GetTimeRandom()
        {
            Random random = new Random();
            return DateTime.Now.ToString("yyyyMMddHHmmssfff") + random.Next(1, 9).ToString();
        }

        /// <summary>
        /// 生成Json格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetJson<T>(T obj)
        {
            var timeConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss" };
            //这里使用自定义日期格式，如果不使用的话，默认是ISO8601格式     
            return JsonConvert.SerializeObject(obj, Formatting.None, timeConverter, new DecimalConverter());
        }
        /// <summary>
        /// 获取Json的Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="szJson"></param>
        /// <returns></returns>
        public static T ParseFromJson<T>(string szJson)
        {
            var obj = Activator.CreateInstance<T>();
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(szJson)))
            {
                var serializer = new DataContractJsonSerializer(obj.GetType());
                return (T)serializer.ReadObject(ms);
            }
        }

        /// <summary>
        /// 安全地记录一个异常对象。
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="type">引发异常的类</param>
        public static void SafeLogException(Exception ex, Type type)
        {
            log4net.ILog logger = log4net.LogManager.GetLogger(type);
            if (ex is MyMessageException)
                return;

            if (ex is HttpException)
            {
                HttpException ee = ex as HttpException;
                if (ee.GetHttpCode() == 404)
                    return;
            }

            try
            {
                string message = string.Concat(
                    "Request Url: ", HttpContext.Current.Request.RawUrl, "\r\n",
                    ExecptionHelper.GetExecptionDetailInfo(ex), "\r\n\r\n\r\n");

                logger.Error(message);
            }
            catch { }
        }

        /// <summary>
        /// 安全地记录一个异常对象。
        /// </summary>
        /// <param name="msg">日志信息</param>
        /// <param name="type">引发异常的类</param>
        public static void SafeLogMsg(object msg, Type type)
        {
            log4net.ILog logger = log4net.LogManager.GetLogger(type);
            if (msg != null)
            {
                try
                {
                    logger.Debug(msg);
                }
                catch { }
            }
        }

        /// <summary>
        /// 將 Enum 類型的名稱和值轉為 ListItem 的 Text 和 Value
        /// </summary>
        /// <typeparam name="T">Enum 類型</typeparam>
        /// <returns>ListItem 集合</returns>
        public static ListItemCollection EnumToList<T>()
        {
            var li = new ListItemCollection();
            foreach (int s in Enum.GetValues(typeof(T)))
            {
                li.Add(new ListItem
                {
                    Value = s.ToString(),
                    Text = Enum.GetName(typeof(T), s)
                });
            }
            return li;
        }

        /// <summary>
        /// 將 Enum 類型的名稱和值轉為 ListItem 的 Text 和 Value
        /// <typeparam name="T">Enum 類型</typeparam>
        /// <typeparam name="TAttr">特性类型</typeparam>
        /// <returns>ListItem 集合</returns>
        /// </summary>
        public static ListItemCollection EnumToList<T, TAttr>()
        {
            var li = new ListItemCollection();
            Type enumType = typeof(T);
            var flds = enumType.GetFields();
            for (int i = 1; i < flds.Length; i++)
            {
                var ignoreAttrs = flds[i].GetCustomAttributes(typeof(TAttr), false);
                if (ignoreAttrs.Length == 0)
                {
                    li.Add(new ListItem
                    {
                        Value = Enum.Format(enumType, Enum.Parse(enumType, flds[i].Name), "d"),
                        Text = flds[i].Name
                    });
                }
            }
            return li;
        }

        public static string GetFileExtension(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return "";
            int lastidx = filename.LastIndexOf('.');
            if (lastidx == -1)
                return "";
            return filename.Substring(lastidx);
        }

        /// <summary>
        /// 检查上传文件的大小是否合适，如合适，返回 true，否则，返回 false
        /// </summary>
        /// <param name="filesize">所上传文件的大小</param>
        /// <returns></returns>
        public static bool CheckFileSize(int filesize)
        {
            int maxSize;
            return CheckFileSize(filesize, out maxSize);
        }

        public static bool CheckFileSize(int filesize, out int maxSize)
        {
            var sMaxImgSize = ConfigurationManager.AppSettings["maxImgSize"];
            var defaultM = 400;
            if (!string.IsNullOrEmpty(sMaxImgSize))
            {
                int.TryParse(sMaxImgSize, out defaultM);
            }
            maxSize = defaultM * 1024;
            return filesize < maxSize;
        }

        /// <summary>
        /// 允许上传的文件扩展名正则式
        /// </summary>
        public static string GetAllowedFileExts()
        {
            var allowed = ConfigurationManager.AppSettings["allowedFileExts"];
            if (string.IsNullOrEmpty(allowed))
            {
                return "(jpg|jpeg)";
            }
            else
            {
                var exts = allowed.Split(',');
                return "(" + string.Join("|", exts) + ")";
            }
        }

        private sealed class DecimalConverter : JsonConverter
        {
            #region Overrides of JsonConverter

            /// <summary>
            /// Writes the JSON representation of the object.
            /// </summary>
            /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param><param name="value">The value.</param><param name="serializer">The calling serializer.</param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                string text = Convert.ToDecimal(value).ToString("N2");
                writer.WriteValue(text);
            }

            /// <summary>
            /// Reads the JSON representation of the object.
            /// </summary>
            /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param><param name="objectType">Type of the object.</param><param name="existingValue">The existing value of object being read.</param><param name="serializer">The calling serializer.</param>
            /// <returns>
            /// The object value.
            /// </returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Determines whether this instance can convert the specified object type.
            /// </summary>
            /// <param name="objectType">Type of the object.</param>
            /// <returns>
            /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
            /// </returns>
            public override bool CanConvert(Type objectType)
            {
                if (objectType == typeof(Decimal))
                    return true;
                return false;
            }

            #endregion
        }
    }
}
