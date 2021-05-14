using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CDTDatabase;
using System.Data;
using BPMAPI.APIControl;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections;

namespace BPMAPI.DataFactory
{
    public class SingleData
    {
        public string TableName;
        public string StructDB;
        public string DataDBb;
        public DataTable tbStruct;
        public DataRow drTable;
        public string sysTableID;
        public SqlField PkMaster;
        public bool _identityPk ;
        public string _sInsert="";
        public string _sUpdate = "";
        public string _sDelete = "";
        public string _sUpdateImage = "";
        public string _sUpdateWs = "";
        public List<SqlField> _vInsert;
        public List<SqlField> _vUpdate;
        public List<SqlField> _vDelete;
        public List<SqlField> _vUpdateImage;
        public DataTable tbData;
         
        public async Task<bool> Insert(string data, Info info)
        {

            if (_sInsert == "")
                GenSQLInsert();
            //Kiểm tra quyền insert của UserID
            bool result = await Task.Run(()=>InsertSQL(data));
            return result;
        }
        private string NotAdminListCondition(Info info)
        {
            string dk = "(";
            string ws = info.UserID.ToString().Trim();
            string tableid = drTable["sysTableID"].ToString().Trim();
            string sql = "select condition from sysAdminDM where systableid=" + tableid + " and (sysUserID=" + ws + " or sysUserGroupID  in (select sysUserGroupID from sysUser where sysUserID=" + ws + " ))";
            sql = UpdateSpecialCondition(sql, info);
            Database dbStruct = Database.NewCustomDatabase(info.StructCon);
            DataTable tbCon = dbStruct.GetDataTable(sql);

            foreach (DataRow dr in tbCon.Rows)
            {
                dk += "(" + dr["condition"].ToString() + ") or ";
            }
            if (dk.Contains("or")) dk = dk.Substring(0, dk.Length - 3) + ")";
            else
            {
                dk = "1=0";
            }
            return dk;
        }
        public string GetQuery(Info info)
        {
            string extrasql = string.Empty;
            //xét trường hợp phân toàn quyền 
            //
            string extraWs = string.Empty;
            string _condition = string.Empty;
            if (drTable["sysUserID"] != null)
            {
                string adminList = drTable["sysUserID"].ToString().Trim();
                if (adminList != string.Empty)
                {
                    if (adminList != info.UserID.ToString().Trim())
                    {
                        string dk = NotAdminListCondition(info);
                        dk = UpdateSpecialCondition(dk, info);
                        extraWs = " (charindex('_" + info.UserID + "_',ws)>0 or charindex('_" + info.UserGroupID.ToString().Trim() + "_',Grws)>0)";
                        if (dk != string.Empty)
                            extraWs += " or " + dk;
                        extraWs = dk;
                    }
                }
            }
            //
            if (drTable.Table.Columns.Contains("ExtraSql"))
                if (drTable["ExtraSql"] != null)
                    extrasql = drTable["Extrasql"].ToString();

            if (extraWs != string.Empty)
            {
                if (extrasql == string.Empty)
                {
                    extrasql = extraWs;
                }
                else
                {
                    extrasql += " and (" + extraWs + ")";
                }
            }

            string queryData = "select * from " + drTable["TableName"].ToString();
            if (_condition != string.Empty && !(_condition.Contains("@")))
            {
                queryData += " where " + _condition;
                if (extrasql != string.Empty)
                    queryData += " and (" + extrasql + ")";
            }
            else
            {
               
                if (extrasql != string.Empty)
                    queryData += " where " + extrasql;

            }
            if (drTable["SortOrder"].ToString() != string.Empty)
                queryData += " order by " + drTable["SortOrder"].ToString();
            return queryData;
        }
        public async Task<DataTable> GetDataFull(Info info)
        {
            string sql = GetQuery(info);
            Database db;
            if (drTable["sysPackageID"].ToString() == "5")
            { db = Database.NewCustomDatabase(info.StructCon); }
            else
            {
                db = Database.NewCustomDatabase(info.DataCon);
            }
            return await Task.Run(() => db.GetDataTable(sql));
            
        }
        
        public string UpdateSpecialCondition(string query,Info info)
        {
            if (info.config.Contains("NamLamViec"))
            {
                query = query.Replace("@@NAM", info.config["NamLamViec"].ToString());
            }
            foreach (DictionaryEntry o in info.config)
            {
                if (info.config[o.Key.ToString()] == null) continue;
                query = query.Replace("@@" + o.Key.ToString(), info.config[o.Key.ToString()].ToString());
                query = query.Replace("@@" + o.Key.ToString().ToUpper(), info.config[o.Key.ToString()].ToString());
            }
            return query;
        }
        private void GenSQLInsert()
        {
            string fieldName;
            int type;

            this._sInsert = "insert into " + TableName + "(";
            this._sUpdate = "update " + TableName + " set ";
            this._sDelete = "delete from " + TableName;
            this._sUpdateImage = "update " + TableName + " set ";
            //Thao tác với file
            //----------------
            //  string userID = Config.GetValue("sysUserID").ToString();
            _vInsert = new List<SqlField>();
            _vUpdate = new List<SqlField>();
            _vDelete = new List<SqlField>();
            _vUpdateImage = new List<SqlField>();
            string condition = string.Empty;
            string tmp = " values(";
            foreach (DataRow drField in tbStruct.Rows)
            {
                fieldName = drField["FieldName"].ToString();
                type = int.Parse(drField["Type"].ToString());
                switch (type)
                {
                    case 0:
                    case 6:
                        condition = " where " + fieldName + " = @" + fieldName;
                        this._vUpdate.Add(new SqlField(fieldName, Factory.GetDbType(type)));
                        this._vDelete.Add(new SqlField(fieldName, Factory.GetDbType(type)));
                        break;
                }
                if (type == 3)
                {
                    this._identityPk = true;
                    condition = " where " + fieldName + " = @" + fieldName;
                    this._vUpdate.Add(new SqlField(fieldName, Factory.GetDbType(type)));
                    this._vDelete.Add(new SqlField(fieldName, Factory.GetDbType(type)));
                }
                else
                {
                    if (type == 12)
                    {
                        this._vUpdateImage.Add(new SqlField(fieldName, Factory.GetDbType(type)));
                        this._sUpdateImage += fieldName + "=@" + fieldName + ",";
                        continue;
                    }

                    if ((((type != 0) && (type != 6)) && (type != 3)))
                    {
                        this._vUpdate.Add(new SqlField(fieldName, Factory.GetDbType(type)));
                        this._sUpdate += fieldName + " = @" + fieldName + ",";
                    }
                    this._sInsert = this._sInsert + fieldName + ",";
                    tmp = tmp + "@" + fieldName + ",";
                    this._vInsert.Add(new SqlField(fieldName, Factory.GetDbType(type)));
                }
            }
            this._sInsert = this._sInsert.Remove(this._sInsert.Length - 1) + ")" + tmp.Remove(tmp.Length - 1) + ")";
            this._sUpdate = this._sUpdate.Remove(this._sUpdate.Length - 1) + condition;
            this._sUpdateImage = this._sUpdateImage.Remove(this._sUpdateImage.Length - 1);
            this._sDelete = this._sDelete + condition;

        }
        private bool InsertSQL(string data)
        {
            DataTable dt = (DataTable)JsonConvert.DeserializeObject(data, (typeof(DataTable)));
            return true;
        }

        internal void GetStructInfo()
        {
            
        
            Database dbStruct = Database.NewCustomDatabase(StructDB);
            string sql;
           

            //Lấy tbStruct
            sql = "select * from sysTable where TableName='" + TableName + "'";
            DataTable tb = dbStruct.GetDataTable(sql);
            if (tb.Rows.Count > 0) drTable = tb.Rows[0];
            sql = "select * from sysField where sysTableID=" + drTable["sysTableID"].ToString();
            tbStruct = dbStruct.GetDataTable(sql);
        }

    }
}