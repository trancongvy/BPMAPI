using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using BPMAPI.APIControl;
using BPMAPI.DataFactory;
using CDTDatabase;

using BPMAPI.CDTControl;
using System.Data;
using Newtonsoft.Json;
using System.Collections;
using System.Web.Http.Cors;

namespace BPMAPI.Controllers
{
    public class MenuController : ApiController
    {
        [EnableCors("*", "*", "*")]
        [HttpPost]
        public async Task<IHttpActionResult> GetMenu(Info i)
        {
            if (i == null) return BadRequest("Dữ liệu không hợp lệ.");
            if (ConnectionInfo.checkInfor(i.Token) == null)
            {
                return Ok(new APIException { ErrorContent="Lỗi hết hạn"});
            }
            sysMenu menu = new sysMenu();
            DataTable tb = await Task.Run(() => menu.GetMenu(i));
            if (tb == null) return BadRequest();
            else 
            return Ok(CDTLib.JsonConverter.ConvertDataTabletoJson(tb));
        }
    }
}
