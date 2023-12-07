using _6S.Context;
using _6S.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _6S.Controllers
{
    public class MucTieuController : Controller
    {
        Model_6S db = new Model_6S();
        Share_All share_All = new Share_All();
        private ILog logger = LogManager.GetLogger(typeof(MucTieuController));
        // GET: MucTieu
        [HttpGet]
        public ActionResult Index_MucTieu()
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                try
                {
                    string Username = Session["Username"]?.ToString();
                    object nhomQuyenObject = Session["NhomQuyen"];
                    string nhomQuyenString = nhomQuyenObject != null ? nhomQuyenObject.ToString() : null;
                    // Sử dụng Select để lấy ra danh sách các chuỗi từ thuộc tính IDMenu
                    List<string> quyensCanKiemTra = db.Tbl_MoTaNhomQuyen
                        .Where(x => x.ID_NhomQuyen.ToString() == nhomQuyenString && x.Status == 1)
                        .Select(x => x.Quyen)
                        .ToList();
                    //check quyền user để hiển thị danh sách
                    Dictionary<Tuple<string, int>, bool> User_Permissions_Quyen_dataList = share_All.User_Permissions(Session["NhomQuyen"]?.ToString(), quyensCanKiemTra);
                    bool User_Root = User_Permissions_Quyen_dataList.Any(pair => pair.Key.Item2 == 1000);
                    if (User_Root)
                    {
                        var query = from muctieu in db.Tbl_MucTieu
                                    from phongban in db.Tbl_PhongBan
                                    where muctieu.ID_PhongBan == phongban.ID_PhongBan
                                    select new Tbl_Join_ALL { PhongBan = phongban , MucTieu = muctieu };
                        logger.Info("Đã lấy được danh sách ở 1000  && !Status: Index_MucTieu" + "User view: " + Session["Username"]?.ToString());
                        Dictionary<string, bool> quyenButtonsDict = share_All.GetQuyenButtonsDict(quyensCanKiemTra, User_Permissions_Quyen_dataList, int.Parse(Session["NhomQuyen"]?.ToString() ?? "0"));
                        foreach (var kvp in quyenButtonsDict)
                        {
                            ViewData[kvp.Key] = kvp.Value;
                        }
                        return View(query);
                    }
                    else
                    {
                        bool User_Root_View = User_Permissions_Quyen_dataList.Any(pair => pair.Key.Item1 == "BC");
                        if (User_Root_View)
                        {
                            var query = from muctieu in db.Tbl_MucTieu
                                        from phongban in db.Tbl_PhongBan
                                        where muctieu.ID_PhongBan == phongban.ID_PhongBan && muctieu.Status != 0
                                        select new Tbl_Join_ALL { PhongBan = phongban, MucTieu = muctieu };
                            logger.Info("Đã lấy được danh sách ở khác 1000 Status != 0: Index_MucTieu" + "User view: " + Session["Username"]?.ToString());
                            Dictionary<string, bool> quyenButtonsDict = share_All.GetQuyenButtonsDict(quyensCanKiemTra, User_Permissions_Quyen_dataList, int.Parse(Session["NhomQuyen"]?.ToString() ?? "0"));
                            foreach (var kvp in quyenButtonsDict)
                            {
                                ViewData[kvp.Key] = kvp.Value;
                            }
                            return View(query);
                        }
                        else
                        {
                            logger.Error("Lỗi: tài khoản " + Session["Username"]?.ToString() + "không có IDMenu quyền 'BC'" + "User_Root_View: " + User_Root_View);
                            ViewBag.error = "Tài khoản chưa đủ quyền";
                            return View();
                        }
                    }
                    throw new Exception("Bắt đầu gửi danh sách mục tiêu");
                }
                catch (Exception ex)
                {
                    logger.Error("Lỗi: ", ex);
                    ViewBag.error = ex.Message;
                    return View("Index_MucTieu", "MucTieu");
                }
            }
            else
            {
                logger.Error("Lỗi: " + "xác thực !checkAccount");
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpGet]
        public ActionResult Add_MucTieu()
        {
            var checkAccount = share_All.CheckAccount(Session["muctieuname"]?.ToString());
            if (checkAccount == true)
            {
                var dataList = db.Tbl_PhongBan.Where(x => x.ID_PhongBan != "PB0").ToList();
                if (dataList != null)
                {
                    return Json(dataList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        success = true,
                        message = "Người dùng không tồn tại."
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
        public ActionResult Add_MucTieu(List<Tbl_MucTieu> muctieu)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                try
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("MucTieu");
                    dataTable.Columns.Add("ThangNam");
                    dataTable.Columns.Add("ID_PhongBan");
                    dataTable.Columns.Add("Status");
                    foreach (var item in muctieu)
                    {
                        DataRow newRow = dataTable.NewRow();
                        newRow["MucTieu"] = item.MucTieu;
                        newRow["ThangNam"] = item.ThangNam;
                        newRow["ID_PhongBan"] = item.ID_PhongBan;
                        newRow["Status"] = item.Status;
                        dataTable.Rows.Add(newRow);
                    }
                    foreach(DataRow row  in dataTable.Rows)
                    {
                        string MucTieu = row["MucTieu"].ToString();
                        string ThangNam = row["ThangNam"].ToString();
                        string ID_PhongBan = row["ID_PhongBan"].ToString();
                        string Status = row["Status"].ToString();
                        SqlParameter[] parameters_MucTieu = new SqlParameter[]
                                {
                                    new SqlParameter("@MucTieu", SqlDbType.Int) { Value = MucTieu },
                                     new SqlParameter("@ThangNam", SqlDbType.VarChar, 20) { Value = ThangNam },
                                      new SqlParameter("@ID_PhongBan", SqlDbType.VarChar, 10) { Value = ID_PhongBan },
                                       new SqlParameter("@Status", SqlDbType.Int) { Value = Status },
                                };
                        db.Database.ExecuteSqlCommand("EXEC sp_Insert_MucTieu @MucTieu, @ThangNam, @ID_PhongBan, @Status", parameters_MucTieu);
                        logger.Info("Đã tạo mục tiêu  6S thành công" + ThangNam + "User add: " + Session["Username"]?.ToString());
                    }
                    var datalist = db.Tbl_MucTieu.ToList();
                    return Json(new
                    {
                        success = true,
                        message = "Đã tạo mục tiêu  6S thành công",
                        userList = datalist
                    });
                }
                catch(Exception ex)
                {
                    logger.Error("Lỗi: ", ex);
                    return Json(new
                    {
                        status = false,
                        message = ex.Message,
                        redirectUrl = Url.Action("Index_MucTieu", "MucTieu")
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
        public ActionResult Edit_MucTieu()
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
        [HttpPost]
        public ActionResult Edit_MucTieu(Tbl_MucTieu muctieu)
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
        [HttpPost]
        public ActionResult Delete_MucTieu(Tbl_MucTieu muctieu)
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
    }
}