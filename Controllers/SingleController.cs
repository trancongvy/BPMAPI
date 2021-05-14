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
namespace BPMAPI.Controllers
{
    public class SingleController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> GetDataFull([FromBody] JObject data)
        {
            string token = data["Token"].ToObject<string>();
            Info info = ConnectionInfo.GetConnection(token);
            if (info == null) return BadRequest();
            string tableName = data["TableName"].ToObject<string>();
            
            SingleData _data = DataFactory.Factory.findSingle(tableName, info.StructCon);
            if (_data == null)
            {
                _data = new SingleData(); _data.TableName = tableName; _data.StructDB = info.StructCon;
                _data.DataDBb = info.DataCon;
                _data.GetStructInfo(); Factory.LstSingle.Add(_data);
            }
            DataTable result = await _data.GetDataFull(info);

            return Ok(result);
        }
        [HttpPost]
        public async Task<IHttpActionResult> PostInSertData([FromBody] JObject data)
        {
            //Kiểm tra Connection, Nếu info nằm trong list Info thì tiếp tục
            string token = data["Token"].ToObject<string>();
            Info info = ConnectionInfo.GetConnection(token);
            if (info == null) return BadRequest();
            string tableName = data["TableName"].ToObject<string>();
            string sData = data["Data"].ToObject<string>();
            SingleData _data = DataFactory.Factory.findSingle(tableName, info.StructCon);
            if (_data == null)
            {
                _data = new SingleData();_data.TableName = tableName;_data.StructDB = info.StructCon;
                _data.DataDBb = info.DataCon;
                _data.GetStructInfo(); Factory.LstSingle.Add(_data);
            }
            bool result = await _data.Insert(sData, info);

            //
            return Ok();
        }
    }
}
