using System.Windows.Forms;
using Shamsullin.Common.Extensions;
using Shamsullin.Win.Properties;

namespace Shamsullin.Win
{
    public static class JQueryExtensions
    {
        public static void JQuery(this HtmlDocument doc)
        {
            var script = doc.CreateElement("script");
            script.SetAttribute("text", Resources.Jquery);
            script.SetAttribute("type", "text/javascript");
            doc.GetElementsByTagName("head")[0].AppendChild(script);
        }

        public static string JQuery(this HtmlDocument doc, string script)
        {
            var result = doc.InvokeScript("eval", new[] { script });
            return result.ToStr().Spaces().Trim();
        }

        public static string JQuery(this WebBrowser ie, string scriptFormat, params object[] args)
        {
            return ie.Document.JQuery(string.Format(scriptFormat, args));
        }

        public static string Spaces(this string input)
        {
			var result = input.ReplaceEx("  ", " ");
            while (result.Contains("  "))
            {
				result = result.ReplaceEx("  ", " ");
            }

            return result;
        }
    }
}
