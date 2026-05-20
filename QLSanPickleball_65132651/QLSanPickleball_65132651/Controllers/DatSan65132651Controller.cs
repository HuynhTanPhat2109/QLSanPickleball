using QLSanPickleball_65132651.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLSanPickleball_65132651.Controllers
{
    public class ChiTietDatSanDTO
    {
        public string maSan { get; set; }
        public string tenSan { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public decimal price { get; set; }
    }

    public class DatSan65132651Controller : Controller
    {
        private QLSanEntities db = new QLSanEntities();

        private readonly string[] TrangThaiHuy =
        {
            "DaHuy",
            "Da huy",
            "Đã hủy",
            "Đã huỷ",
            "Đã hủy",
            "DaHuy",
            "Huy",
            "Hủy"
        };

        public ActionResult ChonHinhThucDat()
        {
            return View();
        }

        // =========================================================
        // DỌN PHIẾU TẠM GIỮ QUÁ 10 PHÚT
        // =========================================================
        private void DonRacTamGiu()
        {
            var dsTamGiu = db.PHIEUDATSAN
                .Where(p => p.TRANGTHAIPHIEU == "Tam_Giu")
                .ToList();

            bool hasExpired = false;

            foreach (var p in dsTamGiu)
            {
                if (!string.IsNullOrEmpty(p.GHICHU)
                    && p.GHICHU.StartsWith("GRP")
                    && p.GHICHU.Length >= 17)
                {
                    string timeStr = p.GHICHU.Substring(3, 14);

                    DateTime thoiGianTao = DateTime.MinValue;

                    bool hopLe = DateTime.TryParseExact(
                        timeStr,
                        "yyyyMMddHHmmss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out thoiGianTao
                    );

                    if (hopLe && DateTime.Now > thoiGianTao.AddMinutes(10))
                    {
                        db.PHIEUDATSAN.Remove(p);
                        hasExpired = true;
                    }
                }
            }

            if (hasExpired)
            {
                db.SaveChanges();
            }
        }

        // =========================================================
        // KHÓA KHUNG GIỜ ĐÃ QUA
        // =========================================================
        private bool LaKhungGioQuaGio(DateTime ngayDat, TimeSpan gioBatDau)
        {
            if (ngayDat.Date < DateTime.Today)
            {
                return true;
            }

            if (ngayDat.Date == DateTime.Today && gioBatDau <= DateTime.Now.TimeOfDay)
            {
                return true;
            }

            return false;
        }

        // =========================================================
        // TÍNH GIÁ 30 PHÚT THEO BẢNG GIÁ + LOẠI SÂN
        // =========================================================
        private decimal TinhTien30Phut(string maLoaiSan, DateTime ngayDat, TimeSpan gioBatDau, bool laKhachVangLai)
        {
            DateTime ngayCuoi = ngayDat.Date.AddDays(1);

            var bangGia = db.BANGGIA
                .Where(g => g.MALOAISAN == maLoaiSan
                         && g.NGAYDIEUCHINH < ngayCuoi
                         && gioBatDau >= g.GIOBATDAU
                         && gioBatDau < g.GIOKETTHUC)
                .OrderByDescending(g => g.NGAYDIEUCHINH)
                .FirstOrDefault();

            if (bangGia == null)
            {
                return 0;
            }

            decimal giaMotGio;

            if (laKhachVangLai)
            {
                giaMotGio = bangGia.GIAVANGLAI;
            }
            else
            {
                giaMotGio = bangGia.GIACODINH;
            }

            return giaMotGio / 2;
        }

        // =========================================================
        // TẠO MÃ PHIẾU ĐẶT
        // =========================================================
        private string TaoMaPhieuDat()
        {
            return "PDS" + Guid.NewGuid()
                .ToString("N")
                .Substring(0, 7)
                .ToUpper();
        }

        // =========================================================
        // LẤY NHÂN VIÊN HỆ THỐNG
        // Nếu CSDL chưa có NV_ONL thì lấy nhân viên hoạt động đầu tiên
        // =========================================================
        private string LayNhanVienHeThong()
        {
            var nvOnline = db.NHANVIEN.FirstOrDefault(x => x.MANV == "NV_ONL");

            if (nvOnline != null)
            {
                return nvOnline.MANV;
            }

            var nv = db.NHANVIEN
                .OrderBy(x => x.MANV)
                .FirstOrDefault(x => x.TRANGTHAI == "Đang hoạt động");

            if (nv == null)
            {
                throw new Exception("Chưa có nhân viên đang hoạt động để tạo phiếu đặt sân.");
            }

            return nv.MANV;
        }

        // =========================================================
        // MÀN HÌNH BOOKING GRID
        // =========================================================
        public ActionResult Booking(DateTime? ngay)
        {
            DonRacTamGiu();

            DateTime ngayDat = ngay ?? DateTime.Today;

            if (ngayDat.Date < DateTime.Today)
            {
                ngayDat = DateTime.Today;
            }

            DateTime ngayBatDau = ngayDat.Date;
            DateTime ngayKetThuc = ngayBatDau.AddDays(1);

            var dsSan = db.SAN
                .Include(x => x.LOAISAN)
                .OrderBy(x => x.MALOAISAN)
                .ThenBy(x => x.TENSAN)
                .ToList();

            var dsPhieu = db.PHIEUDATSAN
                .Where(x => x.NGAYDAT >= ngayBatDau
                         && x.NGAYDAT < ngayKetThuc
                         && !TrangThaiHuy.Contains(x.TRANGTHAIPHIEU))
                .ToList();

            var dsBangGia = db.BANGGIA
                .Where(x => x.NGAYDIEUCHINH <= ngayKetThuc)
                .ToList();

            ViewBag.NgayDat = ngayDat;
            ViewBag.DanhSachPhieu = dsPhieu;
            ViewBag.DanhSachBangGia = dsBangGia;
            ViewBag.LaVangLai = Session["MaUser"] == null;

            return View(dsSan);
        }

        // =========================================================
        // AJAX: TẠO NHIỀU PHIẾU ĐẶT SÂN TẠM GIỮ
        // =========================================================
        [HttpPost]
        public JsonResult TaoNhieuPhieuDat(string ngayDat, List<ChiTietDatSanDTO> danhSachChon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ngayDat))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Ngày đặt không hợp lệ."
                    });
                }

                if (danhSachChon == null || !danhSachChon.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Bạn chưa chọn khung giờ nào."
                    });
                }

                DateTime ngay;

                bool ngayHopLe = DateTime.TryParseExact(
                    ngayDat,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out ngay
                );

                if (!ngayHopLe)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Định dạng ngày đặt không hợp lệ."
                    });
                }

                if (ngay.Date < DateTime.Today)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể đặt sân cho ngày đã qua."
                    });
                }

                DateTime ngayBatDau = ngay.Date;
                DateTime ngayKetThuc = ngayBatDau.AddDays(1);

                string maNhomDatSan = "GRP" + DateTime.Now.ToString("yyyyMMddHHmmss");
                string maKhachHang = null;

                if (Session["MaUser"] != null)
                {
                    maKhachHang = Session["MaUser"].ToString();
                }

                string maNhanVien = LayNhanVienHeThong();
                bool laKhachVangLai = Session["MaUser"] == null;

                foreach (var item in danhSachChon)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.maSan))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Dữ liệu sân không hợp lệ."
                        });
                    }

                    var san = db.SAN
                        .Include(s => s.LOAISAN)
                        .FirstOrDefault(s => s.MASAN == item.maSan);

                    if (san == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Không tìm thấy sân cần đặt."
                        });
                    }

                    if (san.TRANGTHAISAN == "Bảo trì")
                    {
                        return Json(new
                        {
                            success = false,
                            message = san.TENSAN + " đang bảo trì, không thể đặt."
                        });
                    }

                    // FIX LỖI ketThuc:
                    // Khởi tạo sẵn TimeSpan.Zero để tránh lỗi biến chưa được gán.
                    TimeSpan batDau = TimeSpan.Zero;
                    TimeSpan ketThuc = TimeSpan.Zero;

                    bool parseBatDau = TimeSpan.TryParseExact(
                        item.start,
                        @"hh\:mm",
                        CultureInfo.InvariantCulture,
                        out batDau
                    );

                    bool parseKetThuc = TimeSpan.TryParseExact(
                        item.end,
                        @"hh\:mm",
                        CultureInfo.InvariantCulture,
                        out ketThuc
                    );

                    if (!parseBatDau || !parseKetThuc)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Khung giờ không hợp lệ."
                        });
                    }

                    int soPhut = (int)(ketThuc - batDau).TotalMinutes;

                    if (soPhut <= 0 || soPhut % 30 != 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Mỗi khung đặt phải chia hết cho 30 phút."
                        });
                    }

                    if (batDau < TimeSpan.FromHours(5) || ketThuc > TimeSpan.FromHours(22))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Chỉ được đặt sân trong khung 05:00 - 22:00."
                        });
                    }

                    if (LaKhungGioQuaGio(ngay, batDau))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Khung giờ " + item.start + " - " + item.end + " đã qua, không thể đặt."
                        });
                    }

                    bool trungLich = db.PHIEUDATSAN.Any(x =>
                        x.MASAN == item.maSan
                        && x.NGAYDAT >= ngayBatDau
                        && x.NGAYDAT < ngayKetThuc
                        && !TrangThaiHuy.Contains(x.TRANGTHAIPHIEU)
                        && batDau < x.GIOKETTHUC
                        && ketThuc > x.GIOBATDAU
                    );

                    if (trungLich)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Rất tiếc! " + san.TENSAN + " lúc " + item.start + " - " + item.end + " vừa có người đặt."
                        });
                    }

                    decimal tongTien = 0;

                    TimeSpan gioTinhTien = batDau;

                    while (gioTinhTien < ketThuc)
                    {
                        decimal tien30Phut = TinhTien30Phut(
                            san.MALOAISAN,
                            ngay,
                            gioTinhTien,
                            laKhachVangLai
                        );

                        if (tien30Phut <= 0)
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Chưa có bảng giá cho " + san.TENSAN + " lúc " + gioTinhTien.ToString(@"hh\:mm") + "."
                            });
                        }

                        tongTien += tien30Phut;
                        gioTinhTien = gioTinhTien.Add(TimeSpan.FromMinutes(30));
                    }

                    PHIEUDATSAN phieu = new PHIEUDATSAN();

                    phieu.MAPHIEUDAT = TaoMaPhieuDat();
                    phieu.MAKH = maKhachHang;
                    phieu.MASAN = san.MASAN;
                    phieu.MANV = maNhanVien;
                    phieu.NGAYDAT = ngay;
                    phieu.GIOBATDAU = batDau;
                    phieu.GIOKETTHUC = ketThuc;
                    phieu.TONGTIENTAMTINH = tongTien;
                    phieu.TRANGTHAIPHIEU = "Tam_Giu";
                    phieu.GHICHU = maNhomDatSan;
                    phieu.TRANGTHAITHANHTOAN = "ChuaThanhToan";

                    db.PHIEUDATSAN.Add(phieu);
                }

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    maNhom = maNhomDatSan
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // =========================================================
        // TRANG THANH TOÁN / XÁC NHẬN ĐẶT SÂN
        // =========================================================
        public ActionResult ThanhToan(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Booking");
            }

            DonRacTamGiu();

            var danhSachPhieu = db.PHIEUDATSAN
                .Include(p => p.SAN)
                .Include(p => p.SAN.LOAISAN)
                .Where(p => p.GHICHU == id && p.TRANGTHAIPHIEU == "Tam_Giu")
                .ToList();

            if (danhSachPhieu == null || !danhSachPhieu.Any())
            {
                return Content("<h2 style='color:red; text-align:center; margin-top:50px;'>Đã hết hạn 10 phút giữ chỗ. Vui lòng quay lại đặt từ đầu!</h2>");
            }

            ViewBag.MaNhomDatSan = id;
            ViewBag.DanhSachPhieuDat = danhSachPhieu;
            ViewBag.NgayDat = danhSachPhieu.First().NGAYDAT;
            ViewBag.TongTienSan = danhSachPhieu.Sum(p => p.TONGTIENTAMTINH);

            ViewBag.DanhSachDichVu = db.DICHVU
                .Where(d => d.TRANGTHAIKD == "Đang kinh doanh")
                .OrderBy(d => d.TENDV)
                .ToList();

            return View();
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