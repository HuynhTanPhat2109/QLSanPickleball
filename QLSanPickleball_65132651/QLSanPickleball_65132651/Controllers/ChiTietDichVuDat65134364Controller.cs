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

            var dsChiTietQuery = db.CHITIETDICHVUDAT
                .Include(c => c.DICHVU)
                .Include(c => c.PHIEUDATSAN)
                .Include(c => c.PHIEUDATSAN.KHACHHANG)
                .Include(c => c.PHIEUDATSAN.SAN)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                dsChiTietQuery = dsChiTietQuery.Where(c =>
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

                dsChiTietQuery = dsChiTietQuery.Where(c =>
                    c.PHIEUDATSAN.NGAYDAT >= ngayBatDau
                );
            }

            if (denNgay.HasValue)
            {
                DateTime ngayKetThuc = denNgay.Value.Date.AddDays(1);

                dsChiTietQuery = dsChiTietQuery.Where(c =>
                    c.PHIEUDATSAN.NGAYDAT < ngayKetThuc
                );
            }

            var dsChiTietRaw = dsChiTietQuery
                .OrderBy(c => c.PHIEUDATSAN.NGAYDAT)
                .ThenBy(c => c.PHIEUDATSAN.GIOBATDAU)
                .ThenBy(c => c.DICHVU.TENDV)
                .ToList();

            var dsNhom = dsChiTietRaw
                .GroupBy(c => new
                {
                    MaNhom = LayKhoaNhom(c.PHIEUDATSAN),
                    MaDV = c.MADV
                })
                .Select(g =>
                {
                    var itemDau = g
                        .OrderBy(x => x.PHIEUDATSAN.NGAYDAT)
                        .ThenBy(x => x.PHIEUDATSAN.GIOBATDAU)
                        .First();

                    string maNhom = g.Key.MaNhom;

                    var dsPhieuCungNhom = LayDanhSachPhieuCungNhom(maNhom, itemDau.MAPHIEUDAT);

                    string danhSachKhungGio = string.Join(" | ", dsPhieuCungNhom.Select(p =>
                        (p.SAN != null ? p.SAN.TENSAN : p.MASAN)
                        + ": "
                        + p.GIOBATDAU.ToString(@"hh\:mm")
                        + " - "
                        + p.GIOKETTHUC.ToString(@"hh\:mm")
                    ));

                    /*
                        Nếu dữ liệu cũ bị lưu trùng:
                        PDS01: Bóng x 2
                        PDS02: Bóng x 2
                        thì không được Sum = 4.
                        Dùng Max để hiểu đúng khách chọn 2 bóng cho cả lần đặt.
                    */
                    int soLuongDung = g.Max(x => x.SOLUONG);
                    decimal donGia = itemDau.DONGIA;
                    decimal thanhTienDung = soLuongDung * donGia;

                    return new ChiTietDichVuDatNhomDTO
                    {
                        MaNhomDatSan = maNhom,
                        MaPhieuDaiDien = itemDau.MAPHIEUDAT,

                        TenKhachHang = itemDau.PHIEUDATSAN != null && itemDau.PHIEUDATSAN.KHACHHANG != null
                            ? itemDau.PHIEUDATSAN.KHACHHANG.HOTENKH
                            : "Khách vãng lai",

                        SoDienThoai = itemDau.PHIEUDATSAN != null && itemDau.PHIEUDATSAN.KHACHHANG != null
                            ? itemDau.PHIEUDATSAN.KHACHHANG.SODIENTHOAIKH
                            : "",

                        DanhSachKhungGio = danhSachKhungGio,

                        MaDV = itemDau.MADV,

                        TenDV = itemDau.DICHVU != null
                            ? itemDau.DICHVU.TENDV
                            : itemDau.MADV,

                        DonViTinh = itemDau.DICHVU != null
                            ? itemDau.DICHVU.DONVITINH
                            : "",

                        SoLuong = soLuongDung,
                        DonGia = donGia,
                        ThanhTien = thanhTienDung,

                        NgayDat = itemDau.PHIEUDATSAN != null
                            ? itemDau.PHIEUDATSAN.NGAYDAT
                            : DateTime.MinValue,

                                                ThoiGianBatDau = itemDau.PHIEUDATSAN != null
                            ? itemDau.PHIEUDATSAN.NGAYDAT.Date + itemDau.PHIEUDATSAN.GIOBATDAU
                            : DateTime.MinValue
                    };
                })
                .OrderByDescending(x => x.ThoiGianBatDau)
                .ThenBy(x => x.TenKhachHang)
                .ThenBy(x => x.TenDV)
                .ToList();

            int tongSoDong = dsNhom.Count;
            int tongSoTrang = (int)Math.Ceiling((double)tongSoDong / pageSize);

            if (tongSoTrang == 0)
            {
                tongSoTrang = 1;
            }

            if (page > tongSoTrang)
            {
                page = tongSoTrang;
            }

            var ketQua = dsNhom
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
            ViewBag.TongSoLuong = dsNhom.Sum(c => c.SoLuong);
            ViewBag.TongTienDichVu = dsNhom.Sum(c => c.ThanhTien);

            ViewBag.SoPhieuCoDichVu = dsNhom
                .Select(c => c.MaNhomDatSan)
                .Distinct()
                .Count();

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