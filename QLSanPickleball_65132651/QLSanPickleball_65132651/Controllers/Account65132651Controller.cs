using QLSanPickleball_65132651.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập số điện thoại và mật khẩu!";
                return View();
            }

            // 1. Kiểm tra bảng KHACHHANG bằng Số điện thoại
            var kh = db.KHACHHANG.FirstOrDefault(k => k.SODIENTHOAIKH == username && k.MATKHAUKH == password);
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

                // Session riêng cho đặt sân
                Session["MaKH"] = kh.MAKH;
                Session["TenKH"] = kh.HOTENKH;
                Session["SDTKH"] = kh.SODIENTHOAIKH;

                return RedirectToAction("Index", "Home");
            }

            // 2. Kiểm tra bảng NHANVIEN bằng Số điện thoại (Giả sử bảng NHANVIEN có cột SODIENTHOAINV)
            // Nếu bảng Nhân viên của bạn dùng TENDANGNHAP, bạn có thể cho phép họ nhập 1 trong 2
            var nv = db.NHANVIEN.FirstOrDefault(n => (n.SODIENTHOAINV == username || n.TENDANGNHAP == username) && n.MATKHAUNV == password);
            if (nv != null)
            {
                if (nv.TRANGTHAI != "Đang hoạt động")
                {
                    ViewBag.Error = "Tài khoản nhân viên đang bị khóa hoặc không hoạt động!";
                    return View();
                }

                Session["MaUser"] = nv.MANV;
                Session["TenUser"] = nv.HOTENNV;
                Session["Role"] = nv.VAITRO;

                // Nhân viên không phải khách hàng, tránh bị nhận nhầm là khách đặt sân
                Session.Remove("MaKH");
                Session.Remove("TenKH");
                Session.Remove("SDTKH");

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Thông tin đăng nhập không chính xác!";
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
            // 1. Kiểm tra khớp mật khẩu (vì XacNhanMatKhau không nằm trong Model)
            if (model.MATKHAUKH != XacNhanMatKhau)
            {
                ModelState.AddModelError("XacNhanMatKhau", "Mật khẩu xác nhận không khớp!");
            }
            if (string.IsNullOrWhiteSpace(model.SODIENTHOAIKH) ||
                !Regex.IsMatch(model.SODIENTHOAIKH, @"^[0-9]{10}$"))
            {
                ModelState.AddModelError("SODIENTHOAIKH", "Số điện thoại phải gồm đúng 10 chữ số!");
            }

            // ModelState.IsValid tự động kiểm tra các ràng buộc từ Metadata
            if (ModelState.IsValid)
            {
                // 2. Kiểm tra trùng Số điện thoại hoặc Email
                var checkUser = db.KHACHHANG.FirstOrDefault(k => k.SODIENTHOAIKH == model.SODIENTHOAIKH || k.EMAILKH == model.EMAILKH);
                if (checkUser != null)
                {
                    ViewBag.Error = "Số điện thoại hoặc Email đã tồn tại trong hệ thống!";
                    return View(model);
                }

                // 3. Khởi tạo mã khóa chính tự động
                model.MAKH = "KH" + DateTime.Now.ToString("HHmmssff");

                // 4. Thiết lập các giá trị mặc định theo yêu cầu nghiệp vụ
                model.SOLANBUNG = 0;              // Mặc định ban đầu là 0 lần bùng lịch
                model.TRANGTHAITK = "Hoạt động";   // Mặc định tài khoản mở ngay khi đăng ký

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
                            errorMsg += $"- Trường <b>{validationError.PropertyName}</b>: {validationError.ErrorMessage}<br/>";
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
            Session.Clear(); // Xóa toàn bộ Session
            return RedirectToAction("Index", "Home");
        }
    
    }
}