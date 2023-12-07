using _6S.Context;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using log4net.Repository.Hierarchy;
using log4net;
using _6S.Models;

namespace _6S.Controllers
{
    public class PhongBanController : Controller
    {
        // GET: PhongBan
        Model_6S db = new Model_6S();
        Share_All share_All = new Share_All();
        private ILog logger = LogManager.GetLogger(typeof(PhongBanController));
        public ActionResult Index_PhongBan()
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                if (Session["UserLogin"].ToString() != null && Session["NhomQuyen"]?.ToString() == "1000")
                {
                    var datalist = db.Tbl_PhongBan.ToList();
                    logger.Info("Đã lấy danh sách Tbl_PhongBan thành công:");
                    return View(datalist);
                }
                else
                {
                    logger.Error("Lỗi: " + "UserLogin? && NhomQuyen != 1000");
                    return RedirectToAction("Login", "Login");
                }
            }
            else
            {
                logger.Error("Lỗi: " + "xác thực !checkAccount");
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpPost]
        public ActionResult Add_PhongBan(Tbl_PhongBan PhongBan, HttpPostedFile filesPhongBan)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                if (Session["UserLogin"].ToString() != null && Session["NhomQuyen"]?.ToString() == "1000")
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            SqlParameter[] parameters_PhongBan = new SqlParameter[]
                                {
                                new SqlParameter("@ID_PhongBan", SqlDbType.VarChar, 10) { Value = PhongBan.ID_PhongBan },
                                new SqlParameter("@TenPhongBan", SqlDbType.VarChar, 20) { Value = PhongBan.TenPhongBan }, // Sử dụng rowCount
                                new SqlParameter("@TuNgay", SqlDbType.Date) { Value = PhongBan.TuNgay },
                                new SqlParameter("@DenNgay", SqlDbType.Date) { Value = PhongBan.DenNgay },
                                new SqlParameter("@Status", SqlDbType.Int) { Value = PhongBan.Status },
                                };
                            db.Database.ExecuteSqlCommand("EXEC sp_Insert_PhongBan @ID_PhongBan, @TenPhongBan, @TuNgay, @DenNgay, @Status", parameters_PhongBan);
                            logger.Info("Đã tạo phòng ban 6S thành công :" + PhongBan.TenPhongBan + "User add: " + Session["Username"]?.ToString());
                            ViewBag.message = "Đã tạo phòng ban 6S thành công :" + PhongBan.TenPhongBan;
                            return View("Index_PhongBan");
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Lỗi: ", ex);
                            ViewBag.error = "Lỗi: " + ex.Message;
                            return View("Index_PhongBan");
                        }
                    }
                    else
                    {
                        logger.Error("Lỗi: ModelState");
                        ViewBag.error = "Tham số không hợp lệ";
                        return View("Index_PhongBan");
                    }
                }
                else
                {
                    logger.Error("Lỗi: " + "UserLogin? && NhomQuyen != 1000");
                    return RedirectToAction("Login", "Login");
                }
            }
            else
            {
                logger.Error("Lỗi: " + "xác thực !checkAccount");
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpGet]
        public ActionResult Edit_PhongBan(string ID_PhongBan)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                if (Session["UserLogin"]?.ToString() != null && Session["NhomQuyen"]?.ToString() == "1000")
                {
                    // Tạo một đối tượng User
                    Tbl_PhongBan phongban = db.Tbl_PhongBan.SingleOrDefault(u => u.ID_PhongBan == ID_PhongBan); // Hàm này để lấy thông tin Phòng ban từ cơ sở dữ liệu
                    if (phongban != null)
                    {
                        // Trả về dữ liệu Phòng ban và danh sách phòng ban dưới dạng JSON
                        return Json(new
                        {
                            ID_PhongBan = ID_PhongBan,
                            TenPhongBan = phongban.TenPhongBan,
                            TuNgay = phongban.TuNgay,
                            DenNgay = phongban.DenNgay,
                            Status = phongban.Status,
                            Ma_Khoi = phongban.Ma_Khoi,
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new
                        {
                            error = "Phòng ban không tồn tại."
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    logger.Error("Lỗi: " + "UserLogin? && NhomQuyen != 1000");
                    return RedirectToAction("Login", "Login");
                }
            }
            else
            {
                logger.Error("Lỗi: " + "xác thực !checkAccount");
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpPost]
        public ActionResult Edit_PhongBan(Tbl_PhongBan phongban)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                if (Session["UserLogin"].ToString() != null && Session["NhomQuyen"]?.ToString() == "1000")
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            SqlParameter[] parameters_PhongBan = new SqlParameter[]
                                 {
                                new SqlParameter("@ID_PhongBan", SqlDbType.VarChar, 10) { Value = phongban.ID_PhongBan },
                                new SqlParameter("@TenPhongBan", SqlDbType.VarChar, 20) { Value = phongban.TenPhongBan }, // Sử dụng rowCount
                                new SqlParameter("@TuNgay", SqlDbType.Date) { Value = phongban.TuNgay },
                                new SqlParameter("@DenNgay", SqlDbType.Date) { Value = phongban.DenNgay },
                                new SqlParameter("@Status", SqlDbType.Int) { Value = phongban.Status },
                                new SqlParameter("@Ma_Khoi", SqlDbType.VarChar) { Value = phongban.Ma_Khoi },
                                 };
                            db.Database.ExecuteSqlCommand("EXEC sp_Update_PhongBan @ID_PhongBan, @TenPhongBan, @TuNgay, @DenNgay, @Status, @Ma_Khoi", parameters_PhongBan);
                            logger.Info("Đã sửa phòng ban 6S thành công :" + phongban.TenPhongBan + "User edit: " + Session["Username"]?.ToString());
                            var datalist = db.Tbl_PhongBan.ToList();
                            return Json(new
                            {
                                success = true,
                                message = "Đã sửa phòng ban 6S thành công :" + phongban.TenPhongBan,
                                userList = datalist
                            });
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Lỗi: ", ex);
                            var datalist = db.Tbl_PhongBan.ToList();
                            return Json(new
                            {
                                success = false,
                                message = "Lỗi: " + ex.Message,
                                userList = datalist
                            });
                        }
                    }
                    else
                    {
                        logger.Error("Lỗi: ModelState");
                        var datalist = db.Tbl_PhongBan.ToList();
                        return Json(new
                        {
                            success = false,
                            message = "Tham số không hợp lệ",
                            userList = datalist
                        });
                    }
                }
                else
                {
                    logger.Error("Lỗi: " + "UserLogin? && NhomQuyen != 1000");
                    return RedirectToAction("Login", "Login");
                }
            }
            else
            {
                logger.Error("Lỗi: " + "xác thực !checkAccount");
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpPost]
        public ActionResult Delete_PhongBan(string ID_PhongBan , int Status)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                try
                {
                    var recordToDelete = db.Tbl_PhongBan.Find(ID_PhongBan);
                    if (recordToDelete != null)
                    {
                        if (recordToDelete.Status == 0)
                        {
                            logger.Error("Lỗi: " + " ID_PhongBan đã về 0 :" + ID_PhongBan);
                            var datalist = db.Tbl_PhongBan.ToList();
                            return Json(new
                            {
                                success = false,
                                message = "PhongBan đã được xóa trước đó: " + ID_PhongBan,
                                redirectUrl = Url.Action("Index_NhomQuyen", "NhomQuyen"),
                                userList = datalist
                            });
                        }
                        else
                        {
                            recordToDelete.Status = Status;
                            db.Entry(recordToDelete).Property(x => x.Status).IsModified = true;
                            logger.Info("Xóa PhongBan: " + ID_PhongBan + " " + "trạng thái: " + Status + " thành công" + "User del: " + Session["Username"]?.ToString());
                            db.SaveChanges();
                            var datalist = db.Tbl_PhongBan.ToList();
                            return Json(new
                            {
                                success = true,
                                message = "Đã xóa PhongBan 6S thành công :" + ID_PhongBan,
                                redirectUrl = Url.Action("Index_PhongBan", "PhongBan"),
                                userList = datalist
                            });
                        }
                    }
                    else
                    {
                        logger.Error("Lỗi: " + "Không có ID_PhongBan :" + ID_PhongBan);
                        var datalist = db.Tbl_PhongBan.ToList();
                        return Json(new
                        {
                            success = false,
                            message = "Đã xóa PhongBan 6S không thành thành công :" + ID_PhongBan,
                            redirectUrl = Url.Action("Index_PhongBan", "PhongBan"),
                            userList = datalist
                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Lỗi: ", ex);
                    var datalist = db.Tbl_PhongBan.ToList();
                    return Json(new
                    {
                        success = false,
                        message = ex.Message,
                        redirectUrl = Url.Action("Index_PhongBan", "PhongBan"),
                        userList = datalist
                    });
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