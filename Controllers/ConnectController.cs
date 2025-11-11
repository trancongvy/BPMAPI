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
    [EnableCors("*", "*", "*")]
    public class ConnectController : ApiController
    {
        
        [HttpPost]

        public async Task<IHttpActionResult>Login(loginInfo loginInfo)
        {

            Info info = ConnectionInfo.lInfo.Find(m => m.UserName == loginInfo.UserName);
            if (info != null)
            {
                ConnectionInfo.lInfo.Remove(info);
            }

                SysUser user = new SysUser();
                DataRow drUser = await Task.Run(() => user.CheckLogin(loginInfo.UserName, loginInfo.Password));
                if (drUser == null)
                {
                    return Ok(new APIException  { ErrorContent = "User hoặc mật khẩu không đúng!" } );
                }

                else
                {
                    //Trả về 1 token 
                    string  token = CDTLib.Security.EnCode64(DateTime.Now.ToLongDateString() + loginInfo.UserName);

                Info i = new Info
                {
                    Token = token,
                    ExDatetime = DateTime.Now.AddMinutes(30),
                    UserID = drUser["sysUserID"].ToString(),
                    UserGroupID = drUser["sysUserGroupID"].ToString(),
                    UserName = drUser["UserName"].ToString(),
                    FullName = drUser["FullName"].ToString(),
                    GroupName = drUser["GroupName"].ToString(),
                    isAdmin = bool.Parse(drUser["CoreAdmin"].ToString()),
                    DataName = ""

                };
                    ConnectionInfo.lInfo.Add(i);
                        return Ok(i);
                }
            
            
        }
        [HttpPost]
        //Trả về danh sách các DBName
        public async Task<IHttpActionResult> GetPackageForUser(Info i)
        {
            if (ConnectionInfo.checkInfor(i.Token) ==null)
            {
                return Ok(new APIException() { ErrorContent = "Lỗi đăng nhập hết hạn" });
            }
            SysPackage package = new SysPackage();
            DataTable tb = await Task.Run(() => package.GetPackageForUser(i));

            return Ok(tb);
        }
        //Sau khi clien lựa chọn thì Post lại Info để server biết client đã lựa chọn DB nào
        [HttpPost]
        public async Task<IHttpActionResult> PostSelectDBName(Info i)
        {
            if (i == null)
            {
                return Ok(new APIException() { ErrorContent = "Thông tin post không hợp lệ" });
            }
            Info findInfo = await Task.Run(() => ConnectionInfo.checkInfor(i.Token));
            if (findInfo == null)
            {
                return Ok(new APIException() { ErrorContent = "Lỗi đăng nhập hết hạn" });
            }
            else
            {
                findInfo.DataName = i.DataName;
                findInfo.sysDBID = i.drPackage.sysDBID;
                findInfo.sysDBID = int.Parse(i.drPackage.sysDBID.ToString());
                findInfo.drPackage = i.drPackage;
                SysPackage package = new SysPackage();
                DataRow drPackage = await Task.Run(() => package.GetDrPackage(i.sysDBID.ToString()));
                //findInfo.drPackage = drPackage;
                if (drPackage != null)
                {
                    findInfo = await Task.Run(() => package.InitSysvar(findInfo));
                    return Ok(findInfo);
                } else
                {
                    return Ok(new APIException() { ErrorContent = "Không tìm thấy gói dữ liệu" });
                }    

            }
            
        }

        [HttpPost]
        public async Task<IHttpActionResult> PostGetRow(Hashtable i)
        {
            SysPackage package = new SysPackage();
            DataRow drPackage = await Task.Run(() => package.GetDrPackage("2"));
            return Ok(drPackage);
        }
        [HttpPost]
        public async Task<IHttpActionResult> GetConfig(Info i)
        {
            if (ConnectionInfo.checkInfor(i.Token) != null)
            {
                CDTControl.SysConfig cf = new CDTControl.SysConfig();
                DataTable tb = await Task.Run(() => cf.GetUserConfig(i));
                if (tb == null) return BadRequest();
                else
                    return Ok(CDTLib.JsonConverter.ConvertDataTabletoJson(tb));
            }
            else
                return BadRequest();

        }
        [HttpGet]
        public IHttpActionResult GetNgayHT()
        {
            return Ok(DateTime.Now); // hoặc return Ok(new { NgayHT = DateTime.Now });
        }
    }

}
