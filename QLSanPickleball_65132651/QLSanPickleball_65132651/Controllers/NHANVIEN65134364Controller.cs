using QLSanPickleball_65132651.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace QLSanPickleball_65132651.Controllers
{
    public class NHANVIEN65134364Controller : Controller
    {
        private QLSanEntities db = new QLSanEntities();

        // =========================================================
        // PHÂN QUYỀN
        // =========================================================
        private string LayVaiTro()
        {
            if (Session["VAITRO"] == null)
            {
                return "";
            }

            return Session["VAITRO"].ToString().Trim();
        }

        private bool LaAdmin()
        {
            return LayVaiTro() == "Admin";
        }

        private bool LaQuanLy()
        {
            string vaiTro = LayVaiTro();
            return vaiTro == "Quản lý" || vaiTro == "Quan ly";
        }

        private ActionResult KiemTraQuyenXemVaSua()
        {
            if (Session["MANV"] == null)
            {
                return RedirectToAction("Login", "Account65132651");
            }

            if (!LaAdmin() && !LaQuanLy())
            {
                TempData["Error"] = "Bạn không có quyền truy cập chức năng quản lý nhân viên.";
                return RedirectToAction("HomeNv", "Admin65134364");
            }

            return null;
        }

        private ActionResult KiemTraQuyenAdmin()
        {
            if (Session["MANV"] == null)
            {
                return RedirectToAction("Login", "Account65132651");
            }

            if (!LaAdmin())
            {
                TempData["Error"] = "Chức năng này chỉ dành cho Admin.";
                return RedirectToAction("Index");
            }

            return null;
        }

        private void GanViewBagPhanQuyen()
        {
            ViewBag.LaAdmin = LaAdmin();
            ViewBag.LaQuanLy = LaQuanLy();
            ViewBag.DuocThem = LaAdmin();
            ViewBag.DuocSua = LaAdmin() || LaQuanLy();
            ViewBag.DuocKhoa = LaAdmin();
            ViewBag.DuocXoa = LaAdmin();
        }

        // =========================================================
        // GET: NHANVIEN65134364
        // Admin + Quản lý được xem danh sách
        // =========================================================
        public ActionResult Index(string search, string vaiTro, string trangThai)
        {
            var check = KiemTraQuyenXemVaSua();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            var nhanViens = db.NHANVIEN.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                nhanViens = nhanViens.Where(n =>
                    n.MANV.Contains(search) ||
                    n.HOTENNV.Contains(search) ||
                    n.SODIENTHOAINV.Contains(search) ||
                    n.EMAILNV.Contains(search) ||
                    n.TENDANGNHAP.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(vaiTro))
            {
                nhanViens = nhanViens.Where(n => n.VAITRO == vaiTro);
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                nhanViens = nhanViens.Where(n => n.TRANGTHAI == trangThai);
            }

            ViewBag.Search = search;
            ViewBag.VaiTro = vaiTro;
            ViewBag.TrangThai = trangThai;

            return View(nhanViens.OrderBy(n => n.MANV).ToList());
        }

        // =========================================================
        // GET: NHANVIEN65134364/Details/NV01
        // Admin + Quản lý được xem chi tiết
        // =========================================================
        public ActionResult Details(string id)
        {
            var check = KiemTraQuyenXemVaSua();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            NHANVIEN nhanVien = db.NHANVIEN.Find(id);

            if (nhanVien == null)
            {
                return HttpNotFound();
            }

            return View(nhanVien);
        }

        // =========================================================
        // GET: NHANVIEN65134364/Create
        // Chỉ Admin được thêm
        // =========================================================
        public ActionResult Create()
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            GanViewBagPhanQuyen();
            GanViewBagDanhSachLuaChon(null);

            return View();
        }

        // =========================================================
        // POST: NHANVIEN65134364/Create
        // Chỉ Admin được thêm
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MANV,HOTENNV,SODIENTHOAINV,EMAILNV,TENDANGNHAP,MATKHAUNV,VAITRO,TRANGTHAI")] NHANVIEN nhanVien)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            if (string.IsNullOrWhiteSpace(nhanVien.MANV))
            {
                nhanVien.MANV = TaoMaNhanVienMoi();
            }

            if (string.IsNullOrWhiteSpace(nhanVien.TRANGTHAI))
            {
                nhanVien.TRANGTHAI = "Đang hoạt động";
            }

            KiemTraDuLieuNhanVien(nhanVien, true);

            if (ModelState.IsValid)
            {
                db.NHANVIEN.Add(nhanVien);
                db.SaveChanges();

                TempData["Success"] = "Thêm tài khoản nhân viên thành công!";
                return RedirectToAction("Index");
            }

            GanViewBagDanhSachLuaChon(nhanVien);
            return View(nhanVien);
        }

        // =========================================================
        // GET: NHANVIEN65134364/Edit/NV01
        // Admin + Quản lý được sửa
        // =========================================================
        public ActionResult Edit(string id)
        {
            var check = KiemTraQuyenXemVaSua();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            NHANVIEN nhanVien = db.NHANVIEN.Find(id);

            if (nhanVien == null)
            {
                return HttpNotFound();
            }

            GanViewBagDanhSachLuaChon(nhanVien);

            return View(nhanVien);
        }

        // =========================================================
        // POST: NHANVIEN65134364/Edit/NV01
        // Admin + Quản lý được sửa
        // Quản lý không được tự nâng quyền / đổi trạng thái tài khoản
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MANV,HOTENNV,SODIENTHOAINV,EMAILNV,TENDANGNHAP,MATKHAUNV,VAITRO,TRANGTHAI")] NHANVIEN nhanVien)
        {
            var check = KiemTraQuyenXemVaSua();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            NHANVIEN nhanVienCu = db.NHANVIEN.AsNoTracking().FirstOrDefault(n => n.MANV == nhanVien.MANV);

            if (nhanVienCu == null)
            {
                return HttpNotFound();
            }

            // Quản lý chỉ được sửa thông tin cơ bản.
            // Không được đổi vai trò và trạng thái để tránh vượt quyền.
            if (LaQuanLy() && !LaAdmin())
            {
                nhanVien.VAITRO = nhanVienCu.VAITRO;
                nhanVien.TRANGTHAI = nhanVienCu.TRANGTHAI;
            }

            if (string.IsNullOrWhiteSpace(nhanVien.MATKHAUNV))
            {
                nhanVien.MATKHAUNV = nhanVienCu.MATKHAUNV;
            }

            KiemTraDuLieuNhanVien(nhanVien, false);

            if (ModelState.IsValid)
            {
                db.Entry(nhanVien).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Cập nhật thông tin nhân viên thành công!";
                return RedirectToAction("Index");
            }

            GanViewBagDanhSachLuaChon(nhanVien);
            return View(nhanVien);
        }

        // =========================================================
        // GET: NHANVIEN65134364/Delete/NV01
        // Chỉ Admin được xóa
        // =========================================================
        public ActionResult Delete(string id)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            NHANVIEN nhanVien = db.NHANVIEN.Find(id);

            if (nhanVien == null)
            {
                return HttpNotFound();
            }

            return View(nhanVien);
        }

        // =========================================================
        // POST: NHANVIEN65134364/Delete/NV01
        // Chỉ Admin được xóa
        // =========================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            NHANVIEN nhanVien = db.NHANVIEN.Find(id);

            if (nhanVien == null)
            {
                return HttpNotFound();
            }

            string maDangDangNhap = Session["MANV"] != null ? Session["MANV"].ToString() : "";

            if (nhanVien.MANV == maDangDangNhap)
            {
                TempData["Error"] = "Bạn không thể xóa chính tài khoản đang đăng nhập.";
                return RedirectToAction("Index");
            }

            bool daCoPhieuDat = db.PHIEUDATSAN.Any(p => p.MANV == id);
            bool daCoHoaDon = db.HOADON.Any(h => h.MANV == id);
            bool daCoHoiVien = db.HOIVIEN.Any(h => h.MANV == id);

            if (daCoPhieuDat || daCoHoaDon || daCoHoiVien)
            {
                nhanVien.TRANGTHAI = "Đã nghỉ việc";
                db.SaveChanges();

                TempData["Error"] = "Nhân viên đã phát sinh dữ liệu nên không thể xóa. Hệ thống đã chuyển sang trạng thái Đã nghỉ việc.";
                return RedirectToAction("Index");
            }

            db.NHANVIEN.Remove(nhanVien);
            db.SaveChanges();

            TempData["Success"] = "Xóa nhân viên thành công!";
            return RedirectToAction("Index");
        }

        // =========================================================
        // GET: NHANVIEN65134364/KhoaTaiKhoan/NV01
        // Chỉ Admin được khóa
        // =========================================================
        public ActionResult KhoaTaiKhoan(string id)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            NHANVIEN nhanVien = db.NHANVIEN.Find(id);

            if (nhanVien == null)
            {
                return HttpNotFound();
            }

            string maDangDangNhap = Session["MANV"] != null ? Session["MANV"].ToString() : "";

            if (nhanVien.MANV == maDangDangNhap)
            {
                TempData["Error"] = "Bạn không thể khóa chính tài khoản đang đăng nhập.";
                return RedirectToAction("Index");
            }

            nhanVien.TRANGTHAI = "Tạm khóa";
            db.SaveChanges();

            TempData["Success"] = "Đã khóa tài khoản nhân viên!";
            return RedirectToAction("Index");
        }

        // =========================================================
        // GET: NHANVIEN65134364/MoKhoaTaiKhoan/NV01
        // Chỉ Admin được mở khóa
        // =========================================================
        public ActionResult MoKhoaTaiKhoan(string id)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            NHANVIEN nhanVien = db.NHANVIEN.Find(id);

            if (nhanVien == null)
            {
                return HttpNotFound();
            }

            nhanVien.TRANGTHAI = "Đang hoạt động";
            db.SaveChanges();

            TempData["Success"] = "Đã mở khóa tài khoản nhân viên!";
            return RedirectToAction("Index");
        }

        // =========================================================
        // HÀM PHỤ
        // =========================================================
        private void GanViewBagDanhSachLuaChon(NHANVIEN nhanVien)
        {
            ViewBag.VaiTroList = new SelectList(new[]
            {
                "Admin",
                "Quản lý",
                "Nhân viên"
            }, nhanVien != null ? nhanVien.VAITRO : "Nhân viên");

            ViewBag.TrangThaiList = new SelectList(new[]
            {
                "Đang hoạt động",
                "Tạm khóa",
                "Đã nghỉ việc"
            }, nhanVien != null ? nhanVien.TRANGTHAI : "Đang hoạt động");
        }

        private string TaoMaNhanVienMoi()
        {
            var maCuoi = db.NHANVIEN
                .Where(n => n.MANV.StartsWith("NV"))
                .OrderByDescending(n => n.MANV)
                .Select(n => n.MANV)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(maCuoi))
            {
                return "NV01";
            }

            string soCuoi = maCuoi.Replace("NV", "");
            int so = 0;
            int.TryParse(soCuoi, out so);
            so++;

            return "NV" + so.ToString("00");
        }

        private void KiemTraDuLieuNhanVien(NHANVIEN nhanVien, bool laThemMoi)
        {
            if (string.IsNullOrWhiteSpace(nhanVien.MANV))
            {
                ModelState.AddModelError("MANV", "Mã nhân viên không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(nhanVien.HOTENNV))
            {
                ModelState.AddModelError("HOTENNV", "Họ tên nhân viên không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(nhanVien.SODIENTHOAINV))
            {
                ModelState.AddModelError("SODIENTHOAINV", "Số điện thoại không được để trống.");
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(nhanVien.SODIENTHOAINV, @"^[0-9]{10}$"))
            {
                ModelState.AddModelError("SODIENTHOAINV", "Số điện thoại phải gồm đúng 10 chữ số.");
            }

            if (string.IsNullOrWhiteSpace(nhanVien.EMAILNV))
            {
                ModelState.AddModelError("EMAILNV", "Email không được để trống.");
            }
            else if (!nhanVien.EMAILNV.Contains("@"))
            {
                ModelState.AddModelError("EMAILNV", "Email không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(nhanVien.TENDANGNHAP))
            {
                ModelState.AddModelError("TENDANGNHAP", "Tên đăng nhập không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(nhanVien.MATKHAUNV))
            {
                ModelState.AddModelError("MATKHAUNV", "Mật khẩu không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(nhanVien.VAITRO))
            {
                ModelState.AddModelError("VAITRO", "Vui lòng chọn vai trò.");
            }

            if (string.IsNullOrWhiteSpace(nhanVien.TRANGTHAI))
            {
                ModelState.AddModelError("TRANGTHAI", "Vui lòng chọn trạng thái.");
            }

            bool trungTenDangNhap = db.NHANVIEN.Any(n =>
                n.TENDANGNHAP == nhanVien.TENDANGNHAP &&
                (laThemMoi || n.MANV != nhanVien.MANV));

            if (trungTenDangNhap)
            {
                ModelState.AddModelError("TENDANGNHAP", "Tên đăng nhập này đã tồn tại.");
            }

            bool trungEmail = db.NHANVIEN.Any(n =>
                n.EMAILNV == nhanVien.EMAILNV &&
                (laThemMoi || n.MANV != nhanVien.MANV));

            if (trungEmail)
            {
                ModelState.AddModelError("EMAILNV", "Email này đã tồn tại.");
            }

            bool trungSdt = db.NHANVIEN.Any(n =>
                n.SODIENTHOAINV == nhanVien.SODIENTHOAINV &&
                (laThemMoi || n.MANV != nhanVien.MANV));

            if (trungSdt)
            {
                ModelState.AddModelError("SODIENTHOAINV", "Số điện thoại này đã tồn tại.");
            }

            if (laThemMoi)
            {
                bool trungMa = db.NHANVIEN.Any(n => n.MANV == nhanVien.MANV);

                if (trungMa)
                {
                    ModelState.AddModelError("MANV", "Mã nhân viên này đã tồn tại.");
                }
            }
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