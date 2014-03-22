using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
namespace GGWebLib
{
    /// <summary>
    /// 一些专用于WebForm的工具函数
    /// </summary>
    public static class GGWebFormHelper
    {
        /// <summary>
        /// 获取 Request Form 中的__EVENTTARGET
        /// </summary>
        public static string PostBackTarget
        {
            get
            {
                return HttpContext.Current.Request.Form["__EVENTTARGET"];
            }
        }
        /// <summary>
        /// 获取 Request Form 中的__EVENTARGUMENT
        /// </summary>
        public static string PostBackArgument
        {
            get
            {
                return HttpContext.Current.Request.Form["__EVENTARGUMENT"];
            }
        }
        /// <summary>
        /// <para>从指定的Request.Form 中读取指定的值，</para>
        /// <para>由于WebForms控件名可能包含前缀，所以这个方法将使用“尾部匹配”的原则。</para>
        /// <para>如果没有找到匹配项，则返回 null</para>
        /// </summary>
        /// <param name="form">NameValueCollection对象</param>
        /// <param name="key">键名</param>
        /// <returns>读取到的结果</returns>
        public static string GetValueFromRequestForm(NameValueCollection form, string key)
        {
            if (form == null)
            {
                throw new ArgumentNullException("form");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            string text = form[key];
            if (text == null)
            {
                string[] allKeys = form.AllKeys;
                for (int i = 0; i < allKeys.Length; i++)
                {
                    string text2 = allKeys[i];
                    if (text2.EndsWith(key, StringComparison.OrdinalIgnoreCase))
                    {
                        return form[text2];
                    }
                }
            }
            return text;
        }
        /// <summary>
        /// 从指定的Request.Form 中读取指定的值，这个方法将使用“Key名称全字符匹配”的原则，直接调用：form[key];
        /// </summary>
        /// <param name="form">NameValueCollection对象</param>
        /// <param name="key">键名</param>
        /// <returns>读取到的结果</returns>
        public static string GetValueFromRequestFormUseFullKeyNameMatch(NameValueCollection form, string key)
        {
            if (form == null)
            {
                throw new ArgumentNullException("form");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            return form[key];
        }
        /// <summary>
        /// <para>从当前请求的 Request.Form 中读取指定的值，</para>
        /// <para>有时控件名可能包含前缀，所以(有可能)只作部分匹配的原则。</para>
        /// <para>如果没有找到匹配项，则返回 null</para>
        /// </summary>
        /// <param name="key">key name</param>
        /// <returns></returns>
        public static string GetValueFromRequestForm(string key)
        {
            return GGWebFormHelper.GetValueFromRequestForm(HttpContext.Current.Request.Form, key);
        }
        /// <summary>
        /// 直接从Request Form 中读取控件的值， 如果没有找到匹配项，则返回 null
        /// </summary>
        /// <param name="ctl">控件引用</param>
        /// <returns>控件的值</returns>
        public static string GetValueFromRequestForm(Control ctl)
        {
            if (ctl == null)
            {
                throw new ArgumentNullException("ctl");
            }
            string text = HttpContext.Current.Request.Form[ctl.UniqueID];
            return text ?? GGWebFormHelper.GetValueFromRequestForm(ctl.ID);
        }
        /// <summary>
        /// 从当前请求的Request.Form中读取一个 string 值。默认值：""
        /// </summary>
        /// <param name="key">键名</param>
        /// <returns>读取到的结果</returns>
        public static string GetStringFromRequestForm(string key)
        {
            return GGWebFormHelper.GetValueFromRequestForm(key) ?? string.Empty;
        }
        /// <summary>
        /// 从当前请求的Request.Form中读取一个 int 值。默认值：0
        /// </summary>
        /// <param name="key">键名</param>
        /// <returns>读取到的结果</returns>
        public static int GetIntFromRequestForm(string key)
        {
            return StringHelper.TryToInt(GGWebFormHelper.GetValueFromRequestForm(key));
        }
        /// <summary>
        /// 从当前请求的Request.Form中读取一个 int 值。
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="defaultVal">默认值</param>
        /// <returns>读取到的结果</returns>
        public static int GetIntFromRequestForm(string key, int defaultVal)
        {
            return StringHelper.TryToInt(GGWebFormHelper.GetValueFromRequestForm(key), defaultVal);
        }
        /// <summary>
        /// 从当前请求的Request.Form中读取一个 bool 值。默认值：false
        /// </summary>
        /// <param name="key">键名</param>
        /// <returns>读取到的结果</returns>
        public static bool GetBoolFromRequestForm(string key)
        {
            return StringHelper.ObjectIsTrue(GGWebFormHelper.GetValueFromRequestForm(key));
        }
        /// <summary>
        /// 从当前请求的Request.Form中读取一个 DateTime 值。默认值：DateTime.Now
        /// </summary>
        /// <param name="key">键名</param>
        /// <returns>读取到的结果</returns>
        public static DateTime GetDateTimeFromRequestForm(string key)
        {
            DateTime result;
            if (DateTime.TryParse(GGWebFormHelper.GetValueFromRequestForm(key), out result))
            {
                return result;
            }
            return DateTime.Now;
        }
        /// <summary>
        /// <para>检查当前URL中是否存在 returnUrl 的参数。</para>
        /// <para>如果有，则重定向到那个页面，【并不终止】当前页面的执行。</para>
        /// <para>否则，重定向到当前请求地址。</para>
        /// </summary>
        public static void TryRedirectToReturnUrl()
        {
            GGWebFormHelper.TryRedirectToReturnUrl(true);
        }
        /// <summary>
        /// <para>检查当前URL中是否存在 returnUrl 的参数。</para>
        /// <para>如果有，则重定向到那个页面，【并终止】当前页面的执行。</para>
        /// </summary>
        /// <param name="allowRedirectThis">如果没有returnUrl参数，是否用当前地址刷新一次</param>
        public static void TryRedirectToReturnUrl(bool allowRedirectThis)
        {
            HttpContext current = HttpContext.Current;
            string text = current.Request.QueryString["returnUrl"];
            if (!string.IsNullOrEmpty(text))
            {
                current.Response.Redirect(text, true);
                return;
            }
            if (allowRedirectThis)
            {
                current.Response.Redirect(current.Request.RawUrl, true);
            }
        }
        /// <summary>
        /// 检查当前URL中是否存在 returnUrl 的参数。如果有，则重定向到那个页面，【并终止】当前页面的执行。
        /// </summary>
        /// <param name="newURL">如果没有returnUrl的参数，则重定向到这个地址</param>
        public static void TryRedirectToReturnUrl(string newURL)
        {
            if (string.IsNullOrEmpty(newURL))
            {
                throw new ArgumentNullException("newURL");
            }
            HttpContext current = HttpContext.Current;
            string text = current.Request.QueryString["returnUrl"];
            if (!string.IsNullOrEmpty(text))
            {
                current.Response.Redirect(text, true);
                return;
            }
            current.Response.Redirect(newURL, true);
        }
        /// <summary>
        /// 检查当前URL中是否存在 returnUrl 的参数。如果有，则返回，
        /// 否则，如果defaultUrl有效，则返回defaultUrl，
        /// 最后，返回当前地址。
        /// </summary>
        /// <param name="defaultUrl"></param>
        /// <returns></returns>
        public static string TryGetReturnUrl(string defaultUrl)
        {
            HttpContext current = HttpContext.Current;
            if (current == null)
            {
                return defaultUrl;
            }
            string text = current.Request.QueryString["returnUrl"];
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
            if (string.IsNullOrEmpty(defaultUrl))
            {
                return current.Request.RawUrl;
            }
            return defaultUrl;
        }
        /// <summary>
        /// 将页头标记为“不缓存”
        /// </summary>
        /// <param name="page">Page对象</param>
        public static void SetNoCacheMeta(Page page)
        {
            if (page == null)
            {
                throw new ArgumentNullException("page");
            }
            HtmlMeta htmlMeta = new HtmlMeta();
            htmlMeta.Name = "Pragma";
            htmlMeta.Content = "no-cache";
            page.Header.Controls.Add(htmlMeta);
            HtmlMeta htmlMeta2 = new HtmlMeta();
            htmlMeta2.Name = "Expires";
            htmlMeta2.Content = "0";
            page.Header.Controls.Add(htmlMeta2);
        }
        /// <summary>
        /// 显示一个JS的alert()消息框
        /// </summary>
        /// <param name="page">Page对象</param>
        /// <param name="message">要显示的消息</param>
        public static void ShowMessageBox(Page page, string message)
        {
            if (page == null)
            {
                throw new ArgumentNullException("page");
            }
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message");
            }
            page.ClientScript.RegisterStartupScript(page.GetType(), Guid.NewGuid().ToString(), string.Format("alert('{0}');\r\n", message.Replace("'", "\\'")), true);
        }
        /// <summary>
        /// 显示一个JS的alert()消息框，可以指定弹出这个对话框时，这个调用要不要放在JQuery的 $(document).readery();中
        /// </summary>
        /// <param name="page">Page对象</param>
        /// <param name="message">要显示的消息</param>
        /// <param name="wrapByJQuery">是否要放在JQuery的 $(document).readery();中</param>
        public static void ShowMessageBox(Page page, string message, bool wrapByJQuery)
        {
            if (page == null)
            {
                throw new ArgumentNullException("page");
            }
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message");
            }
            if (wrapByJQuery)
            {
                page.ClientScript.RegisterStartupScript(page.GetType(), Guid.NewGuid().ToString(), string.Format("$(function() {{ alert('{0}');  }});\r\n", message.Replace("'", "\\'")), true);
                return;
            }
            GGWebFormHelper.ShowMessageBox(page, message);
        }
        /// <summary>
        /// 加态添加一个样式表文件到页面头
        /// </summary>
        /// <param name="header">HtmlHead对象</param>
        /// <param name="filename">文件路径</param>
        public static void AddCssFileToHeader(HtmlHead header, string filename)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }
            HtmlGenericControl htmlGenericControl = new HtmlGenericControl();
            htmlGenericControl.TagName = "link";
            htmlGenericControl.Attributes.Add("type", "text/css");
            htmlGenericControl.Attributes.Add("href", filename);
            htmlGenericControl.Attributes.Add("rel", "Stylesheet");
            header.Controls.Add(htmlGenericControl);
        }
        /// <summary>
        /// 设置页的一些Meta元素，如：用于SEO的keywords , description
        /// </summary>
        /// <param name="header">HtmlHead对象</param>
        /// <param name="name">name</param>
        /// <param name="content">content</param>
        public static void AddMetaToHeader(HtmlHead header, string name, string content)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException("content");
            }
            HtmlGenericControl htmlGenericControl = new HtmlGenericControl();
            htmlGenericControl.TagName = "meta";
            htmlGenericControl.Attributes.Add("name", name);
            htmlGenericControl.Attributes.Add("content", content);
            header.Controls.Add(htmlGenericControl);
        }
        /// <summary>
        /// <para>设置一个下拉组合框能在用户选择一个项目后，使用浏览器重定向到所选的项目地址。</para>
        /// <para>注意：这就要求下拉组合框的每个option的Value是一个URL</para>
        /// </summary>
        /// <param name="ddl">DropDownList控件</param>
        /// <param name="writeCustTag">是否写入“自定义”的标签“autoRedire=true”</param>
        public static void SetDropDownListEnableAutoRedir(DropDownList ddl, bool writeCustTag)
        {
            if (ddl == null)
            {
                throw new ArgumentNullException("ddl");
            }
            ddl.Attributes.Add("onchange", "javascript:setTimeout('window.location.href = document.getElementById(\\'" + ddl.ClientID + "\\').value', 0)");
            if (writeCustTag)
            {
                ddl.Attributes.Add("autoRedire", "true");
            }
        }
        /// <summary>
        /// 强制一个下拉选择框选中某个选项，如果下拉框没指定的项，则增加它。最终将指定项选择。
        /// </summary>
        /// <param name="ddl">DropDownList控件</param>
        /// <param name="text">如果下拉框没指定的项，用于增加一个新项</param>
        /// <param name="value">要设置的控件值，如果下拉框没指定的项，用于增加一个新项</param>
        public static void ForceSetDropDownListSelectedValue(DropDownList ddl, string text, string value)
        {
            if (ddl == null)
            {
                throw new ArgumentNullException("ddl");
            }
            if (ddl.Items.FindByValue(value) == null)
            {
                ddl.Items.Add(new ListItem(text, value));
            }
            ddl.SelectedValue = value;
        }
        /// <summary>
        /// <para>将一个用户控件添加到网格的脚注行。</para>
        /// <para>警告：一定要在GridView1.DataBind()后调用。在调用前设置GridView1.ShowFooter = true;</para>
        /// </summary>
        /// <param name="grid">GridView对象</param>
        /// <param name="ctl">UserControl对象</param>
        public static void InsertUserControlToGridFooterRow(GridView grid, UserControl ctl)
        {
            if (grid == null)
            {
                throw new ArgumentNullException("grid");
            }
            if (ctl == null)
            {
                throw new ArgumentNullException("ctl");
            }
            if (grid.FooterRow == null)
            {
                return;
            }
            TableCell tableCell = new TableCell();
            tableCell.ColumnSpan = grid.Columns.Count;
            tableCell.Controls.Add(ctl);
            grid.FooterRow.Cells.Clear();
            grid.FooterRow.Cells.Add(tableCell);
        }
        /// <summary>
        /// 合并二个URL路径，有点类似于 System.IO.Path.Combine()
        /// </summary>
        /// <param name="path1">path1</param>
        /// <param name="path2">path2</param>
        /// <returns></returns>
        public static string UrlPathCombin(string path1, string path2)
        {
            if (path2 == null)
            {
                throw new ArgumentNullException("path2");
            }
            if (path2.StartsWith("/"))
            {
                return path2;
            }
            if (string.IsNullOrEmpty(path1))
            {
                return path2;
            }
            if (!path1.EndsWith("/"))
            {
                return path1 + "/" + path2;
            }
            return path1 + path2;
        }
        /// <summary>
        /// 获取当前浏览器的类型。比如：IE6
        /// </summary>
        /// <returns>当前浏览器的类型</returns>
        public static string GetCurrentRequestBrowserType()
        {
            HttpRequest request = HttpContext.Current.Request;
            if (request.Browser == null)
            {
                return string.Empty;
            }
            return request.Browser.Type ?? string.Empty;
        }
    }
}
