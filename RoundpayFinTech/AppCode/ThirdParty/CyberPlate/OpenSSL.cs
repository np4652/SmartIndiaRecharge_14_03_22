using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace CyberPlatOpenSSL
{
    public class OpenSSL
    {
        public const string EXKEY = ":EXCEPTION:";
        public string SessionNo { get; set; }
        public string CERTNo { get; set; }
        public string ERROR { get; set; }
        public string RESULT { get; set; }
        public string message { get; set; }
        public string htmlText { get; set; }
        public string[] lines { get; set; }
        public string ExceptionERROR { get; set; }

        public string CallCryptoAPI(string ReqMsg, string Url)
        {
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(ReqMsg);
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Url);
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.Headers.Add("X-CyberPlat-Proto", "SHA1RSA");
                httpWebRequest.ContentLength = bytes.Length;
                using (Stream requestStream = httpWebRequest.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
                htmlText = streamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                htmlText = EXKEY + ex.ToString();
                ExceptionERROR = ex.ToString();
            }
            return htmlText;
        }

        public string Sign_With_PFX(string strRequest, string keyPath, string KeyPassword)
        {
            try
            {
                X509Certificate2 x509Certificate = new X509Certificate2(keyPath, KeyPassword);

                //(.net framework related )RSACryptoServiceProvider rSACryptoServiceProvider = (RSACryptoServiceProvider)x509Certificate.PrivateKey;
                RSA rSACryptoServiceProvider = (RSA)x509Certificate.PrivateKey;
                byte[] inArray = rSACryptoServiceProvider.SignData(Encoding.ASCII.GetBytes(strRequest), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("BEGIN");
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(strRequest);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append("END");
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append("BEGIN SIGNATURE");
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(Convert.ToBase64String(inArray, Base64FormattingOptions.InsertLineBreaks));
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append("END SIGNATURE");
                stringBuilder.Append(Environment.NewLine);
                message = "inputmessage=" + HttpUtility.UrlEncode(stringBuilder.ToString());
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                ExceptionERROR = ex.ToString();
            }
            return message.ToString();
        }
        public string GenerateRandomSessionNo(int length)
        {
            Random random = new Random();
            string text = string.Empty;
            for (int i = 0; i < length; i++)
            {
                text += random.Next(10).ToString();
            }
            SessionNo = text;
            return text;
        }

        public string[] GetResponseInArray(string Response)
        {
            lines = Response.Split(new string[]
            {
                "\r\n",
                "\n"
            }, StringSplitOptions.None);
            return lines;
        }
    }

}
