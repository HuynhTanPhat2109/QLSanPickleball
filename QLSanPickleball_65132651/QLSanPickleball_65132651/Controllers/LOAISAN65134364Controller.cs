using QLSanPickleball_65132651.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace QLSanPickleball_65132651.Controllers
{
    public class LOAISAN65134364Controller : Controller
    {
        private QLSanEntities db = new QLSanEntities();

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

        private ActionResult KiemTraQuyenQuanTri()
        {
            if (Session["MANV"] == null)
            {
                return RedirectToAction("Login", "Account65132651");
            }

            if (!LaAdmin() && !LaQuanLy())
            {
                TempData["Error"] = "Bạn không có quyền truy cập chức năng quản lý loại sân.";
                return RedirectToAction("HomeNv", "Admin65134364");
            }

            return null;
        }

        private void GanViewBagPhanQuyen()
        {
            ViewBag.LaAdmin = LaAdmin();
            ViewBag.LaQuanLy = LaQuanLy();
            ViewBag.DuocThemSuaXoa = LaAdmin() || LaQuanLy();
        }

        // GET: LOAISAN65134364
        public ActionResult Index(string search)
        {
            var check = KiemTraQuyenQuanTri();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            var dsLoaiSan = db.LOAISAN.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                dsLoaiSan = dsLoaiSan.Where(l =>
                    l.MALOAISAN.Contains(search) ||
                    l.TENLOAISAN.Contains(search));
            }

            ViewBag.Search = search;
            ViewBag.TongLoaiSan = dsLoaiSan.Count();

            return View(dsLoaiSan.OrderBy(l => l.MALOAISAN).ToList());
        }

        // GET: LOAISAN65134364/Details/LS01
        public ActionResult Details(string id)
        {
            var check = KiemTraQuyenQuanTri();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            LOAISAN loaiSan = db.LOAISAN.Find(id);

            if (loaiSan == null)
            {
                return HttpNotFound();
            }

            ViewBag.SoSanThuocLoai = db.SAN.Count(s => s.MALOAISAN == id);
            ViewBag.SoBangGiaThuocLoai = db.BANGGIA.Count(b => b.MALOAISAN == id);

            return View(loaiSan);
        }

        // GET: LOAISAN65134364/Create
        public ActionResult Create()
        {
            var check = KiemTraQuyenQuanTri();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            LOAISAN loaiSan = new LOAISAN();
            loaiSan.MALOAISAN = TaoMaLoaiSanMoi();

            return View(loaiSan);
        }

        // POST: LOAISAN65134364/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MALOAISAN,TENLOAISAN")] LOAISAN loaiSan)
        {
            var check = KiemTraQuyenQuanTri();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            if (string.IsNullOrWhiteSpace(loaiSan.MALOAISAN))
            {
                loaiSan.MALOAISAN = TaoMaLoaiSanMoi();
            }

            KiemTraDuLieuLoaiSan(loaiSan, true);

            if (ModelState.IsValid)
            {
                db.LOAISAN.Add(loaiSan);
                db.SaveChanges();

                TempData["Success"] = "Thêm loại sân thành công!";
                return RedirectToAction("Index");
            }

            return View(loaiSan);
        }

        // GET: LOAISAN65134364/Edit/LS01
        public ActionResult Edit(string id)
        {
            var check = KiemTraQuyenQuanTri();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            LOAISAN loaiSan = db.LOAISAN.Find(id);

            if (loaiSan == null)
            {
                return HttpNotFound();
            }

            return View(loaiSan);
        }

        // POST: LOAISAN65134364/Edit/LS01
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MALOAISAN,TENLOAISAN")] LOAISAN loaiSan)
        {
            var check = KiemTraQuyenQuanTri();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            KiemTraDuLieuLoaiSan(loaiSan, false);

            if (ModelState.IsValid)
            {
                db.Entry(loaiSan).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Cập nhật loại sân thành công!";
                return RedirectToAction("Index");
            }

            return View(loaiSan);
        }

        // GET: LOAISAN65134364/Delete/LS01
        public ActionResult Delete(string id)
        {
            var check = KiemTraQuyenQuanTri();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            LOAISAN loaiSan = db.LOAISAN.Find(id);

            if (loaiSan == null)
            {
                return HttpNotFound();
            }

            ViewBag.SoSanThuocLoai = db.SAN.Count(s => s.MALOAISAN == id);
            ViewBag.SoBangGiaThuocLoai = db.BANGGIA.Count(b => b.MALOAISAN == id);

            return View(loaiSan);
        }

        // POST: LOAISAN65134364/Delete/LS01
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var check = KiemTraQuyenQuanTri();
            if (check != null) return check;

            LOAISAN loaiSan = db.LOAISAN.Find(id);

            if (loaiSan == null)
            {
                return HttpNotFound();
            }

            bool coSan = db.SAN.Any(s => s.MALOAISAN == id);
            bool coBangGia = db.BANGGIA.Any(b => b.MALOAISAN == id);

            if (coSan || coBangGia)
            {
                TempData["Error"] = "Không thể xóa loại sân này vì đã có sân hoặc bảng giá đang sử dụng.";
                return RedirectToAction("Index");
            }

            db.LOAISAN.Remove(loaiSan);
            db.SaveChanges();

            TempData["Success"] = "Xóa loại sân thành công!";
            return RedirectToAction("Index");
        }

        private string TaoMaLoaiSanMoi()
        {
            var maCuoi = db.LOAISAN
                .Where(l => l.MALOAISAN.StartsWith("LS"))
                .OrderByDescending(l => l.MALOAISAN)
                .Select(l => l.MALOAISAN)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(maCuoi))
            {
                return "LS01";
            }

            string soCuoi = maCuoi.Replace("LS", "");
            int so = 0;
            int.TryParse(soCuoi, out so);
            so++;

            return "LS" + so.ToString("00");
        }

        private void KiemTraDuLieuLoaiSan(LOAISAN loaiSan, bool laThemMoi)
        {
            if (string.IsNullOrWhiteSpace(loaiSan.MALOAISAN))
            {
                ModelState.AddModelError("MALOAISAN", "Mã loại sân không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(loaiSan.TENLOAISAN))
            {
                ModelState.AddModelError("TENLOAISAN", "Tên loại sân không được để trống.");
            }

            if (laThemMoi)
            {
                bool trungMa = db.LOAISAN.Any(l => l.MALOAISAN == loaiSan.MALOAISAN);

                if (trungMa)
                {
                    ModelState.AddModelError("MALOAISAN", "Mã loại sân này đã tồn tại.");
                }
            }

            bool trungTen = db.LOAISAN.Any(l =>
                l.TENLOAISAN == loaiSan.TENLOAISAN &&
                (laThemMoi || l.MALOAISAN != loaiSan.MALOAISAN));

            if (trungTen)
            {
                ModelState.AddModelError("TENLOAISAN", "Tên loại sân này đã tồn tại.");
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