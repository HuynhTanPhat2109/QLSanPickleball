using QLSanPickleball_65132651.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace QLSanPickleball_65132651.Controllers
{
    public class THAMSO65134364Controller : Controller
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
                TempData["Error"] = "Bạn không có quyền sử dụng chức năng tham số hệ thống.";
                return RedirectToAction("HomeNv", "Admin65134364");
            }

            return null;
        }

        // GET: THAMSO65134364
        public ActionResult Index(string search, string tinhTrang, int page = 1)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }

            var dsThamSo = db.THAMSO.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                dsThamSo = dsThamSo.Where(t =>
                    t.MATHAMSO.Contains(search) ||
                    t.TENTHAMSO.Contains(search) ||
                    t.KIEU_DVT.Contains(search) ||
                    t.GIATRI.Contains(search) ||
                    t.TINHTRANG.Contains(search)
                );
            }

            if (!string.IsNullOrWhiteSpace(tinhTrang))
            {
                dsThamSo = dsThamSo.Where(t => t.TINHTRANG == tinhTrang);
            }

            int tongSoThamSo = dsThamSo.Count();
            int tongSoTrang = (int)Math.Ceiling((double)tongSoThamSo / pageSize);

            if (tongSoTrang == 0)
            {
                tongSoTrang = 1;
            }

            if (page > tongSoTrang)
            {
                page = tongSoTrang;
            }

            var ketQua = dsThamSo
                .OrderBy(t => t.MATHAMSO)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.TinhTrang = tinhTrang;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = tongSoTrang;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = tongSoThamSo;

            ViewBag.TongThamSo = db.THAMSO.Count();
            ViewBag.DangApDung = db.THAMSO.Count(t => t.TINHTRANG == "Đang áp dụng");
            ViewBag.NgungApDung = db.THAMSO.Count(t => t.TINHTRANG == "Ngừng áp dụng");

            return View(ketQua);
        }

        // GET: THAMSO65134364/Details/TS01
        public ActionResult Details(string id)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var thamSo = db.THAMSO.Find(id);

            if (thamSo == null)
            {
                return HttpNotFound();
            }

            return View(thamSo);
        }

        // GET: THAMSO65134364/Edit/TS01
        public ActionResult Edit(string id)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var thamSo = db.THAMSO.Find(id);

            if (thamSo == null)
            {
                return HttpNotFound();
            }

            ViewBag.TinhTrangList = new SelectList(new[]
            {
                "Đang áp dụng",
                "Ngừng áp dụng"
            }, thamSo.TINHTRANG);

            return View(thamSo);
        }

        // POST: THAMSO65134364/Edit/TS01
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MATHAMSO,TENTHAMSO,KIEU_DVT,GIATRI,TINHTRANG")] THAMSO thamSo)
        {
            var check = KiemTraQuyenAdmin();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(thamSo.MATHAMSO))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(thamSo.TENTHAMSO))
            {
                ModelState.AddModelError("TENTHAMSO", "Vui lòng nhập tên tham số.");
            }

            if (string.IsNullOrWhiteSpace(thamSo.KIEU_DVT))
            {
                ModelState.AddModelError("KIEU_DVT", "Vui lòng nhập kiểu/đơn vị tính.");
            }

            if (string.IsNullOrWhiteSpace(thamSo.GIATRI))
            {
                ModelState.AddModelError("GIATRI", "Vui lòng nhập giá trị tham số.");
            }

            if (string.IsNullOrWhiteSpace(thamSo.TINHTRANG))
            {
                thamSo.TINHTRANG = "Đang áp dụng";
            }

            if (ModelState.IsValid)
            {
                db.Entry(thamSo).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Cập nhật tham số hệ thống thành công.";
                return RedirectToAction("Index");
            }

            ViewBag.TinhTrangList = new SelectList(new[]
            {
                "Đang áp dụng",
                "Ngừng áp dụng"
            }, thamSo.TINHTRANG);

            return View(thamSo);
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