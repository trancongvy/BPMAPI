using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BPMAPI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Connection x = new Connection();
          //  ViewBag.Title = Connection.GetConnection("CBASGD200");

            return View();
        }
    }
}
