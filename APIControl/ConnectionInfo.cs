using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BPMAPI.APIControl
{
    public static class ConnectionInfo
    {
       public static List<Info> lInfo = new List<Info>();
        public static Info GetConnection(string TokenID)
        { Info info = lInfo.Find(m => m.Token == TokenID);
            if (info != null)
            {
                return info;
            }
            return null;
        }
    }
    public class Info
    {

        public string Token { get; set; }
        public string ComputerName { get; set; }
        public string StructCon { get; set; }
        public string DataCon { get; set; }
        public string DataName { get; set; }
        public string StructName { get; set; }
        public DateTime ExDatetime { get; set; }
        public string UserID;
        public string UserGroupID;
        public bool isAdmin;
        public Hashtable config;
        public void JustConnected()
        {

            ExDatetime = DateTime.Now.AddMinutes(5);
             
        }
    }
    
}