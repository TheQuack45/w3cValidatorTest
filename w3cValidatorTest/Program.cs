using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Xml;
using System.Web;

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

            string htmlRequestBody = WebUtility.HtmlEncode(requestBody);
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
            pingReq.KeepAlive = true;
            pingReq.Method = "POST";
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            pingReq.ContentType = "multipart/form-data; boundary=" + boundary;
            //pingReq.ContentLength = unicodeEncodedRequestBodyByteArr.Length;
            #endregion

            #region File addition
            string headerTemplate = "--{0}\r\nContent-Disposition: form-data;\r\nContent-Type: text/html\r\n";
            string header = String.Format(headerTemplate, boundary);
            byte[] encodedHeaderByteArr = Encoding.Unicode.GetBytes(header);
            Stream pingReqStream = pingReq.GetRequestStream();
            pingReqStream.Write(encodedHeaderByteArr, 0, encodedHeaderByteArr.Length);
            pingReqStream.Write(unicodeEncodedRequestBodyByteArr, 0, unicodeEncodedRequestBodyByteArr.Length);
            byte[] newLineBytes = Encoding.Unicode.GetBytes("\r\n");
            pingReqStream.Write(newLineBytes, 0, newLineBytes.Length);
            #endregion

            #region Input stream end
            byte[] finalBytes = Encoding.Unicode.GetBytes("--" + boundary + "--");
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
    }
}
