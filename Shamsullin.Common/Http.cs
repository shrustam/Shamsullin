using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace Shamsullin.Common
{
    /// <summary>
    /// HTTP requests helper.
    /// </summary>
    public class Http
    {
        public static string Get(Uri url, NameValueCollection headers = null)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            if (headers != null) httpRequest.Headers.Add(headers);
            using (var response = (HttpWebResponse) httpRequest.GetResponse())
            {
                var result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return result;
            }
        }

        public static string Post(string url, string body, string contentType = "application/x-www-form-urlencoded", NameValueCollection headers = null)
        {
            var httpRequest = WebRequest.Create(url);
            if (headers != null) httpRequest.Headers.Add(headers);
            httpRequest.ContentType = contentType;
            httpRequest.Method = "POST";
            httpRequest.Timeout = 30000;
            //ServicePointManager.ServerCertificateValidationCallback = ValidateRemoteCertificate;
            byte[] bytes = Encoding.UTF8.GetBytes(body);
            Stream os = null;
            try
            {
                httpRequest.ContentLength = bytes.Length;
                os = httpRequest.GetRequestStream();
                os.Write(bytes, 0, bytes.Length);
            }
            finally
            {
                if (os != null)
                {
                    os.Close();
                }
            }
            var webResponse = httpRequest.GetResponse();
            using (var sr = new StreamReader(webResponse.GetResponseStream()))
            {
                return sr.ReadToEnd().Trim();
            }
        }
    }
}
