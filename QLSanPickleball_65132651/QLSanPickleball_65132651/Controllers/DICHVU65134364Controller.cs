using QLSanPickleball_65132651.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace QLSanPickleball_65132651.Controllers
{
    public class DICHVU65134364Controller : Controller
    {
        private QLSanEntities db = new QLSanEntities();

        private ActionResult KiemTraQuyenAdmin()
        {
            if (Session["MANV"] == null)
            {
                return RedirectToAction("Login", "Account65132651");
            }

            string vaiTro = Session["VAITRO"] != null ? Session["VAITRO"].ToString() : "";

            if (vaiTro != "Admin" && vaiTro != "Quản lý")
            {
                TempData["Error"] = "Bạn không có quyền sử dụng chức năng quản lý dịch vụ.";
                return RedirectToAction("HomeNv", "Admin65134364");
            }

            return null;
        }

        // GET: DICHVU65134364
        public ActionResult Index(string search, string trangThai, int page = 1)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }

            var dsDichVu = db.DICHVU.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                dsDichVu = dsDichVu.Where(d =>
                    d.MADV.Contains(search) ||
                    d.TENDV.Contains(search) ||
                    d.DONVITINH.Contains(search)
                );
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                dsDichVu = dsDichVu.Where(d => d.TRANGTHAIKD == trangThai);
            }

            int tongSoDichVu = dsDichVu.Count();
            int tongSoTrang = (int)Math.Ceiling((double)tongSoDichVu / pageSize);

            if (tongSoTrang == 0)
            {
                tongSoTrang = 1;
            }

            if (page > tongSoTrang)
            {
                page = tongSoTrang;
            }

            var ketQua = dsDichVu
                .OrderBy(d => d.MADV)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.TrangThai = trangThai;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = tongSoTrang;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = tongSoDichVu;

            ViewBag.TongDichVu = db.DICHVU.Count();
            ViewBag.DangKinhDoanh = db.DICHVU.Count(d => d.TRANGTHAIKD == "Đang kinh doanh");
            ViewBag.NgungKinhDoanh = db.DICHVU.Count(d => d.TRANGTHAIKD == "Ngừng kinh doanh");
            ViewBag.TongTonKho = db.DICHVU.Any() ? db.DICHVU.Sum(d => d.SOLUONGTON) : 0;

            return View(ketQua);
        }

        // GET: DICHVU65134364/Details/DV01
        public ActionResult Details(string id)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var dichVu = db.DICHVU.Find(id);

            if (dichVu == null)
            {
                return HttpNotFound();
            }

            ViewBag.SoLanDuocDat = db.CHITIETDICHVUDAT.Count(c => c.MADV == id);
            ViewBag.TongSoLuongDaBan = db.CHITIETDICHVUDAT
                .Where(c => c.MADV == id)
                .Select(c => c.SOLUONG)
                .DefaultIfEmpty(0)
                .Sum();

            ViewBag.TongDoanhThuDichVu = db.CHITIETDICHVUDAT
                .Where(c => c.MADV == id)
                .Select(c => c.THANHTIEN)
                .DefaultIfEmpty(0)
                .Sum();

            return View(dichVu);
        }

        // GET: DICHVU65134364/Create
        public ActionResult Create()
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            DICHVU model = new DICHVU();
            model.MADV = TaoMaDichVuMoi();
            model.TRANGTHAIKD = "Đang kinh doanh";
            model.SOLUONGTON = 0;

            ViewBag.TrangThaiList = new SelectList(new[]
            {
                "Đang kinh doanh",
                "Ngừng kinh doanh"
            });

            return View(model);
        }

        // POST: DICHVU65134364/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MADV,TENDV,DONVITINH,DONGIA,SOLUONGTON,TRANGTHAIKD")] DICHVU dichVu)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(dichVu.MADV))
            {
                dichVu.MADV = TaoMaDichVuMoi();
            }

            if (db.DICHVU.Any(d => d.MADV == dichVu.MADV))
            {
                ModelState.AddModelError("MADV", "Mã dịch vụ đã tồn tại.");
            }

            if (string.IsNullOrWhiteSpace(dichVu.TENDV))
            {
                ModelState.AddModelError("TENDV", "Vui lòng nhập tên dịch vụ.");
            }

            if (string.IsNullOrWhiteSpace(dichVu.DONVITINH))
            {
                ModelState.AddModelError("DONVITINH", "Vui lòng nhập đơn vị tính.");
            }

            if (dichVu.DONGIA < 0)
            {
                ModelState.AddModelError("DONGIA", "Đơn giá không được âm.");
            }

            if (dichVu.SOLUONGTON < 0)
            {
                ModelState.AddModelError("SOLUONGTON", "Số lượng tồn không được âm.");
            }

            if (string.IsNullOrWhiteSpace(dichVu.TRANGTHAIKD))
            {
                dichVu.TRANGTHAIKD = "Đang kinh doanh";
            }

            if (ModelState.IsValid)
            {
                db.DICHVU.Add(dichVu);
                db.SaveChanges();

                TempData["Success"] = "Thêm dịch vụ thành công.";
                return RedirectToAction("Index");
            }

            ViewBag.TrangThaiList = new SelectList(new[]
            {
                "Đang kinh doanh",
                "Ngừng kinh doanh"
            }, dichVu.TRANGTHAIKD);

            return View(dichVu);
        }

        // GET: DICHVU65134364/Edit/DV01
        public ActionResult Edit(string id)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var dichVu = db.DICHVU.Find(id);

            if (dichVu == null)
            {
                return HttpNotFound();
            }

            ViewBag.TrangThaiList = new SelectList(new[]
            {
                "Đang kinh doanh",
                "Ngừng kinh doanh"
            }, dichVu.TRANGTHAIKD);

            return View(dichVu);
        }

        // POST: DICHVU65134364/Edit/DV01
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MADV,TENDV,DONVITINH,DONGIA,SOLUONGTON,TRANGTHAIKD")] DICHVU dichVu)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(dichVu.MADV))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(dichVu.TENDV))
            {
                ModelState.AddModelError("TENDV", "Vui lòng nhập tên dịch vụ.");
            }

            if (string.IsNullOrWhiteSpace(dichVu.DONVITINH))
            {
                ModelState.AddModelError("DONVITINH", "Vui lòng nhập đơn vị tính.");
            }

            if (dichVu.DONGIA < 0)
            {
                ModelState.AddModelError("DONGIA", "Đơn giá không được âm.");
            }

            if (dichVu.SOLUONGTON < 0)
            {
                ModelState.AddModelError("SOLUONGTON", "Số lượng tồn không được âm.");
            }

            if (string.IsNullOrWhiteSpace(dichVu.TRANGTHAIKD))
            {
                dichVu.TRANGTHAIKD = "Đang kinh doanh";
            }

            if (ModelState.IsValid)
            {
                db.Entry(dichVu).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Cập nhật dịch vụ thành công.";
                return RedirectToAction("Index");
            }

            ViewBag.TrangThaiList = new SelectList(new[]
            {
                "Đang kinh doanh",
                "Ngừng kinh doanh"
            }, dichVu.TRANGTHAIKD);

            return View(dichVu);
        }

        // GET: DICHVU65134364/Delete/DV01
        public ActionResult Delete(string id)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var dichVu = db.DICHVU.Find(id);

            if (dichVu == null)
            {
                return HttpNotFound();
            }

            ViewBag.DaDuocSuDung = db.CHITIETDICHVUDAT.Any(c => c.MADV == id);
            ViewBag.SoLanDuocDat = db.CHITIETDICHVUDAT.Count(c => c.MADV == id);

            return View(dichVu);
        }

        // POST: DICHVU65134364/Delete/DV01
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var dichVu = db.DICHVU.Find(id);

            if (dichVu == null)
            {
                return HttpNotFound();
            }

            bool daDuocSuDung = db.CHITIETDICHVUDAT.Any(c => c.MADV == id);

            if (daDuocSuDung)
            {
                dichVu.TRANGTHAIKD = "Ngừng kinh doanh";
                db.Entry(dichVu).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Dịch vụ đã từng được sử dụng nên hệ thống chuyển sang trạng thái Ngừng kinh doanh.";
            }
            else
            {
                db.DICHVU.Remove(dichVu);
                db.SaveChanges();

                TempData["Success"] = "Xóa dịch vụ thành công.";
            }

            return RedirectToAction("Index");
        }

        private string TaoMaDichVuMoi()
        {
            var maCuoi = db.DICHVU
                .Where(d => d.MADV.StartsWith("DV"))
                .OrderByDescending(d => d.MADV)
                .Select(d => d.MADV)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(maCuoi))
            {
                return "DV01";
            }

            int so = 0;
            string phanSo = maCuoi.Replace("DV", "");

            int.TryParse(phanSo, out so);
            so++;

            return "DV" + so.ToString("00");
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