using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using Core.BLL;
using Core.Model;

namespace Core.Common
{
    public class TextHelper
    {
        /// <summary>
        /// 以带单位的形式显示数量
        /// </summary>
        /// <param name="qty"></param>
        /// <returns></returns>
        public static string GetQuantityWithUnit(string qty)
        {
            int iQty = 0;
            int.TryParse(qty, out iQty);
            return GetQuantityWithUnit(iQty);
        }

        public static string GetQuantityWithUnit(int qty)
        {
            return qty < 1000 ? qty + "g" : qty / 1000 + "kg";
        }

        public static string FormatDecimal(object decVal)
        {
            if (decVal == null)
                return "";
            try
            {
                return Convert.ToDecimal(decVal).ToString("N2");
            }
            catch
            {
                return "0.00";
            }
        }


        public enum DateFormat
        {
            yyyyMMddHHmmss,
            yyyyMMddHHmm,
            yyyyMMdd
        }

        public static string FormatDate(object dtVal, DateFormat df = DateFormat.yyyyMMdd)
        {
            if (dtVal == null)
                return "";
            try
            {
                if (df == DateFormat.yyyyMMddHHmmss)
                    return Convert.ToDateTime(dtVal).ToString("yyyy-MM-dd HH:mm:ss");
                if (df == DateFormat.yyyyMMddHHmm)
                    return Convert.ToDateTime(dtVal).ToString("yyyy-MM-dd HH:mm");
                if (df == DateFormat.yyyyMMdd)
                    return Convert.ToDateTime(dtVal).ToString("yyyy-MM-dd");
                return Convert.ToDateTime(dtVal).ToString("yyyy-MM-dd");
            }
            catch
            {
                throw new FormatException("格式化的源对象不正确");
            }
        }

        /// <summary>
        /// 获取日期查询的 SQL 字符串，包含日期对应的列名，但不包含 WHERE 和前后的 AND 关键字
        /// </summary>
        /// <param name="column">列名</param>
        /// <param name="start">开始日期</param>
        /// <param name="end">结束日期</param>
        /// <returns>组合的 SQL 字符串</returns>
        public static string GetDateQueryString(string column, string start, string end)
        {
            string ret = "";
            // 开始日期与结束日期均指定
            if (!String.IsNullOrEmpty(start) && !String.IsNullOrEmpty(end))
            {
                ret = column + " >= '" + start + "' and " + column + " < DATEADD(DAY, 1, '" + end + "') ";
            }
            // 指定开始日期, 结束日期未指定
            else if (!String.IsNullOrEmpty(start))
            {
                ret = column + " > '" + start + "'";
            } // 指定结束日期，开始日期未指定
            else if (!String.IsNullOrEmpty(end))
            {
                ret = column + " < DATEADD(DAY, 1, '" + end + "') ";
            }
            return ret;
        }

        /// <summary>
        /// 获取日期时间查询的 SQL 字符串，包含日期时间对应的列名，但不包含 WHERE 和前后的 AND 关键字
        /// </summary>
        /// <param name="column">列名</param>
        /// <param name="start">开始日期时间</param>
        /// <param name="end">结束日期时间</param>
        /// <returns>组合的 SQL 字符串</returns>
        public static string GetDateQueryString(string column, DateTime? start, DateTime? end)
        {
            string ret = "";
            // 开始日期与结束日期均指定
            if (start.HasValue && end.HasValue)
            {
                ret = column + " >= '" + start.Value.ToString("yyyy-MM-dd") + "' and " + column + " < '" +
                      Convert.ToDateTime(end.Value).AddDays(1).ToString("yyyy-MM-dd") + "'";
            }
            // 指定开始日期, 结束日期未指定
            else if (start.HasValue)
            {
                ret = column + " >= '" + start.Value.ToString("yyyy-MM-dd") + "'";
            } // 指定结束日期，开始日期未指定
            else if (end.HasValue)
            {
                ret = column + " < '" + Convert.ToDateTime(end.Value).AddDays(1).ToString("yyyy-MM-dd") + "'";
            }
            return ret;
        }

        /// <summary>
        /// 未成交订单的下一个有效期
        /// </summary>
        /// <returns></returns>
        public static DateTime Next11AM(DateTime baseTime)
        {
            var ts = new TimeSpan(0, 11, 15, 0);
            var ticks = ts.Ticks;
            if (baseTime.TimeOfDay.CompareTo(ts) <= 0)
            {
                return baseTime.Date.AddTicks(ticks);
            }
            var nextDay = baseTime.Date.AddDays(1);
            var holidayBLL = new HolidayBLL();
            var i = 1;
            while (0 != i)
            {
                var allHoliday = holidayBLL.GetAllByYear(nextDay.Year);
                i = allHoliday.Count(h => h.SetDate == nextDay);
                if (0 != i)
                {
                    // 一直前进，直至有一天不是假日
                    nextDay = nextDay.Date.AddDays(1);
                }
            }
            return nextDay.AddTicks(ticks);
        }

        /// <summary>
        /// 获取上一交易日
        /// </summary>
        /// <returns></returns>
        public static DateTime PreviousTradeDate(DateTime baseTime)
        {
            //var ts = new TimeSpan(0, 11, 15, 0);
            //var ticks = ts.Ticks;
            //if (baseTime.TimeOfDay.CompareTo(ts) <= 0)
            //{
            //    return baseTime;
            //}
            var prevDay = baseTime.Date.AddDays(-1);
            var holidayBLL = new HolidayBLL();
            var i = 1;
            while (0 != i)
            {
                var allHoliday = holidayBLL.GetAllByYear(prevDay.Year);
                i = allHoliday.Count(h => h.SetDate == prevDay);
                if (0 != i)
                {
                    // 一直后退，直至有一天不是假日
                    prevDay = prevDay.Date.AddDays(-1);
                }
            }
            return prevDay;
        }

        /// <summary>
        /// T+2 计算, 从所指定的日期开始，获取 T+2 的日期值
        /// </summary>
        /// <param name="start">开始的日期</param>
        /// <returns>返回T + 2 后的日期</returns>
        public static DateTime GetTPlus2(DateTime start)
        {
            if (start.DayOfWeek <= DayOfWeek.Wednesday)
                return start.Date.AddDays(2);
            return start.Date.AddDays(4);
        }

        /// <summary>
        /// 判断当前时间是不是交易时间
        /// </summary>
        /// <returns></returns>
        public static bool IsTradeTime()
        {
            var siteconfigBll = new SiteConfigBLL();
            var holidayBll = new HolidayBLL();
            var ts = new TimeSpan(0, 11, 15, 0);

            if (DateTime.Now.TimeOfDay.CompareTo(ts) <= 0)
            {
                return true;
            }
            // 定价确认点
            var tsConfirm = new TimeSpan(0, 13, 00, 0);
            var lastPrice = siteconfigBll.GetLastDay(false);
            var lastSetPriceDay = lastPrice.CreateTime.Date;
            var isValid = lastPrice.IsValid;

            if (!holidayBll.IsHoliday(DateTime.Now))
            {
                // 是交易日
                if ( // 超过 11：15 分，但未发布定价
                    (DateTime.Now.TimeOfDay.CompareTo(ts) > 0 &&
                     lastSetPriceDay.Subtract(DateTime.Now.Date).Days < 0) ||
                    // 定价已发布，但未到 13：00 这个确认时间
                    (lastSetPriceDay.Subtract(DateTime.Now.Date).Days == 0 &&
                     DateTime.Now.TimeOfDay.CompareTo(tsConfirm) <= 0) ||
                    isValid != 1)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取随机序列
        /// </summary>
        /// <returns></returns>
        public static string GetRandomedSn()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss") + (new Random().Next(10, 99));
        }

        public static string GetProfile(string profile, string profileValue, string property)
        {
            var propertyValue = "";
            var names = profile.Split(new char[] { ':' });
            if (((names.Length != 0) && (profileValue != null)))
            {
                try
                {
                    for (int i = 0; i < (names.Length / 3); i++)
                    {
                        string str = names[i * 3];
                        if (str != null && str.Equals(property))
                        {
                            int startIndex = int.Parse(names[(i * 3) + 1], CultureInfo.InvariantCulture);
                            int length = int.Parse(names[(i * 3) + 2], CultureInfo.InvariantCulture);
                            if ((length == -1))
                            {
                                // error
                            }
                            propertyValue = profileValue.Substring(startIndex, length);
                        }
                    }
                }
                catch
                {
                    propertyValue = null;
                }
            }
            return propertyValue;
        }
    }
}
