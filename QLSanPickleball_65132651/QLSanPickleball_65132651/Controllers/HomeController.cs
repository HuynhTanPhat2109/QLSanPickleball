using QLSanPickleball_65132651.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace QLSanPickleball_65132651.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string searchString, string maLoai)
        {
            return View();
        }
    }
}