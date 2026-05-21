using QLSanPickleball_65132651.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace QLSanPickleball_65132651.Controllers
{
    public class HoaDonNV65134364Controller : Controller
    {
        private QLSanEntities db = new QLSanEntities();

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

        // GET: HoaDonNV65134364
        public ActionResult Index(string search, DateTime? tuNgay, DateTime? denNgay, int page = 1)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }

            var dsHoaDon = db.HOADON
                .Include(h => h.PHIEUDATSAN)
                .Include(h => h.PHIEUDATSAN.KHACHHANG)
                .Include(h => h.PHIEUDATSAN.SAN)
                .Include(h => h.NHANVIEN)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                dsHoaDon = dsHoaDon.Where(h =>
                    h.SOHD.Contains(search) ||
                    h.MAPHIEUDAT.Contains(search) ||
                    h.HINHTHUCTT.Contains(search) ||
                    h.PHIEUDATSAN.MASAN.Contains(search) ||
                    h.PHIEUDATSAN.SAN.TENSAN.Contains(search) ||
                    h.PHIEUDATSAN.KHACHHANG.HOTENKH.Contains(search) ||
                    h.PHIEUDATSAN.KHACHHANG.SODIENTHOAIKH.Contains(search) ||
                    h.NHANVIEN.HOTENNV.Contains(search)
                );
            }

            if (tuNgay.HasValue)
            {
                DateTime ngayBatDau = tuNgay.Value.Date;
                dsHoaDon = dsHoaDon.Where(h => h.NGAYLAP >= ngayBatDau);
            }

            if (denNgay.HasValue)
            {
                DateTime ngayKetThuc = denNgay.Value.Date.AddDays(1);
                dsHoaDon = dsHoaDon.Where(h => h.NGAYLAP < ngayKetThuc);
            }

            int tongSoHoaDon = dsHoaDon.Count();
            int tongSoTrang = (int)Math.Ceiling((double)tongSoHoaDon / pageSize);

            if (tongSoTrang == 0)
            {
                tongSoTrang = 1;
            }

            if (page > tongSoTrang)
            {
                page = tongSoTrang;
            }

            var ketQua = dsHoaDon
                .OrderByDescending(h => h.NGAYLAP)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.TuNgay = tuNgay.HasValue ? tuNgay.Value.ToString("yyyy-MM-dd") : "";
            ViewBag.DenNgay = denNgay.HasValue ? denNgay.Value.ToString("yyyy-MM-dd") : "";

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = tongSoTrang;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = tongSoHoaDon;

            ViewBag.TongDoanhThu = dsHoaDon.Any() ? dsHoaDon.Sum(h => h.TONGTHANHTOAN) : 0m;

            return View(ketQua);
        }

        // GET: HoaDonNV65134364/Details/HD01
        public ActionResult Details(string id)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var hoaDon = db.HOADON
                .Include(h => h.NHANVIEN)
                .Include(h => h.PHIEUDATSAN)
                .Include(h => h.PHIEUDATSAN.KHACHHANG)
                .Include(h => h.PHIEUDATSAN.SAN)
                .Include(h => h.PHIEUDATSAN.SAN.LOAISAN)
                .FirstOrDefault(h => h.SOHD == id);

            if (hoaDon == null)
            {
                return HttpNotFound();
            }

            ViewBag.DanhSachDichVu = db.CHITIETDICHVUDAT
                .Include(c => c.DICHVU)
                .Where(c => c.MAPHIEUDAT == hoaDon.MAPHIEUDAT)
                .ToList();

            return View(hoaDon);
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