using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;

namespace QLSanPickleball_65132651.Controllers
{
    public class SaoLuuDuLieu65130311Controller : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult BackupDatabase()
        {
            try
            {
                string backupFolder = Server.MapPath("~/App_Data/Backup");

                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                string fileName = "QLSanPickleball_" +
                    DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak";

                string backupPath = Path.Combine(backupFolder, fileName);

                string connectionString =
                    @"Server=DESKTOP-531M556;Database=master;Trusted_Connection=True;";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = $@"
BACKUP DATABASE QLSanPickleball
TO DISK = '{backupPath}'
WITH INIT";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }

                ViewBag.Message = "Sao lưu dữ liệu thành công!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

            return View("Index");
        }
    }
}