using QLSanPickleball_65132651.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace QLSanPickleball_65132651.Controllers
{
    public class Admin65134364Controller : Controller
    {
        private QLSanEntities db = new QLSanEntities();

        // GET: Admin65134364/HomeNv
        public ActionResult HomeNv()
        {
            // Kiểm tra đăng nhập
            if (Session["MANV"] == null)
            {
                return RedirectToAction("Login", "Account65132651");
            }

            // Kiểm tra quyền Admin / Quản lý / Nhân viên
            string vaiTro = Session["VAITRO"] != null ? Session["VAITRO"].ToString() : "";

            if (vaiTro != "Admin" && vaiTro != "Quản lý" && vaiTro != "Nhân viên")
            {
                return RedirectToAction("Index", "Home");
            }

            // ==============================
            // THỐNG KÊ TỔNG QUAN
            // ==============================
            ViewBag.TongNhanVien = db.NHANVIEN.Count();
            ViewBag.TongKhachHang = db.KHACHHANG.Count();
            ViewBag.TongSan = db.SAN.Count();
            ViewBag.TongDichVu = db.DICHVU.Count();
            ViewBag.TongHoiVien = db.HOIVIEN.Count();
            ViewBag.TongPhieuDat = db.PHIEUDATSAN.Count();
            ViewBag.TongHoaDon = db.HOADON.Count();

            // ==============================
            // TRẠNG THÁI SÂN
            // Chỉ hiển thị sân trống và bảo trì
            // ==============================
            ViewBag.SanTrong = db.SAN.Count(s => s.TRANGTHAISAN == "Trống");
            ViewBag.SanBaoTri = db.SAN.Count(s => s.TRANGTHAISAN == "Bảo trì");

            // ==============================
            // DOANH THU NGÀY / TUẦN / THÁNG / NĂM
            // ==============================
            DateTime homNay = DateTime.Today;
            DateTime ngayMai = homNay.AddDays(1);

            int soNgayLechDauTuan = ((int)homNay.DayOfWeek + 6) % 7; // Thứ 2 là đầu tuần
            DateTime dauTuan = homNay.AddDays(-soNgayLechDauTuan);
            DateTime dauTuanSau = dauTuan.AddDays(7);

            DateTime dauThang = new DateTime(homNay.Year, homNay.Month, 1);
            DateTime dauThangSau = dauThang.AddMonths(1);

            DateTime dauNam = new DateTime(homNay.Year, 1, 1);
            DateTime dauNamSau = dauNam.AddYears(1);

            ViewBag.DoanhThuHomNay = TinhDoanhThu(homNay, ngayMai);
            ViewBag.DoanhThuTuan = TinhDoanhThu(dauTuan, dauTuanSau);
            ViewBag.DoanhThuThang = TinhDoanhThu(dauThang, dauThangSau);
            ViewBag.DoanhThuNam = TinhDoanhThu(dauNam, dauNamSau);

            ViewBag.TongDoanhThu = db.HOADON.Any()
                ? db.HOADON.Sum(h => h.TONGTHANHTOAN)
                : 0m;

            // ==============================
            // BIỂU ĐỒ DOANH THU 7 NGÀY GẦN NHẤT
            // ==============================
            List<string> labels7Ngay = new List<string>();
            List<decimal> data7Ngay = new List<decimal>();

            for (int i = 6; i >= 0; i--)
            {
                DateTime ngay = homNay.AddDays(-i);
                DateTime ngayKeTiep = ngay.AddDays(1);

                labels7Ngay.Add(ngay.ToString("dd/MM"));
                data7Ngay.Add(TinhDoanhThu(ngay, ngayKeTiep));
            }

            // ==============================
            // BIỂU ĐỒ DOANH THU 12 THÁNG TRONG NĂM
            // ==============================
            List<string> labelsThang = new List<string>();
            List<decimal> dataThang = new List<decimal>();

            for (int thang = 1; thang <= 12; thang++)
            {
                DateTime batDau = new DateTime(homNay.Year, thang, 1);
                DateTime ketThuc = batDau.AddMonths(1);

                labelsThang.Add("T" + thang);
                dataThang.Add(TinhDoanhThu(batDau, ketThuc));
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            ViewBag.Labels7Ngay = serializer.Serialize(labels7Ngay);
            ViewBag.Data7Ngay = serializer.Serialize(data7Ngay);

            ViewBag.LabelsThang = serializer.Serialize(labelsThang);
            ViewBag.DataThang = serializer.Serialize(dataThang);

            // ==============================
            // PHIẾU ĐẶT SÂN MỚI NHẤT
            // ==============================
            var phieuDatMoi = db.PHIEUDATSAN
                .Include(p => p.KHACHHANG)
                .Include(p => p.SAN)
                .Include(p => p.NHANVIEN)
                .OrderByDescending(p => p.NGAYDAT)
                .ThenByDescending(p => p.GIOBATDAU)
                .Take(5)
                .ToList();

            return View(phieuDatMoi);
        }

        private decimal TinhDoanhThu(DateTime tuNgay, DateTime denNgay)
        {
            var query = db.HOADON
                .Where(h => h.NGAYLAP >= tuNgay && h.NGAYLAP < denNgay);

            if (query.Any())
            {
                return query.Sum(h => h.TONGTHANHTOAN);
            }

            return 0m;
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