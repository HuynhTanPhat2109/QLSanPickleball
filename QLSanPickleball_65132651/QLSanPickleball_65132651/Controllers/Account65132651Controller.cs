using QLSanPickleball_65132651.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLSanPickleball_65132651.Controllers
{
    public class Account65132651Controller : Controller
    {
        private QLSanEntities db = new QLSanEntities();

        // ==========================================
        // 1. CHỨC NĂNG ĐĂNG NHẬP (CHUNG CHO CẢ 2 BÊN)
        // ==========================================
        [HttpGet]
        public ActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì không cho vào trang Login nữa
            if (Session["Role"] != null)
            {
                if (Session["Role"].ToString() == "KhachHang")
                    return RedirectToAction("Index", "Home");
                else
                    return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            // ... (Phần kiểm tra rỗng vẫn giữ nguyên) ...

            // KỊCH BẢN 1: Khách hàng
            var kh = db.KHACHHANG.FirstOrDefault(k => k.SODIENTHOAIKH == username && k.MATKHAUKH == password);
            if (kh != null)
            {
                Session["MaUser"] = kh.MAKH;
                Session["TenUser"] = kh.HOTENKH;
                Session["Role"] = "KhachHang";

                return RedirectToAction("Index", "Home");
            }

            // KỊCH BẢN 2: Cán bộ/Nhân viên
            var nv = db.NHANVIEN.FirstOrDefault(n => n.TENDANGNHAP == username && n.MATKHAUNV == password);
            if (nv != null)
            {
                Session["MaUser"] = nv.MANV;
                Session["TenUser"] = nv.HOTENNV;
                Session["Role"] = nv.VAITRO;

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác!";
            return View();
        }

        // ==========================================
        // 2. CHỨC NĂNG ĐĂNG KÝ (CHỈ DÀNH CHO KHÁCH HÀNG)
        // ==========================================
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(KHACHHANG model, string XacNhanMatKhau)
        {
            if (ModelState.IsValid)
            {
                if (model.MATKHAUKH != XacNhanMatKhau)
                {
                    ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                    return View(model);
                }

                // Kiểm tra trùng SĐT hoặc Email
                var checkUser = db.KHACHHANG.FirstOrDefault(k => k.SODIENTHOAIKH == model.SODIENTHOAIKH || k.EMAILKH == model.EMAILKH);
                if (checkUser != null)
                {
                    ViewBag.Error = "Số điện thoại hoặc Email đã được đăng ký!";
                    return View(model);
                }

                // Khởi tạo các giá trị mặc định cho khách hàng mới
                model.MAKH = "KH" + DateTime.Now.Ticks.ToString().Substring(10);
                model.SOLANBUNG = 0;
                model.TRANGTHAITK = "Hoạt động";

                db.KHACHHANG.Add(model);
                db.SaveChanges();

                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // ==========================================
        // 3. CHỨC NĂNG ĐĂNG XUẤT
        // ==========================================
        public ActionResult Logout()
        {
            Session.Clear(); // Xóa toàn bộ Session
            return RedirectToAction("Login", "Account");
        }
    
    }
}