using Antlr.Runtime.Misc;
using BPMAPI.APIControl;
using BPMAPI.CDTControl;
using CDTDatabase;
using CDTLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;

namespace BPMAPI.DataFactory
{
    
    public class MTDTData
    {
        public string DtTableName;
        public string MtTableName;
        public DataRow drTable;
        public DataRow drTableMaster;
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
        
        public DataTable tbAction = new DataTable();
        public DataTable tbTask = new DataTable();
        public DataTable tbWF;        
        public DataTable tbActionPara;
        public DataTable tbDrStruct;

        public DataSet DsStruct = new DataSet();
        public DataTable tbDetailStruct;//Cấu trúc bảng đính kèm

        public MTDTData(string tableName)
        {

            DtTableName = tableName;
            string sql = "select sysTableID from sysTable where TableName='" + tableName.ToString() +"'";
            Database dbStruct = ConnectionInfo.GetStructDatabase();
            object o = dbStruct.GetValue(sql);
            if (o == null) return;
            sysTableID = o.ToString();
            this.GetStructInfo();            
            GetPkMaster();
            GetAction();
        }
        public MTDTData(int sysTableID)
        {

            this.sysTableID = sysTableID.ToString();
            //Trong mtdt, systableID là ID của bảng DT
            string sql = "select TableName from sysTable where sysTableID=" + sysTableID.ToString();
            Database dbStruct = ConnectionInfo.GetStructDatabase();
            object o = dbStruct.GetValue(sql);
            if (o == null) return;
            DtTableName = o.ToString();

            this.GetStructInfo();           
            GetPkMaster();
            GetAction();
            
        }
        public async Task< DataSet> GetData(Info info,string condition)
        {
            try
            {
                DataSet ds = new DataSet();
                Database dbData;
                dbData = ConnectionInfo.GetDataDatabase(info);
                if (DsStruct == null)
                {
                    this.GetStructInfo();
                }

                DataTable tbUserTask = GetUserTask(info);
                List<string> condTask = GetUserTaskCondition(tbUserTask);
                string sqlMt = "";
                if (condition == "")
                {
                    condition = " 1 = 1 "; // nghĩa là lấy dữ liệu không có điều kiện từ đầu thì chỉ lấy rowcount thôi
                    object oRowCount = info.Config["RowCount"].ToString(); // Config.GetValue("RowCount");
                    sqlMt = "select top (" + oRowCount.ToString() + ") * from " + MtTableName;
                }
                else
                {
                    sqlMt = "select * from " + MtTableName;
                }    
                condition += " and " + condTask[0];               
                 sqlMt += " where " + condition;
                string SOrder = "";
                if(this.tbDrStruct.Rows[0]["SortOrder"]!=DBNull.Value)
                    SOrder = "  order by " + this.tbDrStruct.Rows[0]["SortOrder"].ToString();

                DataTable tb = await Task.Run(() => dbData.GetDataTable(sqlMt +  SOrder));
                ds.Tables.Add(tb);
                GetPkMaster();
                sqlMt = sqlMt.Replace(" * from "," " +  this.PkMaster.FieldName  + " from " );

                DataRow[] RelaRow = this.DsStruct.Tables[1].Select("refTable='" + MtTableName + "'");
                string RelaCol = this.PkMaster.FieldName;
                if (RelaRow.Length > 0) RelaCol = RelaRow[0]["FieldName"].ToString();

                string sqlDT = "select * from " + DtTableName + " where " + RelaCol + " in (" + sqlMt + ")";
                DataTable tbDT= await Task.Run(() => dbData.GetDataTable(sqlDT));
                ds.Tables.Add(tbDT);
                for (int idx = 2; idx < tbDrStruct.Rows.Count; idx++)
                {
                    string DetailTableName = tbDrStruct.Rows[idx]["TableName"].ToString();
                    string sql = "select * from " + DetailTableName + " where MTID in (" + sqlMt + ")";
                    DataTable tbDetail= await Task.Run(() => dbData.GetDataTable(sql));
                    ds.Tables.Add(tbDetail);
                }
                return ds;
            }
            catch
            {
                return null;
            }
        }
       
        
        private void GetPkMaster()
        {
            if (DsStruct == null) this.GetStructInfo();
            foreach (DataRow drField in DsStruct.Tables[0].Rows)
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
            return null;
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
            //Lấy danh sách các bảng MT, DT, và bảng đính kèm.
            Database dbStruct;
            //tbDrStruct.Rows.Clear();
            //DsStruct.Tables.Clear();
            dbStruct = ConnectionInfo.GetStructDatabase();
            string sql;
            sql = "select * from systable where tablename in (select MasterTable from systable where TableName='" + DtTableName + "') ";
            DataTable  _tbDrStruct = dbStruct.GetDataTable(sql);
            if (_tbDrStruct.Rows.Count != 1) return;
            //Lấy tbStruct
            sql = "select * from sysTable where TableName='" + DtTableName + "' ";
            DataTable tb = dbStruct.GetDataTable(sql);
            if (tb.Rows.Count > 0)
            {
                _tbDrStruct.Rows.Add(tb.Rows[0].ItemArray);
            }
            //lấy thông tin các bảng đính kèm từ các bảng chi tiết
            sql = "select t.*,tb.TableName from sysDetail t inner join sysTable tb on t.sysDetailID=tb.sysTableID where t.sysTableID =" + this.sysTableID + " order by t.stt";
            tbDetailStruct = dbStruct.GetDataTable(sql);
            //lấy tbstruct từ các bảng chi tiết
            sql = "select b.* from sysDetail a inner join systable b on a.sysDetailID=b.sysTableID  where a.sysTableID =" + this.sysTableID + "  order by a.stt ";
            DataTable tbdt = dbStruct.GetDataTable(sql);
            foreach (DataRow dr in tbdt.Rows)
            {
                _tbDrStruct.Rows.Add(dr.ItemArray);
            }
            //Đến đây, lấy thông tin sysfield trương ứng cho từng bảng
            try
            {
                DataSet dsstruct = new DataSet();
                foreach (DataRow drSt in _tbDrStruct.Rows)
                {
                    sql = "select * from sysField where sysTableID=" + drSt["sysTableID"].ToString();
                    DataTable tbStruct = dbStruct.GetDataTable(sql);
                    dsstruct.Tables.Add(tbStruct);
                }
                DsStruct = dsstruct;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi lấy cấu trúc dữ liệu: " + ex.Message);
            }

            MtTableName = _tbDrStruct.Rows[0]["TableName"].ToString();
            this.tbDrStruct = _tbDrStruct;
            GenSQLInsert();
        }
        public void GetAction()
        {
            Database dbStruct;
            dbStruct = ConnectionInfo.GetStructDatabase();
            if (tbDrStruct == null) return;
            DataRow DrTable = tbDrStruct.Rows[1];
            if (!DrTable.Table.Columns.Contains("Systableid") || DrTable["systableID"] == DBNull.Value) return;
            string sql = "select * from sysWF where sysTableID=" + DrTable["systableID"].ToString();
            tbWF = dbStruct.GetDataTable(sql);
            if (tbWF == null || tbWF.Rows.Count == 0) return;
            sql = "select * from sysAction where WFID='" + tbWF.Rows[0]["ID"].ToString() + "'";
            tbAction = dbStruct.GetDataTable(sql);
            sql = "select * from sysActionPara where ActionID in  (select id from sysAction where WFID='" + tbWF.Rows[0]["ID"].ToString() + "')";
            tbActionPara = dbStruct.GetDataTable(sql);
            sql = "select * from sysTask where WFID='" + tbWF.Rows[0]["ID"].ToString() + "'";
            tbTask =  dbStruct.GetDataTable(sql);           
        }
        public  DataTable GetUserTask(Info info)
        {
            Database dbStruct;
            dbStruct = ConnectionInfo.GetStructDatabase();
           // if (tbDrStruct == null) return null;
            string sql;
            sql = " select TaskID, CView, CEdit, CDelete, Cprint from sysUserTask where sysUserID =" + info.UserID.ToString() + " and TaskID in (select Id from sysTask where WFID='" + tbWF.Rows[0]["ID"].ToString() + "')";
            sql += " union all ";
            sql += " select a.TaskID, a.CView, CEdit, CDelete, Cprint from sysUserGrTask a inner join sysuser b on a.sysUserGroupID=b.sysUserGroupID where b.sysUserID=  " + info.UserID.ToString() + " and TaskID in (select Id from sysTask where WFID='" + tbWF.Rows[0]["ID"].ToString() + "')";
            return  dbStruct.GetDataTable(sql);
        }
        public DataTable GetUserAction(Info info)
        {
            Database dbStruct;
            dbStruct = ConnectionInfo.GetStructDatabase();
            if (tbDrStruct == null) return null;
            string sql;
            sql = " select b.*,a.CAllow from sysUserAction a inner join sysAction b on a.ActionID = b.Id where sysUserID = " + info.UserID.ToString() + " and b.WFID = '" + tbWF.Rows[0]["ID"].ToString() + "'";
            sql += " union all ";
            sql += " select c.*,a.CAllow from sysUserGrAction a inner join sysuser b on a.sysUserGroupID = b.sysUserGroupID inner join sysAction c on a.ActionID = c.Id where b.sysUserID = " + info.UserID.ToString() + " and c.WFID = '" + tbWF.Rows[0]["ID"].ToString() + "'";

            return dbStruct.GetDataTable(sql);
        }
        public  List<string> GetUserTaskCondition(DataTable tbUTask)
        {
            List<string> con = new List<string>();
            string _conditionViewTask;
            string _conditionEditTask;
            if (tbUTask == null)
            {
                return null;
            }
            if (tbUTask.Rows.Count == 0)
            {
                _conditionViewTask = "(TaskID is null)";
                _conditionEditTask = "(TaskID is null)";
                con.Add(_conditionViewTask);
                con.Add(_conditionEditTask);
                return con;
            }
            if (tbUTask.Rows.Count > 0)
            {
                _conditionViewTask = "(TaskID is null ";
                _conditionEditTask = "(TaskID is null ";
                foreach (DataRow dr in tbUTask.Rows)
                {
                    if (dr["CView"] != DBNull.Value && dr["CView"].ToString() != string.Empty)
                    {
                        _conditionViewTask += " or (TaskID='" + dr["TaskID"].ToString() + "' and (" + dr["CView"].ToString() + "))";
                    }
                    else
                    {
                        _conditionViewTask += " or (TaskID='" + dr["TaskID"].ToString() + "')";
                    }
                    if (dr["CEdit"] != DBNull.Value && dr["CEdit"].ToString() != string.Empty)
                    {
                        _conditionEditTask += " or (TaskID='" + dr["TaskID"].ToString() + "' and (" + dr["CEdit"].ToString() + "))";
                    }
                    else
                    {
                        _conditionEditTask += " or (TaskID='" + dr["TaskID"].ToString() + "')";
                    }
                }
                _conditionViewTask += ")";
                _conditionEditTask += ")";
                con.Add(_conditionViewTask);
                con.Add(_conditionEditTask);
                return con;
            }
            return null;
        }
    }
}