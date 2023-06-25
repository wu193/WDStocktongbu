using MyService.DTO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using WDKisStockToZKH.DTO;

using System.Security.Authentication;

namespace WDKisStockToZKH.Util
{
    public static class APIHelp
    {
        static APIHelp()
        {
            //初始化账号密码
            LogHelper.Info("初始化账号密码");
            //https://honeycomb-app-uat.zkh360.com/   https://honeycomb.zkh360.com/openApi/v1/token
            api_url = "https://honeycomb.zkh360.com";
            //11222@qq.com  asdQWE1234
            api_appid = "sales3@wedotools.com";
           
            app_secret = "5rTNnSJ5";
            GetAccessToken();

        }
         
        static string api_url { get; set; }
        static string api_appid { get; set; }
        static string app_secret { get; set; }
        static string api_token { get; set; } = "";
        static Response Accesstoken { get; set; }


        static void GetAccessToken()
        {

            try
            {
                string url = api_url + "/openApi/v1/token";
                Response token = new Response();
                string postData = "{\"accessId\":\"" + api_appid + "\",\"accessSecret\":\"" + app_secret + "\"}";
                //string state = "";
                //string resoultJson = PostHttpResponse("url", postData,out state);
                //JObject aa= JObject.Parse(resoultJson);
                
                 token = PostRequest<Response>(url, postData);
                
                if (token.code != 200)
                {
                    LogHelper.Error($"Token获取失败api_url:{url},账号:{api_appid}密码:{app_secret} 返回Json{JsonConvert.SerializeObject(token)}", new Exception("Token获取失败"));
                }
                Accesstoken=token;
                if (Accesstoken.data.token != null)
                {
                    api_token = Accesstoken.data.token;
                }
            }
            catch (Exception ex)
            {
                //LogHelper.Error(ex.Message, ex);
                throw;
            }
          
        }
        /// <summary>
        /// 同步库存
        /// </summary>
        /// <returns></returns>
        public static Response PostReportStock(List<ReportStockModle> stockModels)
        {
            
            //LogHelper.Info($"TOKEN{token}");
          

            string url = api_url + "/openApi/v2/common/report/stock";

    
            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(stockModels));
            httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            httpContent.Headers.Add("honeycombToken", api_token);
            //LogHelper.Info($"TOKEN{api_token}");
            using (HttpClient httpClient = new HttpClient())
            {

                httpClient.DefaultRequestHeaders.ConnectionClose = true;
                HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;
                Response result = new Response();
                if (response.IsSuccessStatusCode)
                {
                    LogHelper.Info($"同步库存json{JsonConvert.SerializeObject(stockModels)}");
                    Task<string> t = response.Content.ReadAsStringAsync();
                    string s = t.Result;
                    //Newtonsoft.Json
                    string json = JsonConvert.DeserializeObject(s).ToString();
                    result = JsonConvert.DeserializeObject<Response>(json);
                    if (result.code!=200)
                    {
                        LogHelper.Error($"{JsonConvert.SerializeObject(stockModels)} 返回Json{JsonConvert.SerializeObject(result)}", new Exception("库存同步失败"));
                    }
                    if (result.code==403 || result.msg== "token 已经失效")
                    {
                        GetAccessToken();
                        PostReportStock(stockModels);
                    }

                }
                LogHelper.Info($"返回json{JsonConvert.SerializeObject(result)}");
                return result;
            }

        }
        static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            //直接确认，否则打不开
            return true;
        }

        // Post请求
        static string PostHttpResponse(string url, string postData, out string statusCode)
        {
            string result = string.Empty;
            //设置Http的正文
            HttpContent httpContent = new StringContent(postData);
            //设置Http的内容标头
            httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            //设置Http的内容标头的字符
            httpContent.Headers.ContentType.CharSet = "utf-8";
            using (HttpClient httpClient = new HttpClient())
            {
                //异步Post
                HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;
                //输出Http响应状态码
                statusCode = response.StatusCode.ToString();
                //确保Http响应成功
                if (response.IsSuccessStatusCode)
                {
                    //异步读取json
                    result = response.Content.ReadAsStringAsync().Result;
                }
            }
            return result;
        }
       
        public static T PostRequest<T>(string Url, string Param)
        {
         
             T result = default(T);
            HttpWebRequest request;
            HttpWebResponse response;
            LogHelper.Info($"Post request: {Url}Param：{Param}");
            string strURL = Url;
            string StrDate = "";
            string strValue = "";

            try
            {
                LogHelper.Info($"Post");

                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                //| SecurityProtocolType.Tls11
                //| SecurityProtocolType.Tls12
                //| (SecurityProtocolType)12288
                //| SecurityProtocolType.Ssl3;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | (SecurityProtocolType)3072 | (SecurityProtocolType)768 | (SecurityProtocolType)192 | (SecurityProtocolType)12288;
                //ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                //Util.SetCertificatePolicy();
                //接收证书进行身份验证
                //ServicePointManager.ServerCertificateValidationCallback =
                //    new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
                //ServicePointManager.SecurityProtocol = (SecurityProtocolType)12288;
                //try
                //{ //try TLS 1.3
                //    ServicePointManager.SecurityProtocol = (SecurityProtocolType)12288
                //                                         | (SecurityProtocolType)3072
                //                                         | (SecurityProtocolType)768
                //                                         | SecurityProtocolType.Tls;
                //}
                //catch (NotSupportedException)
                //{
                //    try
                //    { //try TLS 1.2
                //        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072
                //                                             | (SecurityProtocolType)768
                //                                             | SecurityProtocolType.Tls;
                //    }
                //    catch (NotSupportedException)
                //    {
                //        try
                //        { //try TLS 1.1
                //            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768
                //                                                 | SecurityProtocolType.Tls;
                //        }
                //        catch (NotSupportedException)
                //        { //TLS 1.0
                //            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                //        }
                //    }
                //}
                //ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                //ServicePointManager.ServerCertificateValidationCallback = new
                //RemoteCertificateValidationCallback
                //(
                //   delegate { return true; }
                //);
                //ServicePointManager.DefaultConnectionLimit = 50;

                request = (HttpWebRequest)WebRequest.Create(strURL);

                request.Method = "POST";
                request.ContentType = "application/json";
               
                byte[] Data = Encoding.UTF8.GetBytes(Param);
                request.GetRequestStream().Write(Data, 0, Data.Length);
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    LogHelper.Error(ex.Message, ex);
                    response = (HttpWebResponse)ex.Response;
                }
               
                request.GetRequestStream().Close();
                Stream Data_S = response.GetResponseStream();
                StreamReader Reader = new StreamReader(Data_S, Encoding.UTF8);
                while ((StrDate = Reader.ReadLine()) != null)
                {
                    strValue += StrDate + "\r\n";
                }
                Data_S.Close();
                LogHelper.Info(strValue);
                return JsonConvert.DeserializeObject<T>(strValue); ;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                //strValue = ex.Message;
                //response = (HttpWebResponse)ex.Response;

            }
           
            return result;




        }
        public static class Util
        {
            ///  <summary>
            ///  Sets the cert policy.
            ///  </summary>
            public static void SetCertificatePolicy()
            {
                ServicePointManager.ServerCertificateValidationCallback
                           += RemoteCertificateValidate;
            }

            ///  <summary>
            ///  Remotes the certificate validate.
            ///  </summary>
            private static bool RemoteCertificateValidate(
               object sender, X509Certificate cert,
                X509Chain chain, SslPolicyErrors error)
            {
                //  trust any certificate!!!
                System.Console.WriteLine(" Warning, trust any certificate ");
                return true;
            }
        }
        // 泛型：Post请求
        static T PostHttpResponse<T>(string url, string postData) where T : class, new()
        {
            LogHelper.Info("PostHttpResponse");
            T result = default(T);
            try
            {
                
                HttpContent httpContent = new StringContent(postData);
                //设置Http的内容标头
                httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                //设置Http的内容标头的字符
                httpContent.Headers.ContentType.CharSet = "utf-8";

                //httpContent.DefaultRequestHeaders.ConnectionClose = true;
                using (HttpClient httpClient = new HttpClient())
                {
                   
                    //httpClient.DefaultRequestHeaders.ConnectionClose = true;
                    HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        Task<string> t = response.Content.ReadAsStringAsync();
                        string s = t.Result;
                        string json = "";
                        //Newtonsoft.Json
                        try
                        {
                             json = JsonConvert.DeserializeObject(s).ToString();
                            
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error($"错误信息：{ex.Message}，返回信息：{s}", new Exception("序列化报错"));
                            throw;
                        }
                        try
                        {
                           
                            result = JsonConvert.DeserializeObject<T>(json);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error($"错误信息：{ex.Message}，返回信息：{s}", new Exception("序列化报错"));
                            throw;
                        }

                    }
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
           
            

            return result;
        }
    }
}
