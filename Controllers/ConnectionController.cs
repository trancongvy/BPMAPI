using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using BPMAPI.Models;
using CDTLib;
namespace BPMAPI.Controllers
{
    public static class Connection
    {

        public static string GetConnection(string DbName)
        {
            string url = "https://www.phanmemsgd.com/SGDAPI/api/UserConnections1/PostGetConnectionbyDBName";

            string sContentType = "application/json";
            UserConnection u = new UserConnection();
            u.DatabaseName = DbName;
            string ob = JsonConvert.SerializeObject(u);
            HttpContent s = new StringContent(ob, Encoding.UTF8, sContentType);
            HttpClient oHttpClient = new HttpClient();
            try
            {
                Task<HttpResponseMessage> oTaskPostAsync = oHttpClient.PostAsync(url, s);
                if (oTaskPostAsync.Result.StatusCode == HttpStatusCode.BadRequest)
                {
                    return "";
                }
                if (oTaskPostAsync.Result.StatusCode == HttpStatusCode.OK || oTaskPostAsync.Result.StatusCode == HttpStatusCode.Created)
                {
                    string re = oTaskPostAsync.Result.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: true).GetAwaiter()
                        .GetResult();
                    //Get lại 
                     u= JsonConvert.DeserializeObject<UserConnection>(re);
                    if (u == null) return "";
                    else
                    {
                        string connection = Security.DeCode64(u.StructDb);
                        connection += ";Database=" + DbName;
                        return connection;
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public static string GetStructConnection(string DbName)
        {
            string url = "https://www.phanmemsgd.com/SGDAPI/api/UserConnections1/PostGetConnectionbyDBName";
            string sContentType = "application/json";
            UserConnection u = new UserConnection();
            u.DatabaseName = DbName;
            string ob = JsonConvert.SerializeObject(u);
            HttpContent s = new StringContent(ob, Encoding.UTF8, sContentType);
            HttpClient oHttpClient = new HttpClient();
            try
            {
                Task<HttpResponseMessage> oTaskPostAsync = oHttpClient.PostAsync(url, s);
                if (oTaskPostAsync.Result.StatusCode == HttpStatusCode.BadRequest)
                {
                    return "";
                }
                if (oTaskPostAsync.Result.StatusCode == HttpStatusCode.OK || oTaskPostAsync.Result.StatusCode == HttpStatusCode.Created)
                {
                    string re = oTaskPostAsync.Result.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: true).GetAwaiter()
                        .GetResult();
                    //Get lại 
                    u = JsonConvert.DeserializeObject<UserConnection>(re);
                    if (u == null) return "";
                    else
                    {
                        string connection = Security.DeCode64(u.StructDb);
                        connection += ";Database=" + DbName.Replace("CBA", "CDT");
                        return connection;
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}
