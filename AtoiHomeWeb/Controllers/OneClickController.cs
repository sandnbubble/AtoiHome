using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AtoiHomeWeb.Controllers
{
    public class OneClickController : Controller
    {
        // GET: OneClick
        public ActionResult Index()
        {
#if DEBUG
            ViewBag.URL = "https://localhost";
#else
            ViewBag.URL = "https://www.atoihome.site";
#endif
            return View();
        }
    }
}