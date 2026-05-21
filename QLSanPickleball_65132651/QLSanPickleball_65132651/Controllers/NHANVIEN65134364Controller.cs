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

        // Kiểm tra đăng nhập + quyền
        private ActionResult KiemTraQuyen()
        {
            if (Session["MANV"] == null)
            {
                return RedirectToAction("Login", "Account65132651");
            }

            string vaiTro = Session["VAITRO"] != null ? Session["VAITRO"].ToString() : "";

            // Chỉ Admin hoặc Quản lý được quản lý tài khoản nhân viên
            if (vaiTro != "Admin" && vaiTro != "Quản lý")
            {
                return RedirectToAction("HomeNv", "Admin65134364");
            }

            return null;
        }

        // GET: NHANVIEN65134364
        public ActionResult Index(string search, string vaiTro, string trangThai)
        {
            var check = KiemTraQuyen();
            if (check != null) return check;

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

        // GET: NHANVIEN65134364/Details/NV01
        public ActionResult Details(string id)
        {
            var check = KiemTraQuyen();
            if (check != null) return check;

            if (id == null)
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

        // GET: NHANVIEN65134364/Create
        // GET: NHANVIEN65134364/Create
        public ActionResult Create()
        {
            var check = KiemTraQuyen();
            if (check != null) return check;

            ViewBag.VaiTroList = new SelectList(new[]
            {
        "Admin",
        "Quản lý",
        "Nhân viên"
    });

            ViewBag.TrangThaiList = new SelectList(new[]
            {
        "Đang hoạt động",
        "Tạm khóa",
        "Đã nghỉ việc"
    }, "Đang hoạt động");

            return View();
        }

        // POST: NHANVIEN65134364/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MANV,HOTENNV,SODIENTHOAINV,EMAILNV,TENDANGNHAP,MATKHAUNV,VAITRO,TRANGTHAI")] NHANVIEN nhanVien)
        {
            var check = KiemTraQuyen();
            if (check != null) return check;

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

            ViewBag.VaiTroList = new SelectList(new[]
            {
        "Admin",
        "Quản lý",
        "Nhân viên"
    }, nhanVien.VAITRO);

            ViewBag.TrangThaiList = new SelectList(new[]
            {
        "Đang hoạt động",
        "Tạm khóa",
        "Đã nghỉ việc"
    }, nhanVien.TRANGTHAI);

            return View(nhanVien);
        }

        // GET: NHANVIEN65134364/Edit/NV01
        public ActionResult Edit(string id)
        {
            var check = KiemTraQuyen();
            if (check != null) return check;

            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            NHANVIEN nhanVien = db.NHANVIEN.Find(id);

            if (nhanVien == null)
            {
                return HttpNotFound();
            }

            ViewBag.VaiTroList = new SelectList(new[]
            {
        "Admin",
        "Quản lý",
        "Nhân viên"
    }, nhanVien.VAITRO);

            ViewBag.TrangThaiList = new SelectList(new[]
            {
        "Đang hoạt động",
        "Tạm khóa",
        "Đã nghỉ việc"
    }, nhanVien.TRANGTHAI);

            return View(nhanVien);
        }

        // POST: NHANVIEN65134364/Edit/NV01
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MANV,HOTENNV,SODIENTHOAINV,EMAILNV,TENDANGNHAP,MATKHAUNV,VAITRO,TRANGTHAI")] NHANVIEN nhanVien)
        {
            var check = KiemTraQuyen();
            if (check != null) return check;

            KiemTraDuLieuNhanVien(nhanVien, false);

            if (ModelState.IsValid)
            {
                db.Entry(nhanVien).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Cập nhật tài khoản nhân viên thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.VaiTroList = new SelectList(new[]
            {
        "Admin",
        "Quản lý",
        "Nhân viên"
    }, nhanVien.VAITRO);

            ViewBag.TrangThaiList = new SelectList(new[]
            {
        "Đang hoạt động",
        "Tạm khóa",
        "Đã nghỉ việc"
    }, nhanVien.TRANGTHAI);

            return View(nhanVien);
        }

        // GET: NHANVIEN65134364/Delete/NV01
        // GET: NHANVIEN65134364/Delete/NV01
        public ActionResult Delete(string id)
        {
            var check = KiemTraQuyen();
            if (check != null) return check;

            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            NHANVIEN nhanVien = db.NHANVIEN.Find(id);

            if (nhanVien == null)
            {
                return HttpNotFound();
            }

            return View(nhanVien);
        }

        // POST: NHANVIEN65134364/Delete/NV01
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var check = KiemTraQuyen();
            if (check != null) return check;

            NHANVIEN nhanVien = db.NHANVIEN.Find(id);

            if (nhanVien == null)
            {
                return HttpNotFound();
            }

            // Không cho xóa chính tài khoản đang đăng nhập
            if (Session["MANV"] != null && Session["MANV"].ToString() == id)
            {
                TempData["Error"] = "Bạn không thể xóa chính tài khoản đang đăng nhập!";
                return RedirectToAction("Index");
            }

            // Nếu nhân viên đã phát sinh dữ liệu thì không xóa cứng, chỉ chuyển sang Tạm khóa
            bool coPhieuDat = db.PHIEUDATSAN.Any(p => p.MANV == id);
            bool coHoaDon = db.HOADON.Any(h => h.MANV == id);
            bool coHoiVien = db.HOIVIEN.Any(hv => hv.MANV == id);

            if (coPhieuDat || coHoaDon || coHoiVien)
            {
                nhanVien.TRANGTHAI = "Tạm khóa";
                db.SaveChanges();

                TempData["Error"] = "Nhân viên đã có dữ liệu liên quan nên không thể xóa. Hệ thống đã chuyển tài khoản sang Tạm khóa.";
                return RedirectToAction("Index");
            }

            db.NHANVIEN.Remove(nhanVien);
            db.SaveChanges();

            TempData["Success"] = "Xóa nhân viên thành công!";
            return RedirectToAction("Index");
        }

        // GET: NHANVIEN65134364/Khoa/NV01
        public ActionResult Khoa(string id)
        {
            var check = KiemTraQuyen();
            if (check != null) return check;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            NHANVIEN nhanVien = db.NHANVIEN.Find(id);

            if (nhanVien == null)
            {
                return HttpNotFound();
            }

            if (Session["MANV"] != null && Session["MANV"].ToString() == id)
            {
                TempData["Error"] = "Bạn không thể khóa chính tài khoản đang đăng nhập!";
                return RedirectToAction("Index");
            }

            nhanVien.TRANGTHAI = "Tạm khóa";
            db.SaveChanges();

            TempData["Success"] = "Khóa tài khoản nhân viên thành công!";
            return RedirectToAction("Index");
        }

        // GET: NHANVIEN65134364/MoKhoa/NV01
        public ActionResult MoKhoa(string id)
        {
            var check = KiemTraQuyen();
            if (check != null) return check;

            if (id == null)
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

            TempData["Success"] = "Mở khóa tài khoản nhân viên thành công!";
            return RedirectToAction("Index");
        }

        private string TaoMaNhanVienMoi()
        {
            var maCuoi = db.NHANVIEN
                .Where(n => n.MANV.StartsWith("NV"))
                .OrderByDescending(n => n.MANV)
                .Select(n => n.MANV)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(maCuoi))
            {
                return "NV01";
            }

            int so = 0;
            string phanSo = maCuoi.Replace("NV", "");

            int.TryParse(phanSo, out so);
            so++;

            return "NV" + so.ToString("00");
        }

        private void KiemTraDuLieuNhanVien(NHANVIEN nhanVien, bool laThemMoi)
        {
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

            bool trungSDT = db.NHANVIEN.Any(n =>
                n.SODIENTHOAINV == nhanVien.SODIENTHOAINV &&
                (laThemMoi || n.MANV != nhanVien.MANV));

            if (trungSDT)
            {
                ModelState.AddModelError("SODIENTHOAINV", "Số điện thoại này đã tồn tại.");
            }

            bool trungEmail = db.NHANVIEN.Any(n =>
                n.EMAILNV == nhanVien.EMAILNV &&
                (laThemMoi || n.MANV != nhanVien.MANV));

            if (trungEmail)
            {
                ModelState.AddModelError("EMAILNV", "Email này đã tồn tại.");
            }

            bool trungTenDangNhap = db.NHANVIEN.Any(n =>
                n.TENDANGNHAP == nhanVien.TENDANGNHAP &&
                (laThemMoi || n.MANV != nhanVien.MANV));

            if (trungTenDangNhap)
            {
                ModelState.AddModelError("TENDANGNHAP", "Tên đăng nhập này đã tồn tại.");
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