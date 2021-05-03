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
                        receivedData = null;
                        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                        var request = (HttpWebRequest)WebRequest.Create("http://localhost/safelock/");
                        var postData = $"islogin={Uri.EscapeDataString("false")}&username={Uri.EscapeDataString(inData.Username)}&password={Uri.EscapeDataString(inData.Password)}";
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
                        MessageBox.Show(response.StatusCode.ToString());
                        if (response.StatusCode == HttpStatusCode.NotFound)
                            return false;
                        if (response.StatusCode != HttpStatusCode.OK)
                            return false;

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
