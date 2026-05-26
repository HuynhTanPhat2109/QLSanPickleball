using QLSanPickleball_65132651.Models;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;

namespace QLSanPickleball_65132651.Controllers
{
    public class KHACHHANG65130311Controller : Controller
    {
        private QLSanEntities db = new QLSanEntities();

        private ActionResult KiemTraQuyenQuanLy()
        {
            if (Session["MANV"] == null)
                return RedirectToAction("Login", "Account65132651");

            string vaiTro = Session["VAITRO"] != null ? Session["VAITRO"].ToString() : "";

            if (vaiTro != "Admin" && vaiTro != "Quản lý" && vaiTro != "Nhân viên")
                return RedirectToAction("HomeNv", "Admin65134364");

            return null;
        }

        public ActionResult Index(string search, string trangThai)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            var ds = db.KHACHHANG.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                ds = ds.Where(k =>
                    k.MAKH.Contains(search) ||
                    k.HOTENKH.Contains(search) ||
                    k.SODIENTHOAIKH.Contains(search) ||
                    k.EMAILKH.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                ds = ds.Where(k => k.TRANGTHAITK == trangThai);
            }

            ViewBag.Search = search;
            ViewBag.TrangThai = trangThai;
            ViewBag.TongKH = db.KHACHHANG.Count();
            ViewBag.DangHoatDong = db.KHACHHANG.Count(k => k.TRANGTHAITK == "Hoạt động");
            ViewBag.BiKhoa = db.KHACHHANG.Count(k => k.TRANGTHAITK == "Đã khóa");
            ViewBag.NhieuLanBung = db.KHACHHANG.Count(k => k.SOLANBUNG > 0);

            return View(ds.OrderBy(k => k.MAKH).ToList());
        }

        public ActionResult Details(string id)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var khach = db.KHACHHANG.Find(id);

            if (khach == null)
                return HttpNotFound();

            ViewBag.LichSuDatSan = db.PHIEUDATSAN
                .Include(p => p.SAN)
                .Where(p => p.MAKH == id)
                .OrderByDescending(p => p.NGAYDAT)
                .ThenByDescending(p => p.GIOBATDAU)
                .ToList();

            return View(khach);
        }

        public ActionResult Create()
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            KHACHHANG model = new KHACHHANG();
            model.MAKH = TaoMaKhachHangMoi();
            model.SOLANBUNG = 0;
            model.TRANGTHAITK = "Hoạt động";

            LoadDropdown(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MAKH,HOTENKH,GIOITINH,DIACHI,SODIENTHOAIKH,EMAILKH,MATKHAUKH,SOLANBUNG,TRANGTHAITK")] KHACHHANG khachHang, string NGAYSINH)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(khachHang.MAKH))
                khachHang.MAKH = TaoMaKhachHangMoi();

            DateTime ngaySinh;
            if (!string.IsNullOrWhiteSpace(NGAYSINH))
            {
                if (DateTime.TryParseExact(NGAYSINH, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ngaySinh))
                {
                    khachHang.NGAYSINH = ngaySinh;
                }
                else
                {
                    ModelState.AddModelError("NGAYSINH", "Ngày sinh phải đúng định dạng dd/MM/yyyy.");
                }
            }

            if (khachHang.SOLANBUNG < 0)
                khachHang.SOLANBUNG = 0;

            if (string.IsNullOrWhiteSpace(khachHang.TRANGTHAITK))
                khachHang.TRANGTHAITK = "Hoạt động";

            KiemTraDuLieuKhachHang(khachHang, true);

            if (ModelState.IsValid)
            {
                db.KHACHHANG.Add(khachHang);
                db.SaveChanges();

                TempData["Success"] = "Thêm khách hàng thành công.";
                return RedirectToAction("Index");
            }

            LoadDropdown(khachHang);
            return View(khachHang);
        }

        public ActionResult Edit(string id)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var khachHang = db.KHACHHANG.Find(id);

            if (khachHang == null)
                return HttpNotFound();

            LoadDropdown(khachHang);
            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MAKH,HOTENKH,GIOITINH,DIACHI,SODIENTHOAIKH,EMAILKH,TRANGTHAITK")] KHACHHANG khachHang, string NGAYSINH)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            DateTime ngaySinh;
            if (!string.IsNullOrWhiteSpace(NGAYSINH))
            {
                if (DateTime.TryParseExact(NGAYSINH, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ngaySinh))
                {
                    khachHang.NGAYSINH = ngaySinh;
                }
                else
                {
                    ModelState.AddModelError("NGAYSINH", "Ngày sinh phải đúng định dạng dd/MM/yyyy.");
                }
            }

            KiemTraDuLieuKhachHang(khachHang, false);

            if (ModelState.IsValid)
            {
                var khCu = db.KHACHHANG.Find(khachHang.MAKH);

                if (khCu == null)
                {
                    return HttpNotFound();
                }

                khCu.HOTENKH = khachHang.HOTENKH;
                khCu.NGAYSINH = khachHang.NGAYSINH;
                khCu.GIOITINH = khachHang.GIOITINH;
                khCu.DIACHI = khachHang.DIACHI;
                khCu.SODIENTHOAIKH = khachHang.SODIENTHOAIKH;
                khCu.EMAILKH = khachHang.EMAILKH;
                khCu.TRANGTHAITK = khachHang.TRANGTHAITK;

                db.SaveChanges();

                TempData["Success"] = "Cập nhật khách hàng thành công.";
                return RedirectToAction("Index");
            }

            LoadDropdown(khachHang);
            return View(khachHang);
        }

        public ActionResult Delete(string id)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            KHACHHANG khachHang = db.KHACHHANG.Find(id);

            if (khachHang == null)
                return HttpNotFound();

            return View(khachHang);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            KHACHHANG khachHang = db.KHACHHANG.Find(id);

            if (khachHang == null)
                return HttpNotFound();

            bool coPhieuDat = db.PHIEUDATSAN.Any(p => p.MAKH == id);

            if (coPhieuDat)
            {
                khachHang.TRANGTHAITK = "Đã khóa";
                db.SaveChanges();

                TempData["Error"] = "Khách hàng đã có lịch sử đặt sân nên không thể xóa. Hệ thống đã chuyển tài khoản sang Đã khóa.";
                return RedirectToAction("Index");
            }

            db.KHACHHANG.Remove(khachHang);
            db.SaveChanges();

            TempData["Success"] = "Xóa khách hàng thành công.";
            return RedirectToAction("Index");
        }

        public ActionResult Khoa(string id)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            int soDong = db.Database.ExecuteSqlCommand(
                "UPDATE KHACHHANG SET TRANGTHAITK = @p0 WHERE MAKH = @p1",
                "Đã khóa",
                id
            );

            if (soDong == 0)
                return HttpNotFound();

            TempData["Success"] = "Đã khóa tài khoản khách hàng.";
            return RedirectToAction("Index");
        }

        public ActionResult MoKhoa(string id)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            int soDong = db.Database.ExecuteSqlCommand(
                "UPDATE KHACHHANG SET TRANGTHAITK = @p0 WHERE MAKH = @p1",
                "Hoạt động",
                id
            );

            if (soDong == 0)
                return HttpNotFound();

            TempData["Success"] = "Đã mở khóa tài khoản khách hàng.";
            return RedirectToAction("Index");
        }

        public ActionResult ResetSoLanBung(string id)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            var kh = db.KHACHHANG.Find(id);
            if (kh == null) return HttpNotFound();

            kh.SOLANBUNG = 0;
            kh.TRANGTHAITK = "Hoạt động";
            db.SaveChanges();

            TempData["Success"] = "Đã reset số lần bùng lịch.";
            return RedirectToAction("Details", new { id = id });
        }

        private void LoadDropdown(KHACHHANG khachHang = null)
        {
            ViewBag.GioiTinhList = new SelectList(new[]
            {
                "Nam",
                "Nữ",
                "Khác"
            }, khachHang != null ? khachHang.GIOITINH : null);

            ViewBag.TrangThaiList = new SelectList(new[]
            {
                "Hoạt động",
                "Đã khóa"
            }, khachHang != null ? khachHang.TRANGTHAITK : null);
        }

        private void KiemTraDuLieuKhachHang(KHACHHANG kh, bool laThemMoi)
        {
            if (string.IsNullOrWhiteSpace(kh.MAKH))
                ModelState.AddModelError("MAKH", "Mã khách hàng không được để trống.");

            if (string.IsNullOrWhiteSpace(kh.HOTENKH))
                ModelState.AddModelError("HOTENKH", "Họ tên khách hàng không được để trống.");

            if (string.IsNullOrWhiteSpace(kh.SODIENTHOAIKH))
                ModelState.AddModelError("SODIENTHOAIKH", "Số điện thoại không được để trống.");

            if (laThemMoi && string.IsNullOrWhiteSpace(kh.MATKHAUKH))
                ModelState.AddModelError("MATKHAUKH", "Mật khẩu không được để trống.");

            if (kh.SOLANBUNG < 0)
                ModelState.AddModelError("SOLANBUNG", "Số lần bùng không được âm.");

            if (laThemMoi && db.KHACHHANG.Any(k => k.MAKH == kh.MAKH))
                ModelState.AddModelError("MAKH", "Mã khách hàng đã tồn tại.");

            bool trungSDT = db.KHACHHANG.Any(k =>
                k.SODIENTHOAIKH == kh.SODIENTHOAIKH &&
                (laThemMoi || k.MAKH != kh.MAKH));

            if (trungSDT)
                ModelState.AddModelError("SODIENTHOAIKH", "Số điện thoại này đã tồn tại.");

            if (!string.IsNullOrWhiteSpace(kh.EMAILKH))
            {
                bool trungEmail = db.KHACHHANG.Any(k =>
                    k.EMAILKH == kh.EMAILKH &&
                    (laThemMoi || k.MAKH != kh.MAKH));

                if (trungEmail)
                    ModelState.AddModelError("EMAILKH", "Email này đã tồn tại.");
            }
        }

        private string TaoMaKhachHangMoi()
        {
            var maCuoi = db.KHACHHANG
                .Where(k => k.MAKH.StartsWith("KH"))
                .OrderByDescending(k => k.MAKH)
                .Select(k => k.MAKH)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(maCuoi))
                return "KH01";

            int so = 0;
            string phanSo = maCuoi.Replace("KH", "");
            int.TryParse(phanSo, out so);
            so++;

            return "KH" + so.ToString("00");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}