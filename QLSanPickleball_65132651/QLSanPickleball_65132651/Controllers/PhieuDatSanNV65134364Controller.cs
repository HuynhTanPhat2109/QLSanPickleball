using QLSanPickleball_65132651.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;

namespace QLSanPickleball_65132651.Controllers
{
    public class PhieuDatSanNV65134364Controller : Controller
    {
        private QLSanEntities db = new QLSanEntities();

        private readonly string[] TrangThaiHuy =
        {
            "DaHuy",
            "Da huy",
            "Đã hủy",
            "Đã huỷ",
            "Huy",
            "Hủy",
            "Khách không đến",
            "Hủy do bảo trì"
        };

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


        // GET: PhieuDatSanNV65134364
        public ActionResult Index(
            string search,
            string trangThaiPhieu,
            string trangThaiThanhToan,
            DateTime? ngayDat,
            int page = 1
        )
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            CapNhatTrangThaiSanTheoLichHomNay();

            int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }

            var dsPhieu = db.PHIEUDATSAN
                .Include(p => p.KHACHHANG)
                .Include(p => p.SAN)
                .Include(p => p.NHANVIEN)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                dsPhieu = dsPhieu.Where(p =>
                    p.MAPHIEUDAT.Contains(search) ||
                    p.MASAN.Contains(search) ||
                    (p.GHICHU != null && p.GHICHU.Contains(search)) ||
                    (p.MAKH != null && p.MAKH.Contains(search)) ||
                    (p.SAN != null && p.SAN.TENSAN.Contains(search)) ||
                    (p.KHACHHANG != null && p.KHACHHANG.HOTENKH.Contains(search)) ||
                    (p.KHACHHANG != null && p.KHACHHANG.SODIENTHOAIKH.Contains(search))
                );
            }

            if (!string.IsNullOrWhiteSpace(trangThaiPhieu))
            {
                dsPhieu = dsPhieu.Where(p => p.TRANGTHAIPHIEU == trangThaiPhieu);
            }

            if (!string.IsNullOrWhiteSpace(trangThaiThanhToan))
            {
                dsPhieu = dsPhieu.Where(p => p.TRANGTHAITHANHTOAN == trangThaiThanhToan);
            }

            if (ngayDat.HasValue)
            {
                DateTime ngay = ngayDat.Value.Date;
                DateTime ngayMai = ngay.AddDays(1);

                dsPhieu = dsPhieu.Where(p => p.NGAYDAT >= ngay && p.NGAYDAT < ngayMai);
            }

            int tongSoPhieu = dsPhieu.Count();
            int tongSoTrang = (int)Math.Ceiling((double)tongSoPhieu / pageSize);

            if (tongSoTrang == 0)
            {
                tongSoTrang = 1;
            }

            if (page > tongSoTrang)
            {
                page = tongSoTrang;
            }

            var ketQua = dsPhieu
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
                    p.TRANGTHAIPHIEU == "Khách không đến" ||
                    p.TRANGTHAIPHIEU == "Hủy do bảo trì" ? 4 :

                    5
                )
                .ThenByDescending(p => p.NGAYDAT)
                .ThenBy(p => p.GIOBATDAU)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dsMaPhieu = ketQua.Select(p => p.MAPHIEUDAT).ToList();

            var tienDichVuTheoPhieu = db.CHITIETDICHVUDAT
                .Where(c => dsMaPhieu.Contains(c.MAPHIEUDAT))
                .GroupBy(c => c.MAPHIEUDAT)
                .Select(g => new
                {
                    MAPHIEUDAT = g.Key,
                    TongTienDichVu = g.Sum(x => x.THANHTIEN)
                })
                .ToList()
                .ToDictionary(x => x.MAPHIEUDAT, x => x.TongTienDichVu);

            ViewBag.TienDichVuTheoPhieu = tienDichVuTheoPhieu;

            ViewBag.Search = search;
            ViewBag.TrangThaiPhieu = trangThaiPhieu;
            ViewBag.TrangThaiThanhToan = trangThaiThanhToan;
            ViewBag.NgayDat = ngayDat.HasValue ? ngayDat.Value.ToString("yyyy-MM-dd") : "";

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = tongSoTrang;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = tongSoPhieu;

            return View(ketQua);
        }

        // GET: PhieuDatSanNV65134364/Details/P01
        public ActionResult Details(string id)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var phieu = db.PHIEUDATSAN
                .Include(p => p.KHACHHANG)
                .Include(p => p.SAN)
                .Include(p => p.SAN.LOAISAN)
                .Include(p => p.NHANVIEN)
                .FirstOrDefault(p => p.MAPHIEUDAT == id);

            if (phieu == null)
            {
                return HttpNotFound();
            }

            string maNhom = LayMaNhomTuGhiChu(phieu.GHICHU);

            var dsPhieuCungNhom = db.PHIEUDATSAN
                .Include(p => p.KHACHHANG)
                .Include(p => p.SAN)
                .Include(p => p.SAN.LOAISAN)
                .Include(p => p.NHANVIEN)
                .Where(p =>
                    p.MAPHIEUDAT == id ||
                    (!string.IsNullOrEmpty(maNhom) && p.GHICHU != null && p.GHICHU.Contains(maNhom))
                )
                .OrderBy(p => p.MASAN)
                .ThenBy(p => p.GIOBATDAU)
                .ToList();

            if (dsPhieuCungNhom == null || !dsPhieuCungNhom.Any())
            {
                dsPhieuCungNhom = new System.Collections.Generic.List<PHIEUDATSAN>();
                dsPhieuCungNhom.Add(phieu);
            }

            var dsMaPhieu = dsPhieuCungNhom
                .Select(p => p.MAPHIEUDAT)
                .ToList();

            var danhSachDichVu = db.CHITIETDICHVUDAT
                .Include(c => c.DICHVU)
                .Where(c => dsMaPhieu.Contains(c.MAPHIEUDAT))
                .ToList();

            decimal tongTienSan = dsPhieuCungNhom.Sum(p => p.TONGTIENTAMTINH);

            decimal tongTienDichVu = danhSachDichVu.Any()
                ? danhSachDichVu.Sum(c => c.THANHTIEN)
                : 0m;

            TimeSpan gioBatDauNhom = dsPhieuCungNhom.Min(p => p.GIOBATDAU);
            TimeSpan gioKetThucNhom = dsPhieuCungNhom.Max(p => p.GIOKETTHUC);

            ViewBag.MaNhomDatSan = maNhom;
            ViewBag.DanhSachPhieuCungNhom = dsPhieuCungNhom;
            ViewBag.DanhSachDichVu = danhSachDichVu;

            ViewBag.TongTienSan = tongTienSan;
            ViewBag.TongTienDichVu = tongTienDichVu;
            ViewBag.TongTamTinh = tongTienSan + tongTienDichVu;

            ViewBag.KhungGioNhom = gioBatDauNhom.ToString(@"hh\:mm") + " - " + gioKetThucNhom.ToString(@"hh\:mm");

            ViewBag.DaCoHoaDon = db.HOADON.Any(h => dsMaPhieu.Contains(h.MAPHIEUDAT));

            return View(phieu);
        }

        // GET: PhieuDatSanNV65134364/XacNhanThanhToan/P01
        public ActionResult XacNhanThanhToan(string id)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var phieuGoc = db.PHIEUDATSAN.Find(id);

            if (phieuGoc == null)
            {
                return HttpNotFound();
            }

            bool duocXacNhan =
                phieuGoc.TRANGTHAIPHIEU == "Chờ xác nhận" ||
                phieuGoc.TRANGTHAIPHIEU == "Chờ duyệt" ||
                phieuGoc.TRANGTHAITHANHTOAN == "Chờ xác nhận chuyển khoản";

            if (!duocXacNhan)
            {
                TempData["Error"] = "Phiếu này chưa ở trạng thái chờ xác nhận thanh toán.";
                return RedirectToAction("Index");
            }

            string maNhanVien = Session["MANV"].ToString();

            var dsPhieuCungNhom = LayDanhSachPhieuCungNhom(phieuGoc);

            if (dsPhieuCungNhom == null || !dsPhieuCungNhom.Any())
            {
                TempData["Error"] = "Không tìm thấy danh sách phiếu cùng nhóm đặt sân.";
                return RedirectToAction("Index");
            }

            foreach (var phieu in dsPhieuCungNhom)
            {
                bool phieuDaHuy =
                    phieu.TRANGTHAIPHIEU == "Đã hủy" ||
                    phieu.TRANGTHAIPHIEU == "Đã huỷ" ||
                    phieu.TRANGTHAIPHIEU == "Hủy" ||
                    phieu.TRANGTHAIPHIEU == "Huy" ||
                    phieu.TRANGTHAIPHIEU == "DaHuy" ||
                    phieu.TRANGTHAITHANHTOAN == "Đã hủy";

                if (phieuDaHuy)
                {
                    continue;
                }

                phieu.MANV = maNhanVien;
                phieu.TRANGTHAIPHIEU = "Đã xác nhận";
                phieu.TRANGTHAITHANHTOAN = "Đã đặt cọc";

                var san = db.SAN.Find(phieu.MASAN);

                if (san != null && san.TRANGTHAISAN != "Bảo trì")
                {
                    san.TRANGTHAISAN = "Đang đặt";
                }
            }

            db.SaveChanges();

            try
            {
                foreach (var phieu in dsPhieuCungNhom)
                {
                    var phieuGuiMail = db.PHIEUDATSAN
                        .Include(p => p.KHACHHANG)
                        .Include(p => p.SAN)
                        .Include(p => p.SAN.LOAISAN)
                        .Include(p => p.NHANVIEN)
                        .FirstOrDefault(p => p.MAPHIEUDAT == phieu.MAPHIEUDAT);

                    if (phieuGuiMail != null &&
                        phieuGuiMail.KHACHHANG != null &&
                        !string.IsNullOrWhiteSpace(phieuGuiMail.KHACHHANG.EMAILKH))
                    {
                        string subject = "ARMY PICKLEBALL - Xác nhận đặt sân " + phieuGuiMail.MAPHIEUDAT;
                        string body = TaoNoiDungEmailPhieuDat(phieuGuiMail);

                        GuiEmail(phieuGuiMail.KHACHHANG.EMAILKH, subject, body);
                    }
                }

                TempData["Success"] = "Đã xác nhận thanh toán, giữ sân và gửi phiếu đặt sân về email khách hàng.";
            }
            catch
            {
                TempData["Success"] = "Đã xác nhận thanh toán và giữ sân thành công. Tuy nhiên gửi email thất bại.";
            }

            return RedirectToAction("Index");
        }

        // GET: PhieuDatSanNV65134364/KhachVaoSan/P01
        public ActionResult KhachVaoSan(string id)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var phieuGoc = db.PHIEUDATSAN.Find(id);

            if (phieuGoc == null)
            {
                return HttpNotFound();
            }

            if (phieuGoc.TRANGTHAIPHIEU != "Đã xác nhận")
            {
                TempData["Error"] = "Chỉ phiếu đã xác nhận mới được chuyển sang khách vào sân.";
                return RedirectToAction("Index");
            }

            string maNhanVien = Session["MANV"] != null ? Session["MANV"].ToString() : "";

            var dsPhieuCungNhom = LayDanhSachPhieuCungNhom(phieuGoc);

            if (dsPhieuCungNhom == null || !dsPhieuCungNhom.Any())
            {
                dsPhieuCungNhom = db.PHIEUDATSAN
                    .Where(p => p.MAPHIEUDAT == id)
                    .ToList();
            }

            foreach (var phieu in dsPhieuCungNhom)
            {
                bool daHuy =
                    phieu.TRANGTHAIPHIEU == "Đã hủy" ||
                    phieu.TRANGTHAIPHIEU == "Đã huỷ" ||
                    phieu.TRANGTHAIPHIEU == "Hủy" ||
                    phieu.TRANGTHAIPHIEU == "Huy" ||
                    phieu.TRANGTHAIPHIEU == "DaHuy" ||
                    phieu.TRANGTHAIPHIEU == "Khách không đến" ||
                    phieu.TRANGTHAIPHIEU == "Hủy do bảo trì" ||
                    phieu.TRANGTHAITHANHTOAN == "Đã hủy";

                if (daHuy)
                {
                    continue;
                }

                phieu.MANV = maNhanVien;

                // Đổi trạng thái phiếu để ngoài Index không còn đứng yên
                phieu.TRANGTHAIPHIEU = "Đang sử dụng";

                // Giữ trạng thái thanh toán đã đặt cọc / đã thanh toán
                if (string.IsNullOrWhiteSpace(phieu.TRANGTHAITHANHTOAN) ||
                    phieu.TRANGTHAITHANHTOAN == "ChuaThanhToan" ||
                    phieu.TRANGTHAITHANHTOAN == "Cho_Chuyen_Khoan" ||
                    phieu.TRANGTHAITHANHTOAN == "Chờ xác nhận chuyển khoản")
                {
                    phieu.TRANGTHAITHANHTOAN = "Đã đặt cọc";
                }

                var san = db.SAN.Find(phieu.MASAN);

                if (san != null && san.TRANGTHAISAN != "Bảo trì")
                {
                    // Nếu b đã đổi bảng sân chỉ còn Hoạt động/Bảo trì thì để Hoạt động
                    san.TRANGTHAISAN = "Hoạt động";
                }
            }

            db.SaveChanges();

            TempData["Success"] = "Đã cập nhật khách vào sân cho toàn bộ nhóm đặt sân.";
            return RedirectToAction("Index");
        }


        // GET: PhieuDatSanNV65134364/KetThucLuotChoi/P01
        public ActionResult KetThucLuotChoi(string id)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var phieuGoc = db.PHIEUDATSAN.Find(id);

            if (phieuGoc == null)
            {
                return HttpNotFound();
            }

            string maNhanVien = Session["MANV"] != null ? Session["MANV"].ToString() : "";

            var dsPhieuCungNhom = LayDanhSachPhieuCungNhom(phieuGoc);

            if (dsPhieuCungNhom == null || !dsPhieuCungNhom.Any())
            {
                dsPhieuCungNhom = db.PHIEUDATSAN
                    .Where(p => p.MAPHIEUDAT == id)
                    .ToList();
            }

            foreach (var phieu in dsPhieuCungNhom)
            {
                bool daHuy =
                    phieu.TRANGTHAIPHIEU == "Đã hủy" ||
                    phieu.TRANGTHAIPHIEU == "Đã huỷ" ||
                    phieu.TRANGTHAIPHIEU == "Hủy" ||
                    phieu.TRANGTHAIPHIEU == "Huy" ||
                    phieu.TRANGTHAIPHIEU == "DaHuy" ||
                    phieu.TRANGTHAIPHIEU == "Khách không đến" ||
                    phieu.TRANGTHAIPHIEU == "Hủy do bảo trì" ||
                    phieu.TRANGTHAITHANHTOAN == "Đã hủy";

                if (daHuy)
                {
                    continue;
                }

                phieu.MANV = maNhanVien;
                phieu.TRANGTHAIPHIEU = "Hoàn thành";
                phieu.TRANGTHAITHANHTOAN = "Đã thanh toán 100%";

                var san = db.SAN.Find(phieu.MASAN);

                if (san != null && san.TRANGTHAISAN != "Bảo trì")
                {
                    // Vì sân của b giờ chỉ còn Hoạt động / Bảo trì
                    san.TRANGTHAISAN = "Hoạt động";
                }
            }

            db.SaveChanges();

            TempData["Success"] = "Đã kết thúc lượt chơi. Phiếu đã chuyển sang Hoàn thành và Đã thanh toán 100%.";
            return RedirectToAction("Index");
        }


        // GET: PhieuDatSanNV65134364/HuyPhieu/P01
        public ActionResult HuyPhieu(string id)
        {
            return RedirectToAction("HoTroHuySan", new { id = id });
        }

        // GET: PhieuDatSanNV65134364/HoTroHuySan/P01
        public ActionResult HoTroHuySan(string id)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var phieu = db.PHIEUDATSAN
                .Include(p => p.KHACHHANG)
                .Include(p => p.SAN)
                .Include(p => p.SAN.LOAISAN)
                .Include(p => p.NHANVIEN)
                .FirstOrDefault(p => p.MAPHIEUDAT == id);

            if (phieu == null)
            {
                return HttpNotFound();
            }

            if (KhongChoHuySan(phieu))
            {
                TempData["Error"] = "Phiếu đã hoàn thành hoặc đã hủy nên không thể hỗ trợ hủy sân.";
                return RedirectToAction("Details", new { id = id });
            }

            decimal tongTienDichVu = db.CHITIETDICHVUDAT
                .Where(c => c.MAPHIEUDAT == id)
                .Select(c => c.THANHTIEN)
                .DefaultIfEmpty(0)
                .Sum();

            decimal tongTien = phieu.TONGTIENTAMTINH + tongTienDichVu;

            DateTime thoiGianChoi = phieu.NGAYDAT.Date + phieu.GIOBATDAU;
            double soGioTruocGioChoi = (thoiGianChoi - DateTime.Now).TotalHours;

            int phanTramGoiY = TinhPhanTramHoanTienGoiY(phieu);

            ViewBag.TongTienDichVu = tongTienDichVu;
            ViewBag.TongTien = tongTien;
            ViewBag.SoGioTruocGioChoi = soGioTruocGioChoi;
            ViewBag.PhanTramGoiY = phanTramGoiY;
            ViewBag.SoTienHoanGoiY = tongTien * phanTramGoiY / 100m;

            ViewBag.PhanTramList = new SelectList(new[]
            {
        new { Value = "100", Text = "Hoàn 100%" },
        new { Value = "50", Text = "Hoàn 50%" },
        new { Value = "20", Text = "Hoàn 20%" },
        new { Value = "0", Text = "Không hoàn tiền" }
    }, "Value", "Text", phanTramGoiY.ToString());

            return View(phieu);
        }

        // POST: PhieuDatSanNV65134364/HoTroHuySan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HoTroHuySan(string maPhieuDat, int phanTramHoan, string lyDoHuy)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(maPhieuDat))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var phieuGoc = db.PHIEUDATSAN.Find(maPhieuDat);

            if (phieuGoc == null)
            {
                return HttpNotFound();
            }

            if (KhongChoHuySan(phieuGoc))
            {
                TempData["Error"] = "Phiếu đã hoàn thành hoặc đã hủy nên không thể hỗ trợ hủy sân.";
                return RedirectToAction("Details", new { id = maPhieuDat });
            }

            if (phanTramHoan != 100 && phanTramHoan != 50 && phanTramHoan != 20 && phanTramHoan != 0)
            {
                TempData["Error"] = "Phần trăm hoàn tiền không hợp lệ.";
                return RedirectToAction("HoTroHuySan", new { id = maPhieuDat });
            }

            if (string.IsNullOrWhiteSpace(lyDoHuy))
            {
                lyDoHuy = "NV hỗ trợ hủy";
            }

            if (lyDoHuy.Length > 80)
            {
                lyDoHuy = lyDoHuy.Substring(0, 80);
            }

            string maNhanVien = Session["MANV"] != null ? Session["MANV"].ToString() : "";

            var dsPhieuCungNhom = LayDanhSachPhieuCungNhom(phieuGoc);

            if (dsPhieuCungNhom == null || !dsPhieuCungNhom.Any())
            {
                dsPhieuCungNhom = db.PHIEUDATSAN
                    .Where(p => p.MAPHIEUDAT == maPhieuDat)
                    .ToList();
            }

            decimal tongTienTatCaPhieu = 0m;

            foreach (var phieu in dsPhieuCungNhom)
            {
                decimal tienDichVu = db.CHITIETDICHVUDAT
                    .Where(c => c.MAPHIEUDAT == phieu.MAPHIEUDAT)
                    .Select(c => c.THANHTIEN)
                    .DefaultIfEmpty(0)
                    .Sum();

                tongTienTatCaPhieu += phieu.TONGTIENTAMTINH + tienDichVu;
            }

            decimal soTienHoan = tongTienTatCaPhieu * phanTramHoan / 100m;

            string trangThaiThanhToanSauHuy = LayTrangThaiHoanTien(phanTramHoan);

            string ghiChuNgan = "Huy " + phanTramHoan + "% - " + soTienHoan.ToString("N0") + "d";

            if (!string.IsNullOrWhiteSpace(lyDoHuy))
            {
                ghiChuNgan += " - " + lyDoHuy;
            }

            if (ghiChuNgan.Length > 180)
            {
                ghiChuNgan = ghiChuNgan.Substring(0, 180);
            }

            foreach (var phieu in dsPhieuCungNhom)
            {
                bool daHuy =
                    phieu.TRANGTHAIPHIEU == "Đã hủy" ||
                    phieu.TRANGTHAIPHIEU == "Đã huỷ" ||
                    phieu.TRANGTHAIPHIEU == "Hủy" ||
                    phieu.TRANGTHAIPHIEU == "Huy" ||
                    phieu.TRANGTHAIPHIEU == "DaHuy";

                bool daHoanThanh = phieu.TRANGTHAIPHIEU == "Hoàn thành";

                if (daHuy || daHoanThanh)
                {
                    continue;
                }

                phieu.MANV = maNhanVien;
                phieu.TRANGTHAIPHIEU = "Đã hủy";
                phieu.TRANGTHAITHANHTOAN = trangThaiThanhToanSauHuy;

                string ghiChuCu = phieu.GHICHU ?? "";

                if (ghiChuCu.Length > 80)
                {
                    ghiChuCu = ghiChuCu.Substring(0, 80);
                }

                if (string.IsNullOrWhiteSpace(ghiChuCu))
                {
                    phieu.GHICHU = ghiChuNgan;
                }
                else
                {
                    phieu.GHICHU = ghiChuCu + " | " + ghiChuNgan;
                }

                if (phieu.GHICHU != null && phieu.GHICHU.Length > 250)
                {
                    phieu.GHICHU = phieu.GHICHU.Substring(0, 250);
                }

                var san = db.SAN.Find(phieu.MASAN);

                if (san != null && san.TRANGTHAISAN != "Bảo trì")
                {
                    san.TRANGTHAISAN = "Hoạt động";
                }
            }

            db.SaveChanges();

            TempData["Success"] = "Đã hủy sân thành công. Mức hoàn: " + phanTramHoan + "% - Số tiền hoàn dự kiến: " + soTienHoan.ToString("N0") + "đ.";
            return RedirectToAction("Index");
        }
        private string LayTrangThaiHoanTien(int phanTramHoan)
        {
            if (phanTramHoan == 100)
            {
                return "Hoàn cọc 100%";
            }

            if (phanTramHoan == 50)
            {
                return "Hoàn cọc 50%";
            }

            if (phanTramHoan == 20)
            {
                return "Hoàn cọc 20%";
            }

            return "Không hoàn cọc";
        }
        private bool KhongChoHuySan(PHIEUDATSAN phieu)
        {
            if (phieu == null)
            {
                return true;
            }

            bool daHuy =
                phieu.TRANGTHAIPHIEU == "Đã hủy" ||
                phieu.TRANGTHAIPHIEU == "Đã huỷ" ||
                phieu.TRANGTHAIPHIEU == "Hủy" ||
                phieu.TRANGTHAIPHIEU == "Huy" ||
                phieu.TRANGTHAIPHIEU == "DaHuy" ||
                phieu.TRANGTHAITHANHTOAN == "Đã hủy" ||
                phieu.TRANGTHAITHANHTOAN == "Đã huỷ";

            bool daHoanThanh = phieu.TRANGTHAIPHIEU == "Hoàn thành";

            return daHuy || daHoanThanh;
        }

        private int TinhPhanTramHoanTienGoiY(PHIEUDATSAN phieu)
        {
            if (phieu == null)
            {
                return 0;
            }

            DateTime thoiGianChoi = phieu.NGAYDAT.Date + phieu.GIOBATDAU;
            double soGioTruocGioChoi = (thoiGianChoi - DateTime.Now).TotalHours;

            if (soGioTruocGioChoi >= 4)
            {
                return 100;
            }

            if (soGioTruocGioChoi >= 2)
            {
                return 50;
            }

            return 20;
        }

        // =========================================================
        // DỊCH VỤ ĐI KÈM TRONG PHIẾU ĐẶT SÂN
        // =========================================================

        // GET: PhieuDatSanNV65134364/ThemDichVu/PDS01
        public ActionResult ThemDichVu(string id)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var phieu = db.PHIEUDATSAN
                .Include(p => p.KHACHHANG)
                .Include(p => p.SAN)
                .FirstOrDefault(p => p.MAPHIEUDAT == id);

            if (phieu == null)
            {
                return HttpNotFound();
            }

            if (KhongChoSuaDichVu(phieu))
            {
                TempData["Error"] = "Phiếu đã hoàn thành, đã hủy hoặc đã lập hóa đơn nên không thể thêm dịch vụ.";
                return RedirectToAction("Details", new { id = id });
            }

            ViewBag.DanhSachDichVu = db.DICHVU
                .Where(d => d.TRANGTHAIKD == "Đang kinh doanh" && d.SOLUONGTON > 0)
                .OrderBy(d => d.TENDV)
                .ToList();

            return View(phieu);
        }

        // POST: PhieuDatSanNV65134364/ThemDichVu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemDichVu(string maPhieuDat, string maDV, int soLuong)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(maPhieuDat))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var phieu = db.PHIEUDATSAN.Find(maPhieuDat);

            if (phieu == null)
            {
                return HttpNotFound();
            }

            if (KhongChoSuaDichVu(phieu))
            {
                TempData["Error"] = "Phiếu đã hoàn thành, đã hủy hoặc đã lập hóa đơn nên không thể thêm dịch vụ.";
                return RedirectToAction("Details", new { id = maPhieuDat });
            }

            if (string.IsNullOrWhiteSpace(maDV))
            {
                TempData["Error"] = "Vui lòng chọn dịch vụ.";
                return RedirectToAction("ThemDichVu", new { id = maPhieuDat });
            }

            if (soLuong <= 0)
            {
                TempData["Error"] = "Số lượng dịch vụ phải lớn hơn 0.";
                return RedirectToAction("ThemDichVu", new { id = maPhieuDat });
            }

            var dichVu = db.DICHVU.Find(maDV);

            if (dichVu == null)
            {
                TempData["Error"] = "Không tìm thấy dịch vụ.";
                return RedirectToAction("ThemDichVu", new { id = maPhieuDat });
            }

            if (dichVu.TRANGTHAIKD != "Đang kinh doanh")
            {
                TempData["Error"] = "Dịch vụ này đang ngừng kinh doanh.";
                return RedirectToAction("ThemDichVu", new { id = maPhieuDat });
            }

            if (dichVu.SOLUONGTON < soLuong)
            {
                TempData["Error"] = "Số lượng tồn kho không đủ. Hiện còn: " + dichVu.SOLUONGTON;
                return RedirectToAction("ThemDichVu", new { id = maPhieuDat });
            }

            var chiTietCu = db.CHITIETDICHVUDAT
                .FirstOrDefault(c => c.MAPHIEUDAT == maPhieuDat && c.MADV == maDV);

            if (chiTietCu != null)
            {
                chiTietCu.SOLUONG += soLuong;
                chiTietCu.DONGIA = dichVu.DONGIA;
                chiTietCu.THANHTIEN = chiTietCu.SOLUONG * chiTietCu.DONGIA;

                db.Entry(chiTietCu).State = EntityState.Modified;
            }
            else
            {
                CHITIETDICHVUDAT chiTietMoi = new CHITIETDICHVUDAT
                {
                    MAPHIEUDAT = maPhieuDat,
                    MADV = maDV,
                    SOLUONG = soLuong,
                    DONGIA = dichVu.DONGIA,
                    THANHTIEN = soLuong * dichVu.DONGIA
                };

                db.CHITIETDICHVUDAT.Add(chiTietMoi);
            }

            dichVu.SOLUONGTON -= soLuong;
            db.Entry(dichVu).State = EntityState.Modified;

            db.SaveChanges();

            TempData["Success"] = "Đã thêm dịch vụ vào phiếu đặt sân.";
            return RedirectToAction("Details", new { id = maPhieuDat });
        }

        // GET: PhieuDatSanNV65134364/XoaDichVu?maPhieuDat=PDS01&maDV=DV01
        public ActionResult XoaDichVu(string maPhieuDat, string maDV)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(maPhieuDat) || string.IsNullOrWhiteSpace(maDV))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var phieu = db.PHIEUDATSAN.Find(maPhieuDat);

            if (phieu == null)
            {
                return HttpNotFound();
            }

            if (KhongChoSuaDichVu(phieu))
            {
                TempData["Error"] = "Phiếu đã hoàn thành, đã hủy hoặc đã lập hóa đơn nên không thể xóa dịch vụ.";
                return RedirectToAction("Details", new { id = maPhieuDat });
            }

            var chiTiet = db.CHITIETDICHVUDAT
                .FirstOrDefault(c => c.MAPHIEUDAT == maPhieuDat && c.MADV == maDV);

            if (chiTiet == null)
            {
                TempData["Error"] = "Không tìm thấy dịch vụ trong phiếu.";
                return RedirectToAction("Details", new { id = maPhieuDat });
            }

            var dichVu = db.DICHVU.Find(maDV);

            if (dichVu != null)
            {
                dichVu.SOLUONGTON += chiTiet.SOLUONG;
                db.Entry(dichVu).State = EntityState.Modified;
            }

            db.CHITIETDICHVUDAT.Remove(chiTiet);
            db.SaveChanges();

            TempData["Success"] = "Đã xóa dịch vụ khỏi phiếu đặt sân.";
            return RedirectToAction("Details", new { id = maPhieuDat });
        }

        // GET: PhieuDatSanNV65134364/LapHoaDon/P01
        public ActionResult LapHoaDon(string id)
        {
            var check = KiemTraQuyenNhanVien();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var phieuGoc = db.PHIEUDATSAN
                .Include(p => p.KHACHHANG)
                .Include(p => p.SAN)
                .FirstOrDefault(p => p.MAPHIEUDAT == id);

            if (phieuGoc == null)
            {
                return HttpNotFound();
            }

            var dsPhieuCungNhom = LayDanhSachPhieuCungNhom(phieuGoc);

            if (dsPhieuCungNhom == null || !dsPhieuCungNhom.Any())
            {
                dsPhieuCungNhom = db.PHIEUDATSAN
                    .Where(p => p.MAPHIEUDAT == id)
                    .ToList();
            }

            var dsMaPhieu = dsPhieuCungNhom
                .Select(p => p.MAPHIEUDAT)
                .ToList();

            var hoaDonCu = db.HOADON
                .FirstOrDefault(h => dsMaPhieu.Contains(h.MAPHIEUDAT));

            if (hoaDonCu != null)
            {
                TempData["Error"] = "Nhóm phiếu này đã có hóa đơn.";
                return RedirectToAction("Details", "HoaDonNV65134364", new { id = hoaDonCu.SOHD });
            }

            string maNhanVien = Session["MANV"] != null ? Session["MANV"].ToString() : "";

            decimal tienSan = dsPhieuCungNhom.Sum(p => p.TONGTIENTAMTINH);

            decimal tienDichVu = db.CHITIETDICHVUDAT
                .Where(c => dsMaPhieu.Contains(c.MAPHIEUDAT))
                .Select(c => c.THANHTIEN)
                .DefaultIfEmpty(0)
                .Sum();

            decimal giamGiaHoiVien = TinhGiamGiaHoiVien(phieuGoc.MAKH, tienSan + tienDichVu);

            decimal tongThanhToan = tienSan + tienDichVu - giamGiaHoiVien;

            HOADON hd = new HOADON
            {
                SOHD = TaoMaHoaDonMoi(),

                // Hóa đơn vẫn liên kết 1 phiếu đại diện,
                // nhưng tiền trong hóa đơn là tổng của cả nhóm.
                MAPHIEUDAT = phieuGoc.MAPHIEUDAT,

                MANV = maNhanVien,
                NGAYLAP = DateTime.Now,
                HINHTHUCTT = "Chuyển khoản",
                TIENTHUESAN = tienSan,
                TIENDICHVU = tienDichVu,
                GIAMGIAHOIVIEN = giamGiaHoiVien,
                TONGTHANHTOAN = tongThanhToan
            };

            db.HOADON.Add(hd);

            foreach (var phieu in dsPhieuCungNhom)
            {
                bool daHuy =
                    phieu.TRANGTHAIPHIEU == "Đã hủy" ||
                    phieu.TRANGTHAIPHIEU == "Đã huỷ" ||
                    phieu.TRANGTHAIPHIEU == "Hủy" ||
                    phieu.TRANGTHAIPHIEU == "Huy" ||
                    phieu.TRANGTHAIPHIEU == "DaHuy" ||
                    phieu.TRANGTHAIPHIEU == "Khách không đến" ||
                    phieu.TRANGTHAIPHIEU == "Hủy do bảo trì" ||
                    phieu.TRANGTHAITHANHTOAN == "Đã hủy" ||
                    phieu.TRANGTHAITHANHTOAN == "Đã huỷ";

                if (daHuy)
                {
                    continue;
                }

                phieu.MANV = maNhanVien;
                phieu.TRANGTHAIPHIEU = "Hoàn thành";
                phieu.TRANGTHAITHANHTOAN = "Đã thanh toán 100%";

                var san = db.SAN.Find(phieu.MASAN);

                if (san != null && san.TRANGTHAISAN != "Bảo trì")
                {
                    san.TRANGTHAISAN = "Hoạt động";
                }
            }

            db.SaveChanges();

            try
            {
                var hoaDonGuiMail = db.HOADON
                    .Include(h => h.NHANVIEN)
                    .Include(h => h.PHIEUDATSAN)
                    .Include(h => h.PHIEUDATSAN.KHACHHANG)
                    .Include(h => h.PHIEUDATSAN.SAN)
                    .FirstOrDefault(h => h.SOHD == hd.SOHD);

                var dsDichVu = db.CHITIETDICHVUDAT
                    .Include(c => c.DICHVU)
                    .Where(c => dsMaPhieu.Contains(c.MAPHIEUDAT))
                    .ToList();

                if (hoaDonGuiMail != null &&
                    hoaDonGuiMail.PHIEUDATSAN != null &&
                    hoaDonGuiMail.PHIEUDATSAN.KHACHHANG != null &&
                    !string.IsNullOrWhiteSpace(hoaDonGuiMail.PHIEUDATSAN.KHACHHANG.EMAILKH))
                {
                    string emailKhach = hoaDonGuiMail.PHIEUDATSAN.KHACHHANG.EMAILKH;
                    string subject = "ARMY PICKLEBALL - Hóa đơn " + hoaDonGuiMail.SOHD;
                    string body = TaoNoiDungEmailHoaDon(hoaDonGuiMail, dsDichVu);

                    GuiEmail(emailKhach, subject, body);

                    TempData["Success"] = "Lập hóa đơn thành công và đã gửi hóa đơn về email khách hàng: " + hoaDonGuiMail.SOHD;
                }
                else
                {
                    TempData["Success"] = "Lập hóa đơn thành công: " + hd.SOHD + ". Khách hàng chưa có email nên không gửi được.";
                }
            }
            catch
            {
                TempData["Success"] = "Lập hóa đơn thành công: " + hd.SOHD + ". Tuy nhiên gửi email thất bại.";
            }

            return RedirectToAction("Details", "HoaDonNV65134364", new { id = hd.SOHD });
        }

        // =========================================================
        // HÀM PHỤ
        // =========================================================

        private bool KhongChoSuaDichVu(PHIEUDATSAN phieu)
        {
            if (phieu == null)
            {
                return true;
            }

            bool daHuy =
                phieu.TRANGTHAIPHIEU == "Đã hủy" ||
                phieu.TRANGTHAIPHIEU == "Đã huỷ" ||
                phieu.TRANGTHAIPHIEU == "Hủy" ||
                phieu.TRANGTHAIPHIEU == "Huy" ||
                phieu.TRANGTHAIPHIEU == "DaHuy" ||
                phieu.TRANGTHAITHANHTOAN == "Đã hủy" ||
                phieu.TRANGTHAITHANHTOAN == "Đã huỷ";

            bool daHoanThanh = phieu.TRANGTHAIPHIEU == "Hoàn thành";

            bool daCoHoaDon = db.HOADON.Any(h => h.MAPHIEUDAT == phieu.MAPHIEUDAT);

            return daHuy || daHoanThanh || daCoHoaDon;
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

            int viTriKetThuc = chuoiTuGRP.IndexOf(" ");

            if (viTriKetThuc > 0)
            {
                chuoiTuGRP = chuoiTuGRP.Substring(0, viTriKetThuc);
            }

            int viTriGach = chuoiTuGRP.IndexOf("|");

            if (viTriGach > 0)
            {
                chuoiTuGRP = chuoiTuGRP.Substring(0, viTriGach);
            }

            return chuoiTuGRP.Trim();
        }

        private System.Collections.Generic.List<PHIEUDATSAN> LayDanhSachPhieuCungNhom(PHIEUDATSAN phieuGoc)
        {
            string maNhom = LayMaNhomTuGhiChu(phieuGoc.GHICHU);

            if (string.IsNullOrWhiteSpace(maNhom))
            {
                return db.PHIEUDATSAN
                    .Where(p => p.MAPHIEUDAT == phieuGoc.MAPHIEUDAT)
                    .ToList();
            }

            return db.PHIEUDATSAN
                .Where(p => p.GHICHU != null && p.GHICHU.Contains(maNhom))
                .ToList();
        }

        private void CapNhatTrangThaiSanTheoLichHomNay()
        {
            DateTime homNay = DateTime.Today;
            DateTime ngayMai = homNay.AddDays(1);
            TimeSpan gioHienTai = DateTime.Now.TimeOfDay;

            var dsSan = db.SAN.ToList();

            var dsPhieuHomNay = db.PHIEUDATSAN
                .Where(p => p.NGAYDAT >= homNay
                         && p.NGAYDAT < ngayMai
                         && !TrangThaiHuy.Contains(p.TRANGTHAIPHIEU))
                .ToList();

            bool coThayDoi = false;

            foreach (var san in dsSan)
            {
                if (san.TRANGTHAISAN == "Bảo trì")
                {
                    continue;
                }

                string trangThaiMoi = "Trống";

                var dsPhieuCuaSan = dsPhieuHomNay
                    .Where(p => p.MASAN == san.MASAN)
                    .ToList();

                var phieuDangDienRa = dsPhieuCuaSan.FirstOrDefault(p =>
                    p.GIOBATDAU <= gioHienTai &&
                    p.GIOKETTHUC > gioHienTai
                );

                if (phieuDangDienRa != null)
                {
                    if (phieuDangDienRa.TRANGTHAIPHIEU == "Đã xác nhận")
                    {
                        trangThaiMoi = "Đang sử dụng";
                    }
                    else
                    {
                        trangThaiMoi = "Đang đặt";
                    }
                }
                else
                {
                    bool coLichSapToi = dsPhieuCuaSan.Any(p => p.GIOBATDAU > gioHienTai);

                    trangThaiMoi = coLichSapToi ? "Đang đặt" : "Trống";
                }

                if (san.TRANGTHAISAN != trangThaiMoi)
                {
                    san.TRANGTHAISAN = trangThaiMoi;
                    coThayDoi = true;
                }
            }

            if (coThayDoi)
            {
                db.SaveChanges();
            }
        }

        private decimal TinhGiamGiaHoiVien(string maKH, decimal tongTien)
        {
            if (string.IsNullOrWhiteSpace(maKH))
            {
                return 0;
            }

            var hoiVien = db.HOIVIEN
                .Where(h => h.MAKH == maKH
                         && h.TRANGTHAIPHI == "Đã thanh toán"
                         && h.NGAYBATDAU <= DateTime.Today
                         && h.NGAYKETTHUC >= DateTime.Today)
                .OrderByDescending(h => h.NGAYKETTHUC)
                .FirstOrDefault();

            if (hoiVien == null)
            {
                return 0;
            }

            if (hoiVien.LOAITHE == "Luxury")
            {
                return tongTien * 0.10m;
            }

            if (hoiVien.LOAITHE == "VIP")
            {
                return tongTien * 0.05m;
            }

            return 0;
        }

        private string TaoMaHoaDonMoi()
        {
            var maCuoi = db.HOADON
                .Where(h => h.SOHD.StartsWith("HD"))
                .OrderByDescending(h => h.SOHD)
                .Select(h => h.SOHD)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(maCuoi))
            {
                return "HD01";
            }

            int so = 0;
            string phanSo = maCuoi.Replace("HD", "");

            int.TryParse(phanSo, out so);
            so++;

            return "HD" + so.ToString("00");
        }

        // =========================================================
        // GỬI EMAIL
        // =========================================================

        private void GuiEmail(string emailNhan, string tieuDe, string noiDungHtml)
        {
            if (string.IsNullOrWhiteSpace(emailNhan))
            {
                return;
            }

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("vu.tn.65cntt@ntu.edu.vn", "ARMY PICKLEBALL");
            mail.To.Add(emailNhan);
            mail.Subject = tieuDe;
            mail.Body = noiDungHtml;
            mail.IsBodyHtml = true;
            mail.BodyEncoding = Encoding.UTF8;
            mail.SubjectEncoding = Encoding.UTF8;

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential("vu.tn.65cntt@ntu.edu.vn", "zkjx jikv krzu asuv");
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }

        private string TaoNoiDungEmailPhieuDat(PHIEUDATSAN phieu)
        {
            string tenKhach = phieu.KHACHHANG != null ? phieu.KHACHHANG.HOTENKH : "Quý khách";
            string tenSan = phieu.SAN != null ? phieu.SAN.TENSAN : phieu.MASAN;
            string loaiSan = phieu.SAN != null && phieu.SAN.LOAISAN != null ? phieu.SAN.LOAISAN.TENLOAISAN : "";
            string tenNhanVien = phieu.NHANVIEN != null ? phieu.NHANVIEN.HOTENNV : "Nhân viên sân";

            return @"
<div style='font-family:Arial,sans-serif;background:#f4f6f9;padding:24px;'>
    <div style='max-width:720px;margin:auto;background:white;border-radius:18px;overflow:hidden;border:1px solid #e5e7eb;'>
        <div style='background:#0f172a;color:white;padding:24px;'>
            <h2 style='margin:0;'>ARMY PICKLEBALL</h2>
            <p style='margin:6px 0 0;color:#cbd5e1;'>Phiếu xác nhận đặt sân</p>
        </div>

        <div style='padding:24px;'>
            <p>Xin chào <b>" + tenKhach + @"</b>,</p>
            <p>Lịch đặt sân của bạn đã được nhân viên xác nhận.</p>

            <table style='width:100%;border-collapse:collapse;margin-top:18px;'>
                <tr>
                    <td style='padding:10px;border:1px solid #e5e7eb;background:#f9fafb;font-weight:bold;'>Mã phiếu</td>
                    <td style='padding:10px;border:1px solid #e5e7eb;'>" + phieu.MAPHIEUDAT + @"</td>
                </tr>
                <tr>
                    <td style='padding:10px;border:1px solid #e5e7eb;background:#f9fafb;font-weight:bold;'>Sân</td>
                    <td style='padding:10px;border:1px solid #e5e7eb;'>" + tenSan + @" - " + loaiSan + @"</td>
                </tr>
                <tr>
                    <td style='padding:10px;border:1px solid #e5e7eb;background:#f9fafb;font-weight:bold;'>Ngày chơi</td>
                    <td style='padding:10px;border:1px solid #e5e7eb;'>" + phieu.NGAYDAT.ToString("dd/MM/yyyy") + @"</td>
                </tr>
                <tr>
                    <td style='padding:10px;border:1px solid #e5e7eb;background:#f9fafb;font-weight:bold;'>Khung giờ</td>
                    <td style='padding:10px;border:1px solid #e5e7eb;'>" + phieu.GIOBATDAU.ToString(@"hh\:mm") + @" - " + phieu.GIOKETTHUC.ToString(@"hh\:mm") + @"</td>
                </tr>
                <tr>
                    <td style='padding:10px;border:1px solid #e5e7eb;background:#f9fafb;font-weight:bold;'>Tạm tính</td>
                    <td style='padding:10px;border:1px solid #e5e7eb;color:#047857;font-weight:bold;'>" + phieu.TONGTIENTAMTINH.ToString("N0") + @" đ</td>
                </tr>
                <tr>
                    <td style='padding:10px;border:1px solid #e5e7eb;background:#f9fafb;font-weight:bold;'>Trạng thái</td>
                    <td style='padding:10px;border:1px solid #e5e7eb;'>" + phieu.TRANGTHAIPHIEU + @" - " + phieu.TRANGTHAITHANHTOAN + @"</td>
                </tr>
            </table>

            <p style='margin-top:18px;'>Nhân viên xác nhận: <b>" + tenNhanVien + @"</b></p>
            <p style='color:#64748b;'>Vui lòng đến sân đúng giờ. Cảm ơn bạn đã sử dụng dịch vụ tại ARMY PICKLEBALL.</p>
        </div>
    </div>
</div>";
        }

        private string TaoNoiDungEmailHoaDon(HOADON hd, System.Collections.Generic.List<CHITIETDICHVUDAT> dsDichVu)
        {
            var phieu = hd.PHIEUDATSAN;

            string tenKhach = phieu != null && phieu.KHACHHANG != null ? phieu.KHACHHANG.HOTENKH : "Quý khách";
            string tenSan = phieu != null && phieu.SAN != null ? phieu.SAN.TENSAN : "";
            string maSan = phieu != null ? phieu.MASAN : "";
            string tenNhanVien = hd.NHANVIEN != null ? hd.NHANVIEN.HOTENNV : "Nhân viên sân";

            StringBuilder dichVuHtml = new StringBuilder();

            if (dsDichVu != null && dsDichVu.Any())
            {
                foreach (var item in dsDichVu)
                {
                    string tenDV = item.DICHVU != null ? item.DICHVU.TENDV : item.MADV;

                    dichVuHtml.Append(@"
<tr>
    <td style='padding:10px;border:1px solid #e5e7eb;'>" + tenDV + @"</td>
    <td style='padding:10px;border:1px solid #e5e7eb;text-align:right;'>" + item.DONGIA.ToString("N0") + @" đ</td>
    <td style='padding:10px;border:1px solid #e5e7eb;text-align:right;'>" + item.SOLUONG + @"</td>
    <td style='padding:10px;border:1px solid #e5e7eb;text-align:right;font-weight:bold;'>" + item.THANHTIEN.ToString("N0") + @" đ</td>
</tr>");
                }
            }
            else
            {
                dichVuHtml.Append(@"
<tr>
    <td style='padding:10px;border:1px solid #e5e7eb;'>Không sử dụng dịch vụ</td>
    <td style='padding:10px;border:1px solid #e5e7eb;text-align:right;'>0 đ</td>
    <td style='padding:10px;border:1px solid #e5e7eb;text-align:right;'>0</td>
    <td style='padding:10px;border:1px solid #e5e7eb;text-align:right;font-weight:bold;'>0 đ</td>
</tr>");
            }

            return @"
<div style='font-family:Arial,sans-serif;background:#f4f6f9;padding:24px;'>
    <div style='max-width:760px;margin:auto;background:white;border-radius:18px;overflow:hidden;border:1px solid #e5e7eb;'>
        <div style='background:#0f172a;color:white;padding:24px;'>
            <h2 style='margin:0;'>ARMY PICKLEBALL</h2>
            <p style='margin:6px 0 0;color:#cbd5e1;'>Hóa đơn thanh toán - " + hd.SOHD + @"</p>
        </div>

        <div style='padding:24px;'>
            <p>Xin chào <b>" + tenKhach + @"</b>,</p>
            <p>ARMY PICKLEBALL gửi bạn hóa đơn thanh toán như sau:</p>

            <table style='width:100%;border-collapse:collapse;margin-top:16px;'>
                <tr>
                    <td style='padding:10px;border:1px solid #e5e7eb;background:#f9fafb;font-weight:bold;'>Số hóa đơn</td>
                    <td style='padding:10px;border:1px solid #e5e7eb;'>" + hd.SOHD + @"</td>
                </tr>
                <tr>
                    <td style='padding:10px;border:1px solid #e5e7eb;background:#f9fafb;font-weight:bold;'>Mã phiếu</td>
                    <td style='padding:10px;border:1px solid #e5e7eb;'>" + hd.MAPHIEUDAT + @"</td>
                </tr>
                <tr>
                    <td style='padding:10px;border:1px solid #e5e7eb;background:#f9fafb;font-weight:bold;'>Sân</td>
                    <td style='padding:10px;border:1px solid #e5e7eb;'>" + tenSan + @" - " + maSan + @"</td>
                </tr>
                <tr>
                    <td style='padding:10px;border:1px solid #e5e7eb;background:#f9fafb;font-weight:bold;'>Ngày lập</td>
                    <td style='padding:10px;border:1px solid #e5e7eb;'>" + hd.NGAYLAP.ToString("dd/MM/yyyy HH:mm") + @"</td>
                </tr>
                <tr>
                    <td style='padding:10px;border:1px solid #e5e7eb;background:#f9fafb;font-weight:bold;'>Hình thức thanh toán</td>
                    <td style='padding:10px;border:1px solid #e5e7eb;'>" + hd.HINHTHUCTT + @"</td>
                </tr>
            </table>

            <h3 style='margin-top:22px;'>Chi tiết thanh toán</h3>

            <table style='width:100%;border-collapse:collapse;'>
                <thead>
                    <tr>
                        <th style='padding:10px;border:1px solid #e5e7eb;background:#f3f4f6;text-align:left;'>Nội dung</th>
                        <th style='padding:10px;border:1px solid #e5e7eb;background:#f3f4f6;text-align:right;'>Đơn giá</th>
                        <th style='padding:10px;border:1px solid #e5e7eb;background:#f3f4f6;text-align:right;'>SL</th>
                        <th style='padding:10px;border:1px solid #e5e7eb;background:#f3f4f6;text-align:right;'>Thành tiền</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td style='padding:10px;border:1px solid #e5e7eb;'>Tiền thuê sân</td>
                        <td style='padding:10px;border:1px solid #e5e7eb;text-align:right;'>" + hd.TIENTHUESAN.ToString("N0") + @" đ</td>
                        <td style='padding:10px;border:1px solid #e5e7eb;text-align:right;'>1</td>
                        <td style='padding:10px;border:1px solid #e5e7eb;text-align:right;font-weight:bold;'>" + hd.TIENTHUESAN.ToString("N0") + @" đ</td>
                    </tr>
                    " + dichVuHtml.ToString() + @"
                </tbody>
            </table>

            <table style='width:100%;border-collapse:collapse;margin-top:20px;'>
                <tr>
                    <td style='padding:10px;text-align:right;font-weight:bold;'>Tiền sân:</td>
                    <td style='padding:10px;text-align:right;width:180px;'>" + hd.TIENTHUESAN.ToString("N0") + @" đ</td>
                </tr>
                <tr>
                    <td style='padding:10px;text-align:right;font-weight:bold;'>Tiền dịch vụ:</td>
                    <td style='padding:10px;text-align:right;'>" + hd.TIENDICHVU.ToString("N0") + @" đ</td>
                </tr>
                <tr>
                    <td style='padding:10px;text-align:right;font-weight:bold;'>Giảm giá hội viên:</td>
                    <td style='padding:10px;text-align:right;'>-" + hd.GIAMGIAHOIVIEN.ToString("N0") + @" đ</td>
                </tr>
                <tr>
                    <td style='padding:12px;text-align:right;font-size:20px;font-weight:bold;color:#047857;'>Tổng thanh toán:</td>
                    <td style='padding:12px;text-align:right;font-size:20px;font-weight:bold;color:#047857;'>" + hd.TONGTHANHTOAN.ToString("N0") + @" đ</td>
                </tr>
            </table>

            <p style='margin-top:18px;'>Nhân viên lập hóa đơn: <b>" + tenNhanVien + @"</b></p>
            <p style='color:#64748b;'>Cảm ơn quý khách đã sử dụng dịch vụ tại ARMY PICKLEBALL.</p>
        </div>
    </div>
</div>";
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