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
using System.Configuration;

namespace BPMAPI.DataFactory
{
    public enum DataAction { Insert, Update, Delete, IUD };
    public class SingleData
    {
        public string TableName;
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
       

        public SingleData(string tableName)
        {
            TableName = tableName;
            this.GetStructInfo();
            GetPkMaster();
        }
        public SingleData(int sysTableID)
        {
            string sql = "select TableName from sysTable where sysTableID=" + sysTableID.ToString();
            Database dbStruct = ConnectionInfo.GetStructDatabase();
            object o = dbStruct.GetValue(sql);
            if (o == null) return;
            TableName = o.ToString();

            this.GetStructInfo();
            GetPkMaster();
            this.sysTableID = sysTableID.ToString();
        }

        private string NotAdminListCondition(Info info)
        {
            string dk = "(";
            string ws = info.UserID.ToString().Trim();
            string tableid = drTable["sysTableID"].ToString().Trim();
            string sql = "select condition from sysAdminDM where systableid=" + tableid + " and (sysUserID=" + ws + " or sysUserGroupID  in (select sysUserGroupID from sysUser where sysUserID=" + ws + " ))";
            sql = UpdateSpecialCondition(sql, info);
            
           
            Database dbStruct = ConnectionInfo.GetStructDatabase();
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
        private void GetPkMaster()
        {
            if (tbStruct == null) this.GetStructInfo();
            foreach (DataRow drField in tbStruct.Rows)
            {
                string fieldName = drField["FieldName"].ToString();
                int type = int.Parse(drField["Type"].ToString());
                switch (type)
                {
                    case 0:
                    case 6:
                        this.PkMaster = new SqlField(fieldName, this.GetDbType(type));
                        this.quote = "'";
                        break;
                    case 3:
                        this.PkMaster = new SqlField(fieldName, this.GetDbType(type));
                        break;
                }
            }
        }
        string quote = "";
        private SqlDbType GetDbType(int fType)
        {
            SqlDbType tmp = SqlDbType.VarChar;
            switch (fType)
            {
                case 0:
                case 1:
                    return SqlDbType.NVarChar;

                case 2:
                case 16:
                    return SqlDbType.NVarChar;
                case 3:
                case 4:
                case 5:
                    return SqlDbType.Int;

                case 6:
                case 7:
                case 15:
                    return SqlDbType.UniqueIdentifier;

                case 8:
                    return SqlDbType.Decimal;

                case 9:
                case 11:
                case 14:
                    return SqlDbType.DateTime;

                case 10:
                    return SqlDbType.Bit;

                case 12:
                    return SqlDbType.Image;

                case 13:
                    return SqlDbType.NText;
            }
            return tmp;
        }
        public DataRow CheckRule(Info info ,DataRow drData)
        {
            if (tbStruct == null) this.GetStructInfo();
            foreach (DataRow drField in tbStruct.Rows)
            {
                if (drField["Visible"].ToString() == "0")
                {
                    continue;
                }
                string fieldName = drField["FieldName"].ToString();
                int pType = int.Parse(drField["Type"].ToString());
                switch (pType)
                {
                    case 3:
                    case 6:
                        {
                            continue;
                        }
                }
                string fieldValue = drData[fieldName].ToString();
                // Kiểm tra AllowNull theo điều kiện 
                bool AllowNull = drField["AllowNull"].ToString() == "1";
                if (drField["AllowNull"] != DBNull.Value && drField["AllowNull"].ToString() != "0" && drField["AllowNull"].ToString() != "1")
                {
                    string conditionNull = PkMaster.FieldName + "=" + quote + drData[this.PkMaster.FieldName] + quote + " and (" + drField["AllowNull"].ToString() + ")";
                    DataRow[] ldrMt = drData.Table.Select(conditionNull);
                    AllowNull = (ldrMt.Length != 0);
                }
                if (!AllowNull && fieldValue == string.Empty)
                {
                    drData.SetColumnError(fieldName, "Phải nhập");
                    //LogFile.AppendToFile("CheckErr.txt",, fieldName + "_" + "Phải nhập");
                }
                else
                {
                    drData.SetColumnError(fieldName, string.Empty);
                }
                if (fieldValue != string.Empty)
                {
                    if (bool.Parse(drField["IsUnique"].ToString()))
                    {
                        string tableName = this.TableName;
                        string pk = this.PkMaster.FieldName;
                        string pkValue = drData[pk].ToString();
                        if (this.IsUnique(info, fieldValue, fieldName, tableName, pk, pkValue))
                        {
                            drData.SetColumnError(fieldName, string.Empty);
                        }
                    }
                }

            }
            return drData;
        }
        protected bool IsUnique(Info info,string value, string fieldName, string tableName, string pk, string pkValue)
        {
            string sql = "select " + fieldName + " from " + tableName + " where " + fieldName + " = " + quote + value + quote;

                sql += " and " + pk + " <> " + quote + pkValue + quote;
            Database dbData;
            if (drTable["sysPackageID"].ToString() == "5")
            {
                dbData = ConnectionInfo.GetStructDatabase();
            }
            else
            {
                dbData = ConnectionInfo.GetDataDatabase(info);
            }

            DataTable dtData = dbData.GetDataTable(sql);
            return ((dtData == null) || (dtData.Rows.Count == 0));
        }
        public bool CheckRightInsert(Info info, DataRow drData)
        {
            if (drTable["sysUserID"] != null)
            {
                return true;
            }
            else
            {
                try
                {
                    if (info.UserID.Trim() == drTable["sysUserID"].ToString().Trim()) return true;
                    string ws = info.UserID.ToString().Trim();
                    string tableid = drTable["sysTableID"].ToString().Trim();
                    string sql = "select condition from sysAdminDM where systableid=" + tableid + " and (sysUserID=" + ws + " or sysUserGroupID  in (select sysUserGroupID from sysUser where sysUserID=" + ws + " ))";
                    sql = UpdateSpecialCondition(sql, info);
                    Database dbStruct = ConnectionInfo.GetStructDatabase();
                    DataTable tbCon = dbStruct.GetDataTable(sql);
                    string dk = "(";
                    foreach (DataRow dr in tbCon.Rows)
                    {
                        dk += "(" + dr["condition"].ToString() + ") or ";
                    }
                    if (dk.Contains("or")) dk = dk.Substring(0, dk.Length - 3) + ")";
                    if (drData.Table.Select(dk).Length > 0) return true;
                    else
                        return false;
                }
                catch (Exception ex)
                { return false; }
            }
        }
        public bool CheckRightDelete(Info info, DataRow drData)
        {
            if (drTable["sysUserID"] != null)
            {
                return true;
            }
            else
            {
                try
                {
                    if (info.UserID.Trim() == drTable["sysUserID"].ToString().Trim()) return true;
                    string ws = info.UserID.ToString().Trim();
                    string tableid = drTable["sysTableID"].ToString().Trim();
                    string sql = "select condition from sysAdminDM where systableid=" + tableid + " and (sysUserID=" + ws + " or sysUserGroupID  in (select sysUserGroupID from sysUser where sysUserID=" + ws + " ))";
                    sql = UpdateSpecialCondition(sql, info);
                    Database dbStruct = ConnectionInfo.GetStructDatabase();
                    DataTable tbCon = dbStruct.GetDataTable(sql);
                    string dk = "(";
                    foreach (DataRow dr in tbCon.Rows)
                    {
                        dk += "(" + dr["condition"].ToString() + ") or ";
                    }
                    if (dk.Contains("or")) dk = dk.Substring(0, dk.Length - 3) + ")";
                    if (drData.Table.Select(dk).Length > 0) return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
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
            Database dbData;
            if (drTable["sysPackageID"].ToString() == "5")
            {
                dbData = ConnectionInfo.GetStructDatabase();
            }
            else
            {
                dbData = ConnectionInfo.GetDataDatabase(info);
            }
            return await Task.Run(() => dbData.GetDataTable(sql));            
        }
        
        public string UpdateSpecialCondition(string query,Info info)
        {
            if (info.Config.Contains("NamLamViec"))
            {
                query = query.Replace("@@NAM", info.Config["NamLamViec"].ToString());
            }
            foreach (DictionaryEntry o in info.Config)
            {
                if (info.Config[o.Key.ToString()] == null) continue;
                query = query.Replace("@@" + o.Key.ToString(), info.Config[o.Key.ToString()].ToString());
                query = query.Replace("@@" + o.Key.ToString().ToUpper(), info.Config[o.Key.ToString()].ToString());
            }
            return query;
        }
        private void GenSQLInsert()
        {
            string fieldName;
            int type;
            string _sInsert1 = "";
            string _sUpdate1 = "";
            string _sDelete1 = "";
            string _sUpdateImage1 = "";
            
            List<SqlField> _vInsert1;
            List<SqlField> _vUpdate1;
            List<SqlField> _vDelete1;
            List<SqlField> _vUpdateImage1;

            _sInsert1 = "insert into " + TableName + "(";
            _sUpdate1 = "update " + TableName + " set ";
            _sDelete1 = "delete from " + TableName;
            _sUpdateImage1 = "update " + TableName + " set ";
            //Thao tác với file
            //----------------
            //  string userID = Config.GetValue("sysUserID").ToString();
            _vInsert1 = new List<SqlField>();
            _vUpdate1 = new List<SqlField>();
            _vDelete1 = new List<SqlField>();
            _vUpdateImage1 = new List<SqlField>();
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
                        _vUpdate1.Add(new SqlField(fieldName, Factory.GetDbType(type)));
                        _vDelete1.Add(new SqlField(fieldName, Factory.GetDbType(type)));
                        break;
                }
                if (type == 3)
                {
                    this._identityPk = true;
                    condition = " where " + fieldName + " = @" + fieldName;
                    _vUpdate1.Add(new SqlField(fieldName, Factory.GetDbType(type)));
                    _vDelete1.Add(new SqlField(fieldName, Factory.GetDbType(type)));
                }
                else
                {
                    if (type == 12)
                    {
                        _vUpdateImage1.Add(new SqlField(fieldName, Factory.GetDbType(type)));
                        _sUpdateImage1 += fieldName + "=@" + fieldName + ",";
                        continue;
                    }

                    if ((((type != 0) && (type != 6)) && (type != 3)))
                    {
                        _vUpdate1.Add(new SqlField(fieldName, Factory.GetDbType(type)));
                        _sUpdate1 += fieldName + " = @" + fieldName + ",";
                    }
                    _sInsert1 = _sInsert1 + fieldName + ",";
                    tmp = tmp + "@" + fieldName + ",";
                    _vInsert1.Add(new SqlField(fieldName, Factory.GetDbType(type)));
                }
            }
            _sInsert1 = _sInsert1.Remove(_sInsert1.Length - 1) + ")" + tmp.Remove(tmp.Length - 1) + ")";
            _sUpdate1 =_sUpdate1.Remove(_sUpdate1.Length - 1) + condition;
            _sUpdateImage =_sUpdateImage1.Remove(_sUpdateImage1.Length - 1);
            _sDelete1 =_sDelete1 + condition;
            this._vInsert = _vInsert1;
            this._vUpdate = _vUpdate1;
            this._vDelete = _vDelete1;
            this._sInsert = _sInsert1;
            this._sUpdate = _sUpdate1;
            this._sDelete = _sDelete1;
            this._vUpdateImage = _vUpdateImage1;

        }
        public async Task<DataRow> Insert(Info info, DataRow data)
        {

            if (_sInsert == "")
                GenSQLInsert();
            //Kiểm tra quyền insert của UserID

          return  await Task.Run(() => InsertSQL(info, data));
            
        }
        public async Task<DataRow> Update(Info info, DataRow data)
        {

            if (_sInsert == "")
                GenSQLInsert();
            //Kiểm tra quyền insert của UserID

            return await Task.Run(() => UpdateSql(info, data, DataAction.Update));

        }
        public async Task<DataRow> Delete(Info info, DataRow data)
        {

            if (_sInsert == "")
                GenSQLInsert();
            //Kiểm tra quyền insert của UserID

            return await Task.Run(() => UpdateSql(info, data, DataAction.Delete));

        }
        private DataRow InsertSQL(Info info,DataRow drData)
        {
            Database dbData;
            if (drTable["sysPackageID"].ToString() == "5")
            {
                dbData = ConnectionInfo.GetStructDatabase();
            }
            else
            {
                dbData = ConnectionInfo.GetDataDatabase(info);
            }
            dbData.BeginMultiTrans();
            try {
                drData= UpdateSql (info,drData,DataAction.Insert);
                if (drData != null)
                {
                    
                    dbData.EndMultiTrans();
                    return drData;
                }
                else
                {
                    dbData.RollbackMultiTrans();
                    return null;
                }
            }catch(Exception ex)
            {

                dbData.RollbackMultiTrans();
                return null;
            }
    
        }
            private DataRow UpdateSql(Info info, DataRow drData, DataAction action)
        {
            string fieldName;
            Database dbData;
            if (drTable["sysPackageID"].ToString() == "5")
            {
                dbData = ConnectionInfo.GetStructDatabase();
            }
            else
            {
                dbData = ConnectionInfo.GetDataDatabase(info);
            }

            List<SqlField> tmp = new List<SqlField>();
            List<string> paraNames = new List<string>();
            List<object> paraValues = new List<object>();
            List<SqlDbType> paraTypes = new List<SqlDbType>();
            string sql = string.Empty;
            bool updateIdentity = false;
            bool isDelete = false;
            switch (action)
            {
                case DataAction.Insert:
                    if (this._identityPk)
                    {
                        updateIdentity = true;
                    }
                    tmp = this._vInsert;
                    sql = this._sInsert;
                    break;

                case DataAction.Delete:
                    tmp = this._vDelete;
                    sql = this._sDelete;
                    //drData.RejectChanges();
                    isDelete = true;
                    break;

                case DataAction.Update:
                    tmp = this._vUpdate;
                    sql = this._sUpdate;
                    break;
            }
            foreach (SqlField sqlField in tmp)
            {
                fieldName = sqlField.FieldName;
                paraNames.Add(fieldName);
                if (drData[fieldName].ToString() != string.Empty)
                {
                    if(sqlField.DbType== SqlDbType.UniqueIdentifier)
                    {
                        if (drData[fieldName] != DBNull.Value && drData[fieldName].ToString() != string.Empty)
                            paraValues.Add(Guid.Parse(drData[fieldName].ToString()));
                        else
                        {
                            paraValues.Add(null);
                        }    
                    }
                    else
                        paraValues.Add(drData[fieldName]);
                }
                else
                {
                    paraValues.Add(DBNull.Value);
                }
                paraTypes.Add(sqlField.DbType);
            }
            bool updateWsCompleted = true;


            if (sql == string.Empty)
            {
                return null;
            }
            bool result = dbData.UpdateData(sql, paraNames.ToArray(), paraValues.ToArray(), paraTypes.ToArray());
            string pk = string.Empty;
            pk = drTable["Pk"].ToString();
            if (result && updateIdentity)
            {
                object o = dbData.GetValue("select @@identity");
                if (o != null)
                {
                    drData[pk] = o;
                }
            }
            //Update dữ liệu File, dữ liệu Image


            if (drData != null)//Update ws
            {
                if ( drData.Table.Columns.Contains("ws"))
                {
                    sql = "update " + this.TableName + " set ws='_" +info.UserID.ToString() + "_' where " + pk + "='" + drData[pk].ToString() + "'";
                    updateWsCompleted = dbData.UpdateByNonQuery(sql);
                    sql = "update " + this.TableName + " set Grws='_" + info.UserGroupID.ToString() + "_' where " + pk + "='" + drData[pk].ToString() + "'";
                    updateWsCompleted = updateWsCompleted && dbData.UpdateByNonQuery(sql);
                }
            }
            result = result && updateWsCompleted;
            if ((!result || (this._vUpdateImage.Count <= 0)) || isDelete)
            {
                return drData;
            }
            string exsql = string.Empty;
            if (drData[pk].GetType() == typeof(int))
            {
                exsql = "";
            }
            else
            {
                exsql = "'";
            }

                sql = this._sUpdateImage + " where " + pk + "=" + exsql + drData[pk].ToString() + exsql;

            List<object> pImValue = new List<object>();
            List<SqlDbType> pImType = new List<SqlDbType>();
            List<string> pImName = new List<string>();
            foreach (SqlField sqlField in this._vUpdateImage)
            {
                fieldName = sqlField.FieldName;
                pImName.Add(fieldName);
                if (drData[fieldName].ToString() != string.Empty)
                {
                    pImValue.Add(drData[fieldName]);
                }
                else
                {
                    pImValue.Add(DBNull.Value);
                }
                pImType.Add(sqlField.DbType);
            }
            result =result && dbData.UpdateData(sql, pImName.ToArray(), pImValue.ToArray(), pImType.ToArray());
            if (result) return drData;
            else return null;
        }
        public async Task<bool> updateJsonLayout( string json)
        {
            try
            {
                Database dbStruct = ConnectionInfo.GetStructDatabase();
                string sql = "update systable set jsonLayout=@FileLayout where systableID=@systableID";
                return await Task.Run(() => dbStruct.UpdateDatabyPara(sql, new string[] { "@FileLayout", "@systableID" }, new object[] { json, drTable["systableID"] }));
            }catch
            {
                return await Task.Run(()=>false);
            }
        }
        internal void GetStructInfo()
        {
            Database dbStruct;
            dbStruct = ConnectionInfo.GetStructDatabase();
            string sql;           

            //Lấy tbStruct
            sql = "select * from sysTable where TableName='" + TableName + "'";
            DataTable tb = dbStruct.GetDataTable(sql);
            if (tb.Rows.Count > 0) drTable = tb.Rows[0];
            else return;
            sql = "select * from sysField where sysTableID=" + drTable["sysTableID"].ToString();
            tbStruct = dbStruct.GetDataTable(sql);
            GenSQLInsert();
        }

    }
}