using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Core.Common
{
    public class ExcelHelper
    {
        public static void GridExportToExcel(GridView gdvShow, Page p, string excelName)
        {
            GridExportToExcel(gdvShow, p, excelName, System.Text.Encoding.Default.BodyName);
        }

        public static void GridExportToExcel(GridView gdvShow, Page p, string excelName, string charset)
        {
            gdvShow.Visible = true;
            p.Response.Clear();
            p.Response.Buffer = false;
            if (charset == string.Empty)
            {
                p.Response.Charset = "utf-8";
            }
            else
            {
                p.Response.Charset = charset;
            }
            p.Response.AppendHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(excelName, System.Text.Encoding.UTF8) + ".xls");
            p.Response.ContentEncoding = System.Text.Encoding.GetEncoding(charset);
            p.Response.ContentType = "application/ms-excel";
            p.EnableViewState = false;
            System.IO.StringWriter oStringWriter = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter oHtmlTextWriter = new System.Web.UI.HtmlTextWriter(oStringWriter);
            gdvShow.RenderControl(oHtmlTextWriter);
            p.Response.Write(oStringWriter.ToString());
            p.Response.End();
            gdvShow.Visible = false;
        }

        public static void GridExportToExcel(HtmlGenericControl div, Page p, string excelName, string charset)
        {
            div.Visible = true;
            p.Response.Clear();
            p.Response.Buffer = false;
            if (charset == string.Empty)
            {
                p.Response.Charset = "utf-8";
            }
            else
            {
                p.Response.Charset = charset;
            }
            p.Response.AppendHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(excelName, System.Text.Encoding.UTF8) + ".xls");
            p.Response.ContentEncoding = System.Text.Encoding.GetEncoding(charset);
            p.Response.ContentType = "application/ms-excel";
            p.EnableViewState = false;
            System.IO.StringWriter oStringWriter = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter oHtmlTextWriter = new System.Web.UI.HtmlTextWriter(oStringWriter);
            div.RenderControl(oHtmlTextWriter);
            p.Response.Write(oStringWriter.ToString());
            p.Response.End();
            div.Visible = false;
        }

        public static void GridExportToExcel(Page p, string excelName, string charset, params GridView[] gdvShow)
        {
            foreach (var gv in gdvShow)
            {
                gv.Visible = true;
            }

            p.Response.Clear();
            p.Response.Buffer = false;
            if (charset == string.Empty)
            {
                p.Response.Charset = "utf-8";
            }
            else
            {
                p.Response.Charset = "utf-8";
            }
            p.Response.AppendHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(excelName, System.Text.Encoding.UTF8) + ".xls");
            p.Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
            p.Response.ContentType = "application/ms-excel";
            p.EnableViewState = false;
            System.IO.StringWriter oStringWriter = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter oHtmlTextWriter = new System.Web.UI.HtmlTextWriter(oStringWriter);
            foreach (var gv in gdvShow)
            {
                gv.RenderControl(oHtmlTextWriter);
            }
            p.Response.Write(oStringWriter.ToString());
            p.Response.End();

            foreach (var gv in gdvShow)
            {
                gv.Visible = false;
            }
        }
    }
}
