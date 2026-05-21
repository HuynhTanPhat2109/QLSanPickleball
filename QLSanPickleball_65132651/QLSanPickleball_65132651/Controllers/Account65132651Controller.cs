using QLSanPickleball_65132651.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace QLSanPickleball_65132651.Controllers
{
    public class Account65132651Controller : Controller
    {
        private QLSanEntities db = new QLSanEntities();

        // ==========================================
        // 1. CHỨC NĂNG ĐĂNG NHẬP
        // ==========================================
        [HttpGet]
        public ActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì chuyển đúng trang theo vai trò
            if (Session["Role"] != null)
            {
                string role = Session["Role"].ToString();

                if (role == "KhachHang")
                {
                    return RedirectToAction("Index", "Home");
                }

                if (role == "Admin" || role == "Quản lý" || role == "Nhân viên")
                {
                    return RedirectToAction("HomeNv", "Admin65134364");
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập số điện thoại/tên đăng nhập và mật khẩu!";
                return View();
            }

            // 1. Kiểm tra bảng KHACHHANG bằng số điện thoại
            var kh = db.KHACHHANG.FirstOrDefault(k =>
                k.SODIENTHOAIKH == username &&
                k.MATKHAUKH == password);

            if (kh != null)
            {
                if (kh.TRANGTHAITK != "Hoạt động")
                {
                    ViewBag.Error = "Tài khoản khách hàng đang bị khóa hoặc không hoạt động!";
                    return View();
                }

                Session["MaUser"] = kh.MAKH;
                Session["TenUser"] = kh.HOTENKH;
                Session["Role"] = "KhachHang";

                // Session riêng cho khách hàng đặt sân
                Session["MaKH"] = kh.MAKH;
                Session["TenKH"] = kh.HOTENKH;
                Session["SDTKH"] = kh.SODIENTHOAIKH;

                // Xóa session nhân viên nếu có
                Session.Remove("MANV");
                Session.Remove("HOTENNV");
                Session.Remove("VAITRO");

                return RedirectToAction("Index", "Home");
            }

            // 2. Kiểm tra bảng NHANVIEN bằng số điện thoại hoặc tên đăng nhập
            var nv = db.NHANVIEN.FirstOrDefault(n =>
                (n.SODIENTHOAINV == username || n.TENDANGNHAP == username) &&
                n.MATKHAUNV == password);

            if (nv != null)
            {
                if (nv.TRANGTHAI != "Đang hoạt động")
                {
                    ViewBag.Error = "Tài khoản nhân viên đang bị khóa hoặc không hoạt động!";
                    return View();
                }

                // Session dùng chung
                Session["MaUser"] = nv.MANV;
                Session["TenUser"] = nv.HOTENNV;
                Session["Role"] = nv.VAITRO;

                // Session riêng cho trang Admin/HomeNv
                Session["MANV"] = nv.MANV;
                Session["HOTENNV"] = nv.HOTENNV;
                Session["VAITRO"] = nv.VAITRO;

                // Nhân viên không phải khách hàng
                Session.Remove("MaKH");
                Session.Remove("TenKH");
                Session.Remove("SDTKH");

                // Admin / Quản lý / Nhân viên đều vào trang HomeNv
                if (nv.VAITRO == "Admin" || nv.VAITRO == "Quản lý" || nv.VAITRO == "Nhân viên")
                {
                    return RedirectToAction("HomeNv", "Admin65134364");
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Thông tin đăng nhập không chính xác!";
            return View();
        }

        // ==========================================
        // 2. CHỨC NĂNG ĐĂNG KÝ KHÁCH HÀNG
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
            if (model.MATKHAUKH != XacNhanMatKhau)
            {
                ModelState.AddModelError("XacNhanMatKhau", "Mật khẩu xác nhận không khớp!");
            }

            if (string.IsNullOrWhiteSpace(model.SODIENTHOAIKH) ||
                !Regex.IsMatch(model.SODIENTHOAIKH, @"^[0-9]{10}$"))
            {
                ModelState.AddModelError("SODIENTHOAIKH", "Số điện thoại phải gồm đúng 10 chữ số!");
            }

            if (ModelState.IsValid)
            {
                var checkUser = db.KHACHHANG.FirstOrDefault(k =>
                    k.SODIENTHOAIKH == model.SODIENTHOAIKH ||
                    k.EMAILKH == model.EMAILKH);

                if (checkUser != null)
                {
                    ViewBag.Error = "Số điện thoại hoặc Email đã tồn tại trong hệ thống!";
                    return View(model);
                }

                model.MAKH = "KH" + DateTime.Now.ToString("HHmmssff");
                model.SOLANBUNG = 0;
                model.TRANGTHAITK = "Hoạt động";

                try
                {
                    db.KHACHHANG.Add(model);
                    db.SaveChanges();

                    TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập tài khoản.";
                    return RedirectToAction("Login");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    string errorMsg = "Lỗi cấu trúc dữ liệu CSDL: <br/>";

                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            errorMsg += "- Trường <b>" + validationError.PropertyName + "</b>: " + validationError.ErrorMessage + "<br/>";
                        }
                    }

                    ViewBag.Error = errorMsg;
                    return View(model);
                }
            }

            return View(model);
        }

        // ==========================================
        // 3. CHỨC NĂNG ĐĂNG XUẤT
        // ==========================================
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Login", "Account65132651");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}