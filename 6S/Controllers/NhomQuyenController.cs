using _6S.Context;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using log4net;
using System.Web.UI.WebControls;
using _6S.Models;

namespace _6S.Controllers
{
    public class NhomQuyenController : Controller
    {
        // GET: NhomQuyen
        Model_6S db = new Model_6S();
        Share_All share_All = new Share_All();
        private ILog logger = LogManager.GetLogger(typeof(NhomQuyenController));
        public ActionResult Index_NhomQuyen()
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                if (Session["UserLogin"].ToString() != null && Session["NhomQuyen"]?.ToString() == "1000")
                {
                    var datalist = db.Tbl_NhomQuyen.ToList();
                    logger.Info("Đã lấy được danh sách: Index_NhomQuyen" + "User view: " + Session["Username"]?.ToString());
                    return View(datalist);
                }
                else
                {
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
        public ActionResult Add_NhomQuyen(Tbl_NhomQuyen NhomQuyen)
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
                            SqlParameter[] parameters_NhomQuyen = new SqlParameter[]
                                {
                                    new SqlParameter("@ID_NhomQuyen", SqlDbType.Int) { Value = NhomQuyen.ID_NhomQuyen },
                                    new SqlParameter("@IDMenu", SqlDbType.VarChar, 10) { Value = NhomQuyen.IDMenu },
                                    new SqlParameter("@Status", SqlDbType.Int) { Value = NhomQuyen.Status },
                                };
                            db.Database.ExecuteSqlCommand("EXEC sp_Insert_NhomQuyen @ID_NhomQuyen, @IDMenu, @Status", parameters_NhomQuyen);
                            logger.Info("Đã tạo nhóm quyền 6S thành công :" + NhomQuyen.ID_NhomQuyen + "User add: " + Session["Username"]?.ToString());
                            ViewBag.message = "Đã tạo nhóm quyền 6S thành công :" + NhomQuyen.ID_NhomQuyen;
                            return View("Index_NhomQuyen");
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Lỗi: ", ex);
                            ViewBag.error = "Lỗi: " + ex.Message;
                            return View("Index_NhomQuyen");
                        }
                    }
                    else
                    {
                        ViewBag.error = "Tham số không hợp lệ";
                        return View("Index_NhomQuyen");
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
        public ActionResult Edit_NhomQuyen(int ID_NhomQuyen , string IDMenu)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                if (Session["UserLogin"]?.ToString() != null && Session["NhomQuyen"]?.ToString() == "1000")
                {
                    // Tạo một đối tượng User
                    Tbl_NhomQuyen nhomquyen = db.Tbl_NhomQuyen.SingleOrDefault(u => u.ID_NhomQuyen == ID_NhomQuyen && u.IDMenu == IDMenu); // Hàm này để lấy thông tin người dùng từ cơ sở dữ liệu
                    if (nhomquyen != null)
                    {
                        // Trả về dữ liệu người dùng và danh sách phòng ban dưới dạng JSON
                        return Json(new
                        {
                            ID_NhomQuyen = nhomquyen.ID_NhomQuyen,
                            IDMenu = nhomquyen.IDMenu,
                            Status = nhomquyen.Status,
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new
                        {
                            error = "Người dùng không tồn tại."
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
        public ActionResult Edit_NhomQuyen(Tbl_NhomQuyen NhomQuyen)
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
                            SqlParameter[] parameters_NhomQuyen = new SqlParameter[]
                                {
                                new SqlParameter("@ID_NhomQuyen", SqlDbType.Int) { Value = NhomQuyen.ID_NhomQuyen },
                                    new SqlParameter("@IDMenu", SqlDbType.VarChar, 10) { Value = NhomQuyen.IDMenu },
                                    new SqlParameter("@Status", SqlDbType.Int) { Value = NhomQuyen.Status },
                                };
                            db.Database.ExecuteSqlCommand("EXEC sp_Update_NhomQuyen @ID_NhomQuyen, @IDMenu, @Status", parameters_NhomQuyen);
                            logger.Info("Đã sửa nhóm quyền 6S thành công :" + NhomQuyen.ID_NhomQuyen + "User edit: " + Session["Username"]?.ToString());
                            var datalist = db.Tbl_NhomQuyen.ToList();
                            return Json(new
                            {
                                success = true,
                                message = "Đã sửa nhóm quyền 6S thành công :" + NhomQuyen.ID_NhomQuyen,
                                userList = datalist
                            });
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Lỗi: ", ex);
                            var datalist = db.Tbl_NhomQuyen.ToList();
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
                        logger.Error("Lỗi: " + "lỗi ModelState");
                        var datalist = db.Tbl_NhomQuyen.ToList();
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
        public ActionResult Delete_NhomQuyen(int? ID_NhomQuyen, string IDMenu, int? Status)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                try
                {
                    var recordToDelete = db.Tbl_NhomQuyen
                        .Where(nq => nq.ID_NhomQuyen == ID_NhomQuyen && nq.IDMenu == IDMenu)
                        .FirstOrDefault();
                    if (recordToDelete != null)
                    {
                        if (recordToDelete.Status == 0)
                        {
                            logger.Error("Lỗi: " + " ID_NhomQuyen đã về 0 :" + ID_NhomQuyen);
                            var datalist = db.Tbl_NhomQuyen.ToList();
                            return Json(new
                            {
                                success = false,
                                message = "NhomQuyen đã được xóa trước đó: " + ID_NhomQuyen,
                                redirectUrl = Url.Action("Index_NhomQuyen", "NhomQuyen"),
                                userList = datalist
                            });
                        }
                        else
                        {
                            recordToDelete.Status = Status;
                            db.Entry(recordToDelete).Property(x => x.Status).IsModified = true;
                            logger.Info("Xóa NhomQuyen: " + ID_NhomQuyen + " " + "trạng thái: " + Status + " thành công" + "User del: " + Session["Username"]?.ToString());
                            db.SaveChanges();
                            var datalist = db.Tbl_NhomQuyen.ToList();
                            return Json(new
                            {
                                success = true,
                                message = "Đã xóa NhomQuyen 6S thành công :" + ID_NhomQuyen,
                                redirectUrl = Url.Action("Index_NhomQuyen", "NhomQuyen"),
                                userList = datalist
                            });
                        }
                    }
                    else
                    {
                        logger.Error("Lỗi: " + "không có ID_NhomQuyenn :" + ID_NhomQuyen);
                        var datalist = db.Tbl_NhomQuyen.ToList();
                        return Json(new
                        {
                            success = false,
                            message = "Đã xóa NhomQuyen 6S không thành thành công :" + ID_NhomQuyen,
                            redirectUrl = Url.Action("Index_NhomQuyen", "NhomQuyen"),
                            userList = datalist
                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Lỗi: ", ex);
                    var datalist = db.Tbl_NhomQuyen.ToList();
                    return Json(new
                    {
                        success = false,
                        message = ex.Message,
                        redirectUrl = Url.Action("Index_NhomQuyen", "NhomQuyen"),
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