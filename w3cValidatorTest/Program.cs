using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Xml;
using System.Web;
using System.Collections.Specialized;

namespace w3cValidatorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("What website to make a request to?");
            string inputUrl = Console.ReadLine();
            Console.WriteLine("WebRequest body:");
            string requestBody = Console.ReadLine();
            Console.WriteLine("File name:");
            string fileName = Console.ReadLine();

            string htmlRequestBody = //WebUtility.HtmlEncode(requestBody);
                requestBody;
            /*UnicodeEncoding unicodeEncodedRequestBody = new UnicodeEncoding();
            byte[] unicodeEncodedRequestBodyByteArr = unicodeEncodedRequestBody.GetBytes(htmlRequestBody);*/
            //byte[] unicodeEncodedRequestBodyByteArr = Encoding.Unicode.GetBytes(htmlRequestBody);
            byte[] unicodeEncodedRequestBodyByteArr = Encoding.Unicode.GetBytes(requestBody);

            #region debug TODO: remove
            Console.WriteLine("Debug:");
            Console.WriteLine(System.Text.Encoding.Default.GetString(unicodeEncodedRequestBodyByteArr));
            #endregion

            HttpWebRequest pingReq = HttpWebRequest.CreateHttp(inputUrl);
            #region POST header
            pingReq.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:45.0) Gecko/20100101 Firefox/45.0";
            pingReq.KeepAlive = true;
            pingReq.Method = "POST";
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            pingReq.ContentType = "multipart/form-data; boundary=" + boundary;
            byte[] boundaryBytes = Encoding.Unicode.GetBytes("\r\n--" + boundary + "\r\n");
            //pingReq.ContentLength = unicodeEncodedRequestBodyByteArr.Length;
            #endregion

            #region File addition
            //string headerTemplate = "Content-Disposition: form-data;\r\nContent-Type: text/html\r\n";
            string header = "Content-Disposition: form-data; name=\"file\"; filename=\"" + fileName + "\"\r\nContent-Type: text/html\r\n";
            //string header = String.Format(headerTemplate);
            byte[] encodedHeaderByteArr = Encoding.Unicode.GetBytes(header);
            Stream pingReqStream = pingReq.GetRequestStream();
            pingReqStream.Write(boundaryBytes, 0, boundaryBytes.Length);
            pingReqStream.Write(encodedHeaderByteArr, 0, encodedHeaderByteArr.Length);
            pingReqStream.Write(unicodeEncodedRequestBodyByteArr, 0, unicodeEncodedRequestBodyByteArr.Length);
            /*byte[] newLineBytes = Encoding.Unicode.GetBytes("\r\n");
            pingReqStream.Write(newLineBytes, 0, newLineBytes.Length);*/
            #endregion

            #region Input stream end
            byte[] finalBytes = Encoding.Unicode.GetBytes("\r\n--" + boundary + "--\r\n");
            pingReqStream.Write(finalBytes, 0, finalBytes.Length);
            pingReqStream.Close();
            #endregion

            HttpWebResponse pingReqResponse = (HttpWebResponse)pingReq.GetResponse();
            Stream pingReqResponseStream = pingReqResponse.GetResponseStream();
            StreamReader streamRead = new StreamReader(pingReqResponseStream);
            string pingReqResponseString = streamRead.ReadToEnd();
            /*XmlDocument responseXml = new XmlDocument();
            responseXml.LoadXml(pingReqResponseString);
            XmlNode responseXmlChild = responseXml.ChildNodes[1];
            Console.WriteLine(responseXmlChild.InnerText);*/
            Console.WriteLine(pingReqResponseString);

            Console.ReadKey();
        }

        public static void MainOld(string[] args)
        {
            Console.WriteLine("What website to make a request to?");
            string inputUrl = Console.ReadLine();
            /*Console.WriteLine("File path:");
            string requestBody = Console.ReadLine();*/

            const string filePath = "index.html";

            HttpUploadFile(inputUrl, filePath, "", "text/html", new NameValueCollection());

            Console.ReadKey();
        }

        public static void HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            //log.Debug(string.Format("Uploading {0} to {1}", file, url));
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                Console.WriteLine("File uploaded, server response is: {0}", reader2.ReadToEnd());
            }
            catch (Exception ex)
            {
                //log.Error("Error uploading file", ex);
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }
    }
}
