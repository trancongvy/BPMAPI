using System;
using System.Collections.Generic;
using System.Data;
using CDTDatabase;
using CDTLib;
using System.Configuration;
using BPMAPI.APIControl;
namespace BPMAPI.CDTControl
{
    public class SysConfig
    {

        Database _dbStruct=ConnectionInfo.GetStructDatabase();

        DataSet _dsStartConfig;
        public SysConfig()
        {

        }

        public DataSet DsStartConfig
        {
            get { return _dsStartConfig; }
            set { _dsStartConfig = value; }
        }

        public DataTable GetUserConfig(Info user)
        {
            if (user.drPackage == null) return null;
            string sysPackageID = user.drPackage.sysPackageID.ToString();
            string sysDBID = user.sysDBID.ToString();
            string sql = "select a.sysUserID, b.UserName from systable a inner join sysUser b on a.sysUserID=b.sysUserID where a.TableName='sysConfig' and a.sysUserID is not null";
            DataTable tb = _dbStruct.GetDataTable(sql);
            if (tb.Rows.Count == 0)
            {
                return _dbStruct.GetDataTable("select sysConfigID, _Key, _Value from sysConfig where IsUser = 1 and sysPackageID = " + sysPackageID + " and (sysDBID is null or sysDBID=" + sysDBID + ")");
            }
            else
            {
                string AdminID = tb.Rows[0]["UserName"].ToString();
                string UserName = user.UserName.ToString();
                if (AdminID == UserName)
                    return _dbStruct.GetDataTable("select sysConfigID, _Key, _Value from sysConfig where IsUser = 1 and sysPackageID = " + sysPackageID + " and (sysDBID is null or sysDBID=" + sysDBID + ")");
                else
                    return _dbStruct.GetDataTable("select sysConfigID, _Key, _Value from sysConfig  where   IsUser = 1 and sysPackageID = " + sysPackageID + " and (sysDBID is null or sysDBID=" + sysDBID + ") and sysUserID in (select sysUserID from sysUser where UserName='" + UserName + "')");
            }
        }

        public DataTable GetStartConfig(Info user)
        {
            if (user.drPackage == null) return null;
            string sysPackageID = user.drPackage.sysPackageID.ToString();
            string sysDBID = user.sysDBID.ToString();
            return _dbStruct.GetDataTable("select sysConfigID, _Key, _Value from sysConfig where StartConfig = 1 and sysPackageID = " + sysPackageID + " and (sysDBID is null or sysDBID=" + sysDBID + ")");
        }

        private void UpdateCurrentConfig()
        {
            if (_dsStartConfig == null) return;
            foreach (DataRow dr in _dsStartConfig.Tables[0].Rows)
            {
                if (dr.RowState == DataRowState.Modified)
                {
                    Config.Variables.Remove(dr["_Key"].ToString());
                    Config.NewKeyValue(dr["_Key"], dr["_Value"]);
                }
            }
        }

        public bool UpdateStartConfig()
        {
            UpdateCurrentConfig();
            if (_dsStartConfig == null) return true;
            return (_dbStruct.UpdateDataSet(_dsStartConfig));
        }
    }
}
