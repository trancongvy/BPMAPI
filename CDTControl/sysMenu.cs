using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using CDTDatabase;
using CDTLib;
using BPMAPI.APIControl;
using System.Configuration;

namespace BPMAPI.CDTControl
{

    public class sysMenu
    {
        public Database _dbStruct = ConnectionInfo.GetStructDatabase();
        public string StructServer = "";
        public DataTable GetMenu(Info user)
        {
            if (user.drPackage == null) return null;
            string sysPackageID =user.drPackage.sysPackageID.ToString();
            string sysUserID = user.UserID.ToString();
            string sysDBID = user.sysDBID.ToString();
            DataTable dtUserPackage = _dbStruct.GetDataTable("select * from sysUserPackage  where sysUserGroupID in (select sysUserGroupID from sysuser where sysUserID = " + sysUserID + ") and sysDBID = " + sysDBID);
            if (dtUserPackage == null)
                return null;
            if (dtUserPackage.Rows.Count == 0)
                return (GetMenuForAdmin(sysPackageID));
            string sysUserPackageID = dtUserPackage.Rows[0]["sysUserPackageID"].ToString();
            //Config.NewKeyValue("sysUserPackageID", sysUserPackageID);
            bool isAdmin = Boolean.Parse(dtUserPackage.Rows[0]["isAdmin"].ToString());
            if (isAdmin)
            {
                return (GetMenuForAdmin(sysPackageID));
            }
            else
            {
                //SynMenuforUser();
                
                string sql = "select t.*,r.*,m.*,ut.* from sysMenu m left join sysTable t on m.sysTableID = t.sysTableID left join sysReport r on  m.sysReportID = r.sysReportID " +
                    " left join  sysUserMenu um on m.sysMenuID = um.sysMenuID left join sysUserTable ut on t.sysTableID=ut.sysTableID and um.sysUserPackageID=ut.sysUserPackageID" +
                    " left join FileList f on f.PkID=m.sysMenuID and f.sysFieldID=423 " +
                    " where (sysMenuParent is null or sysmenuParent in (select sysmenuid from sysmenu where isVisible=1))  and " +
                    " (isVisible=1 and  um.Executable = 1 and um.sysUserPackageID = " + sysUserPackageID +// " and um.sysUserPackageID = " + sysUserPackageID +
                    ") order by m.MenuOrder";
                DataTable dtMenu = _dbStruct.GetDataTable(sql);
                return (dtMenu);
                //isVisible=1 and
            }
        }
        private DataTable GetMenuForAdmin(string sysPackageID)
        {


            //DataTable dtMenu = _dbStruct.GetDataTable("select t.*,r.*,m.* from sysMenu m left join sysTable t on m.sysTableID = t.sysTableID left join sysReport r on  m.sysReportID = r.sysReportID " +
            //        " where (sysMenuParent is null or sysmenuParent in (select sysmenuid from sysmenu where isVisible=1) ) and " +
            //        "  isVisible=1 and   (m.sysPackageID is null or m.sysPackageID = " + sysPackageID +
            //        ") order by m.sysPackageID, m.MenuOrder");
            //string sql1 = "select t.*,r.*,m.* from sysMenu m left join sysTable t on m.sysTableID = t.sysTableID left join sysReport r on  m.sysReportID = r.sysReportID " +
            //        " where (sysMenuParent is null or sysmenuParent in (select sysmenuid from sysmenu where isVisible=1) ) and " +
            //        "  isVisible=1 and   (m.sysPackageID is null or m.sysPackageID = " + sysPackageID +
            //        ") order by m.sysPackageID, m.MenuOrder";
            string sql = "select t.*,r.*,m.*,ut.*, f.fData as Image1 from sysMenu m " +
                    " left join sysTable t on m.sysTableID = t.sysTableID " +
                    " left join sysReport r on  m.sysReportID = r.sysReportID " +
                  " left join sysUserTable ut on t.sysTableID=ut.sysTableID and 1=0 " +
                   " left join FileList f on f.PkID=m.sysMenuID and f.sysFieldID=423 " +
                   " where (sysMenuParent is null or sysmenuParent in (select sysmenuid from sysmenu where isVisible=1) ) and " +
                   "  isVisible=1   and   (m.sysPackageID is null or m.sysPackageID = " + sysPackageID +
                   ")  order by m.sysPackageID, m.MenuOrder ";
            DataTable dtMenu = _dbStruct.GetDataTable(sql);
            return (dtMenu);
        }
    }
}