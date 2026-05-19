using QLSanPickleball_65132651.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace QLSanPickleball_65132651.Controllers
{
    public class HomeController : Controller
    {
        private QLSanEntities db = new QLSanEntities();
        public ActionResult Index(string searchString, string maLoai)
        {
            // 1. Lấy danh sách loại sân để đổ vào Dropdown Filter (giữ nguyên như cũ)
            ViewBag.MaLoaiSan = db.LOAISAN.ToList();

            // 2. Bắt đầu với truy vấn cơ bản (Queryable để nối thêm điều kiện lọc)
            var courts = db.SAN.Include(s => s.LOAISAN).AsQueryable();

            // 3. Lọc theo tên sân nếu người dùng có nhập
            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                courts = courts.Where(s => s.TENSAN.ToLower().Contains(searchString));
            }

            // 4. Lọc theo loại sân nếu người dùng có chọn
            if (!String.IsNullOrEmpty(maLoai))
            {
                courts = courts.Where(s => s.MALOAISAN == maLoai);
            }

            // Lưu lại giá trị đã lọc để hiển thị ngược lại trên ô nhập (UX tốt)
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentFilter = maLoai;

            return View(courts.ToList());
        }
    }
}