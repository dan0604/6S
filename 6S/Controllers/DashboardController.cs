using _6S.Context;
using _6S.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Text;
using Newtonsoft.Json;
using System.Web.UI.WebControls;

namespace _6S.Controllers
{
    public class DashboardController : Controller
    {
        Model_6S db = new Model_6S();
        Share_All share_All = new Share_All();
        private ILog logger = LogManager.GetLogger(typeof(DashboardController));
        // GET: Dashboard
        [HttpGet]
        public ActionResult Dashboard_checklist()
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                return View();
            }
            else
            {
                logger.Error("Lỗi: " + "xác thực !checkAccount");
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpGet]
        public ActionResult view_Checklist(string ThangNam)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                List<BC6SView> List_BC6SView = new List<BC6SView>();
                try
                {
                    string jsonData = share_All.DecodeBase64String(ThangNam);
                    string inputThangNam = jsonData;
                    string outputThangNam = jsonData.Replace("-", ""); // chuỗi liền 202310
                    DateTime dateTime = DateTime.ParseExact(inputThangNam, "yyyy-MM", null);
                    string outputString = dateTime.ToString("MM/yyyy");
                    using (var db = new Model_6S())
                    {
                        SqlParameter[] parameters_BC6SView = new SqlParameter[]
                        {
                            new SqlParameter("@thangnam", SqlDbType.VarChar, 10){ Value = outputThangNam},
                            new SqlParameter("@thangnam2", SqlDbType.VarChar, 10){ Value = outputString},
                        };
                        List_BC6SView = db.Database.SqlQuery<BC6SView>("EXEC dbo.proc_BaoCaoCham6S @thangnam, @thangnam2", parameters_BC6SView).ToList();
                    }
                    if(List_BC6SView.Count > 0)
                    {
                        try
                        {
                            DataTable dataTable = new DataTable();
                            dataTable.Columns.Add("Tenkhoi");
                            dataTable.Columns.Add("TenPhongBan");
                            dataTable.Columns.Add("MucTieu");
                            dataTable.Columns.Add("DiemChamNoiBo");
                            dataTable.Columns.Add("TongHinhChamNoiBo");
                            dataTable.Columns.Add("DiemChamCheo");
                            dataTable.Columns.Add("TongHinhChamCheo");
                            dataTable.Columns.Add("DiemChamHieuChinh");
                            dataTable.Columns.Add("TongHinhChamHieuChinh");
                            // Thêm cột mới
                            dataTable.Columns.Add("TongDiemThang");
                            dataTable.Columns.Add("DiemTrungBinhKhoi");
                            // Chạy vòng để tính giá trị cho cột mới
                            foreach (var item in List_BC6SView)
                            {
                                // Thêm dữ liệu vào từng cột như bạn đã làm trước đó
                                dataTable.Rows.Add(item.Tenkhoi, item.TenPhongBan, item.MucTieu, item.DiemChamNoiBo, item.TongHinhChamNoiBo, item.DiemChamCheo, item.TongHinhChamCheo, item.DiemChamHieuChinh, item.TongHinhChamHieuChinh);

                                // Tính giá trị cho cột TongDiemThang
                                int DiemChamNoiBo = Convert.ToInt32(item.DiemChamNoiBo);
                                int DiemChamCheo = Convert.ToInt32(item.DiemChamCheo);
                                int DiemChamHieuChinh = Convert.ToInt32(item.DiemChamHieuChinh);
                                int TongDiemThang = 0;

                                if (DiemChamHieuChinh != 0)
                                {
                                    TongDiemThang = DiemChamHieuChinh;
                                }
                                else if (DiemChamCheo != 0)
                                {
                                    TongDiemThang = DiemChamCheo;
                                }
                                else if (DiemChamNoiBo != 0)
                                {
                                    TongDiemThang = DiemChamNoiBo;
                                }
                                else
                                {
                                    logger.Error("Lỗi: giá trị TongDiemThang không hợp lệ");
                                    return Json(new { success = false, message = "Lỗi: Giá trị không hợp lệ " + inputThangNam }, JsonRequestBehavior.AllowGet);
                                }
                                // Thêm giá trị vào cột TongDiemThang
                                dataTable.Rows[dataTable.Rows.Count - 1]["TongDiemThang"] = TongDiemThang;
                            }
                            // Tính cột DiemTrungBinhKhoi
                            var groups = dataTable.AsEnumerable().GroupBy(row => row.Field<string>("Tenkhoi"));
                            foreach (var group in groups)
                            {
                                int sum = 0;
                                int count = 0;

                                foreach (DataRow row in group)
                                {
                                    int TongDiemThang;
                                    if (int.TryParse(row.Field<string>("TongDiemThang"), out TongDiemThang))
                                    {
                                        sum += TongDiemThang;
                                        count++;
                                    }
                                    else
                                    {
                                        // Xử lý trường hợp không thể chuyển đổi giá trị sang kiểu int
                                        logger.Error("Lỗi: Sai kiểu dữ liệu");
                                        return Json(new { success = false, message = "Lỗi: Sai kiểu dữ liệu" + inputThangNam }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                //count lần xuất hiện
                                if (count > 0)
                                {
                                    int average = sum / count;

                                    foreach (DataRow row in group)
                                    {
                                        row["DiemTrungBinhKhoi"] = average;
                                    }
                                }
                                else
                                {
                                    // Xử lý trường hợp count = 0 (không có giá trị hợp lệ để tính trung bình)
                                    logger.Error("Lỗi: không có giá trị hợp lệ để tính trung bình");
                                    return Json(new { success = false, message = "Lỗi: Không có giá trị hợp lệ để tính trung bình" + inputThangNam });
                                }
                            }
                            // Chuyển đổi đối tượng JSON thành chuỗi JSON
                            string json = JsonConvert.SerializeObject(dataTable);
                            // Mã hóa chuỗi JSON thành Base64
                            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                            string encodedJson = Convert.ToBase64String(jsonBytes);
                            // Trả về chuỗi JSON đã được mã hóa Base64
                            return Json(new { success = true, message = "Lấy dữ liệu thành công báo cáo tháng " + inputThangNam, data = encodedJson }, JsonRequestBehavior.AllowGet);
                        }
                        catch(Exception ex)
                        {
                            logger.Error("Lỗi: ", ex);
                            return Json(new { success = false, message = "Lỗi: " + ex.Message}, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        logger.Error("Lỗi: Dữ liệu rỗng" + inputThangNam);
                        return Json(new { success = false, message = "Lỗi: Không có data trong tháng " + inputThangNam}, JsonRequestBehavior.AllowGet);
                    }
                }
                catch(Exception ex)
                {
                    logger.Error("Lỗi: ", ex);
                    return Json(new { success = false, message = "Lỗi: " + ex.Message}, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                logger.Error("Lỗi: " + "xác thực !checkAccount");
                return RedirectToAction("Login", "Login");
            }
        }
    }
}