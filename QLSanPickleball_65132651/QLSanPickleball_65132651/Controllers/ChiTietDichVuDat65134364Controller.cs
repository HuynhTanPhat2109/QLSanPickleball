using QLSanPickleball_65132651.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace QLSanPickleball_65132651.Controllers
{
    public class ChiTietDichVuDatNhomDTO
    {
        public string MaNhomDatSan { get; set; }
        public string MaPhieuDaiDien { get; set; }

        public string TenKhachHang { get; set; }
        public string SoDienThoai { get; set; }

        public string DanhSachKhungGio { get; set; }

        public string MaDV { get; set; }
        public string TenDV { get; set; }
        public string DonViTinh { get; set; }

        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }

        public DateTime NgayDat { get; set; }
        public DateTime ThoiGianBatDau { get; set; }

    }

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

        public ActionResult Index(string search, DateTime? ngayDat, int page = 1)
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
                    c.PHIEUDATSAN.KHACHHANG.HOTENKH.Contains(search) ||
                    c.PHIEUDATSAN.KHACHHANG.SODIENTHOAIKH.Contains(search) ||
                    c.PHIEUDATSAN.SAN.TENSAN.Contains(search)
                );
            }

            if (ngayDat.HasValue)
            {
                DateTime ngay = ngayDat.Value.Date;
                DateTime ngayMai = ngay.AddDays(1);

                dsChiTiet = dsChiTiet.Where(c =>
                    c.PHIEUDATSAN.NGAYDAT >= ngay &&
                    c.PHIEUDATSAN.NGAYDAT < ngayMai
                );
            }

            ViewBag.TongTienDichVu = dsChiTiet.Any()
                ? dsChiTiet.Sum(c => c.THANHTIEN)
                : 0m;

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
                .ThenByDescending(c => c.PHIEUDATSAN.GIOBATDAU)
                .ThenBy(c => c.PHIEUDATSAN.MASAN)
                .ThenBy(c => c.DICHVU.TENDV)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.NgayDat = ngayDat.HasValue ? ngayDat.Value.ToString("yyyy-MM-dd") : "";

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = tongSoTrang;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = tongSoDong;

            return View(ketQua);
        }

        private string LayKhoaNhom(PHIEUDATSAN phieu)
        {
            if (phieu == null)
            {
                return "";
            }

            string maNhom = LayMaNhomTuGhiChu(phieu.GHICHU);

            if (!string.IsNullOrWhiteSpace(maNhom))
            {
                return maNhom;
            }

            return phieu.MAPHIEUDAT;
        }

        private string LayMaNhomTuGhiChu(string ghiChu)
        {
            if (string.IsNullOrWhiteSpace(ghiChu))
            {
                return "";
            }

            int viTri = ghiChu.IndexOf("GRP");

            if (viTri < 0)
            {
                return "";
            }

            string chuoiTuGRP = ghiChu.Substring(viTri);

            int viTriKhoangTrang = chuoiTuGRP.IndexOf(" ");
            if (viTriKhoangTrang > 0)
            {
                chuoiTuGRP = chuoiTuGRP.Substring(0, viTriKhoangTrang);
            }

            int viTriGach = chuoiTuGRP.IndexOf("|");
            if (viTriGach > 0)
            {
                chuoiTuGRP = chuoiTuGRP.Substring(0, viTriGach);
            }

            return chuoiTuGRP.Trim();
        }

        private List<PHIEUDATSAN> LayDanhSachPhieuCungNhom(string maNhom, string maPhieuDaiDien)
        {
            if (!string.IsNullOrWhiteSpace(maNhom) && maNhom.StartsWith("GRP"))
            {
                var ds = db.PHIEUDATSAN
                    .Include(p => p.SAN)
                    .Where(p => p.GHICHU != null && p.GHICHU.Contains(maNhom))
                    .OrderBy(p => p.NGAYDAT)
                    .ThenBy(p => p.GIOBATDAU)
                    .ToList();

                if (ds.Any())
                {
                    return ds;
                }
            }

            return db.PHIEUDATSAN
                .Include(p => p.SAN)
                .Where(p => p.MAPHIEUDAT == maPhieuDaiDien)
                .ToList();
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