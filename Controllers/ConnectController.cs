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
namespace BPMAPI.Controllers
{
    public class ConnectController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> PostConnect(Info i)
        {
            if(i.Token=="" || i.Token == null)
            {
                Database db = Database.NewCustomDatabase(i.StructCon);
                bool con = await Task.Run(() => db.OpenConnection());
                if (con)//Kết nối thành công
                {
                    //Gen1token
                    i.Token = CDTLib.Security.EnCode64(DateTime.Now.ToLongDateString() + i.ComputerName);
                    i.ExDatetime = DateTime.Now;
                    ConnectionInfo.lInfo.Add(i);                    
                    return Ok(i);
                }
                else
                {
                    return BadRequest();
                }    
            }
            else
            {
                Info info = await Task.Run(() => ConnectionInfo.GetConnection(i.Token.ToString()));
                info.JustConnected();
                if (info != null) return Ok(info);
                else return BadRequest();
            }    
            
            
        }
        
    }

}
