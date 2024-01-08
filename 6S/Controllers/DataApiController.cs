using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using _6S.Context;
using _6S.Models;
using log4net;
using Newtonsoft.Json;

namespace _6S.Controllers
{
    public class DataApiController : Controller
    {
        Model_6S db = new Model_6S();
        Share_All share_All = new Share_All();
        private ILog logger = LogManager.GetLogger(typeof(HomeController));
        [HttpGet]
        public JsonResult Get_PhongBanUsers_From_Khoi(string maKhoi)
        {
            try
            {
                string NhomQuyen = Session["NhomQuyen"]?.ToString();
                string decodedmaKhoi = share_All.DecodeBase64String(maKhoi);
                var phongBanList = (from phongBan in db.Tbl_PhongBan
                                    where phongBan.Ma_Khoi == decodedmaKhoi && phongBan.ID_PhongBan != "PB0"
                                    select new
                                    {
                                        phongBan.ID_PhongBan,
                                        phongBan.TenPhongBan
                                    }).ToList();
                var userList = (from user in db.Tbl_User
                                join phongBan in db.Tbl_PhongBan
                                on user.ID_PhongBan equals phongBan.ID_PhongBan
                                where phongBan.Ma_Khoi == decodedmaKhoi
                                select new
                                {
                                    user.Username,
                                    user.Fullname,
                                    phongBan.ID_PhongBan,
                                    phongBan.TenPhongBan
                                }).ToList();
                var result = new
                {
                    NhomQuyen = NhomQuyen,
                    PhongBanList = phongBanList,
                    UserList = userList
                };
                // Chuyển đổi đối tượng JSON thành chuỗi JSON
                string json = JsonConvert.SerializeObject(result);
                // Mã hóa chuỗi JSON thành Base64
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                string encodedJson = Convert.ToBase64String(jsonBytes);
                // Trả về chuỗi JSON đã được mã hóa Base64
                return Json(encodedJson, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ ở đây
                logger.Error("Lỗi: ", ex);
                return Json(new { message = "Đã xảy ra lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult Get_PhanCongTime()
        {
            try
            {
                // Lấy danh sách Tbl_PhanCong dựa trên điều kiện Username và NhomQuyen
                string Username = Session["Username"]?.ToString();
                string NhomQuyen = Session["NhomQuyen"]?.ToString();
                if (NhomQuyen == "1000" && Username != null)
                {
                    var loaiBCList = (from loaibc in db.Tbl_LoaiBC
                                  select new
                                  {
                                      Loai_BC = loaibc.MaLoai,
                                      TenLoai = loaibc.TenLoai
                                  }).ToList();
                    var phongBanList = (from phongban in db.Tbl_PhongBan where phongban.Status == 1
                                    select new
                                    {
                                        ID_PhongBan = phongban.ID_PhongBan,
                                        PhongBan = phongban.TenPhongBan
                                    }).ToList();
                    var result = new
                    {
                        LoaiBCList = loaiBCList,
                        PhongBanList = phongBanList
                    };
                    string json = JsonConvert.SerializeObject(result);
                    byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                    string encodedJson = Convert.ToBase64String(jsonBytes);
                    return Json(new { success = true, message = "Lấy dữ liệu thành công !", data = encodedJson }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var currentDate = DateTime.Now.Date;
                    var checkView_PhanCongCham6S = db.PhanCongCham6S
                        .Where(x => x.Username == Username && x.TuNgay <= currentDate && x.DenNgay >= currentDate)
                        .ToList();

                    if (checkView_PhanCongCham6S.Any())
                    {
                        var dropdownData = checkView_PhanCongCham6S
                            .Select(item => new { Loai_BC = item.Loai_BC, TenLoai = item.TenLoai, ID_PhongBan = item.ID_PhongBan, PhongBan = item.TenPhongBan })
                            .ToList();
                        string json = JsonConvert.SerializeObject(dropdownData);
                        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                        string encodedJson = Convert.ToBase64String(jsonBytes);
                        return Json(new { success = true, message = "Lấy dữ liệu thành công !", data = encodedJson }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Bạn không có lịch chấm 6S trong ngày: " + DateTime.Now }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ ở đây
                logger.Error("Lỗi: ", ex);
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}