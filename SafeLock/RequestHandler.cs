using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SafeLock
{
    public enum REQUEST_TYPE_
    {
        LOGIN,
        CHECK
    }
    public class RequestHandler
    {
        public bool SendData(REQUEST_TYPE_ type, LoginCredentials inData, out string receivedData)
        {
            switch (type)
            {
                case REQUEST_TYPE_.LOGIN:
                    try
                    {
                        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
                        //ServicePointManager.ServerCertificateValidationCallback = (snder, cert, chain, error) => true;
                        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                        var request = (HttpWebRequest)WebRequest.Create("https://hakuni.net/SafeLock/index.php");
                        var postData = $"islogin={Uri.EscapeDataString("true")}&username={Uri.EscapeDataString(inData.Username)}&password={Uri.EscapeDataString(inData.Password)}";
                        var data = Encoding.ASCII.GetBytes(postData);
                        request.UserAgent = "HakuniLogin";
                        request.Accept = "*/*";
                        request.UseDefaultCredentials = true;
                        request.Proxy.Credentials = CredentialCache.DefaultCredentials;

                        request.Method = "POST";
                        request.ContentType = "application/x-www-form-urlencoded"; 
                        request.ContentLength = data.Length;
                        using (var stream = request.GetRequestStream())
                            stream.Write(data, 0, data.Length);
                        var response = (HttpWebResponse)request.GetResponse();
                        receivedData = new StreamReader(response.GetResponseStream()).ReadToEnd();
                        return true;
                    }
                    catch(Exception e)
                    {
                        receivedData = e.ToString();
                        return false;
                    }
                default:
                    break;
            }
            receivedData = null;
            return false;
        }
    }
}
