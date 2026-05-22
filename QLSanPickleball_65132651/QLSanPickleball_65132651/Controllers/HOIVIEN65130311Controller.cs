using QLSanPickleball_65132651.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace QLSanPickleball_65132651.Controllers
{
    public class HOIVIEN65130311Controller : Controller
    {
        private QLSanEntities db = new QLSanEntities();

        private ActionResult KiemTraQuyenQuanLy()
        {
            if (Session["MANV"] == null)
            {
                return RedirectToAction("Login", "Account65132651");
            }

            string vaiTro = Session["VAITRO"] != null ? Session["VAITRO"].ToString() : "";

            if (vaiTro != "Admin" && vaiTro != "Quản lý")
            {
                TempData["Error"] = "Bạn không có quyền sử dụng chức năng duyệt hội viên.";
                return RedirectToAction("HomeNv", "Admin65134364");
            }

            return null;
        }

        public ActionResult Index(string search, string loaiThe, string trangThaiPhi)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            var dsHoiVien = db.HOIVIEN
                .Include(h => h.NHANVIEN)
                .Include(h => h.KHACHHANG)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                dsHoiVien = dsHoiVien.Where(h =>
                    h.MAHOIVIEN.Contains(search) ||
                    h.MAKH.Contains(search) ||
                    h.KHACHHANG.HOTENKH.Contains(search) ||
                    h.KHACHHANG.SODIENTHOAIKH.Contains(search) ||
                    h.KHACHHANG.EMAILKH.Contains(search)
                );
            }

            if (!string.IsNullOrWhiteSpace(loaiThe))
            {
                dsHoiVien = dsHoiVien.Where(h => h.LOAITHE == loaiThe);
            }

            if (!string.IsNullOrWhiteSpace(trangThaiPhi))
            {
                dsHoiVien = dsHoiVien.Where(h => h.TRANGTHAIPHI == trangThaiPhi);
            }

            ViewBag.Search = search;
            ViewBag.LoaiThe = loaiThe;
            ViewBag.TrangThaiPhi = trangThaiPhi;

            ViewBag.TongHoiVien = db.HOIVIEN.Count();
            ViewBag.ChoDuyet = db.HOIVIEN.Count(h => h.TRANGTHAIPHI == "Chờ duyệt");
            ViewBag.DaThanhToan = db.HOIVIEN.Count(h => h.TRANGTHAIPHI == "Đã thanh toán");
            ViewBag.ChuaThanhToan = db.HOIVIEN.Count(h => h.TRANGTHAIPHI == "Chưa thanh toán");

            return View(dsHoiVien.OrderByDescending(h => h.NGAYBATDAU).ToList());
        }

        public ActionResult Details(string id)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var hoiVien = db.HOIVIEN
                .Include(h => h.NHANVIEN)
                .Include(h => h.KHACHHANG)
                .FirstOrDefault(h => h.MAHOIVIEN == id);

            if (hoiVien == null)
            {
                return HttpNotFound();
            }

            return View(hoiVien);
        }

        public ActionResult Create()
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            HOIVIEN model = new HOIVIEN();
            model.MAHOIVIEN = TaoMaHoiVienMoi();
            model.MANV = Session["MANV"] != null ? Session["MANV"].ToString() : "";
            model.NGAYBATDAU = DateTime.Today;
            model.NGAYKETTHUC = DateTime.Today.AddMonths(3);
            model.TRANGTHAIPHI = "Chờ duyệt";

            LoadDropdown(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MAHOIVIEN,MANV,MAKH,LOAITHE,NGAYBATDAU,NGAYKETTHUC,PHIDANGKY,TRANGTHAIPHI")] HOIVIEN hoiVien)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(hoiVien.MAHOIVIEN))
            {
                hoiVien.MAHOIVIEN = TaoMaHoiVienMoi();
            }

            if (string.IsNullOrWhiteSpace(hoiVien.MANV))
            {
                hoiVien.MANV = Session["MANV"] != null ? Session["MANV"].ToString() : "";
            }

            KiemTraDuLieu(hoiVien, true);

            if (ModelState.IsValid)
            {
                db.HOIVIEN.Add(hoiVien);
                db.SaveChanges();

                TempData["Success"] = "Thêm yêu cầu hội viên thành công.";
                return RedirectToAction("Index");
            }

            LoadDropdown(hoiVien);
            return View(hoiVien);
        }

        public ActionResult Edit(string id)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            HOIVIEN hoiVien = db.HOIVIEN.Find(id);

            if (hoiVien == null)
            {
                return HttpNotFound();
            }

            LoadDropdown(hoiVien);
            return View(hoiVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MAHOIVIEN,MANV,MAKH,LOAITHE,NGAYBATDAU,NGAYKETTHUC,PHIDANGKY,TRANGTHAIPHI")] HOIVIEN hoiVien)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            KiemTraDuLieu(hoiVien, false);

            if (ModelState.IsValid)
            {
                db.Entry(hoiVien).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Cập nhật hội viên thành công.";
                return RedirectToAction("Index");
            }

            LoadDropdown(hoiVien);
            return View(hoiVien);
        }

        public ActionResult Duyet(string id)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var hoiVien = db.HOIVIEN.Find(id);

            if (hoiVien == null)
            {
                return HttpNotFound();
            }

            if (hoiVien.TRANGTHAIPHI != "Đã thanh toán")
            {
                TempData["Error"] = "Chỉ duyệt hội viên khi trạng thái phí là Đã thanh toán.";
                return RedirectToAction("Index");
            }

            hoiVien.TRANGTHAIPHI = "Đã duyệt";

            hoiVien.MANV = Session["MANV"].ToString();

            if (hoiVien.NGAYBATDAU == DateTime.MinValue)
            {
                hoiVien.NGAYBATDAU = DateTime.Today;
            }

            if (hoiVien.NGAYKETTHUC <= hoiVien.NGAYBATDAU)
            {
                hoiVien.NGAYKETTHUC = hoiVien.NGAYBATDAU.AddMonths(3);
            }

            db.SaveChanges();

            TempData["Success"] = "Duyệt đăng ký hội viên thành công.";
            return RedirectToAction("Index");
        }

        public ActionResult XacNhanDongPhi(string id)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            var hoiVien = db.HOIVIEN.Find(id);

            if (hoiVien == null)
            {
                return HttpNotFound();
            }

            hoiVien.TRANGTHAIPHI = "Đã thanh toán";
            hoiVien.MANV = Session["MANV"].ToString();

            db.SaveChanges();

            TempData["Success"] = "Đã xác nhận khách đóng phí hội viên.";
            return RedirectToAction("Index");
        }

        public ActionResult TuChoi(string id)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            var hoiVien = db.HOIVIEN.Find(id);

            if (hoiVien == null)
            {
                return HttpNotFound();
            }

            hoiVien.TRANGTHAIPHI = "Từ chối";
            hoiVien.MANV = Session["MANV"].ToString();

            db.SaveChanges();

            TempData["Success"] = "Đã từ chối yêu cầu hội viên.";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(string id)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            HOIVIEN hoiVien = db.HOIVIEN.Find(id);

            if (hoiVien == null)
            {
                return HttpNotFound();
            }

            return View(hoiVien);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var check = KiemTraQuyenQuanLy();
            if (check != null) return check;

            HOIVIEN hoiVien = db.HOIVIEN.Find(id);

            if (hoiVien != null)
            {
                db.HOIVIEN.Remove(hoiVien);
                db.SaveChanges();
                TempData["Success"] = "Xóa hội viên thành công.";
            }

            return RedirectToAction("Index");
        }

        private void KiemTraDuLieu(HOIVIEN hoiVien, bool laThemMoi)
        {
            if (string.IsNullOrWhiteSpace(hoiVien.MAHOIVIEN))
            {
                ModelState.AddModelError("MAHOIVIEN", "Vui lòng nhập mã hội viên.");
            }

            if (string.IsNullOrWhiteSpace(hoiVien.MAKH))
            {
                ModelState.AddModelError("MAKH", "Vui lòng chọn khách hàng.");
            }

            if (string.IsNullOrWhiteSpace(hoiVien.MANV))
            {
                ModelState.AddModelError("MANV", "Vui lòng chọn nhân viên duyệt.");
            }

            if (string.IsNullOrWhiteSpace(hoiVien.LOAITHE))
            {
                ModelState.AddModelError("LOAITHE", "Vui lòng chọn loại thẻ.");
            }

            if (hoiVien.NGAYKETTHUC <= hoiVien.NGAYBATDAU)
            {
                ModelState.AddModelError("NGAYKETTHUC", "Ngày kết thúc phải lớn hơn ngày bắt đầu.");
            }

            if (hoiVien.PHIDANGKY < 0)
            {
                ModelState.AddModelError("PHIDANGKY", "Phí đăng ký không được âm.");
            }

            if (laThemMoi && db.HOIVIEN.Any(h => h.MAHOIVIEN == hoiVien.MAHOIVIEN))
            {
                ModelState.AddModelError("MAHOIVIEN", "Mã hội viên đã tồn tại.");
            }
        }

        private void LoadDropdown(HOIVIEN hoiVien = null)
        {
            ViewBag.MANV = new SelectList(db.NHANVIEN.ToList(), "MANV", "HOTENNV", hoiVien != null ? hoiVien.MANV : null);
            ViewBag.MAKH = new SelectList(db.KHACHHANG.ToList(), "MAKH", "HOTENKH", hoiVien != null ? hoiVien.MAKH : null);

            ViewBag.LoaiTheList = new SelectList(new[]
            {
                "Thường",
                "VIP",
                "Luxury"
            }, hoiVien != null ? hoiVien.LOAITHE : null);

            ViewBag.TrangThaiPhiList = new SelectList(new[]
            {
                "Chờ duyệt",
                "Chưa thanh toán",
                "Đã thanh toán",
                "Đã duyệt",
                "Từ chối"
            }, hoiVien != null ? hoiVien.TRANGTHAIPHI : null);
        }

        private string TaoMaHoiVienMoi()
        {
            var maCuoi = db.HOIVIEN
                .OrderByDescending(h => h.MAHOIVIEN)
                .Select(h => h.MAHOIVIEN)
                .FirstOrDefault();

            int so = 1;

            if (!string.IsNullOrWhiteSpace(maCuoi) && maCuoi.Length >= 3)
            {
                int.TryParse(maCuoi.Substring(2), out so);
                so++;
            }

            return "HV" + so.ToString("00");
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