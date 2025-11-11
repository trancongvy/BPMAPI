using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using CDTDatabase;
using CDTLib;
using System.IO;
using System.Configuration;
using BPMAPI.APIControl;
namespace BPMAPI.CDTControl
{
    public class SysUser
    {

        public DataRow DrUser;
        Database _dbStruct = ConnectionInfo.GetStructDatabase();
        public string maskPwd = "************";
        private string _sysUserID = string.Empty;
        private string _userName = string.Empty;
        private string _password = string.Empty;

        public SysUser()
        {

        }

        public SysUser(string sysUserID)
        {
            _sysUserID = sysUserID;
        }
        public DataTable GetUserList()
        {
            string s = "select * from sysUser";
            DataTable dt = _dbStruct.GetDataTable(s);
            return dt;
        }
        public DataTable GetUserGroupList()
        {
            string s = "select * from sysUserGroup";
            DataTable dt = _dbStruct.GetDataTable(s);
            return dt;
        }
        public DataRow CheckLogin(string userName, string password)
        {
            string userName1 = userName.Replace("'", "").Replace("\"", "").Replace(";", "").Replace("=", "").Replace(">", "").Replace("'<", "").Replace(" ", "").Replace("-", "");
            password = Security.EnCode(password);
            if (userName1 != userName) return null;
            string s = "select a.*, b.* from sysUser a inner join sysUserGroup b on a.sysUserGroupID=b.sysUserGroupID where UserName = '" + userName;
            if (password == string.Empty)
                s += " 'and (Password is null or Password = '')";
            else
                s += "' and Password = '" + password + "'";
            DataTable dt = _dbStruct.GetDataTable(s);
            if (dt == null)
                return null;
            if (dt.Rows.Count == 0)
            {
                if (CheckCoreAdmin(userName, password))
                {
                    DrUser = dt.NewRow();
                    DrUser["sysUserID"] = "1000";
                    DrUser["UserName"] = userName;
                    DrUser["Password"] = password;
                    DrUser["FullName"] = "Core Administrator";
                    DrUser["CoreAdmin"] = true;
                    return DrUser;
                }
                else
                    return null;
            }
            DrUser = dt.Rows[0];

            //DrUser["CoreAdmin"] = false;
            return DrUser;
        }

        private bool CheckCoreAdmin(string userName, string password)
        {
            if (!File.Exists(System.Environment.SystemDirectory + "\\info.dat"))
                return false;
            string[] userInfo = File.ReadAllLines(System.Environment.SystemDirectory + "\\info.dat");
            return (userName.ToUpper() == userInfo[0].ToUpper() && password == userInfo[1]);
        }

        public bool ValidUser(string password)
        {
            string s1 = "select * from sysUser where sysUserID = " + _sysUserID;
            if (password != string.Empty)
                s1 += " and Password = '" + Security.EnCode(password) + "'";
            else
                s1 += " and (Password = '" + Security.EnCode(string.Empty) + "' or Password is null)";
            DataTable dtUser = _dbStruct.GetDataTable(s1);
            if (dtUser == null || dtUser.Rows.Count == 0)
                return false;
            return true;
        }

        public bool ChangePassword(string newPassword)
        {
            string s2 = "update sysUser set Password = '" + Security.EnCode(newPassword) + "' where sysUserID = " + _sysUserID;
            return (_dbStruct.UpdateByNonQuery(s2));
        }
    }

}