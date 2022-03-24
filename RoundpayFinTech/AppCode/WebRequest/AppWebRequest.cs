using Fintech.AppCode.HelperClass;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.WebRequest;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
namespace Fintech.AppCode.WebRequest
{
    public class AppWebRequest : IAppWebRequest
    {
        public static AppWebRequest O { get { return Instance.Value; } }
        private static Lazy<AppWebRequest> Instance = new Lazy<AppWebRequest>(() => new AppWebRequest());
        private AppWebRequest() { }

        #region GetMethods
        public string CallUsingHttpWebRequest_GET(string URL)
        {
            HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
            http.Timeout = 2 * 60 * 1000;
            WebResponse response = http.GetResponse();
            string result = string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = sr.ReadToEnd();
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }
        public async Task<string> CallUsingHttpWebRequest_GETAsync(string URL, IDictionary<string, string> headers = null)
        {
            HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
            http.Timeout = 5 * 60 * 1000;
            string result = string.Empty;
            try
            {
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        http.Headers.Add(item.Key, item.Value);
                    }
                }

                using (var response = await http.GetResponseAsync())
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        result = await sr.ReadToEndAsync();
                    }
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = await sr.ReadToEndAsync();
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        public async Task<string> CallUsingHttpWebRequest_PATCHAsync(string URL, IDictionary<string, string> headers = null)
        {
            HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
            http.Method = HttpMethod.Patch.ToString();
            http.Timeout = 5 * 60 * 1000;
            string result = string.Empty;
            try
            {
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        http.Headers.Add(item.Key, item.Value);
                    }
                }

                using (var response = await http.GetResponseAsync())
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        result = await sr.ReadToEndAsync();
                    }
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = await sr.ReadToEndAsync();
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        public int GETStatusCode(string URL)
        {
            try
            {
                HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
                HttpWebResponse response = (HttpWebResponse)http.GetResponse();
                return (int)response.StatusCode;
            }
            catch (UriFormatException)
            {
                return -1;
            }
            catch (WebException)
            {
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task<int> GETStatusCodeAsync(string URL)
        {
            try
            {
                HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
                HttpWebResponse response = (HttpWebResponse)await http.GetResponseAsync();
                return (int)response.StatusCode;
            }
            catch (UriFormatException)
            {
                return -1;
            }
            catch (WebException)
            {
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public string CallUsingWebClient_GET(string URL, int timeout = 0)
        {
            AppWebClient appWebClient = (timeout == 0) ? new AppWebClient() : new AppWebClient(timeout);
            appWebClient.Encoding = Encoding.UTF8;
            var result = appWebClient.DownloadString(URL);
            return result;
        }
        public async Task<string> CallUsingWebClient_GETAsync(string URL, int timeout = 0)
        {
            AppWebClient appWebClient = (timeout == 0) ? new AppWebClient() : new AppWebClient(timeout);
            appWebClient.Encoding = Encoding.UTF8;
            var result = await appWebClient.DownloadStringTaskAsync(URL);
            return result;
        }
        #endregion

        #region PostMethods
        public async Task<string> CallHWRQueryString_PostAsync(string URL, IDictionary<string, string> headers = null)
        {
            HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
            http.Timeout = 5 * 60 * 1000;
            http.Method = HttpMethod.Post.ToString();
            string result = string.Empty;
            try
            {
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        http.Headers.Add(item.Key, item.Value);
                    }
                }

                using (var response = await http.GetResponseAsync())
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        result = await sr.ReadToEndAsync();
                    }
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = await sr.ReadToEndAsync();
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
        public string CallUsingHttpWebRequest_POST(string URL, string PostData, string ContentType = "application/x-www-form-urlencoded")
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
            var data = Encoding.ASCII.GetBytes(PostData);
            http.Method = HttpMethod.Post.ToString();
            http.ContentType = ContentType;
            http.ContentLength = data.Length;
            http.Timeout = 5 * 60 * 1000;
            using (Stream stream = http.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            string result = "";
            try
            {
                WebResponse response = http.GetResponse();
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = sr.ReadToEnd();
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception EX)
            {
                throw new Exception(EX.Message);
            }

            return result;
        }
        public string CallUsingHttpWebRequest_POSTWithCER(string URL, string PostData, string CertificatePath)
        {
            X509Certificate2 certificate2 = new X509Certificate2(CertificatePath);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
            http.ClientCertificates.Add(certificate2);
            var data = Encoding.ASCII.GetBytes(PostData);
            http.Method = HttpMethod.Post.ToString();
            http.ContentType = ContentType.x_wwww_from_urlencoded;
            http.ContentLength = data.Length;
            http.Timeout = 5 * 60 * 1000;
            using (Stream stream = http.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            string result = "";
            try
            {
                WebResponse response = http.GetResponse();
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = sr.ReadToEnd();
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception EX)
            {
                throw new Exception(EX.Message);
            }

            return result;
        }
        public string PostJsonDataUsingHWR(string URL, object PostData)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string result = "";
            try
            {
                var http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
                http.Timeout = 3 * 60 * 1000;
                var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(PostData, Formatting.Indented));
                http.Method = "POST";
                http.Accept = ContentType.application_json;
                http.ContentType = ContentType.application_json;
                http.ContentLength = data.Length;
                using (Stream stream = http.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                var response = http.GetResponse();

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = sr.ReadToEnd();
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
        public string PostJsonDataUsingHWR(string URL, string PostData)
        {
            string result = "";
            try
            {
                var http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
                var data = Encoding.UTF8.GetBytes(PostData);
                http.Method = "POST";
                http.Accept = ContentType.application_json;
                http.ContentType = ContentType.application_json;
                http.ContentLength = data.Length;
                http.Timeout = 5 * 60 * 1000;
                using (Stream stream = http.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                var response = http.GetResponse();

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = sr.ReadToEnd();
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
        public async Task<string> PostJsonDataUsingHWRAsync(string URL, object PostData)
        {
            string result = "";
            try
            {
                HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
                var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(PostData, Formatting.Indented));
                http.Method = "POST";
                http.Accept = ContentType.application_json;
                http.ContentType = ContentType.application_json;
                http.ContentLength = data.Length;
                using (Stream stream = http.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                WebResponse response = await http.GetResponseAsync();

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = await sr.ReadToEndAsync();
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = await sr.ReadToEndAsync().ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
        public async Task<string> PostJsonDataUsingHWRTLS(string URL, object PostData)
        {
            string result = "";
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
                var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(PostData, Formatting.Indented));
                http.Method = "POST";
                http.Accept = ContentType.application_json;
                http.ContentType = ContentType.application_json;
                http.ContentLength = data.Length;
                using (Stream stream = http.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                WebResponse response = await http.GetResponseAsync().ConfigureAwait(false);

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = await sr.ReadToEndAsync().ConfigureAwait(false);
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = await sr.ReadToEndAsync();
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        public async Task<string> PostJsonDataUsingHWRTLS(string URL, object PostData, IDictionary<string, string> headers)
        {
            string result = "";
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
                http.Timeout = 3 * 60 * 1000;
                var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(PostData));
                http.Method = "POST";
                http.Accept = ContentType.application_json;
                http.ContentType = ContentType.application_json;
                http.MediaType = ContentType.application_json;
                http.ContentLength = data.Length;
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        http.Headers.Add(item.Key, item.Value);
                    }
                }
                using (Stream stream = http.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                WebResponse response = await http.GetResponseAsync().ConfigureAwait(false);

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = await sr.ReadToEndAsync().ConfigureAwait(false);
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = await sr.ReadToEndAsync();
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
        
        public async Task<string> PatchJsonDataUsingHWRTLS(string URL, object PostData, IDictionary<string, string> headers)
        {
            string result = "";
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
                var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(PostData));
                http.Method = HttpMethod.Patch.ToString();
                http.Accept = ContentType.application_json;
                http.ContentType = ContentType.application_json;
                http.MediaType = ContentType.application_json;
                http.ContentLength = data.Length;
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        http.Headers.Add(item.Key, item.Value);
                    }
                }
                using (Stream stream = http.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                WebResponse response = await http.GetResponseAsync().ConfigureAwait(false);

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = await sr.ReadToEndAsync().ConfigureAwait(false);
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = await sr.ReadToEndAsync();
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
        public async Task<WebRequestModel> PostJsonDataUsingHWRTLS(string URL, object PostData, IDictionary<string, string> headers, string KeyPath)
        {
            var webRequest = new WebRequestModel {
                Response = string.Empty,
                EncryptedData=string.Empty
            };
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
                var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(PostData));
                http.Method = "POST";
                http.ContentType = "text/plain";
                http.KeepAlive = false;
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        http.Headers.Add(item.Key, item.Value);
                    }
                }
                webRequest.EncryptedData = HashEncryption.O.EncryptUsingPublicKey(data, KeyPath);
                using (var streamWriter = new StreamWriter(http.GetRequestStream()))
                {
                    streamWriter.Write(webRequest.EncryptedData);
                    streamWriter.Flush();
                }
                WebResponse response = http.GetResponse();

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    webRequest.Response = await sr.ReadToEndAsync().ConfigureAwait(false);
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            webRequest.Response = await sr.ReadToEndAsync().ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return webRequest;
        }
        public async Task<string> PostJsonDataUsingHWRTLSWithCertificate(string URL, string PostData, IDictionary<string, string> headers, string CertificatePath, string Password)
        {
            string result = "";
            try
            {
                X509Certificate2 certificate2 = new X509Certificate2(CertificatePath, Password);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
                http.PreAuthenticate = true;
                http.AllowAutoRedirect = true;
                http.ClientCertificates.Add(certificate2);
                var data = Encoding.UTF8.GetBytes(PostData);
                http.Method = HttpMethod.Post.ToString();
               // http.Accept = ContentType.application_json;
                http.ContentType = ContentType.application_atom_plus_xml;
                http.MediaType = ContentType.application_atom_plus_xml;
                http.ContentLength = data.Length;
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        http.Headers.Add(item.Key, item.Value);
                    }
                }
                using (Stream stream = http.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                WebResponse response = await http.GetResponseAsync().ConfigureAwait(false);

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = await sr.ReadToEndAsync().ConfigureAwait(false);
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = await sr.ReadToEndAsync().ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
        public async Task<string> CallUsingHttpWebRequest_POSTAsync(string URL, string PostData, string ContentType = "application/x-www-form-urlencoded")
        {
            HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
            http.Timeout = 5 * 60 * 1000;
            var data = Encoding.ASCII.GetBytes(PostData);
            http.Method = "POST";
            http.ContentType = ContentType;
            http.ContentLength = data.Length;
            using (Stream stream = await http.GetRequestStreamAsync().ConfigureAwait(false))
            {
                await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            }
            string result = "";
            try
            {
                WebResponse response = await http.GetResponseAsync().ConfigureAwait(false);
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = await sr.ReadToEndAsync().ConfigureAwait(false);
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = await sr.ReadToEndAsync().ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
        public async Task<string> HWRPostAsync(string URL, string PostData, IDictionary<string, string> headers)
        {
            HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
            var data = Encoding.ASCII.GetBytes(PostData);
            http.Method = HttpMethod.Post.ToString();
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    http.Headers.Add(item.Key, item.Value);
                }
            }
            http.ContentLength = data.Length;
            using (Stream stream = await http.GetRequestStreamAsync().ConfigureAwait(false))
            {
                await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            }
            string result = "";
            try
            {
                WebResponse response = await http.GetResponseAsync().ConfigureAwait(false);

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = await sr.ReadToEndAsync();
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = await sr.ReadToEndAsync();
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
        public string HWRPost(string URL, string PostData, IDictionary<string, string> headers)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
            http.Timeout = 5 * 60 * 1000;
            var data = Encoding.ASCII.GetBytes(PostData);
            http.Method = HttpMethod.Post.ToString();
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    http.Headers.Add(item.Key, item.Value);
                }
            }
            http.ContentLength = data.Length;
            string result = string.Empty;
            try
            {
                using (Stream stream = http.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                WebResponse response = http.GetResponse();

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = sr.ReadToEnd();
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
        #endregion

        #region PutMethods
        public async Task<string> HWRPUTAsync(string URL, string PutData, IDictionary<string, string> headers)
        {
            HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
            var data = Encoding.ASCII.GetBytes(PutData);
            http.Method = HttpMethod.Put.ToString();
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    http.Headers.Add(item.Key, item.Value);
                }
            }
            http.ContentLength = data.Length;
            using (Stream stream = await http.GetRequestStreamAsync())
            {
                await stream.WriteAsync(data, 0, data.Length);
            }
            WebResponse response = await http.GetResponseAsync();
            string result = "";
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                result = await sr.ReadToEndAsync();
            }
            return result;
        }
        public async Task<string> HWRPUTWithFilesAsync(string URL, IDictionary<string, string> FormData, IDictionary<string, string> headers, IDictionary<string, string> files)
        {
            HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
            http.Method = HttpMethod.Put.ToString();

            string boundary = "------------------" + DateTime.Now.Ticks.ToString("x");
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    http.Headers.Add(item.Key, item.Value);
                }
            }
            http.ContentType = ContentType.multipart_form_data + ";boundary=" + boundary;
            http.KeepAlive = true;

            http.Timeout = 10 * 60 * 1000;

            Stream MS = new MemoryStream();
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            await MS.WriteAsync(boundaryBytes, 0, boundaryBytes.Length);
            string formHeaderTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (var key in FormData.Keys)
            {
                string formItemHeader = string.Format(formHeaderTemplate, key, FormData[key]);
                byte[] formItemHeaderBytes = Encoding.UTF8.GetBytes(formItemHeader);
                await MS.WriteAsync(formItemHeaderBytes, 0, formItemHeaderBytes.Length);
                await MS.WriteAsync(boundaryBytes, 0, boundaryBytes.Length);
            }
            string fheaderTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n Content-Type:"
                + ContentType.application_octet_stream + "\r\n\r\n";

            foreach (var fKey in files.Keys)
            {
                string fHeader = string.Format(fheaderTemplate, fKey, new FileInfo(files[fKey]).Name);
                byte[] fHeaderBytes = Encoding.UTF8.GetBytes(fHeader);
                await MS.WriteAsync(fHeaderBytes, 0, fHeaderBytes.Length);
                FileStream FS = new FileStream(files[fKey], FileMode.Open, FileAccess.Read);
                byte[] fBuffer = new byte[4096];
                int byteRead = 0;

                while ((byteRead = await FS.ReadAsync(fBuffer, 0, fBuffer.Length)) != 0)
                {
                    await MS.WriteAsync(fBuffer, 0, byteRead);
                }
                await MS.WriteAsync(boundaryBytes, 0, boundaryBytes.Length);
                FS.Close();
            }
            http.ContentLength = MS.Length;
            using (Stream stream = await http.GetRequestStreamAsync())
            {
                MS.Position = 0;
                byte[] bufferTemp = new byte[MS.Length];
                await MS.ReadAsync(bufferTemp, 0, bufferTemp.Length);
                MS.Close();
                await stream.WriteAsync(bufferTemp, 0, bufferTemp.Length);
            }
            WebResponse response = await http.GetResponseAsync();
            string result = "";
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                result = await sr.ReadToEndAsync();
            }
            return result;
        }
        public async Task<string> UploadFilesToRemoteUrl(string filename, string url, IFormFile filess, IDictionary<string, string> headers)
        {
            string result = "";
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(url);
            try
            {

                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        request.Headers.Add(item.Key, item.Value);
                    }
                }
                request.ContentType = "multipart/form-data; boundary=" +
                                        boundary;
                request.Method = "POST";
                request.KeepAlive = true;
                Stream memStream = new System.IO.MemoryStream();
                var boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" +
                                                                        boundary + "\r\n");
                var endBoundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" +
                                                                            boundary + "--");
                string formdataTemplate = "\r\n--" + boundary +
                                            "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";
                string formitem = string.Format(formdataTemplate, "file", "image/png");
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                memStream.Write(formitembytes, 0, formitembytes.Length);

                string headerTemplate =
                    "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
                    "Content-Type: image/png\r\n\r\n";
                memStream.Write(boundarybytes, 0, boundarybytes.Length);
                var header = string.Format(headerTemplate, "file", "-");
                var headerbytes = System.Text.Encoding.UTF8.GetBytes(header);

                memStream.Write(headerbytes, 0, headerbytes.Length);
                string filedatapath = "Image/WatsappImage/" + filename;
                using (var fileStream = new FileStream(filedatapath, FileMode.Open, FileAccess.Read))
                {
                    var buffer = new byte[1024];
                    var bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        memStream.Write(buffer, 0, bytesRead);
                    }
                }
                memStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                request.ContentLength = memStream.Length;
                using (Stream requestStream = request.GetRequestStream())
                {
                    memStream.Position = 0;
                    byte[] tempBuffer = new byte[memStream.Length];
                    memStream.Read(tempBuffer, 0, tempBuffer.Length);
                    memStream.Close();
                    requestStream.Write(tempBuffer, 0, tempBuffer.Length);
                }

                using (var response = request.GetResponse())
                {
                    Stream stream2 = response.GetResponseStream();
                    StreamReader reader2 = new StreamReader(stream2);
                    result = reader2.ReadToEnd();
                }
                //  return result;
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = await sr.ReadToEndAsync();
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
        public async Task<string> UploadFilesToRemoteUrl(string filename, string mimetype, string url, IFormFile filess, IDictionary<string, string> headers)
        {
            string result = "";
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(url);
            try
            {

                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        request.Headers.Add(item.Key, item.Value);
                    }
                }
                request.ContentType = "multipart/form-data; boundary=" +
                                        boundary;
                request.Method = "POST";
                request.KeepAlive = true;
                Stream memStream = new System.IO.MemoryStream();
                var boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" +
                                                                        boundary + "\r\n");
                var endBoundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" +
                                                                            boundary + "--");
                string formdataTemplate = "\r\n--" + boundary +
                                            "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";
                string formitem = string.Format(formdataTemplate, "file", mimetype);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                memStream.Write(formitembytes, 0, formitembytes.Length);

                string headerTemplate =
                    "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
                    "Content-Type: " + mimetype + "\r\n\r\n";
                memStream.Write(boundarybytes, 0, boundarybytes.Length);
                var header = string.Format(headerTemplate, "file", filename);
                var headerbytes = System.Text.Encoding.UTF8.GetBytes(header);

                memStream.Write(headerbytes, 0, headerbytes.Length);
                string filedatapath = "Image/WatsappImage/" + filename;
                using (var fileStream = new FileStream(filedatapath, FileMode.Open, FileAccess.Read))
                {
                    var buffer = new byte[1024];
                    var bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        memStream.Write(buffer, 0, bytesRead);
                    }
                }
                memStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                request.ContentLength = memStream.Length;
                using (Stream requestStream = request.GetRequestStream())
                {
                    memStream.Position = 0;
                    byte[] tempBuffer = new byte[memStream.Length];
                    memStream.Read(tempBuffer, 0, tempBuffer.Length);
                    memStream.Close();
                    requestStream.Write(tempBuffer, 0, tempBuffer.Length);
                }

                using (var response = request.GetResponse())
                {
                    Stream stream2 = response.GetResponseStream();
                    StreamReader reader2 = new StreamReader(stream2);
                    result = reader2.ReadToEnd();
                }
                //  return result;
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                if (wx.Response != null)
                {
                    using (var ErrorResponse = wx.Response)
                    {
                        using (StreamReader sr = new StreamReader(ErrorResponse.GetResponseStream()))
                        {
                            result = await sr.ReadToEndAsync();
                        }
                    }
                }
                else
                {
                    throw new Exception(wx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }


        public string WebClientPutFiles(string URL, IDictionary<string, string> FormData, IDictionary<string, string> headers, IDictionary<string, string> files)
        {
            using (WebClient client = new WebClient())
            {

                List<MimePart> mimeParts = new List<MimePart>();

                try
                {
                    foreach (string key in headers.Keys)
                    {
                        client.Headers.Add(key, headers[key]);
                    }

                    foreach (string key in FormData.Keys)
                    {
                        MimePart part = new MimePart();

                        part.Headers["Content-Disposition"] = "form-data; name=\"" + key + "\"";
                        part.Data = new MemoryStream(Encoding.UTF8.GetBytes(FormData[key]));

                        mimeParts.Add(part);
                    }

                    foreach (var fKey in files.Keys)
                    {
                        MimePart part = new MimePart();
                        string name = fKey;
                        string fileName = new FileInfo(files[fKey]).Name;

                        part.Headers["Content-Disposition"] = "form-data; name=\"" + name + "\"; filename=\"" + fileName + "\"";
                        part.Headers["Content-Type"] = "application/octet-stream";

                        part.Data = new MemoryStream(File.ReadAllBytes(files[fKey]));

                        mimeParts.Add(part);
                    }

                    string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
                    client.Headers.Add(HttpRequestHeader.ContentType, "multipart/form-data; boundary=" + boundary);

                    long contentLength = 0;

                    byte[] _footer = Encoding.UTF8.GetBytes("--" + boundary + "--\r\n");

                    foreach (MimePart mimePart in mimeParts)
                    {
                        contentLength += mimePart.GenerateHeaderFooterData(boundary);
                    }

                    byte[] buffer = new byte[8192];
                    byte[] afterFile = Encoding.UTF8.GetBytes("\r\n");
                    int read;

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        foreach (MimePart mimePart in mimeParts)
                        {
                            memoryStream.Write(mimePart.Header, 0, mimePart.Header.Length);

                            while ((read = mimePart.Data.Read(buffer, 0, buffer.Length)) > 0)
                                memoryStream.Write(buffer, 0, read);

                            mimePart.Data.Dispose();

                            memoryStream.Write(afterFile, 0, afterFile.Length);
                        }

                        memoryStream.Write(_footer, 0, _footer.Length);
                        byte[] responseBytes = client.UploadData(URL, HttpMethod.Put.ToString(), memoryStream.ToArray());
                        string responseString = Encoding.UTF8.GetString(responseBytes);
                        return responseString;
                    }
                }
                catch (Exception ex)
                {
                    foreach (MimePart part in mimeParts)
                        if (part.Data != null)
                            part.Data.Dispose();

                    if (ex.GetType().Name == "WebException")
                    {
                        WebException webException = (WebException)ex;
                        HttpWebResponse response = (HttpWebResponse)webException.Response;
                        string responseString;

                        using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                        {
                            responseString = reader.ReadToEnd();
                        }
                        return responseString;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
        
        #endregion

        #region DeleteMethods
        public async Task<string> HWRDELETEAsync(string URL, string PutData, IDictionary<string, string> headers)
        {
            HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
            var data = Encoding.ASCII.GetBytes(PutData);
            http.Method = HttpMethod.Delete.ToString();
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    http.Headers.Add(item.Key, item.Value);
                }
            }
            http.ContentLength = data.Length;
            using (Stream stream = await http.GetRequestStreamAsync())
            {
                await stream.WriteAsync(data, 0, data.Length);
            }
            WebResponse response = await http.GetResponseAsync();
            string result = "";
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                result = await sr.ReadToEndAsync();
            }
            return result;
        }
        #endregion
        public async Task<byte[]> CallUsingHttpWebRequest_GETImageAsync(string URL, IDictionary<string, string> headers = null)
        {
            HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
            http.Timeout = 5 * 60 * 1000;
            byte[] result;

            try
            {
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        http.Headers.Add(item.Key, item.Value);
                    }
                }
                using (var response = await http.GetResponseAsync())
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        byte[] buffer = new byte[16 * 1024];
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int read;
                            while ((read = response.GetResponseStream().Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, read);
                            }
                            result = ms.ToArray();
                        }
                        //  Image1.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(response.RawBytes);
                    }
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                throw new Exception(wx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        public async Task<byte[]> CallUsingHttpWebRequest_GETVideoAsync(string URL, IDictionary<string, string> headers = null)
        {
            HttpWebRequest http = (HttpWebRequest)System.Net.WebRequest.Create(URL);
            http.Timeout = 5 * 60 * 1000;
            byte[] result;

            try
            {
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        http.Headers.Add(item.Key, item.Value);
                    }
                }
                using (var response = await http.GetResponseAsync())
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {

                        byte[] buffer = new byte[16 * 1024];
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int read;
                            while ((read = response.GetResponseStream().Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, read);
                            }
                            result = ms.ToArray();
                        }
                        //string responseresult = "data:video/m4v;base64," + Convert.ToBase64String(result1);
                        //return responseresult;
                    }
                }
            }
            catch (UriFormatException ufx)
            {
                throw new Exception(ufx.Message);
            }
            catch (WebException wx)
            {
                throw new Exception(wx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
        public void DownloadFileUsingWebClient(string DownloadUrl, string FilePath)
        {
            WebClient wc = new WebClient();
            wc.DownloadFile(DownloadUrl, FilePath);
        }
    }
    public class MimePart
    {
        public NameValueCollection Headers { get; } = new NameValueCollection();

        public byte[] Header { get; private set; }

        public long GenerateHeaderFooterData(string boundary)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("--");
            stringBuilder.Append(boundary);
            stringBuilder.AppendLine();
            foreach (string key in Headers.AllKeys)
            {
                stringBuilder.Append(key);
                stringBuilder.Append(": ");
                stringBuilder.AppendLine(Headers[key]);
            }
            stringBuilder.AppendLine();

            Header = Encoding.UTF8.GetBytes(stringBuilder.ToString());

            return Header.Length + Data.Length + 2;
        }

        public Stream Data { get; set; }
       
    }
    
}
