using _6S.Context;
using _6S.Models;
using log4net;
using System;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace _6S.Controllers
{
    public class LoginController : Controller
    {
        Model_6S db = new Model_6S();
        Share_All share_All = new Share_All();
        private ILog logger = LogManager.GetLogger(typeof(LoginController));
        // GET: Login
        [HttpGet]
        public ActionResult Login()
        {
            try
            {
                var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
                if (checkAccount == true)
                {
                    return RedirectToAction("Dashboard_checklist", "Dashboard");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                logger.Error("Lỗi: ", ex);
                return Json(new
                {
                    status = "error",
                    message = "Lỗi: " + ex.Message,
                    redirectUrl = Url.Action("Login", "Login")
                });
            }
        }
        [HttpPost]
        public ActionResult Login(string Username, string Password)
        {
            if (ModelState.IsValid)
            {
                byte[] usernameBytes = Convert.FromBase64String(Username);
                byte[] passwordBytes = Convert.FromBase64String(Password);
                string decodedUsername = Encoding.UTF8.GetString(usernameBytes);
                string decodedPassword = Encoding.UTF8.GetString(passwordBytes);
                var md5_Password = share_All.GetMD5(decodedPassword);
                try
                {
                    using (MemoryCache cache = MemoryCache.Default)
                    {
                        var cacheKey = $"{decodedUsername}:{md5_Password}";
                        var user = cache.Get(cacheKey) as Tbl_User;
                        if (user == null)
                        {
                            // Truy vấn database để lấy thông tin user
                            user = db.Tbl_User.FirstOrDefault(x => x.Username == decodedUsername && x.Pass == md5_Password);
                            if (user != null)
                            {
                                // Thêm thông tin user vào MemoryCache
                                cache.Add(cacheKey, user, new CacheItemPolicy());
                            }
                        }
                        if (user != null)
                        {
                            // Lưu thông tin vào session
                            Session["Manv"] = user.Manv;
                            Session["Fullname"] = user.Fullname;
                            Session["Username"] = user.Username;
                            Session["trangthai"] = user.Status;
                            Session["NhomQuyen"] = user.NhomQuyen;
                            Session["UserLogin"] = user;
                            // Đăng nhập thành công
                            logger.Info("Đăng nhập: " + Session["Username"]?.ToString());
                            return Json(new
                            {
                                status = "success",
                                message = "Đăng nhập thành công",
                                redirectUrl = Url.Action("Dashboard_checklist", "Dashboard")
                            });
                        }
                        else
                        {
                            logger.Error("Tên đăng nhập hoặc mật khẩu không đúng");
                            // Tên đăng nhập hoặc mật khẩu không đúng
                            return Json(new
                            {
                                status = "error",
                                message = "Tên đăng nhập hoặc mật khẩu không đúng",
                                redirectUrl = Url.Action("Login", "Login")
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Lỗi: ", ex);
                    return Json(new
                    {
                        status = "error",
                        message = "Lỗi:" +ex.Message,
                        redirectUrl = Url.Action("Login", "Login")
                    });
                }
            }
            else
            {
                logger.Error("Sai giá trị");
                return Json(new
                {
                    status = "error",
                    message = "Vui lòng kiểm tra lại giá trị",
                    redirectUrl = Url.Action("Login", "Login")
                });
            }
        }
        [HttpGet]
        public ActionResult LogOff()
        {
            try
            {
                string userLoggedOut = Session["Username"]?.ToString();
                logger.Info("Đăng xuất: " + userLoggedOut);
                // Hủy bỏ phiên đăng nhập và xóa thông tin đăng nhập
                Session.Abandon();
                Session.Clear();
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoStore();
                // Đăng xuất người dùng khỏi hệ thống
                System.Web.Security.FormsAuthentication.SignOut();
                logger.Info("Đăng xuất thành công");
            }
            catch (Exception ex)
            {
                logger.Error("Lỗi khi đăng xuất: " + ex.Message);
                return View(ex.Message);
            }
            return RedirectToAction("Login", "Login");
        }
    }
}