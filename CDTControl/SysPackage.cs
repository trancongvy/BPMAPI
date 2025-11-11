using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


using System.Data;
using CDTDatabase;
using CDTLib;
using System.Configuration;

using BPMAPI.APIControl;
namespace BPMAPI.CDTControl
{
    public class SysPackage
    {
        public Database _dbStruct = ConnectionInfo.GetStructDatabase();
        public string StructServer = "";
        public SysPackage()
        {

        }
        public DataTable GetPackageForUser(Info user)
        {
            string queryPackage;
            //sẽ kiểm tra core admin bằng cách khác
            if (user.isAdmin)
                queryPackage = "select b.sysDBID,a.sysPackageID,a.Package, a.Copyright, a.Version,  b.DBName as PackageName, b.DBName2 as PackageName2, b.DatabaseName AS DbName, c.isAdmin, c.sysUserPackageID from syspackage a inner join sysdb b on a.syspackageid=b.syspackageid inner join sysuserPackage c on b.sysDBID=c.sysDBID inner join sysUser d on c.sysUserGroupID=d.sysUserGroupID where d.sysUserID = " + user.UserID;
            else
                queryPackage = "select b.sysDBID,a.sysPackageID,a.Package, a.Copyright, a.Version,  b.DBName as PackageName, b.DBName2 as PackageName2, b.DatabaseName AS DbName, c.isAdmin, c.sysUserPackageID from syspackage a inner join sysdb b on a.syspackageid=b.syspackageid inner join sysuserPackage c on b.sysDBID=c.sysDBID inner join sysUser d on c.sysUserGroupID=d.sysUserGroupID  where d.sysUserID = " + user.UserID;
            DataTable dt1 = _dbStruct.GetDataTable(queryPackage);
            StructServer = _dbStruct.Connection.DataSource;
            if (dt1 != null)
                return dt1;
            else return null;

        }
        public DataRow GetDrPackage(string sysDBID)
        {
            string queryPackage;
            queryPackage = "select b.sysDBID,a.sysPackageID,a.Package, a.Copyright, a.Version,  b.DBName as PackageName, b.DBName2 as PackageName2, b.DatabaseName AS DbName, c.isAdmin, c.sysUserPackageID from syspackage a inner join sysdb b on a.syspackageid=b.syspackageid inner join sysuserPackage c on b.sysDBID=c.sysDBID inner join sysUser d on c.sysUserGroupID=d.sysUserGroupID  where b.sysDBID = " + sysDBID;
            DataTable dt1 = _dbStruct.GetDataTable(queryPackage);
            if(dt1 != null && dt1.Rows.Count>0)
                return dt1.Rows[0];
            else return null;

        }
        public Info InitSysvar(Info info)
        {
            DataTable dtConfig = _dbStruct.GetDataTable("select * from sysConfig where (sysPackageID is null or sysPackageID = " + info.drPackage.sysPackageID.ToString() + ") and (sysDBID is null or sysDBID=" + info.drPackage.sysDBID.ToString() + ")");
            if (dtConfig != null)
            {
              info.Config=  Config.InitData(dtConfig);
            }
            return info;
        }
        public DateTime ngayht()
        {
            try
            {
                string sql = "select cast(convert(nvarchar(11),getdate()) as datetime)";
                object o = _dbStruct.GetValue(sql);
                if (o == null)
                    return DateTime.Parse(DateTime.Now.ToShortDateString());
                return DateTime.Parse(o.ToString());
            }
            catch { return DateTime.Now; }
        }
        public DateTime LastUpdate()
        {
            try
            {
                string sql = "select max(Ngay) from sysupdate";
                object o = _dbStruct.GetValue(sql);
                if (o != null)
                    return DateTime.Parse(o.ToString());
            }
            catch
            {

            }
            return DateTime.Parse("01/01/2000");
        }
        public void InitDictionary()
        {
            if (UIDictionary.Contents.Count > 0)
                return;
            DataTable dtDictionary = _dbStruct.GetDataTable("select * from Dictionary");
            if (dtDictionary != null)
                UIDictionary.InitData(dtDictionary);
        }
    }
}
