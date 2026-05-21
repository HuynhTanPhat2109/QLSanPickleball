using QLSanPickleball_65132651.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace QLSanPickleball_65132651.Controllers
{
    public class ChiTietDichVuDat65134364Controller : Controller
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

        // GET: ChiTietDichVuDat65134364
        public ActionResult Index(
            string search,
            DateTime? tuNgay,
            DateTime? denNgay,
            int page = 1
        )
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }

            var dsChiTiet = db.CHITIETDICHVUDAT
                .Include(c => c.DICHVU)
                .Include(c => c.PHIEUDATSAN)
                .Include(c => c.PHIEUDATSAN.KHACHHANG)
                .Include(c => c.PHIEUDATSAN.SAN)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                dsChiTiet = dsChiTiet.Where(c =>
                    c.MAPHIEUDAT.Contains(search) ||
                    c.MADV.Contains(search) ||
                    c.DICHVU.TENDV.Contains(search) ||
                    c.PHIEUDATSAN.MASAN.Contains(search) ||
                    c.PHIEUDATSAN.SAN.TENSAN.Contains(search) ||
                    c.PHIEUDATSAN.KHACHHANG.HOTENKH.Contains(search) ||
                    c.PHIEUDATSAN.KHACHHANG.SODIENTHOAIKH.Contains(search)
                );
            }

            if (tuNgay.HasValue)
            {
                DateTime ngayBatDau = tuNgay.Value.Date;

                dsChiTiet = dsChiTiet.Where(c =>
                    c.PHIEUDATSAN.NGAYDAT >= ngayBatDau
                );
            }

            if (denNgay.HasValue)
            {
                DateTime ngayKetThuc = denNgay.Value.Date.AddDays(1);

                dsChiTiet = dsChiTiet.Where(c =>
                    c.PHIEUDATSAN.NGAYDAT < ngayKetThuc
                );
            }

            int tongSoDong = dsChiTiet.Count();
            int tongSoTrang = (int)Math.Ceiling((double)tongSoDong / pageSize);

            if (tongSoTrang == 0)
            {
                tongSoTrang = 1;
            }

            if (page > tongSoTrang)
            {
                page = tongSoTrang;
            }

            var ketQua = dsChiTiet
                .OrderByDescending(c => c.PHIEUDATSAN.NGAYDAT)
                .ThenByDescending(c => c.MAPHIEUDAT)
                .ThenBy(c => c.DICHVU.TENDV)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.TuNgay = tuNgay.HasValue ? tuNgay.Value.ToString("yyyy-MM-dd") : "";
            ViewBag.DenNgay = denNgay.HasValue ? denNgay.Value.ToString("yyyy-MM-dd") : "";

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = tongSoTrang;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = tongSoDong;

            ViewBag.TongSoDong = tongSoDong;

            ViewBag.TongSoLuong = dsChiTiet.Any()
                ? dsChiTiet.Sum(c => c.SOLUONG)
                : 0;

            ViewBag.TongTienDichVu = dsChiTiet.Any()
                ? dsChiTiet.Sum(c => c.THANHTIEN)
                : 0m;

            ViewBag.SoPhieuCoDichVu = dsChiTiet
                .Select(c => c.MAPHIEUDAT)
                .Distinct()
                .Count();

            return View(ketQua);
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