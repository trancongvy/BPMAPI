using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using CDTDatabase;
using System.Configuration;
using System.Drawing;
using System.Text;
using Newtonsoft.Json;

namespace BPMAPI.APIControl
{
    
    public static class ConnectionInfo
    {
        public static List<Info> lInfo = new List<Info>();
        public static Info checkInfor(string Token)
        {
            if (Token == "" || Token == null)
            {
                return null;
            }
            else
            {
                Info info = GetConnection(Token.ToString());
                if (info == null) return null;
                info.JustConnected();
                return info;
            }
        }
        public static string ConvertRowtoString(DataRow dr)
        {
            if (dr == null) return null;
            string str = "{";
            foreach (DataColumn col in dr.Table.Columns)
            {
                if(col.DataType == typeof(string) || col.DataType==typeof(Guid) || col.DataType == typeof(DateTime) )
                {
                    str+= "\"" +col.ColumnName + "\":";
                    if (dr[col.ColumnName] != DBNull.Value)
                    {
                        str += "\"" + dr[col.ColumnName].ToString() + "\",";
                    }
                    else str += "null,";
                }
                else if(col.DataType == typeof(int) || col.DataType == typeof(decimal)|| col.DataType == typeof(double) || col.DataType == typeof(bool))
                {
                    str += "\"" + col.ColumnName + "\":";
                    if (dr[col.ColumnName] != DBNull.Value)
                    {
                        str +=  dr[col.ColumnName].ToString() + ",";
                    }
                    else str += "null,";
                }
                else if (col.DataType == typeof(Byte[]))
                {
                    str += "\"" + col.ColumnName + "\":";
                    //string jsonStr = Encoding.UTF8.GetString(dr[col.ColumnName] as Byte[]);
                    string s=JsonConvert.SerializeObject(dr[col.ColumnName] as Byte[]);
                    str += "" + s + ",";
                }
            }
            return str.Substring(0,str.Length-1) +"}";
        }
        public static Info GetConnection(string TokenID)
        {
            Info info = lInfo.Find(m => m.Token == TokenID && m.ExDatetime>=DateTime.Now);
            if (info != null)
            {
                info.JustConnected();
                return info;
            }
            else
            {
                Info info1 = lInfo.Find(m => m.Token == TokenID && m.ExDatetime < DateTime.Now);
                lInfo.Remove(info1);
            }
           
            return null;
        }
        public static Database GetStructDatabase()
        {
            string Construct = ConfigurationManager.AppSettings["StructCon"];
           return Database.NewCustomDatabase(Construct);
        }
        public static Database GetDataDatabase(Info info)
        {
            
            string DbCon = ConfigurationManager.AppSettings["DBCon"] + ";Database=" + info.DataName;
            return Database.NewCustomDatabase(DbCon);
        }
    }

    public class loginInfo
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class Info
    {
        public string Token { get; set; }
        public DateTime ExDatetime { get; set; }
        public string UserID { get; set; }
        public string UserGroupID { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string GroupName { get; set; }
        public bool isAdmin { get; set; }
        public string DataName { get; set; }
        public int sysDBID { get; set; }
        public DrPackage drPackage { get; set; }
        public Hashtable Config { get; set; }
        public void JustConnected()
        {
            ExDatetime = DateTime.Now.AddMinutes(30);
        }
    }
    public class APIException
    {
        internal string error;

        public string ErrorContent { get; set; }
        public string Content { get; internal set; }
    }
    public class DrPackage
    {
        public int sysDBID { get; set; }
        public int sysPackageID { get; set; }
        public string Package { get; set; }
        public string Copyright { get; set; }
        public string Version { get; set; }
        public string PackageName { get; set; }
        public string PackageName2 { get; set; }
        public string DbName { get; set; }
        public bool isAdmin { get; set; }
        public int sysUserPackageID { get; set; }

    }
    
}