using QLSanPickleball_65132651.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity.Validation;
using System.IO;

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

    public class DichVuChonDTO
    {
        public string maDV { get; set; }
        public int soLuong { get; set; }
    }
    public class SanDaDatChiTietDTO
    {
        public string TenSan { get; set; }
        public string LoaiSan { get; set; }
        public DateTime NgayDat { get; set; }
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
        public decimal TienSan { get; set; }
    }

    public class SanDaDatNhomDTO
    {
        public string MaNhomDatSan { get; set; }

        public DateTime? NgayGioDat { get; set; }

        public string TenKhachHang { get; set; }
        public string SoDienThoai { get; set; }
        public string TrangThaiPhieu { get; set; }
        public string TrangThaiThanhToan { get; set; }
        public decimal TongTienSan { get; set; }
        public decimal TongTienDichVu { get; set; }
        public decimal TongThanhToan { get; set; }
        public List<SanDaDatChiTietDTO> ChiTiet { get; set; }
    }

    public class SanDaDatViewModel
    {
        public bool LaDangNhap { get; set; }
        public string SoDienThoaiTraCuu { get; set; }
        public List<SanDaDatNhomDTO> DanhSach { get; set; }
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
        // DỌN PHIẾU TẠM GIỮ QUÁ 5 PHÚT
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

                    if (hopLe && DateTime.Now > thoiGianTao.AddMinutes(5))
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

        private bool NhomDatSanDaQuaGio(List<PHIEUDATSAN> dsPhieu)
        {
            if (dsPhieu == null || !dsPhieu.Any())
            {
                return true;
            }

            foreach (var p in dsPhieu)
            {
                if (p.NGAYDAT.Date < DateTime.Today)
                {
                    return true;
                }

                if (p.NGAYDAT.Date == DateTime.Today &&
                    p.GIOBATDAU <= DateTime.Now.TimeOfDay)
                {
                    return true;
                }
            }

            return false;
        }

        private DateTime? LayThoiGianTaoTuMaNhom(string maNhomDatSan)
        {
            if (string.IsNullOrWhiteSpace(maNhomDatSan)
                || !maNhomDatSan.StartsWith("GRP")
                || maNhomDatSan.Length < 17)
            {
                return null;
            }

            string timeStr = maNhomDatSan.Substring(3, 14);

            DateTime thoiGianTao;

            bool hopLe = DateTime.TryParseExact(
                timeStr,
                "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out thoiGianTao
            );

            if (!hopLe)
            {
                return null;
            }

            return thoiGianTao;
        }

        private bool NhomTamGiuDaHetHan(string maNhomDatSan, int soPhut)
        {
            DateTime? thoiGianTao = LayThoiGianTaoTuMaNhom(maNhomDatSan);

            if (thoiGianTao == null)
            {
                return true;
            }

            return DateTime.Now > thoiGianTao.Value.AddMinutes(soPhut);
        }

        private void XoaGiuChoTheoNhom(string maNhomDatSan, List<PHIEUDATSAN> dsPhieu = null)
        {
            if (string.IsNullOrWhiteSpace(maNhomDatSan))
            {
                return;
            }

            if (dsPhieu == null)
            {
                dsPhieu = db.PHIEUDATSAN
                    .Where(p => p.GHICHU == maNhomDatSan
                             && (p.TRANGTHAIPHIEU == "Tam_Giu"
                                 || p.TRANGTHAIPHIEU == "Cho_Thanh_Toan"))
                    .ToList();
            }

            if (dsPhieu.Any())
            {
                db.PHIEUDATSAN.RemoveRange(dsPhieu);
                db.SaveChanges();
            }

            Session.Remove("DichVuChon_" + maNhomDatSan);
            Session.Remove("ThanhToanHetHan_" + maNhomDatSan);
        }

        private void XoaTamGiuTheoNhom(string maNhomDatSan, List<PHIEUDATSAN> dsPhieu = null)
        {
            XoaGiuChoTheoNhom(maNhomDatSan, dsPhieu);
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

        [HttpPost]
        public JsonResult HuyTamGiu(string maNhomDatSan)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maNhomDatSan))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Mã giữ chỗ không hợp lệ."
                    });
                }

                XoaTamGiuTheoNhom(maNhomDatSan);

                return Json(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                string loi = ex.Message;

                if (ex.InnerException != null)
                {
                    loi += " | Inner: " + ex.InnerException.Message;

                    if (ex.InnerException.InnerException != null)
                    {
                        loi += " | SQL: " + ex.InnerException.InnerException.Message;
                    }
                }

                return Json(new
                {
                    success = false,
                    message = loi
                });
            }
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
                .Where(x => x.TRANGTHAI == "Đang hoạt động")
                .OrderBy(x => x.MANV)
                .FirstOrDefault();

            if (nv == null)
            {
                nv = db.NHANVIEN
                    .OrderBy(x => x.MANV)
                    .FirstOrDefault();
            }

            if (nv == null)
            {
                throw new Exception("Chưa có nhân viên để tạo phiếu đặt sân.");
            }

            return nv.MANV;
        }

        private string LayMaKhachHangDangNhap()
        {
            if (Session["MaKH"] != null)
            {
                string maKH = Session["MaKH"].ToString();

                bool tonTai = db.KHACHHANG.Any(k => k.MAKH == maKH);

                if (tonTai)
                {
                    return maKH;
                }
            }

            if (Session["MaUser"] != null)
            {
                string maUser = Session["MaUser"].ToString();

                bool tonTai = db.KHACHHANG.Any(k => k.MAKH == maUser);

                if (tonTai)
                {
                    return maUser;
                }
            }

            return null;
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
            ViewBag.LaVangLai = string.IsNullOrEmpty(LayMaKhachHangDangNhap());

            return View(dsSan);
        }

        [HttpGet]
        public JsonResult LayTrangThaiLich(string ngay)
        {
            try
            {
                DonRacTamGiu();

                DateTime ngayDat;

                bool ngayHopLe = DateTime.TryParseExact(
                    ngay,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out ngayDat
                );

                if (!ngayHopLe)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Ngày không hợp lệ."
                    }, JsonRequestBehavior.AllowGet);
                }

                DateTime ngayBatDau = ngayDat.Date;
                DateTime ngayKetThuc = ngayBatDau.AddDays(1);

                var dsPhieu = db.PHIEUDATSAN
                    .Where(p => p.NGAYDAT >= ngayBatDau
                             && p.NGAYDAT < ngayKetThuc
                             && !TrangThaiHuy.Contains(p.TRANGTHAIPHIEU))
                    .Select(p => new
                    {
                        maSan = p.MASAN,
                        start = p.GIOBATDAU,
                        end = p.GIOKETTHUC
                    })
                    .ToList()
                    .Select(p => new
                    {
                        maSan = p.maSan,
                        start = p.start.ToString(@"hh\:mm"),
                        end = p.end.ToString(@"hh\:mm")
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = dsPhieu
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
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

                string maKhachHang = LayMaKhachHangDangNhap();

                bool laKhachVangLai = string.IsNullOrEmpty(maKhachHang);

                string maNhanVienHeThong = LayNhanVienHeThong();

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
                    phieu.MANV = maNhanVienHeThong;
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
                string loi = ex.Message;

                if (ex.InnerException != null)
                {
                    loi += " | Inner: " + ex.InnerException.Message;

                    if (ex.InnerException.InnerException != null)
                    {
                        loi += " | SQL: " + ex.InnerException.InnerException.Message;
                    }
                }

                return Json(new
                {
                    success = false,
                    message = loi
                });
            }
        }

        public ActionResult XacNhanDatSan(string id)
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
                return Content("<h2 style='color:red; text-align:center; margin-top:50px;'>Đã hết hạn 5 phút giữ chỗ. Vui lòng quay lại đặt từ đầu!</h2>");
            }

            if (NhomDatSanDaQuaGio(danhSachPhieu))
            {
                XoaTamGiuTheoNhom(id, danhSachPhieu);

                return Content("<h2 style='color:red; text-align:center; margin-top:50px;'>Khung giờ đã bắt đầu hoặc đã qua. Vui lòng quay lại đặt khung giờ khác!</h2>");
            }

            ViewBag.MaNhomDatSan = id;
            ViewBag.DanhSachPhieuDat = danhSachPhieu;
            ViewBag.NgayDat = danhSachPhieu.First().NGAYDAT;
            ViewBag.TongTienSan = danhSachPhieu.Sum(p => p.TONGTIENTAMTINH);
            ViewBag.LaVangLai = string.IsNullOrEmpty(LayMaKhachHangDangNhap());

            ViewBag.DanhSachDichVu = db.DICHVU
                .Where(d => d.TRANGTHAIKD == "Đang kinh doanh")
                .OrderBy(d => d.TENDV)
                .ToList();

            return View();
        }

        [HttpPost]
        public JsonResult LuuXacNhanDatSan(
            string maNhomDatSan,
            string tenKhach,
            string sdtKhach,
            List<DichVuChonDTO> dichVuChon
        )
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maNhomDatSan))
                {
                    return Json(new { success = false, message = "Mã giữ chỗ không hợp lệ." });
                }

                var dsPhieu = db.PHIEUDATSAN
                    .Where(p => p.GHICHU == maNhomDatSan && p.TRANGTHAIPHIEU == "Tam_Giu")
                    .ToList();

                if (dsPhieu == null || !dsPhieu.Any())
                {
                    return Json(new { success = false, message = "Phiếu giữ chỗ không tồn tại hoặc đã hết hạn." });
                }

                if (NhomTamGiuDaHetHan(maNhomDatSan, 5))
                {
                    XoaGiuChoTheoNhom(maNhomDatSan, dsPhieu);

                    return Json(new
                    {
                        success = false,
                        message = "Đã hết 5 phút giữ chỗ. Vui lòng quay lại đặt sân từ đầu."
                    });
                }

                if (NhomDatSanDaQuaGio(dsPhieu))
                {
                    XoaTamGiuTheoNhom(maNhomDatSan, dsPhieu);

                    return Json(new
                    {
                        success = false,
                        message = "Khung giờ đã bắt đầu hoặc đã qua. Vui lòng quay lại chọn khung giờ khác."
                    });
                }

                string maKhachDangNhap = LayMaKhachHangDangNhap();
                bool laVangLai = string.IsNullOrEmpty(maKhachDangNhap);

                string maKhachHangGanVaoPhieu = "";

                if (laVangLai)
                {
                    if (string.IsNullOrWhiteSpace(tenKhach))
                    {
                        return Json(new { success = false, message = "Vui lòng nhập họ tên khách hàng." });
                    }

                    if (string.IsNullOrWhiteSpace(sdtKhach))
                    {
                        return Json(new { success = false, message = "Vui lòng nhập số điện thoại khách hàng." });
                    }

                    sdtKhach = sdtKhach.Trim();
                    tenKhach = tenKhach.Trim();

                    if (!System.Text.RegularExpressions.Regex.IsMatch(sdtKhach, @"^[0-9]{10}$"))
                    {
                        return Json(new { success = false, message = "Số điện thoại phải gồm đúng 10 chữ số." });
                    }

                    var khTonTai = db.KHACHHANG.FirstOrDefault(k => k.SODIENTHOAIKH == sdtKhach);

                    if (khTonTai != null)
                    {
                        maKhachHangGanVaoPhieu = khTonTai.MAKH;

                        khTonTai.HOTENKH = tenKhach;

                        if (string.IsNullOrWhiteSpace(khTonTai.MATKHAUKH))
                        {
                            khTonTai.MATKHAUKH = "Kvl@123456";
                        }

                        if (string.IsNullOrWhiteSpace(khTonTai.TRANGTHAITK))
                        {
                            khTonTai.TRANGTHAITK = "Hoạt động";
                        }
                    }
                    else
                    {
                        maKhachHangGanVaoPhieu = "KV" + DateTime.Now.ToString("HHmmssff");

                        KHACHHANG khMoi = new KHACHHANG
                        {
                            MAKH = maKhachHangGanVaoPhieu,
                            HOTENKH = tenKhach,
                            NGAYSINH = DateTime.Today,
                            GIOITINH = "Khác",
                            DIACHI = "",
                            SODIENTHOAIKH = sdtKhach,
                            EMAILKH = maKhachHangGanVaoPhieu.ToLower() + "@khachvanglai.com",
                            MATKHAUKH = "Kvl@123456",
                            SOLANBUNG = 0,
                            TRANGTHAITK = "Hoạt động"
                        };

                        db.KHACHHANG.Add(khMoi);
                    }
                }
                else
                {
                    maKhachHangGanVaoPhieu = maKhachDangNhap;
                }

                // GÁN KHÁCH HÀNG VÀO TOÀN BỘ PHIẾU TRONG NHÓM
                foreach (var p in dsPhieu)
                {
                    p.MAKH = maKhachHangGanVaoPhieu;
                }

                db.SaveChanges();

                foreach (var p in dsPhieu)
                {
                    p.TRANGTHAIPHIEU = "Cho_Thanh_Toan";
                    p.TRANGTHAITHANHTOAN = "Cho_Chuyen_Khoan";
                }

                db.SaveChanges();

                Session["DichVuChon_" + maNhomDatSan] = dichVuChon ?? new List<DichVuChonDTO>();
                Session["ThanhToanHetHan_" + maNhomDatSan] = DateTime.Now.AddMinutes(6);

                return Json(new { success = true });
            }
            catch (DbEntityValidationException ex)
            {
                string loi = "";

                foreach (var eve in ex.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        loi += "Trường " + ve.PropertyName + ": " + ve.ErrorMessage + " | ";
                    }
                }

                return Json(new { success = false, message = loi });
            }
            catch (Exception ex)
            {
                string loi = ex.Message;

                if (ex.InnerException != null)
                {
                    loi += " | Inner: " + ex.InnerException.Message;

                    if (ex.InnerException.InnerException != null)
                    {
                        loi += " | SQL: " + ex.InnerException.InnerException.Message;
                    }
                }

                return Json(new { success = false, message = loi });
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
                .Where(p => p.GHICHU == id && p.TRANGTHAIPHIEU == "Cho_Thanh_Toan")
                .ToList();

            if (danhSachPhieu == null || !danhSachPhieu.Any())
            {
                return Content("<h2 style='color:red; text-align:center; margin-top:50px;'>Đã hết hạn giữ chỗ. Vui lòng quay lại đặt từ đầu!</h2>");
            }

            string keyHetHanThanhToan = "ThanhToanHetHan_" + id;

            if (Session[keyHetHanThanhToan] == null)
            {
                XoaGiuChoTheoNhom(id, danhSachPhieu);

                return Content("<h2 style='color:red; text-align:center; margin-top:50px;'>Phiên thanh toán đã hết hạn. Vui lòng quay lại đặt sân!</h2>");
            }

            DateTime thoiGianHetHanThanhToan = (DateTime)Session[keyHetHanThanhToan];

            if (DateTime.Now > thoiGianHetHanThanhToan)
            {
                XoaGiuChoTheoNhom(id, danhSachPhieu);

                return Content("<h2 style='color:red; text-align:center; margin-top:50px;'>Đã hết 6 phút thanh toán. Vui lòng quay lại đặt sân!</h2>");
            }

            if (NhomDatSanDaQuaGio(danhSachPhieu))
            {
                XoaTamGiuTheoNhom(id, danhSachPhieu);

                return Content("<h2 style='color:red; text-align:center; margin-top:50px;'>Khung giờ đã bắt đầu hoặc đã qua. Vui lòng quay lại đặt khung giờ khác!</h2>");
            }

            var dichVuChon = Session["DichVuChon_" + id] as List<DichVuChonDTO>;

            if (dichVuChon == null)
            {
                dichVuChon = new List<DichVuChonDTO>();
            }

            decimal tienSan = danhSachPhieu.Sum(p => p.TONGTIENTAMTINH);
            decimal tienDichVu = 0;

            var chiTietDichVu = new List<dynamic>();

            foreach (var item in dichVuChon)
            {
                var dv = db.DICHVU.FirstOrDefault(x => x.MADV == item.maDV);

                if (dv != null && item.soLuong > 0)
                {
                    decimal thanhTien = dv.DONGIA * item.soLuong;
                    tienDichVu += thanhTien;

                    chiTietDichVu.Add(new
                    {
                        MADV = dv.MADV,
                        TENDV = dv.TENDV,
                        DONVITINH = dv.DONVITINH,
                        DONGIA = dv.DONGIA,
                        SOLUONG = item.soLuong,
                        THANHTIEN = thanhTien
                    });
                }
            }

            string maKhachHang = danhSachPhieu
            .Where(p => !string.IsNullOrWhiteSpace(p.MAKH))
            .Select(p => p.MAKH)
            .FirstOrDefault();

            int phanTramGiamGia = TinhPhanTramGiamGiaHoiVien(maKhachHang);

            decimal tienGiamGia = tienSan * phanTramGiamGia / 100m;

            decimal tongThanhToan = tienSan + tienDichVu - tienGiamGia;

            if (tongThanhToan < 0)
            {
                tongThanhToan = 0;
            }

            ViewBag.PhanTramGiamGia = phanTramGiamGia;
            ViewBag.TienGiamGia = tienGiamGia;

            ViewBag.MaNhomDatSan = id;
            ViewBag.DanhSachPhieuDat = danhSachPhieu
                .OrderBy(p => p.GIOBATDAU)
                .ThenBy(p => p.SAN == null ? p.MASAN : p.SAN.TENSAN)
                .ToList();
            ViewBag.ThoiGianHetHanThanhToan = thoiGianHetHanThanhToan.ToString("o");
            ViewBag.NgayDat = danhSachPhieu.First().NGAYDAT;
            ViewBag.TienSan = tienSan;
            ViewBag.TienDichVu = tienDichVu;
            ViewBag.TongThanhToan = tongThanhToan;
            ViewBag.ChiTietDichVu = chiTietDichVu;

            ViewBag.TenChuTaiKhoan = "ARMY PICKLEBALL";
            ViewBag.SoTaiKhoan = "0358990541";
            ViewBag.MaNganHang = "MB";
            ViewBag.NoiDungCK = "DAT SAN " + id;

            TempData.Remove("Success");
            return View();
        }

        private int TinhPhanTramGiamGiaHoiVien(string maKhachHang)
        {
            if (string.IsNullOrWhiteSpace(maKhachHang))
            {
                return 0;
            }

            DateTime homNay = DateTime.Today;

            var hoiVien = db.HOIVIEN
                .Where(h => h.MAKH == maKhachHang
                    && h.NGAYBATDAU <= homNay
                    && h.NGAYKETTHUC >= homNay)
                .OrderByDescending(h => h.NGAYBATDAU)
                .FirstOrDefault();

            if (hoiVien == null)
            {
                return 0;
            }

            // Kiểm tra trạng thái phí hội viên
            // Chỉ giảm giá khi hội viên đã đóng phí / đã thanh toán phí
            if (!string.IsNullOrWhiteSpace(hoiVien.TRANGTHAIPHI))
            {
                string trangThaiPhi = hoiVien.TRANGTHAIPHI.Trim().ToLower();

                bool daDongPhi =
                    trangThaiPhi.Contains("đã") ||
                    trangThaiPhi.Contains("da") ||
                    trangThaiPhi.Contains("đã đóng") ||
                    trangThaiPhi.Contains("da dong") ||
                    trangThaiPhi.Contains("đã thanh toán") ||
                    trangThaiPhi.Contains("Đã thanh toán") ||
                    trangThaiPhi.Contains("hoàn tất") ||
                    trangThaiPhi.Contains("hoan tat");

                if (!daDongPhi)
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }

            string loaiThe = "";

            if (!string.IsNullOrWhiteSpace(hoiVien.LOAITHE))
            {
                loaiThe = hoiVien.LOAITHE.Trim().ToLower();
            }

            if (loaiThe.Contains("luxury"))
            {
                return 40;
            }

            if (loaiThe.Contains("vip"))
            {
                return 30;
            }

            if (loaiThe.Contains("thường") ||
                loaiThe.Contains("thuong") ||
                loaiThe.Contains("hội viên") ||
                loaiThe.Contains("hoi vien"))
            {
                return 20;
            }

            return 0;
        }

        [HttpPost]
        public ActionResult GuiMinhChungThanhToan(string maNhomDatSan, HttpPostedFileBase anhMinhChung)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maNhomDatSan))
                {
                    TempData["Error"] = "Mã giữ chỗ không hợp lệ.";
                    return RedirectToAction("Booking");
                }

                var dsPhieu = db.PHIEUDATSAN
                    .Where(p => p.GHICHU == maNhomDatSan && p.TRANGTHAIPHIEU == "Cho_Thanh_Toan")
                    .ToList();

                if (dsPhieu == null || !dsPhieu.Any())
                {
                    TempData["Error"] = "Phiếu đặt sân không tồn tại hoặc đã hết hạn.";
                    return RedirectToAction("Booking");
                }

                string keyHetHanThanhToan = "ThanhToanHetHan_" + maNhomDatSan;

                if (Session[keyHetHanThanhToan] != null)
                {
                    DateTime thoiGianHetHanThanhToan = (DateTime)Session[keyHetHanThanhToan];

                    if (DateTime.Now > thoiGianHetHanThanhToan)
                    {
                        XoaGiuChoTheoNhom(maNhomDatSan, dsPhieu);

                        TempData["Error"] = "Đã hết 6 phút thanh toán. Vui lòng đặt sân lại.";
                        return RedirectToAction("Booking");
                    }
                }
                else
                {
                    XoaGiuChoTheoNhom(maNhomDatSan, dsPhieu);

                    TempData["Error"] = "Phiên thanh toán đã hết hạn. Vui lòng đặt sân lại.";
                    return RedirectToAction("Booking");
                }

                if (NhomDatSanDaQuaGio(dsPhieu))
                {
                    XoaTamGiuTheoNhom(maNhomDatSan, dsPhieu);

                    TempData["Error"] = "Khung giờ đã bắt đầu hoặc đã qua. Vui lòng đặt lại khung giờ khác.";
                    return RedirectToAction("Booking");
                }

                if (anhMinhChung == null || anhMinhChung.ContentLength <= 0)
                {
                    TempData["Error"] = "Vui lòng tải ảnh minh chứng thanh toán.";
                    return RedirectToAction("ThanhToan", new { id = maNhomDatSan });
                }

                string ext = Path.GetExtension(anhMinhChung.FileName).ToLower();

                string[] allowExt = { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowExt.Contains(ext))
                {
                    TempData["Error"] = "Chỉ cho phép tải ảnh JPG, PNG hoặc WEBP.";
                    return RedirectToAction("ThanhToan", new { id = maNhomDatSan });
                }

                string folder = Server.MapPath("~/Content/Uploads/MinhChungThanhToan/");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string fileName = maNhomDatSan + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ext;
                string fullPath = Path.Combine(folder, fileName);

                anhMinhChung.SaveAs(fullPath);

                string relativePath = "/Content/Uploads/MinhChungThanhToan/" + fileName;

                foreach (var p in dsPhieu)
                {
                    p.TRANGTHAIPHIEU = "Chờ xác nhận";
                    p.TRANGTHAITHANHTOAN = "Chờ xác nhận chuyển khoản";

                    if (string.IsNullOrWhiteSpace(p.GHICHU))
                    {
                        p.GHICHU = maNhomDatSan + " | Minh chứng: " + relativePath;
                    }
                    else
                    {
                        p.GHICHU = p.GHICHU + " | Minh chứng: " + relativePath;
                    }
                }

                // QUAN TRỌNG:
                // Lưu dịch vụ khách đã chọn từ Session vào bảng CHITIETDICHVUDAT
                // trước khi xóa Session.
                LuuDichVuKhachChonVaoChiTiet(maNhomDatSan, dsPhieu);

                db.SaveChanges();

                Session.Remove("ThanhToanHetHan_" + maNhomDatSan);
                Session.Remove("DichVuChon_" + maNhomDatSan);

                TempData["Success"] = "Đã gửi minh chứng thanh toán. Nhân viên sẽ kiểm tra và xác nhận lịch đặt.";
                return RedirectToAction("ThanhToanThanhCong", new { id = maNhomDatSan });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi khi gửi minh chứng: " + ex.Message;
                return RedirectToAction("ThanhToan", new { id = maNhomDatSan });
            }
        }

        // =========================================================
        // LƯU DỊCH VỤ KHÁCH CHỌN VÀO CHITIETDICHVUDAT
        // =========================================================
        private void LuuDichVuKhachChonVaoChiTiet(string maNhomDatSan, List<PHIEUDATSAN> dsPhieu)
        {
            var dichVuChon = Session["DichVuChon_" + maNhomDatSan] as List<DichVuChonDTO>;

            if (dichVuChon == null || !dichVuChon.Any())
            {
                return;
            }

            if (dsPhieu == null || !dsPhieu.Any())
            {
                return;
            }

            // Chỉ lấy 1 phiếu đại diện đầu tiên trong nhóm đặt sân.
            // Không lưu dịch vụ lặp cho từng khung giờ.
            var phieuDaiDien = dsPhieu
                .OrderBy(p => p.NGAYDAT)
                .ThenBy(p => p.GIOBATDAU)
                .FirstOrDefault();

            if (phieuDaiDien == null)
            {
                return;
            }

            foreach (var item in dichVuChon)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.maDV) || item.soLuong <= 0)
                {
                    continue;
                }

                var dichVu = db.DICHVU.FirstOrDefault(d => d.MADV == item.maDV);

                if (dichVu == null)
                {
                    continue;
                }

                var chiTietCu = db.CHITIETDICHVUDAT
                    .FirstOrDefault(c => c.MAPHIEUDAT == phieuDaiDien.MAPHIEUDAT
                                      && c.MADV == dichVu.MADV);

                if (chiTietCu != null)
                {
                    chiTietCu.SOLUONG = item.soLuong;
                    chiTietCu.DONGIA = dichVu.DONGIA;
                    chiTietCu.THANHTIEN = item.soLuong * dichVu.DONGIA;
                }
                else
                {
                    CHITIETDICHVUDAT chiTietMoi = new CHITIETDICHVUDAT
                    {
                        MAPHIEUDAT = phieuDaiDien.MAPHIEUDAT,
                        MADV = dichVu.MADV,
                        SOLUONG = item.soLuong,
                        DONGIA = dichVu.DONGIA,
                        THANHTIEN = item.soLuong * dichVu.DONGIA
                    };

                    db.CHITIETDICHVUDAT.Add(chiTietMoi);
                }
            }
        }

        public ActionResult ThanhToanThanhCong(string id)
        {
            ViewBag.MaNhomDatSan = id;
            return View();
        }

        private string TachMaNhomTuGhiChu(string ghiChu)
        {
            if (string.IsNullOrWhiteSpace(ghiChu))
            {
                return "";
            }

            return ghiChu.Split('|')[0].Trim();
        }

        public ActionResult SanDaDat(string sdt)
        {
            DonRacTamGiu();

            string maKhachDangNhap = LayMaKhachHangDangNhap();
            bool laDangNhap = !string.IsNullOrEmpty(maKhachDangNhap);

            SanDaDatViewModel model = new SanDaDatViewModel
            {
                LaDangNhap = laDangNhap,
                SoDienThoaiTraCuu = sdt,
                DanhSach = new List<SanDaDatNhomDTO>()
            };

            List<string> dsMaKH = new List<string>();

            if (laDangNhap)
            {
                dsMaKH.Add(maKhachDangNhap);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(sdt))
                {
                    return View(model);
                }

                sdt = sdt.Trim();

                if (!System.Text.RegularExpressions.Regex.IsMatch(sdt, @"^[0-9]{10}$"))
                {
                    ViewBag.Error = "Số điện thoại phải gồm đúng 10 chữ số.";
                    return View(model);
                }

                dsMaKH = db.KHACHHANG
                    .Where(k => k.SODIENTHOAIKH == sdt)
                    .Select(k => k.MAKH)
                    .ToList();

                if (!dsMaKH.Any())
                {
                    ViewBag.Error = "Không tìm thấy lịch đặt nào theo số điện thoại này.";
                    return View(model);
                }
            }

            var dsPhieu = db.PHIEUDATSAN
                .Include(p => p.SAN)
                .Include(p => p.SAN.LOAISAN)
                .Include(p => p.KHACHHANG)
                .Where(p => p.MAKH != null
                         && dsMaKH.Contains(p.MAKH)
                         && p.TRANGTHAIPHIEU != "Tam_Giu"
                         && p.TRANGTHAIPHIEU != "Cho_Thanh_Toan")
                .OrderByDescending(p => p.NGAYDAT)
                .ThenBy(p => p.GIOBATDAU)
                .ToList();

            var nhomPhieu = dsPhieu
                .GroupBy(p => TachMaNhomTuGhiChu(p.GHICHU))
                .ToList();

            foreach (var nhom in nhomPhieu)
            {
                var dsTrongNhom = nhom.ToList();
                var phieuDau = dsTrongNhom.FirstOrDefault();

                if (phieuDau == null)
                {
                    continue;
                }

                var dsMaPhieu = dsTrongNhom.Select(p => p.MAPHIEUDAT).ToList();

                decimal tongTienDichVu = db.CHITIETDICHVUDAT
                    .Where(c => dsMaPhieu.Contains(c.MAPHIEUDAT))
                    .Select(c => (decimal?)c.THANHTIEN)
                    .Sum() ?? 0;

                decimal tongTienSan = dsTrongNhom.Sum(p => p.TONGTIENTAMTINH);

                SanDaDatNhomDTO item = new SanDaDatNhomDTO
                {
                    MaNhomDatSan = nhom.Key,
                    NgayGioDat = LayThoiGianTaoTuMaNhom(nhom.Key),
                    TenKhachHang = phieuDau.KHACHHANG == null ? "" : phieuDau.KHACHHANG.HOTENKH,
                    SoDienThoai = phieuDau.KHACHHANG == null ? "" : phieuDau.KHACHHANG.SODIENTHOAIKH,
                    TrangThaiPhieu = phieuDau.TRANGTHAIPHIEU,
                    TrangThaiThanhToan = phieuDau.TRANGTHAITHANHTOAN,
                    TongTienSan = tongTienSan,
                    TongTienDichVu = tongTienDichVu,
                    TongThanhToan = tongTienSan + tongTienDichVu,
                    ChiTiet = dsTrongNhom
                        .OrderBy(p => p.NGAYDAT)
                        .ThenBy(p => p.GIOBATDAU)
                        .ThenBy(p => p.SAN == null ? p.MASAN : p.SAN.TENSAN)
                        .Select(p => new SanDaDatChiTietDTO
                        {
                            TenSan = p.SAN == null ? p.MASAN : p.SAN.TENSAN,
                            LoaiSan = p.SAN == null || p.SAN.LOAISAN == null
                                ? "Chưa phân loại"
                                : p.SAN.LOAISAN.TENLOAISAN,
                            NgayDat = p.NGAYDAT,
                            GioBatDau = p.GIOBATDAU,
                            GioKetThuc = p.GIOKETTHUC,
                            TienSan = p.TONGTIENTAMTINH
                        })
                        .ToList()
                };

                model.DanhSach.Add(item);
            }

            model.DanhSach = model.DanhSach
                .OrderByDescending(x => x.ChiTiet.FirstOrDefault() == null ? DateTime.MinValue : x.ChiTiet.First().NgayDat)
                .ToList();

            return View(model);
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