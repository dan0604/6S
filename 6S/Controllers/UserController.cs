using _6S.Context;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _6S.Models;
using log4net.Repository.Hierarchy;
using log4net;
using System.Web.UI.WebControls;

namespace _6S.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        Model_6S db = new Model_6S();
        Share_All share_All = new Share_All();
        private ILog logger = LogManager.GetLogger(typeof(UserController));
        [HttpGet]
        public ActionResult Index_user()
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                if (Session["UserLogin"]?.ToString() != null && Session["NhomQuyen"]?.ToString() == "1000")
                {
                    var datalist = db.Tbl_User.ToList();
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
        public ActionResult Add_user(Tbl_User user)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                if (Session["UserLogin"]?.ToString() != null)
                {
                    if (ModelState.IsValid)
                    {
                        var md5_Password = share_All.GetMD5(user.Pass);
                        try
                        {
                            SqlParameter[] parameters_User = new SqlParameter[]
                            {
                                new SqlParameter("@Username", SqlDbType.VarChar, 30) { Value = user.Username },
                                new SqlParameter("@Pass", SqlDbType.VarChar, 50) { Value = md5_Password },
                                new SqlParameter("@Manv", SqlDbType.VarChar, 50) { Value = user.Manv },
                                new SqlParameter("@Fullname", SqlDbType.NVarChar, 50) { Value = user.Fullname },
                                new SqlParameter("@NhomQuyen", SqlDbType.Int) { Value = user.NhomQuyen },
                                new SqlParameter("@ID_PhongBan", SqlDbType.VarChar, 10) { Value = user.ID_PhongBan },
                                new SqlParameter("@Status", SqlDbType.Int) { Value = user.Status },
                            };
                            db.Database.ExecuteSqlCommand("EXEC sp_Insert_User @Username, @Pass, @Manv, @Fullname, @NhomQuyen, @ID_PhongBan, @Status", parameters_User);
                            logger.Info("Đã tạo tài khoản 6S thành công :" + user.Fullname + "User add: " + Session["Username"]?.ToString());
                            ViewBag.message = "Đã tạo tài khoản 6S thành công :" + user.Fullname;
                            return View("Index_user");
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Lỗi: ", ex);
                            ViewBag.error = "Lỗi :" + ex.Message;
                            return View("Index_user");
                        }
                    }
                    else
                    {
                        logger.Error("Lỗi: " + "ModelState");
                        ViewBag.error = "Tham số chưa hợp lệ";
                        return View("Index_user");
                    }
                }
                else
                {
                    logger.Error("Lỗi: " + "UserLogin?");
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
        public ActionResult Edit_user(string Username)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                if (Session["UserLogin"]?.ToString() != null && Session["NhomQuyen"]?.ToString() == "1000")
                {
                    // Tạo một đối tượng User
                    Tbl_User user = db.Tbl_User.SingleOrDefault(u => u.Username == Username); // Hàm này để lấy thông tin người dùng từ cơ sở dữ liệu
                    if (user != null)
                    {
                        // Trả về dữ liệu người dùng và danh sách phòng ban dưới dạng JSON
                        return Json(new
                        {
                            Username = user.Username,
                            Manv = user.Manv,
                            Fullname = user.Fullname,
                            NhomQuyen = user.NhomQuyen,
                            ID_PhongBan = user.ID_PhongBan,
                            Status = user.Status,
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
        public ActionResult Edit_user(Tbl_User user)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                if (Session["UserLogin"]?.ToString() != null && Session["NhomQuyen"]?.ToString() == "1000")
                {
                    if (ModelState.IsValid)
                    {
                        var check_User = db.Tbl_User.Where(x => x.Username == user.Username);
                        if (check_User != null)
                        {
                            try
                            {
                                SqlParameter[] parameters_User = new SqlParameter[]
                                {
                                    new SqlParameter("@Username", SqlDbType.VarChar, 30) { Value = user.Username },
                                    new SqlParameter("@Manv", SqlDbType.NVarChar, 50) { Value = user.Manv },
                                    new SqlParameter("@Fullname", SqlDbType.NVarChar, 50) { Value = user.Fullname },
                                    new SqlParameter("@NhomQuyen", SqlDbType.Int) { Value = user.NhomQuyen },
                                    new SqlParameter("@ID_PhongBan", SqlDbType.VarChar, 10) { Value = user.ID_PhongBan },
                                    new SqlParameter("@Status", SqlDbType.Int) { Value = user.Status },
                                };
                                db.Database.ExecuteSqlCommand("EXEC sp_Update_User @Username, @Manv, @Fullname, @NhomQuyen, @ID_PhongBan, @Status", parameters_User);
                                logger.Info("Đã sửa tài khoản 6S thành công :" + user.Fullname + "User add: " + Session["Username"]?.ToString());
                                var datalist = db.Tbl_User.ToList();
                                if (user.Status == 0)
                                {
                                    Session.Remove(user.Username);
                                    return Json(new
                                    {
                                        success = true,
                                        message = "Đã sửa tài khoản 6S thành công: " + user.Fullname,
                                        userList = datalist
                                    });
                                }
                                else
                                {
                                    return Json(new
                                    {
                                        success = true,
                                        message = "Đã sửa tài khoản 6S thành công: " + user.Fullname,
                                        userList = datalist
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Lỗi: ", ex);
                                var datalist = db.Tbl_User.ToList();
                                return Json(new
                                {
                                    success = false,
                                    userList = datalist,
                                    message = "Lỗi khi sửa tài khoản: " + ex.Message
                                });
                            }
                        }
                        else
                        {
                            logger.Error("Lỗi: " + "Tài khoản không có");
                            ViewBag.error = "Vui lòng kiểm tra lại tài khoản";
                            var datalist = db.Tbl_User.ToList();
                            return Json(new
                            {
                                success = false,
                                userList = datalist,
                                message = "Tài khoản không tồn tại"
                            });
                        }
                    }
                    else
                    {
                        logger.Error("Lỗi: " + "ModelState");
                        var datalist = db.Tbl_User.ToList();
                        return Json(new
                        {
                            success = false,
                            userList = datalist,
                            message = "Tham số chưa hợp lệ"
                        });
                    }
                }
                else
                {
                    logger.Error("Lỗi: " + "hết thời gian");
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
        public ActionResult Edit_Pass(string Username, string Pass_Old, string Pass_New, string Re_Pass_New)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                if (Session["UserLogin"]?.ToString() != null && Session["NhomQuyen"]?.ToString() == "1000")
                {
                    if (ModelState.IsValid)
                    {
                        if (Re_Pass_New != Pass_New)
                        {
                            logger.Error("Lỗi: " + "Nhập lại mật khẩu chưa khớp");
                            var datalist = db.Tbl_User.ToList();
                            return Json(new
                            {
                                success = false,
                                message = "Nhập lại mật khẩu chưa khớp",
                                userList = datalist
                            });
                        }
                        else
                        {
                            string md5_Pass_Old = share_All.GetMD5(Pass_Old);//mã hóa mật khẩu mới trước
                            var check_User_Pass = db.Tbl_User.Where(x => x.Username == Username && x.Pass == md5_Pass_Old).FirstOrDefault();///check tài khoản và mật khẩu khớp chưa
                            if (check_User_Pass != null)
                            {
                                if (check_User_Pass.Status == 0)
                                {
                                    logger.Error("Lỗi: " + "Status 0");
                                    var datalist = db.Tbl_User.ToList();
                                    return Json(new
                                    {
                                        success = false,
                                        message = "Tài khoản đã xóa vui lòng liên hệ Dev xử lý.",
                                        userList = datalist
                                    });
                                }
                                else
                                {
                                    string md5_Pass_New = share_All.GetMD5(Pass_New);
                                    if (check_User_Pass.Pass == md5_Pass_New)
                                    {
                                        logger.Error("Lỗi: " + "Mật khẩu củ");
                                        var datalist = db.Tbl_User.ToList();
                                        return Json(new
                                        {
                                            success = false,
                                            message = "Mật khẩu của bạn đang trùng khớp với mật khẩu hiển tại",
                                            userList = datalist
                                        });
                                    }
                                    else
                                    {
                                        try
                                        {
                                            SqlParameter[] parameters_User_Pass = new SqlParameter[]
                                            {
                                            new SqlParameter("@Username", SqlDbType.VarChar, 30) { Value = Username },
                                            new SqlParameter("@Pass", SqlDbType.VarChar, 50) { Value = md5_Pass_New },
                                            };
                                            db.Database.ExecuteSqlCommand("EXEC sp_Update_User_Pass @Username, @Pass", parameters_User_Pass);
                                            logger.Info("Đổi mật khẩu tài khoản 6S thành công :" + check_User_Pass.Fullname + "User add: " + Session["Username"]?.ToString());
                                            var datalist = db.Tbl_User.ToList();
                                            return Json(new
                                            {
                                                success = true,
                                                message = "Đổi mật khẩu tài khoản 6S thành công :" + check_User_Pass.Fullname,
                                                userList = datalist
                                            });
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Error("Lỗi: ", ex);
                                            var datalist = db.Tbl_User.ToList();
                                            return Json(new
                                            {
                                                success = false,
                                                message = "Lỗi: " + ex.Message,
                                                userList = datalist
                                            });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                logger.Error("Lỗi: " + "Tài khoản không có");
                                var datalist = db.Tbl_User.ToList();
                                return Json(new
                                {
                                    success = false,
                                    message = "Không có tài khoản",
                                    userList = datalist
                                });
                            }
                        }
                    }
                    else
                    {
                        logger.Error("Lỗi: " + "ModelState");
                        var datalist = db.Tbl_User.ToList();
                        return Json(new
                        {
                            success = false,
                            message = "Tham số chưa hợp lệ",
                            userList = datalist
                        });
                    }
                }
                else
                {
                    logger.Error("Lỗi: " + "hết thời gian");
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
        public ActionResult Reset_Pass(string Username)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                if (Session["NhomQuyen"]?.ToString() == "1000")
                {
                    try
                    {
                        var recordToReset_Pass = db.Tbl_User.Find(Username);
                        string md5_Reset_Pass = share_All.GetMD5(Username);
                        if (recordToReset_Pass != null)
                        {
                            recordToReset_Pass.Pass = md5_Reset_Pass;
                            db.Entry(recordToReset_Pass).Property(x => x.Pass).IsModified = true;
                            db.SaveChanges();
                            logger.Info("Reset_Pas User: " + Username + " " + " thành công" + "User reset: " + Session["Username"]?.ToString());
                            var datalist = db.Tbl_User.ToList();
                            return Json(new
                            {
                                success = true,
                                message = "Reset mật khẩu tài khoản 6S thành công :" + recordToReset_Pass.Fullname,
                                userList = datalist
                            });
                        }
                        else
                        {
                            logger.Error("Lỗi: " + "Danh sách rỗng");
                            var datalist = db.Tbl_User.ToList();
                            return Json(new
                            {
                                success = false,
                                message = "Lỗi: " + "Danh sách rỗng",
                                userList = datalist
                            });
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.Error("Lỗi: " + ex.Message);
                        var datalist = db.Tbl_User.ToList();
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
                    logger.Error("Lỗi: " + "Tài khoản không đủ quyền");
                    var datalist = db.Tbl_User.ToList();
                    return Json(new
                    {
                        success = false,
                        message = "Lỗi: " + "Tài khoản không đủ quyền",
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
        [HttpPost]
        public ActionResult Delete_User(string Username , int Status)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                if (Session["NhomQuyen"]?.ToString() == "1000")
                {
                    try
                    {
                        var recordToDelete = db.Tbl_User.Find(Username);
                        if (recordToDelete != null)
                        {
                            if (recordToDelete.Status == 0)
                            {
                                logger.Error("Lỗi: " + " Username đã về 0 :" + Username);
                                var datalist = db.Tbl_User.ToList();
                                return Json(new
                                {
                                    userList = datalist,
                                    success = false,
                                    message = "User đã được xóa trước đó :" + Username,
                                    redirectUrl = Url.Action("Index_user", "User")
                                });
                            }
                            else
                            {
                                recordToDelete.Status = Status;
                                db.Entry(recordToDelete).Property(x => x.Status).IsModified = true;
                                db.SaveChanges();
                                logger.Info("Xóa User: " + Username + " " + "trạng thái: " + Status + " thành công" + "User del: " + Session["Username"]?.ToString());
                                var datalist = db.Tbl_User.ToList();
                                return Json(new
                                {
                                    userList = datalist,
                                    success = true,
                                    message = "Đã xóa User 6S thành công :" + Username,
                                    redirectUrl = Url.Action("Index_user", "User")
                                });
                            }
                        }
                        else
                        {
                            logger.Error("Lỗi: " + "Không có Username :" + Username);
                            var datalist = db.Tbl_User.ToList();
                            return Json(new
                            {
                                userList = datalist,
                                success = false,
                                message = "Đã xóa User 6S không thành công :" + Username,
                                redirectUrl = Url.Action("Index_user", "User")
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Lỗi: ", ex);
                        var datalist = db.Tbl_User.ToList();
                        return Json(new
                        {
                            userList = datalist,
                            success = false,
                            message = ex.Message,
                            redirectUrl = Url.Action("Index_user", "User")
                        });
                    }
                }
                else
                {
                    logger.Error("Lỗi: " + "Tài khoản không đủ quyền");
                    var datalist = db.Tbl_User.ToList();
                    return Json(new
                    {
                        success = false,
                        message = "Lỗi: " + "Tài khoản không đủ quyền",
                        userList = datalist,
                        redirectUrl = Url.Action("Index_user", "User")
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