using QLSanPickleball_65132651.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace QLSanPickleball_65132651.Controllers
{
    public class SAN65134364Controller : Controller
    {
        private QLSanEntities db = new QLSanEntities();

        private readonly string[] TrangThaiHuySan =
        {
            "DaHuy",
            "Da huy",
            "Đã hủy",
            "Đã huỷ",
            "Huy",
            "Hủy"
        };

        private ActionResult KiemTraQuyenAdmin()
        {
            if (Session["MANV"] == null)
            {
                return RedirectToAction("Login", "Account65132651");
            }

            string vaiTro = Session["VAITRO"] != null ? Session["VAITRO"].ToString() : "";

            if (vaiTro != "Admin" && vaiTro != "Quản lý")
            {
                TempData["Error"] = "Bạn không có quyền sử dụng chức năng quản lý sân.";
                return RedirectToAction("HomeNv", "Admin65134364");
            }

            return null;
        }

        private ActionResult KiemTraQuyenNhanVien()
        {
            if (Session["MANV"] == null)
            {
                return RedirectToAction("Login", "Account65132651");
            }

            string vaiTro = Session["VAITRO"] != null ? Session["VAITRO"].ToString() : "";

            if (vaiTro != "Admin" && vaiTro != "Quản lý" && vaiTro != "Nhân viên")
            {
                return RedirectToAction("Index", "Home");
            }

            return null;
        }

        // =========================================================
        // ĐỒNG BỘ TRẠNG THÁI SÂN TỪ PHIẾU ĐẶT SÂN
        // =========================================================
        private void CapNhatTrangThaiSanTuLichDat()
        {
            DateTime homNay = DateTime.Today;
            DateTime ngayMai = homNay.AddDays(1);
            TimeSpan gioHienTai = DateTime.Now.TimeOfDay;

            var dsSan = db.SAN.ToList();

            var dsPhieuHomNay = db.PHIEUDATSAN
                .Where(p => p.NGAYDAT >= homNay
                         && p.NGAYDAT < ngayMai
                         && !TrangThaiHuySan.Contains(p.TRANGTHAIPHIEU))
                .ToList();

            bool coThayDoi = false;

            foreach (var san in dsSan)
            {
                if (san.TRANGTHAISAN == "Bảo trì")
                {
                    continue;
                }

                string trangThaiMoi = "Trống";

                var dsPhieuCuaSan = dsPhieuHomNay
                    .Where(p => p.MASAN == san.MASAN)
                    .ToList();

                var phieuDangDienRa = dsPhieuCuaSan.FirstOrDefault(p =>
                    p.GIOBATDAU <= gioHienTai &&
                    p.GIOKETTHUC > gioHienTai
                );

                if (phieuDangDienRa != null)
                {
                    if (phieuDangDienRa.TRANGTHAIPHIEU == "Đã xác nhận")
                    {
                        trangThaiMoi = "Đang sử dụng";
                    }
                    else
                    {
                        trangThaiMoi = "Đang đặt";
                    }
                }
                else
                {
                    bool coLichSapToi = dsPhieuCuaSan.Any(p => p.GIOBATDAU > gioHienTai);
                    trangThaiMoi = coLichSapToi ? "Đang đặt" : "Trống";
                }

                if (san.TRANGTHAISAN != trangThaiMoi)
                {
                    san.TRANGTHAISAN = trangThaiMoi;
                    coThayDoi = true;
                }
            }

            if (coThayDoi)
            {
                db.SaveChanges();
            }
        }

        // GET: SAN65134364
        public ActionResult Index(string search, string maLoaiSan, string trangThai)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            CapNhatTrangThaiSanTuLichDat();

            string vaiTro = Session["VAITRO"] != null ? Session["VAITRO"].ToString() : "";
            ViewBag.LaAdminHoacQuanLy = vaiTro == "Admin" || vaiTro == "Quản lý";

            var dsSan = db.SAN.Include(s => s.LOAISAN).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                dsSan = dsSan.Where(s =>
                    s.MASAN.Contains(search) ||
                    s.TENSAN.Contains(search) ||
                    s.MOTASAN.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(maLoaiSan))
            {
                dsSan = dsSan.Where(s => s.MALOAISAN == maLoaiSan);
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                dsSan = dsSan.Where(s => s.TRANGTHAISAN == trangThai);
            }

            ViewBag.Search = search;
            ViewBag.MaLoaiSan = maLoaiSan;
            ViewBag.TrangThai = trangThai;
            ViewBag.LoaiSanList = new SelectList(db.LOAISAN.ToList(), "MALOAISAN", "TENLOAISAN", maLoaiSan);

            ViewBag.TongSan = db.SAN.Count();
            ViewBag.SanTrong = db.SAN.Count(s => s.TRANGTHAISAN == "Trống");
            ViewBag.SanDangDat = db.SAN.Count(s => s.TRANGTHAISAN == "Đang đặt");
            ViewBag.SanDangSuDung = db.SAN.Count(s => s.TRANGTHAISAN == "Đang sử dụng");
            ViewBag.SanBaoTri = db.SAN.Count(s => s.TRANGTHAISAN == "Bảo trì");

            return View(dsSan.OrderBy(s => s.MASAN).ToList());
        }

        // GET: SAN65134364/Details/S01
        public ActionResult Details(string id)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SAN san = db.SAN
                .Include(s => s.LOAISAN)
                .FirstOrDefault(s => s.MASAN == id);

            if (san == null)
            {
                return HttpNotFound();
            }

            return View(san);
        }

        // GET: SAN65134364/Create
        public ActionResult Create()
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            ViewBag.MALOAISAN = new SelectList(db.LOAISAN.ToList(), "MALOAISAN", "TENLOAISAN");

            ViewBag.TrangThaiList = new SelectList(new[]
            {
                "Trống",
                "Đang đặt",
                "Đang sử dụng",
                "Bảo trì"
            }, "Trống");

            return View();
        }

        // POST: SAN65134364/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MASAN,MALOAISAN,TENSAN,TRANGTHAISAN,MOTASAN")] SAN san)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(san.MASAN))
            {
                san.MASAN = TaoMaSanMoi();
            }

            if (string.IsNullOrWhiteSpace(san.TRANGTHAISAN))
            {
                san.TRANGTHAISAN = "Trống";
            }

            KiemTraDuLieuSan(san, true);

            if (ModelState.IsValid)
            {
                db.SAN.Add(san);
                db.SaveChanges();

                TempData["Success"] = "Thêm sân thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.MALOAISAN = new SelectList(db.LOAISAN.ToList(), "MALOAISAN", "TENLOAISAN", san.MALOAISAN);

            ViewBag.TrangThaiList = new SelectList(new[]
            {
                "Trống",
                "Đang đặt",
                "Đang sử dụng",
                "Bảo trì"
            }, san.TRANGTHAISAN);

            return View(san);
        }

        // GET: SAN65134364/Edit/S01
        public ActionResult Edit(string id)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SAN san = db.SAN.Find(id);

            if (san == null)
            {
                return HttpNotFound();
            }

            ViewBag.MALOAISAN = new SelectList(db.LOAISAN.ToList(), "MALOAISAN", "TENLOAISAN", san.MALOAISAN);

            ViewBag.TrangThaiList = new SelectList(new[]
            {
                "Trống",
                "Đang đặt",
                "Đang sử dụng",
                "Bảo trì"
            }, san.TRANGTHAISAN);

            return View(san);
        }

        // POST: SAN65134364/Edit/S01
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MASAN,MALOAISAN,TENSAN,TRANGTHAISAN,MOTASAN")] SAN san)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            KiemTraDuLieuSan(san, false);

            if (ModelState.IsValid)
            {
                db.Entry(san).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Cập nhật sân thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.MALOAISAN = new SelectList(db.LOAISAN.ToList(), "MALOAISAN", "TENLOAISAN", san.MALOAISAN);

            ViewBag.TrangThaiList = new SelectList(new[]
            {
                "Trống",
                "Đang đặt",
                "Đang sử dụng",
                "Bảo trì"
            }, san.TRANGTHAISAN);

            return View(san);
        }

        // GET: SAN65134364/Delete/S01
        public ActionResult Delete(string id)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SAN san = db.SAN
                .Include(s => s.LOAISAN)
                .FirstOrDefault(s => s.MASAN == id);

            if (san == null)
            {
                return HttpNotFound();
            }

            return View(san);
        }

        // POST: SAN65134364/Delete/S01
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            SAN san = db.SAN.Find(id);

            if (san == null)
            {
                return HttpNotFound();
            }

            bool coPhieuDat = db.PHIEUDATSAN.Any(p => p.MASAN == id);

            if (coPhieuDat)
            {
                san.TRANGTHAISAN = "Bảo trì";
                db.SaveChanges();

                TempData["Error"] = "Sân đã có phiếu đặt nên không thể xóa. Hệ thống đã chuyển sân sang trạng thái Bảo trì.";
                return RedirectToAction("Index");
            }

            db.SAN.Remove(san);
            db.SaveChanges();

            TempData["Success"] = "Xóa sân thành công!";
            return RedirectToAction("Index");
        }

        // GET: SAN65134364/CapNhatTrangThai/S01
        public ActionResult CapNhatTrangThai(string id)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SAN san = db.SAN
                .Include(s => s.LOAISAN)
                .FirstOrDefault(s => s.MASAN == id);

            if (san == null)
            {
                return HttpNotFound();
            }

            ViewBag.TrangThaiList = new SelectList(new[]
            {
                "Trống",
                "Đang đặt",
                "Đang sử dụng",
                "Bảo trì"
            }, san.TRANGTHAISAN);

            return View(san);
        }

        // POST: SAN65134364/CapNhatTrangThai
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatTrangThai(string MASAN, string TRANGTHAISAN)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(MASAN))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SAN san = db.SAN.Find(MASAN);

            if (san == null)
            {
                return HttpNotFound();
            }

            if (!LaTrangThaiSanHopLe(TRANGTHAISAN))
            {
                TempData["Error"] = "Trạng thái sân không hợp lệ!";
                return RedirectToAction("CapNhatTrangThai", new { id = MASAN });
            }

            san.TRANGTHAISAN = TRANGTHAISAN;
            db.SaveChanges();

            TempData["Success"] = "Cập nhật trạng thái sân thành công!";
            return RedirectToAction("Index");
        }

        // GET: SAN65134364/ChuyenTrangThai/S01?trangThai=Trống
        public ActionResult ChuyenTrangThai(string id, string trangThai)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            if (id == null || string.IsNullOrWhiteSpace(trangThai))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SAN san = db.SAN.Find(id);

            if (san == null)
            {
                return HttpNotFound();
            }

            if (!LaTrangThaiSanHopLe(trangThai))
            {
                TempData["Error"] = "Trạng thái sân không hợp lệ!";
                return RedirectToAction("Index");
            }

            san.TRANGTHAISAN = trangThai;
            db.SaveChanges();

            TempData["Success"] = "Cập nhật trạng thái sân thành công!";
            return RedirectToAction("Index");
        }

        private bool LaTrangThaiSanHopLe(string trangThai)
        {
            return trangThai == "Trống"
                || trangThai == "Đang đặt"
                || trangThai == "Đang sử dụng"
                || trangThai == "Bảo trì";
        }

        private string TaoMaSanMoi()
        {
            var maCuoi = db.SAN
                .Where(s => s.MASAN.StartsWith("S"))
                .OrderByDescending(s => s.MASAN)
                .Select(s => s.MASAN)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(maCuoi))
            {
                return "S01";
            }

            int so = 0;
            string phanSo = maCuoi.Replace("S", "");
            int.TryParse(phanSo, out so);
            so++;

            return "S" + so.ToString("00");
        }

        private void KiemTraDuLieuSan(SAN san, bool laThemMoi)
        {
            if (string.IsNullOrWhiteSpace(san.MASAN))
            {
                ModelState.AddModelError("MASAN", "Mã sân không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(san.TENSAN))
            {
                ModelState.AddModelError("TENSAN", "Tên sân không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(san.MALOAISAN))
            {
                ModelState.AddModelError("MALOAISAN", "Vui lòng chọn loại sân.");
            }

            if (string.IsNullOrWhiteSpace(san.TRANGTHAISAN))
            {
                ModelState.AddModelError("TRANGTHAISAN", "Vui lòng chọn trạng thái sân.");
            }

            if (laThemMoi)
            {
                bool maSanTonTai = db.SAN.Any(s => s.MASAN == san.MASAN);

                if (maSanTonTai)
                {
                    ModelState.AddModelError("MASAN", "Mã sân này đã tồn tại.");
                }
            }

            bool tenSanTonTai = db.SAN.Any(s =>
                s.TENSAN == san.TENSAN &&
                (laThemMoi || s.MASAN != san.MASAN));

            if (tenSanTonTai)
            {
                ModelState.AddModelError("TENSAN", "Tên sân này đã tồn tại.");
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