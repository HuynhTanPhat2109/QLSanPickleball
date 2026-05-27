using QLSanPickleball_65132651.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace QLSanPickleball_65132651.Controllers
{
    public class BANGGIA65134364Controller : Controller
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

        private ActionResult KiemTraQuyenQuanTri()
        {
            if (Session["MANV"] == null)
            {
                return RedirectToAction("Login", "Account65132651");
            }

            if (!LaAdmin() && !LaQuanLy())
            {
                TempData["Error"] = "Bạn không có quyền truy cập chức năng quản lý bảng giá.";
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

        private void GanViewBagLoaiSan(string maLoaiSan)
        {
            ViewBag.MALOAISAN = new SelectList(
                db.LOAISAN.OrderBy(l => l.TENLOAISAN).ToList(),
                "MALOAISAN",
                "TENLOAISAN",
                maLoaiSan
            );
        }

        private void GanViewBagThu(string thu)
        {
            ViewBag.ThuList = new SelectList(new[]
            {
                "Thứ 2",
                "Thứ 3",
                "Thứ 4",
                "Thứ 5",
                "Thứ 6",
                "Thứ 7",
                "Chủ nhật",
                "Tất cả"
            }, thu);
        }

        // =========================================================
        // GET: BANGGIA65134364
        // =========================================================
        public ActionResult Index(string search, string maLoaiSan, string thu)
        {
            var check = KiemTraQuyenQuanTri();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            var dsBangGia = db.BANGGIA
                .Include(b => b.LOAISAN)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                dsBangGia = dsBangGia.Where(b =>
                    b.MAGIA.Contains(search) ||
                    b.MALOAISAN.Contains(search) ||
                    b.THU.Contains(search) ||
                    b.LOAISAN.TENLOAISAN.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(maLoaiSan))
            {
                dsBangGia = dsBangGia.Where(b => b.MALOAISAN == maLoaiSan);
            }

            if (!string.IsNullOrWhiteSpace(thu))
            {
                dsBangGia = dsBangGia.Where(b => b.THU == thu);
            }

            ViewBag.Search = search;
            ViewBag.MaLoaiSan = maLoaiSan;
            ViewBag.Thu = thu;

            ViewBag.LoaiSanList = new SelectList(
                db.LOAISAN.OrderBy(l => l.TENLOAISAN).ToList(),
                "MALOAISAN",
                "TENLOAISAN",
                maLoaiSan
            );

            ViewBag.ThuList = new SelectList(new[]
            {
                "Thứ 2",
                "Thứ 3",
                "Thứ 4",
                "Thứ 5",
                "Thứ 6",
                "Thứ 7",
                "Chủ nhật",
                "Tất cả"
            }, thu);

            ViewBag.TongBangGia = dsBangGia.Count();

            return View(dsBangGia
                .OrderByDescending(b => b.NGAYDIEUCHINH)
                .ThenBy(b => b.MALOAISAN)
                .ThenBy(b => b.THU)
                .ThenBy(b => b.GIOBATDAU)
                .ToList());
        }

        // =========================================================
        // GET: BANGGIA65134364/Details/BG01
        // =========================================================
        public ActionResult Details(string id)
        {
            var check = KiemTraQuyenQuanTri();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BANGGIA bangGia = db.BANGGIA
                .Include(b => b.LOAISAN)
                .FirstOrDefault(b => b.MAGIA == id);

            if (bangGia == null)
            {
                return HttpNotFound();
            }

            return View(bangGia);
        }

        // =========================================================
        // GET: BANGGIA65134364/Create
        // =========================================================
        public ActionResult Create()
        {
            var check = KiemTraQuyenQuanTri();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            BANGGIA bangGia = new BANGGIA
            {
                MAGIA = TaoMaGiaMoi(),
                NGAYDIEUCHINH = DateTime.Today,
                THU = "Tất cả",
                GIOBATDAU = new TimeSpan(5, 0, 0),
                GIOKETTHUC = new TimeSpan(22, 0, 0),
                GIAVANGLAI = 0,
                GIACODINH = 0
            };

            GanViewBagLoaiSan(bangGia.MALOAISAN);
            GanViewBagThu(bangGia.THU);

            return View(bangGia);
        }

        // =========================================================
        // POST: BANGGIA65134364/Create
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MAGIA,MALOAISAN,NGAYDIEUCHINH,THU,GIOBATDAU,GIOKETTHUC,GIACODINH,GIAVANGLAI")] BANGGIA bangGia)
        {
            var check = KiemTraQuyenQuanTri();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            if (string.IsNullOrWhiteSpace(bangGia.MAGIA))
            {
                bangGia.MAGIA = TaoMaGiaMoi();
            }

            KiemTraDuLieuBangGia(bangGia, true);

            if (ModelState.IsValid)
            {
                db.BANGGIA.Add(bangGia);
                db.SaveChanges();

                TempData["Success"] = "Thêm bảng giá sân thành công!";
                return RedirectToAction("Index");
            }

            GanViewBagLoaiSan(bangGia.MALOAISAN);
            GanViewBagThu(bangGia.THU);

            return View(bangGia);
        }

        // =========================================================
        // GET: BANGGIA65134364/Edit/BG01
        // =========================================================
        public ActionResult Edit(string id)
        {
            var check = KiemTraQuyenQuanTri();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BANGGIA bangGia = db.BANGGIA.Find(id);

            if (bangGia == null)
            {
                return HttpNotFound();
            }

            GanViewBagLoaiSan(bangGia.MALOAISAN);
            GanViewBagThu(bangGia.THU);

            return View(bangGia);
        }

        // =========================================================
        // POST: BANGGIA65134364/Edit/BG01
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MAGIA,MALOAISAN,NGAYDIEUCHINH,THU,GIOBATDAU,GIOKETTHUC,GIACODINH,GIAVANGLAI")] BANGGIA bangGia)
        {
            var check = KiemTraQuyenQuanTri();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            KiemTraDuLieuBangGia(bangGia, false);

            if (ModelState.IsValid)
            {
                db.Entry(bangGia).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Cập nhật bảng giá sân thành công!";
                return RedirectToAction("Index");
            }

            GanViewBagLoaiSan(bangGia.MALOAISAN);
            GanViewBagThu(bangGia.THU);

            return View(bangGia);
        }

        // =========================================================
        // GET: BANGGIA65134364/Delete/BG01
        // =========================================================
        public ActionResult Delete(string id)
        {
            var check = KiemTraQuyenQuanTri();
            if (check != null) return check;

            GanViewBagPhanQuyen();

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BANGGIA bangGia = db.BANGGIA
                .Include(b => b.LOAISAN)
                .FirstOrDefault(b => b.MAGIA == id);

            if (bangGia == null)
            {
                return HttpNotFound();
            }

            return View(bangGia);
        }

        // =========================================================
        // POST: BANGGIA65134364/Delete/BG01
        // =========================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var check = KiemTraQuyenQuanTri();
            if (check != null) return check;

            BANGGIA bangGia = db.BANGGIA.Find(id);

            if (bangGia == null)
            {
                return HttpNotFound();
            }

            db.BANGGIA.Remove(bangGia);
            db.SaveChanges();

            TempData["Success"] = "Xóa bảng giá sân thành công!";
            return RedirectToAction("Index");
        }

        // =========================================================
        // HÀM PHỤ
        // =========================================================
        private string TaoMaGiaMoi()
        {
            var maCuoi = db.BANGGIA
                .Where(b => b.MAGIA.StartsWith("BG"))
                .OrderByDescending(b => b.MAGIA)
                .Select(b => b.MAGIA)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(maCuoi))
            {
                return "BG01";
            }

            string soCuoi = maCuoi.Replace("BG", "");
            int so = 0;
            int.TryParse(soCuoi, out so);
            so++;

            return "BG" + so.ToString("00");
        }

        private void KiemTraDuLieuBangGia(BANGGIA bangGia, bool laThemMoi)
        {
            if (string.IsNullOrWhiteSpace(bangGia.MAGIA))
            {
                ModelState.AddModelError("MAGIA", "Mã giá không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(bangGia.MALOAISAN))
            {
                ModelState.AddModelError("MALOAISAN", "Vui lòng chọn loại sân.");
            }

            if (string.IsNullOrWhiteSpace(bangGia.THU))
            {
                ModelState.AddModelError("THU", "Vui lòng chọn thứ áp dụng.");
            }

            if (bangGia.GIOKETTHUC <= bangGia.GIOBATDAU)
            {
                ModelState.AddModelError("GIOKETTHUC", "Giờ kết thúc phải lớn hơn giờ bắt đầu.");
            }

            if (bangGia.GIAVANGLAI <= 0)
            {
                ModelState.AddModelError("GIAVANGLAI", "Giá vãng lai phải lớn hơn 0.");
            }

            if (bangGia.GIACODINH <= 0)
            {
                ModelState.AddModelError("GIACODINH", "Giá cố định phải lớn hơn 0.");
            }

            if (bangGia.NGAYDIEUCHINH == DateTime.MinValue)
            {
                ModelState.AddModelError("NGAYDIEUCHINH", "Vui lòng chọn ngày điều chỉnh.");
            }

            if (laThemMoi)
            {
                bool trungMa = db.BANGGIA.Any(b => b.MAGIA == bangGia.MAGIA);

                if (trungMa)
                {
                    ModelState.AddModelError("MAGIA", "Mã giá này đã tồn tại.");
                }
            }

            bool trungKhungGio = db.BANGGIA.Any(b =>
                b.MALOAISAN == bangGia.MALOAISAN &&
                b.THU == bangGia.THU &&
                b.NGAYDIEUCHINH == bangGia.NGAYDIEUCHINH &&
                b.GIOBATDAU == bangGia.GIOBATDAU &&
                b.GIOKETTHUC == bangGia.GIOKETTHUC &&
                (laThemMoi || b.MAGIA != bangGia.MAGIA));

            if (trungKhungGio)
            {
                ModelState.AddModelError("", "Bảng giá này đã tồn tại cho loại sân, thứ, ngày điều chỉnh và khung giờ đã chọn.");
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