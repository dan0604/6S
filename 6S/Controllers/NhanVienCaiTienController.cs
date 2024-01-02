using _6S.Context;
using _6S.Models;
using log4net;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _6S.Controllers
{
    public class NhanVienCaiTienController : Controller
    {
        Model_6S db = new Model_6S();
        Share_All share_All = new Share_All();
        private ILog logger = LogManager.GetLogger(typeof(NhanVienCaiTienController));
        // GET: NhanVienCaiTien
        [HttpGet]
        public ActionResult Index_NhanVienCaiTien()
        {
            var checkAccount = share_All.CheckAccount(Session["ID"]?.ToString());
            if (checkAccount == true)
            {
                var datalist = db.Tbl_NhanVienCaiTien.ToList();
                logger.Info("Đã lấy được danh sách: Index_NhanVienCaiTien" + "NhanVienCaiTien view: " + Session["ID"]?.ToString());
                return View(datalist);
            }
            else
            {
                logger.Error("Lỗi: " + "xác thực !checkAccount");
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpPost]
        public ActionResult Add_NhanVienCaiTien( Tbl_NhanVienCaiTien NhanVienCaiTien)
        {
            var checkAccount = share_All.CheckAccount(Session["ID"]?.ToString());
            if (checkAccount == true)
            {
                try
                {
                    SqlParameter[] parameters_NhanVienCaiTien = new SqlParameter[]
                    {
                                new SqlParameter("@TenNV", SqlDbType.NVarChar, 50) { Value = NhanVienCaiTien.TenNV },
                    };
                    db.Database.ExecuteSqlCommand("EXEC sp_Insert_NhanVienCaiTien @TenNV",parameters_NhanVienCaiTien);
                    logger.Info("Đã tạo Nhân viên cải tiến 6S thành công :" + NhanVienCaiTien.TenNV + "User add: " + Session["Username"]?.ToString());
                    var datalist = db.Tbl_NhanVienCaiTien.ToList();
                    return Json(new
                    {
                        NhanVienCaiTienList = datalist,
                        success = true,
                        message = "Đã tạo Nhân viên cải tiến 6S thành công :" + NhanVienCaiTien.TenNV,
                        redirectUrl = Url.Action("Index_NhanVienCaiTien", "NhanVienCaiTien")
                    });
                }
                catch (Exception ex)
                {
                    logger.Error("Lỗi: ", ex);
                    var datalist = db.Tbl_NhanVienCaiTien.ToList();
                    return Json(new
                    {
                        NhanVienCaiTienList = datalist,
                        success = true,
                        message = "Lỗi :" + ex.Message,
                        redirectUrl = Url.Action("Index_NhanVienCaiTien", "NhanVienCaiTien")
                    });
                }
            }
            else
            {
                logger.Error("Lỗi: " + "xác thực !checkAccount");
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpGet]
        public ActionResult Edit_NhanVienCaiTien(int ID)
        {
            var checkAccount = share_All.CheckAccount(Session["ID"]?.ToString());
            if (checkAccount == true)
            {
                var data = db.Tbl_NhanVienCaiTien.SingleOrDefault(x => x.ID == ID);
                if (data != null)
                {
                    return Json(new
                    {
                        ID = data.ID,
                        TenNV = data.TenNV,
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        error = "Nhân viên cải tiến không tồn tại."
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                logger.Error("Lỗi: " + "xác thực !checkAccount");
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpPost]
        public ActionResult Edit_NhanVienCaiTien(Tbl_NhanVienCaiTien NhanVienCaiTien , int ID)
        {
            var checkAccount = share_All.CheckAccount(Session["ID"]?.ToString());
            if (checkAccount == true)
            {
                try
                {
                    var recordToUpdate = db.Tbl_NhanVienCaiTien.Find(ID);
                    if (recordToUpdate != null)
                    {
                        recordToUpdate.TenNV = NhanVienCaiTien.TenNV;
                        db.Entry(recordToUpdate).Property(x => x.TenNV).IsModified = true;
                        db.SaveChanges();
                        logger.Info("cập nhật NhanVienCaiTien: " + ID + " " + " thành công" + "NhanVienCaiTien Update: " + Session["ID"]?.ToString());
                        var datalist = db.Tbl_NhanVienCaiTien.ToList();
                        return Json(new
                        {
                            NhanVienCaiTienList = datalist,
                            success = true,
                            message = "Đã cập nhật NhanVienCaiTien 6S thành công :" + ID,
                            redirectUrl = Url.Action("Index_NhanVienCaiTien", "NhanVienCaiTien")
                        });
                    }
                    else
                    {
                        logger.Error("Lỗi: " + "Không có ID :" + ID);
                        var datalist = db.Tbl_NhanVienCaiTien.ToList();
                        return Json(new
                        {
                            NhanVienCaiTienList = datalist,
                            success = false,
                            message = "Đã cập nhật NhanVienCaiTien 6S không thành công :" + ID,
                            redirectUrl = Url.Action("Index_NhanVienCaiTien", "NhanVienCaiTien")
                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Lỗi: ", ex);
                    var datalist = db.Tbl_NhanVienCaiTien.ToList();
                    return Json(new
                    {
                        NhanVienCaiTienList = datalist,
                        success = false,
                        message = ex.Message,
                        redirectUrl = Url.Action("Index_NhanVienCaiTien", "NhanVienCaiTien")
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
        public ActionResult Delete_NhanVienCaiTien(int ID)
        {
            var checkAccount = share_All.CheckAccount(Session["ID"]?.ToString());
            if (checkAccount == true)
            {
                try
                {
                    var recordToDelete = db.Tbl_NhanVienCaiTien.Find(ID);
                    if (recordToDelete != null)
                    {
                        db.Tbl_NhanVienCaiTien.Remove(recordToDelete);
                        db.SaveChanges();
                        logger.Info("Xóa NhanVienCaiTien: " + ID + " " + " thành công" + "NhanVienCaiTien del: " + Session["ID"]?.ToString());
                        var datalist = db.Tbl_NhanVienCaiTien.ToList();
                        return Json(new
                        {
                            NhanVienCaiTienList = datalist,
                            success = true,
                            message = "Đã xóa NhanVienCaiTien 6S thành công :" + ID,
                            redirectUrl = Url.Action("Index_NhanVienCaiTien", "NhanVienCaiTien")
                        });
                    }
                    else
                    {
                        logger.Error("Lỗi: " + "Không có ID :" + ID);
                        var datalist = db.Tbl_NhanVienCaiTien.ToList();
                        return Json(new
                        {
                            NhanVienCaiTienList = datalist,
                            success = false,
                            message = "Đã xóa NhanVienCaiTien 6S không thành công :" + ID,
                            redirectUrl = Url.Action("Index_NhanVienCaiTien", "NhanVienCaiTien")
                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Lỗi: ", ex);
                    var datalist = db.Tbl_NhanVienCaiTien.ToList();
                    return Json(new
                    {
                        NhanVienCaiTienList = datalist,
                        success = false,
                        message = ex.Message,
                        redirectUrl = Url.Action("Index_NhanVienCaiTien", "NhanVienCaiTien")
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