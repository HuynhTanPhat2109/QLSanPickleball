using QLSanPickleball_65132651.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace QLSanPickleball_65132651.Controllers
{
    // Class phụ trợ để nhận dữ liệu JSON từ Frontend gửi lên
    public class ChiTietDatSanDTO
    {
        public string maSan { get; set; }
        public string tenSan { get; set; }
        public string start { get; set; } // Giờ bắt đầu (vd: "06:00")
        public string end { get; set; }   // Giờ kết thúc (vd: "07:30")
        public decimal price { get; set; } // Tổng giá của block này
    }
    public class DatSan65132651Controller : Controller
    {
        private QLSanEntities db = new QLSanEntities();

        // Trang hiển thị danh sách sân để khách đặt
        public ActionResult ChonHinhThucDat()
        {
            return View();
        }
        // =========================================
        // BOOKING GRID
        // =========================================

        public ActionResult Booking(DateTime? ngay)
        {
            // ----------------------------------------------------------------
            // 1. DỌN RÁC: XÓA CÁC PHIẾU "TẠM GIỮ" ĐÃ QUÁ 10 PHÚT
            // ----------------------------------------------------------------
            var dsTamGiu = db.PHIEUDATSAN.Where(p => p.TRANGTHAIPHIEU == "Tam_Giu").ToList();
            bool hasExpired = false;

            foreach (var p in dsTamGiu)
            {
                // Trích xuất thời gian từ GHICHU (vd: GRP20260520145029)
                if (!string.IsNullOrEmpty(p.GHICHU) && p.GHICHU.StartsWith("GRP") && p.GHICHU.Length >= 17)
                {
                    string timeStr = p.GHICHU.Substring(3, 14); // Lấy chuỗi yyyyMMddHHmmss
                    DateTime thoiGianTao;
                    if (DateTime.TryParseExact(timeStr, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out thoiGianTao))
                    {
                        // Nếu thời gian hiện tại > thời gian tạo + 10 phút -> Xóa
                        if (DateTime.Now > thoiGianTao.AddMinutes(10))
                        {
                            db.PHIEUDATSAN.Remove(p);
                            hasExpired = true;
                        }
                    }
                }
            }

            if (hasExpired)
            {
                db.SaveChanges(); // Cập nhật lại CSDL để giải phóng sân
            }

            // ----------------------------------------------------------------
            // 2. LẤY DỮ LIỆU ĐỔ RA GIAO DIỆN
            // ----------------------------------------------------------------
            DateTime ngayDat = ngay ?? DateTime.Today;

            var dsSan = db.SAN
                          .Include(x => x.LOAISAN)
                          .Where(x => x.TRANGTHAISAN != "Bảo trì")
                          .ToList();

            var dsPhieu = db.PHIEUDATSAN
                            .Where(x => DbFunctions.TruncateTime(x.NGAYDAT) == ngayDat.Date
                                     && x.TRANGTHAIPHIEU != "DaHuy")
                            .ToList();

            ViewBag.NgayDat = ngayDat;
            ViewBag.DanhSachPhieu = dsPhieu;

            return View(dsSan);
        }

        // =========================================
        // API: TẠO NHIỀU PHIẾU ĐẶT SÂN CÙNG LÚC (AJAX)
        // =========================================
        [HttpPost]
        public JsonResult TaoNhieuPhieuDat(string ngayDat, List<ChiTietDatSanDTO> danhSachChon, string tenKhach, string sdtKhach)
        {
            try
            {
                if (danhSachChon == null || !danhSachChon.Any())
                {
                    return Json(new { success = false, message = "Không có khung giờ nào được chọn!" });
                }

                DateTime ngay = DateTime.Parse(ngayDat);
                string maNhomDatSan = "GRP" + DateTime.Now.ToString("yyyyMMddHHmmss");
                string maKhachHang = "";

                // --- XỬ LÝ KHÁCH HÀNG ---
                if (Session["MaUser"] != null)
                {
                    maKhachHang = Session["MaUser"].ToString();
                }
                else
                {
                    // TỰ ĐỘNG SINH MÃ KHÁCH VÃNG LAI
                    maKhachHang = "KVL" + DateTime.Now.ToString("ddHHmmss");

                    KHACHHANG khMoi = new KHACHHANG
                    {
                        MAKH = maKhachHang,
                        HOTENKH = tenKhach, // Lấy từ tham số truyền vào
                        SODIENTHOAIKH = sdtKhach, // Lấy từ tham số truyền vào
                        NGAYSINH = DateTime.Today,
                        GIOITINH = "Chưa rõ",
                        EMAILKH = "khachvanglai@gmail.com", // Có thể để mặc định
                        MATKHAUKH = "123", // Mật khẩu mặc định cho KVL
                        SOLANBUNG = 0,
                        TRANGTHAITK = "Active"
                    };

                    db.KHACHHANG.Add(khMoi);
                    // Lưu ngay để database tạo mã khách hàng này trước khi gán vào phiếu đặt
                    db.SaveChanges();
                }

                // --- TẠO PHIẾU ĐẶT SÂN ---
                foreach (var item in danhSachChon)
                {
                    TimeSpan batDau = TimeSpan.Parse(item.start);
                    TimeSpan ketThuc = TimeSpan.Parse(item.end);

                    // Kiểm tra trùng lịch
                    bool trungLich = db.PHIEUDATSAN.Any(x =>
                        x.MASAN == item.maSan
                        && DbFunctions.TruncateTime(x.NGAYDAT) == ngay.Date
                        && x.TRANGTHAIPHIEU != "DaHuy"
                        && (batDau < x.GIOKETTHUC && ketThuc > x.GIOBATDAU)
                    );

                    if (trungLich)
                    {
                        return Json(new { success = false, message = $"Rất tiếc! Sân {item.tenSan} lúc {item.start}-{item.end} vừa có người đặt." });
                    }

                    PHIEUDATSAN phieu = new PHIEUDATSAN();
                    phieu.MAPHIEUDAT = "PDS" + Guid.NewGuid().ToString().Substring(0, 7).ToUpper();
                    phieu.MASAN = item.maSan;
                    phieu.NGAYDAT = ngay;
                    phieu.GIOBATDAU = batDau;
                    phieu.GIOKETTHUC = ketThuc;
                    phieu.TONGTIENTAMTINH = item.price;
                    phieu.TRANGTHAIPHIEU = "Tam_Giu";
                    phieu.TRANGTHAITHANHTOAN = "ChuaThanhToan";
                    phieu.GHICHU = maNhomDatSan;
                    phieu.MANV = "NV_ONL"; // Nhân viên hệ thống
                    phieu.MAKH = maKhachHang; // Mã khách vừa lấy từ Session hoặc vừa tạo mới

                    db.PHIEUDATSAN.Add(phieu);
                }

                db.SaveChanges();

                return Json(new { success = true, maNhom = maNhomDatSan });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // =========================================
        // TRANG THANH TOÁN & CHỌN DỊCH VỤ THÊM
        // =========================================
        public ActionResult ThanhToan(string id)
        {
            // id chính là maNhom (ví dụ: GRP20260520...)
            var danhSachPhieu = db.PHIEUDATSAN
                                  .Include(p => p.SAN)
                                  .Where(p => p.GHICHU == id && p.TRANGTHAIPHIEU == "Tam_Giu")
                                  .ToList();

            if (danhSachPhieu == null || !danhSachPhieu.Any())
            {
                // Nếu load trang mà không thấy phiếu, nghĩa là đã quá 10 phút và bị hệ thống tự quét xóa
                return Content("<h2 style='color:red; text-align:center; margin-top:50px;'>Đã hết hạn 10 phút giữ chỗ. Vui lòng quay lại đặt từ đầu!</h2>");
            }

            ViewBag.DanhSachPhieuDat = danhSachPhieu;
            ViewBag.NgayDat = danhSachPhieu.First().NGAYDAT;
            ViewBag.TongTienSan = danhSachPhieu.Sum(p => p.TONGTIENTAMTINH);
            ViewBag.DanhSachDichVu = db.DICHVU.Where(d => d.TRANGTHAIKD == "Đang kinh doanh").ToList();

            return View();
        }
    }
}