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
        private QLSanEntities db = new QLSanEntities();
        public ActionResult Index()
        {
            // Lấy danh sách sân
            var dsSan = db.SAN.ToList();

            // Mặc định gọi Layout Khách Hàng
            return View(dsSan);
        }
    }
}