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

        // =========================================================
        // HÀM LẤY VAI TRÒ
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

        private bool LaNhanVien()
        {
            return LayVaiTro() == "Nhân viên";
        }

        private bool DuocQuanTri()
        {
            return LaAdmin() || LaQuanLy();
        }

        private bool DuocVaoHeThongNhanVien()
        {
            return LaAdmin() || LaQuanLy() || LaNhanVien();
        }

        private string TenVaiTroHienThi()
        {
            if (LaAdmin())
            {
                return "Admin";
            }

            if (LaQuanLy())
            {
                return "Quản lý";
            }

            if (LaNhanVien())
            {
                return "Nhân viên";
            }

            return "Chưa phân quyền";
        }

        private ActionResult KiemTraDangNhapVaVaiTro()
        {
            if (Session["MANV"] == null)
            {
                return RedirectToAction("Login", "Account65132651");
            }

            if (!DuocVaoHeThongNhanVien())
            {
                return RedirectToAction("Index", "Home");
            }

            return null;
        }

        // =========================================================
        // DASHBOARD ADMIN / QUẢN LÝ / NHÂN VIÊN
        // =========================================================
        // GET: Admin65134364/HomeNv
        public ActionResult HomeNv()
        {
            var check = KiemTraDangNhapVaVaiTro();

            if (check != null)
            {
                return check;
            }

            string vaiTro = LayVaiTro();

            ViewBag.VaiTro = vaiTro;
            ViewBag.TenVaiTroHienThi = TenVaiTroHienThi();

            ViewBag.LaAdmin = LaAdmin();
            ViewBag.LaQuanLy = LaQuanLy();
            ViewBag.LaNhanVien = LaNhanVien();

            ViewBag.DuocQuanTri = DuocQuanTri();
            ViewBag.DuocNghiepVuNhanVien = DuocVaoHeThongNhanVien();

            // =====================================================
            // THỐNG KÊ TỔNG QUAN
            // Admin / Quản lý dùng đầy đủ
            // Nhân viên vẫn có dữ liệu để dashboard không lỗi
            // =====================================================
            ViewBag.TongNhanVien = db.NHANVIEN.Count();
            ViewBag.TongKhachHang = db.KHACHHANG.Count();
            ViewBag.TongSan = db.SAN.Count();
            ViewBag.TongDichVu = db.DICHVU.Count();
            ViewBag.TongHoiVien = db.HOIVIEN.Count();
            ViewBag.TongPhieuDat = db.PHIEUDATSAN.Count();
            ViewBag.TongHoaDon = db.HOADON.Count();

            // =====================================================
            // TRẠNG THÁI SÂN
            // Hệ mới chỉ dùng Hoạt động / Bảo trì
            // Có cộng thêm trạng thái cũ để tránh dữ liệu cũ bị lệch
            // =====================================================
            ViewBag.SanHoatDong = db.SAN.Count(s =>
                s.TRANGTHAISAN == "Hoạt động" ||
                s.TRANGTHAISAN == "Trống" ||
                s.TRANGTHAISAN == "Đang đặt" ||
                s.TRANGTHAISAN == "Đang sử dụng"
            );

            ViewBag.SanBaoTri = db.SAN.Count(s => s.TRANGTHAISAN == "Bảo trì");

            // Giữ lại tên cũ nếu View HomeNv của b còn dùng
            ViewBag.SanTrong = ViewBag.SanHoatDong;
            ViewBag.SanDangDat = 0;
            ViewBag.SanDangSuDung = 0;

            // =====================================================
            // THỐNG KÊ PHIẾU THEO TRẠNG THÁI
            // =====================================================
            ViewBag.PhieuChoXacNhan = db.PHIEUDATSAN.Count(p =>
                p.TRANGTHAIPHIEU == "Chờ xác nhận" ||
                p.TRANGTHAIPHIEU == "Chờ duyệt" ||
                p.TRANGTHAITHANHTOAN == "Chờ xác nhận chuyển khoản"
            );

            ViewBag.PhieuDaXacNhan = db.PHIEUDATSAN.Count(p =>
                p.TRANGTHAIPHIEU == "Đã xác nhận" ||
                p.TRANGTHAIPHIEU == "Đang sử dụng"
            );

            ViewBag.PhieuHoanThanh = db.PHIEUDATSAN.Count(p =>
                p.TRANGTHAIPHIEU == "Hoàn thành"
            );

            ViewBag.PhieuDaHuy = db.PHIEUDATSAN.Count(p =>
                p.TRANGTHAIPHIEU == "Đã hủy" ||
                p.TRANGTHAIPHIEU == "Đã huỷ" ||
                p.TRANGTHAIPHIEU == "Hủy" ||
                p.TRANGTHAIPHIEU == "Huy" ||
                p.TRANGTHAIPHIEU == "DaHuy" ||
                p.TRANGTHAIPHIEU == "Hủy do bảo trì" ||
                p.TRANGTHAITHANHTOAN == "Hoàn cọc 100%" ||
                p.TRANGTHAITHANHTOAN == "Hoàn cọc 50%" ||
                p.TRANGTHAITHANHTOAN == "Hoàn cọc 20%" ||
                p.TRANGTHAITHANHTOAN == "Không hoàn cọc"
            );

            // =====================================================
            // DOANH THU NGÀY / TUẦN / THÁNG / NĂM
            // =====================================================
            DateTime homNay = DateTime.Today;
            DateTime ngayMai = homNay.AddDays(1);

            int soNgayLechDauTuan = ((int)homNay.DayOfWeek + 6) % 7;
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

            // =====================================================
            // BIỂU ĐỒ DOANH THU 7 NGÀY GẦN NHẤT
            // =====================================================
            List<string> labels7Ngay = new List<string>();
            List<decimal> data7Ngay = new List<decimal>();

            for (int i = 6; i >= 0; i--)
            {
                DateTime ngay = homNay.AddDays(-i);
                DateTime ngayKeTiep = ngay.AddDays(1);

                labels7Ngay.Add(ngay.ToString("dd/MM"));
                data7Ngay.Add(TinhDoanhThu(ngay, ngayKeTiep));
            }

            // =====================================================
            // BIỂU ĐỒ DOANH THU 12 THÁNG
            // =====================================================
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

            // =====================================================
            // BIỂU ĐỒ TRÒN TRẠNG THÁI PHIẾU
            // Nếu view HomeNv có dùng chart tròn thì lấy dữ liệu này
            // =====================================================
            List<string> labelsTrangThaiPhieu = new List<string>
            {
                "Chờ xác nhận",
                "Đã xác nhận",
                "Hoàn thành",
                "Đã hủy"
            };

            List<int> dataTrangThaiPhieu = new List<int>
            {
                ViewBag.PhieuChoXacNhan,
                ViewBag.PhieuDaXacNhan,
                ViewBag.PhieuHoanThanh,
                ViewBag.PhieuDaHuy
            };

            ViewBag.LabelsTrangThaiPhieu = serializer.Serialize(labelsTrangThaiPhieu);
            ViewBag.DataTrangThaiPhieu = serializer.Serialize(dataTrangThaiPhieu);

            // =====================================================
            // PHIẾU ĐẶT SÂN MỚI / CẦN XỬ LÝ
            // Sắp theo nghiệp vụ: chờ xác nhận trước, rồi mới tới các phiếu khác
            // =====================================================
            var phieuDatMoi = db.PHIEUDATSAN
                .Include(p => p.KHACHHANG)
                .Include(p => p.SAN)
                .Include(p => p.NHANVIEN)
                .OrderBy(p =>
                    p.TRANGTHAIPHIEU == "Chờ xác nhận" ||
                    p.TRANGTHAIPHIEU == "Chờ duyệt" ||
                    p.TRANGTHAITHANHTOAN == "Chờ xác nhận chuyển khoản" ? 1 :

                    p.TRANGTHAIPHIEU == "Đã xác nhận" ||
                    p.TRANGTHAIPHIEU == "Đang sử dụng" ? 2 :

                    p.TRANGTHAIPHIEU == "Hoàn thành" ? 3 :

                    p.TRANGTHAIPHIEU == "Đã hủy" ||
                    p.TRANGTHAIPHIEU == "Đã huỷ" ||
                    p.TRANGTHAIPHIEU == "Hủy" ||
                    p.TRANGTHAIPHIEU == "Huy" ||
                    p.TRANGTHAIPHIEU == "DaHuy" ||
                    p.TRANGTHAIPHIEU == "Hủy do bảo trì" ? 4 :

                    5
                )
                .ThenByDescending(p => p.NGAYDAT)
                .ThenBy(p => p.GIOBATDAU)
                .Take(5)
                .ToList();

            return View(phieuDatMoi);
        }

        // =========================================================
        // HÀM TÍNH DOANH THU
        // =========================================================
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