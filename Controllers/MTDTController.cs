using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using BPMAPI.APIControl;
using BPMAPI.DataFactory;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using CDTDatabase;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Web.Http.Cors;
namespace BPMAPI.Controllers
{
    [EnableCors("*", "*", "*")]
    public class MTDTController : ApiController
    {
        public Database _dbStruct = ConnectionInfo.GetStructDatabase();
        [HttpGet]
        public async Task<IHttpActionResult> GetTableName(int systableID)
        {
            try
            {
                MTDTData _data = DataFactory.Factory.findMTDT(systableID);
                if (_data == null)
                {
                    _data = await Task.Run(() => new MTDTData(systableID));
                    if (!Factory.LstMtDt.ContainsKey(_data.DtTableName))
                        Factory.LstMtDt.TryAdd(_data.DtTableName, _data);
                    return Ok(_data.DtTableName);
                }
                return Ok(_data.DtTableName);
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpGet]
        public async Task<IHttpActionResult> GetTableID(string TableName)
        {
            try
            {
                MTDTData _data = DataFactory.Factory.findMTDT(TableName);
                if (_data == null)
                {
                    _data = await Task.Run(() => new MTDTData(TableName));
                    if (!Factory.LstMtDt.ContainsKey(_data.DtTableName))
                        Factory.LstMtDt.TryAdd(TableName, _data);
                    return Ok(_data.sysTableID);
                }
                return Ok(_data.sysTableID);
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpGet]
        public async Task<IHttpActionResult> GetWF(string TableName)
        {
            try
            {
                MTDTData _data = DataFactory.Factory.findMTDT(TableName);
                if (_data == null)
                {
                    _data = await Task.Run(() => new MTDTData(TableName));
                    if (!Factory.LstMtDt.ContainsKey(_data.DtTableName))
                        Factory.LstMtDt.TryAdd(TableName, _data);
                    
                }
                if (_data.tbWF == null)
                {
                    _data.GetAction();
                }
                DataSet dstmp = new DataSet();
                dstmp.Tables.AddRange(new DataTable[] { _data.tbWF.Copy(), _data.tbAction.Copy(), _data.tbTask.Copy(), _data.tbActionPara.Copy() });
                return Ok(CDTLib.JsonConverter.ConvertDataSetToJsonWithSchema(dstmp));
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetListDrStruct([FromBody] JObject data)
        {
            string token = "";
            foreach (var header in Request.Headers)
            {
                if (header.Key.ToLower() == "authorization")
                {
                    token = (header.Value as string[])[0];
                    break;
                }
            }
            Info info = ConnectionInfo.GetConnection(token);
            APIException tb = new APIException { ErrorContent = "Lỗi đăng nhập hết hạn" };
            if (info == null) return Ok(tb);
            try
            {
                string tableName = data["TableName"].ToObject<string>();
                MTDTData _data = DataFactory.Factory.findMTDT(tableName);
                if (_data == null)
                {

                    _data = await Task.Run(() => new MTDTData(tableName));
                    if (!Factory.LstMtDt.ContainsKey(_data.DtTableName))
                        Factory.LstMtDt.TryAdd(tableName, _data);

                }
                JObject re = CDTLib.JsonConverter.ConvertDataTableToJsonWithSchema(_data.tbDrStruct, "sysTableID");

                return Ok(re);
                //DataTable result = await _data.GetDataFull(info);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
        [HttpPost]
        public async Task<IHttpActionResult> GetDetailTablesInfo([FromBody] JObject data)
        {
            string token = "";
            foreach (var header in Request.Headers)
            {
                if (header.Key.ToLower() == "authorization")
                {
                    token = (header.Value as string[])[0];
                    break;
                }
            }
            Info info = ConnectionInfo.GetConnection(token);
            APIException tb = new APIException { ErrorContent = "Lỗi đăng nhập hết hạn" };
            if (info == null) return Ok(tb);
            try
            {
                string tableName = data["TableName"].ToObject<string>();
                MTDTData _data = DataFactory.Factory.findMTDT(tableName);
                if (_data == null)
                {

                    _data = await Task.Run(() => new MTDTData(tableName));
                    if (!Factory.LstMtDt.ContainsKey(_data.DtTableName))
                        Factory.LstMtDt.TryAdd(tableName, _data);

                }
                JObject re = CDTLib.JsonConverter.ConvertDataTableToJsonWithSchema(_data.tbDetailStruct,"stt");

                return Ok(re);
                //DataTable result = await _data.GetDataFull(info);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
        [HttpPost]
        public async Task<IHttpActionResult> GetDsStruct([FromBody] JObject data)
        {
            string token = "";
            foreach (var header in Request.Headers)
            {
                if (header.Key.ToLower() == "authorization")
                {
                    token = (header.Value as string[])[0];
                    break;
                }
            }
            Info info = ConnectionInfo.GetConnection(token);
            APIException tb = new APIException { error = "Lỗi đăng nhập hết hạn" };
            if (info == null) return Ok(tb);
            try
            {
                string tableName = data.GetValue("TableName").ToString();
               MTDTData _data = DataFactory.Factory.findMTDT(tableName.Replace("\"",""));
                if (_data == null)
                {
                    _data = await Task.Run(() => new MTDTData(tableName));
                    if (!Factory.LstMtDt.ContainsKey(tableName))
                        Factory.LstMtDt.TryAdd(tableName, _data);
                }
                else
                {
                    _data.GetStructInfo();
                }
                _data.GetStructInfo();//Tạm thời trong giai đoạn triển khai, struct thay đổi nhiều
                _data.GetAction();
                //
                //string re = ConnectionInfo.ConvertRowtoString(_data.drTable);


                return Ok(CDTLib.JsonConverter.ConvertDataSetToJsonWithSchema(_data.DsStruct));
                //DataTable result = await _data.GetDataFull(info);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
        [HttpPost]
        public async Task<IHttpActionResult> GetUserAction([FromBody] JObject data)
        {
            string token = "";
            foreach (var header in Request.Headers)
            {
                if (header.Key.ToLower() == "authorization")
                {
                    token = (header.Value as string[])[0];
                    break;
                }
            }
            Info info = ConnectionInfo.GetConnection(token);
            APIException tb = new APIException { error = "Lỗi đăng nhập hết hạn" };
            if (info == null) return Ok(tb);
            try
            {
                string tableName = data.GetValue("TableName").ToString();
                MTDTData _data = DataFactory.Factory.findMTDT(tableName.Replace("\"", ""));
                if (_data == null)
                {
                    _data = await Task.Run(() => new MTDTData(tableName));
                    if (!Factory.LstMtDt.ContainsKey(tableName))
                        Factory.LstMtDt.TryAdd(tableName, _data);
                }
                else
                {
                    _data.GetStructInfo();
                }
                //
                //string re = ConnectionInfo.ConvertRowtoString(_data.drTable);

                DataTable userAction = _data.GetUserAction(info);
                return Ok(CDTLib.JsonConverter.ConvertDataTableToJsonWithSchema(userAction));
                //DataTable result = await _data.GetDataFull(info);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
        [HttpPost]
        public async Task<IHttpActionResult> GetUserTask([FromBody] JObject data)
        {
            string token = "";
            foreach (var header in Request.Headers)
            {
                if (header.Key.ToLower() == "authorization")
                {
                    token = (header.Value as string[])[0];
                    break;
                }
            }
            Info info = ConnectionInfo.GetConnection(token);
            APIException tb = new APIException { error = "Lỗi đăng nhập hết hạn" };
            if (info == null) return Ok(tb);
            try
            {
                string tableName = data.GetValue("TableName").ToString();
                MTDTData _data = DataFactory.Factory.findMTDT(tableName.Replace("\"", ""));
                if (_data == null)
                {
                    _data = await Task.Run(() => new MTDTData(tableName));
                    if (!Factory.LstMtDt.ContainsKey(tableName))
                        Factory.LstMtDt.TryAdd(tableName, _data);
                }
                else
                {
                    _data.GetStructInfo();
                }
                //
                //string re = ConnectionInfo.ConvertRowtoString(_data.drTable);

                DataTable userTask = _data.GetUserTask(info);
                return Ok(CDTLib.JsonConverter.ConvertDataTableToJsonWithSchema(userTask));
                //DataTable result = await _data.GetDataFull(info);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
        [HttpPost]
        public async Task<IHttpActionResult> GetDataFull([FromBody] JObject data)
        {
            //Lấy token từ header chứ ko phải lấy từ data
            string token = "";
            string condition = "";
            foreach (var header in Request.Headers)
            {
                if(header.Key.ToLower() == "authorization")
                {
                    token = (header.Value as string[])[0];
                    break;
                }
                if (header.Key.ToLower() == "condition")
                {
                    condition = (header.Value as string[])[0];
                    break;
                }
            }
            Info info = ConnectionInfo.GetConnection(token);
            APIException tb = new APIException { error = "Lỗi đăng nhập hết hạn" };
            if (info == null) return Ok(tb);
            string Construct = ConfigurationManager.AppSettings["StructCon"];
            try
            {
                string tableName = data["TableName"].ToObject<string>();
                MTDTData _data = DataFactory.Factory.findMTDT(tableName);
                if (_data == null)
                {
                    _data = await Task.Run(() => new MTDTData(tableName));
                    if (!Factory.LstMtDt.ContainsKey(_data.DtTableName))
                        Factory.LstMtDt.TryAdd(tableName, _data);
                }
                DataSet result = await _data.GetData(info,condition);
                if(result == null) return BadRequest();
                
                return Ok(CDTLib.JsonConverter.ConvertDataSetToJsonWithSchema(result));
            }
            catch(Exception ex)
            {
                return BadRequest();
            }            
        }
        [HttpPost]

        public async Task<IHttpActionResult> UpdateLayoutJson([FromBody] JObject data)
        {
            string token = "";
            foreach (var header in Request.Headers)
            {
                if (header.Key.ToLower() == "authorization")
                {
                    token = (header.Value as string[])[0];
                    break;
                }
            }
            Info info = ConnectionInfo.GetConnection(token);
            APIException tb = new APIException { error = "Lỗi đăng nhập hết hạn" };
            if (info == null) return Ok(tb);
            try
            {
                string tableName = data["TableName"].ToObject<string>();
                string json= data["jsonLayout"].ToObject<string>();
                SingleData _data = DataFactory.Factory.findSingle(tableName);
                await _data.updateJsonLayout(json);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
            
        }
        [HttpPost]
        public async Task<IHttpActionResult> PostInSertData([FromBody] JObject data)
        {
            //Kiểm tra Connection, Nếu info nằm trong list Info thì tiếp tục
            try
            {
                string token = "";
                foreach (var header in Request.Headers)
                {
                    if (header.Key.ToLower() == "authorization")
                    {
                        token = (header.Value as string[])[0];
                        break;
                    }
                }
                Info info = ConnectionInfo.GetConnection(token);
                APIException tb = new APIException { error = "Lỗi đăng nhập hết hạn" };
                if (info == null) return Ok(tb);

                string tableName = data["TableName"].ToObject<string>();
                string sData = data["Data"].ToString(Formatting.None);
                DataRow drData = CDTLib.JsonConverter.ConvertJsonToRow(sData);
                if(drData==null ) return BadRequest("Dữ liệu chưa hợp lệ");
                MTDTData _data = DataFactory.Factory.findMTDT(tableName);
                if (_data == null)
                {
                    _data = new MTDTData(tableName);
                    Factory.LstMtDt.TryAdd(tableName,_data);
                }
                if (_data.CheckRightInsert(info, drData))
                {
                    drData = _data.CheckRule(info, drData);
                    if (drData.HasErrors)
                    {
                        string strError = "";
                        foreach (DataColumn col in drData.Table.Columns)
                        {
                            if (drData.GetColumnError(col) != string.Empty)
                                strError += "//" + col.ColumnName + ":" + drData.GetColumnError(col).ToString();
                        }

                        return BadRequest("Dữ liệu chưa hợp lệ:" + strError);
                    }
                    DataRow RowResult = await _data.Insert(info, drData);
                    if (RowResult != null)
                    {
                        return Ok(RowResult);
                    }
                    else BadRequest();
                }
                else
                {

                    return BadRequest("User không có quyền thêm dữ liệu");
                }
                //bool result = await _data.Insert(sData, info);
                //
            }
            catch
            {
                return BadRequest();
            }
            return BadRequest();
        }
        public async Task<IHttpActionResult> PostEditData([FromBody] JObject data)
        {
            //Kiểm tra Connection, Nếu info nằm trong list Info thì tiếp tục
            try
            {
                string token = "";
                foreach (var header in Request.Headers)
                {
                    if (header.Key.ToLower() == "authorization")
                    {
                        token = (header.Value as string[])[0];
                        break;
                    }
                }
                Info info = ConnectionInfo.GetConnection(token);
                APIException tb = new APIException { error = "Lỗi đăng nhập hết hạn" };
                if (info == null) return Ok(tb);

                string tableName = data["TableName"].ToObject<string>();
                string sData = data["Data"].ToString(Formatting.None);
                DataRow drData = CDTLib.JsonConverter.ConvertJsonToRow(sData);
                if (drData == null) return BadRequest("Dữ liệu chưa hợp lệ");
                SingleData _data = DataFactory.Factory.findSingle(tableName);
                if (_data == null)
                {
                    _data = new SingleData(tableName);
                    Factory.LstSingle.TryAdd(tableName,_data);
                }
                if (_data.CheckRightInsert(info, drData))
                {
                    drData = _data.CheckRule(info, drData);
                    if (drData.HasErrors)
                    {
                        string strError = "";
                        foreach (DataColumn col in drData.Table.Columns)
                        {
                            if (drData.GetColumnError(col) != string.Empty) strError += "//" + col.ColumnName + ":" + drData.GetColumnError(col).ToString();
                        }

                        return BadRequest("Dữ liệu chưa hợp lệ:" + strError);
                    }
                    DataRow RowResult = await _data.Update(info, drData);
                    if (RowResult != null)
                    {
                        return Ok(RowResult);
                    }
                    else BadRequest();
                }
                else
                {

                    return BadRequest("User không có quyền thêm dữ liệu");
                }
                //bool result = await _data.Insert(sData, info);
                //
            }
            catch
            {
                return BadRequest();
            }
            return BadRequest();
        }
        public async Task<IHttpActionResult> PostEditDelete([FromBody] JObject data)
        {
            //Kiểm tra Connection, Nếu info nằm trong list Info thì tiếp tục
            try
            {
                string token = "";
                foreach (var header in Request.Headers)
                {
                    if (header.Key.ToLower() == "authorization")
                    {
                        token = (header.Value as string[])[0];
                        break;
                    }
                }
                Info info = ConnectionInfo.GetConnection(token);
                APIException tb = new APIException { error = "Lỗi đăng nhập hết hạn" };
                if (info == null) return Ok(tb);

                string tableName = data["TableName"].ToObject<string>();
                string sData = data["Data"].ToString(Formatting.None);
                DataRow drData = CDTLib.JsonConverter.ConvertJsonToRow(sData);
                if (drData == null) return BadRequest("Dữ liệu chưa hợp lệ");
                SingleData _data = DataFactory.Factory.findSingle(tableName);
                if (_data == null)
                {
                    _data = new SingleData(tableName);
                    Factory.LstSingle.TryAdd(tableName,_data);
                }
                if (_data.CheckRightDelete(info, drData))
                {
                    drData = _data.CheckRule(info, drData);
                    if (drData.HasErrors)
                    {
                        string strError = "";
                        foreach (DataColumn col in drData.Table.Columns)
                        {
                            if (drData.GetColumnError(col) != string.Empty) strError += "//" + col.ColumnName + ":" + drData.GetColumnError(col).ToString();
                        }

                        return BadRequest("Dữ liệu chưa hợp lệ:" + strError);
                    }
                    DataRow RowResult = await _data.Delete(info, drData);
                    if (RowResult != null)
                    {
                        return Ok(RowResult);
                    }
                    else BadRequest();
                }
                else
                {

                    return BadRequest("User không có quyền thêm dữ liệu");
                }
                //bool result = await _data.Insert(sData, info);
                //
            }
            catch
            {
                return BadRequest();
            }
            return BadRequest();
        }
    }
    
}
