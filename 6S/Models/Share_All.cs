using _6S.Context;
using _6S.Controllers;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace _6S.Models
{
    public class Share_All
    {
        Model_6S db = new Model_6S();
        private ILog logger = LogManager.GetLogger(typeof(Share_All));
        private Dictionary<string, bool> quyenButtonsDict;
        //Mã hóa pass md5
        public string GetMD5(string str)
        {
            try
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] byteArray = System.Text.Encoding.ASCII.GetBytes(str);
                byteArray = md5.ComputeHash(byteArray);
                string hashedValue = "";

                foreach (byte b in byteArray)
                {
                    hashedValue += b.ToString("x2");
                }
                return hashedValue;
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ ở đây
                logger.Error("Lỗi MD5: ", ex);
                return "Đã xảy ra lỗi khi mã hóa MD5: " + ex.Message;
            }
        }
        //Giải mã base64
        public string DecodeBase64String(string base64String)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(base64String);
                string result = Encoding.UTF8.GetString(bytes);
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Lỗi base64: ", ex);
                return "Đã xảy ra lỗi khi mã hóa Base64: " + ex.Message;
            }
        }
        //Xác thực tài khoản
        public bool CheckAccount(string username)
        {
            try
            {
                using (var db = new Model_6S())
                {
                    var Tbl_Session_Login = HttpContext.Current.Session["UserLogin"] as Tbl_User;
                    var user = db.Tbl_User.FirstOrDefault(u => u.Username == Tbl_Session_Login.Username);
                    if(Tbl_Session_Login != null)
                    {
                        if (user != null && user.Status == 1 && user.Pass == Tbl_Session_Login.Pass)
                        {
                            logger.Info("User xác thực CheckAccount: " + Tbl_Session_Login.Fullname);
                            return true;
                        }
                    }
                    else
                    {
                        logger.Error("Xác thực thất bại CheckAccount: hết session");
                        return false;
                    }
                }
                logger.Error("Xác thực thất bại CheckAccount: " + HttpContext.Current.Session["Username"]?.ToString());
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("Xác thực thất bại CheckAccount: " + HttpContext.Current.Session["Username"]?.ToString() + ". Lỗi: " + ex.Message);
                return false;
            }
        }
        //Xác thực phân quyền
        public Dictionary<Tuple<string, int>, bool> User_Permissions(string ID_NhomQuyen, List<string> quyensCanKiemTra)
        {
            try
            {
                var permissions = new Dictionary<Tuple<string, int>, bool>();
                var dataQuyen = db.Tbl_NhomQuyen.Where(x => x.ID_NhomQuyen.ToString() == ID_NhomQuyen).ToList();

                if (quyensCanKiemTra.Count > 0)
                {
                    if (ID_NhomQuyen == "1000")
                    {
                        foreach (var row in dataQuyen)
                        {
                            foreach (var quyen in quyensCanKiemTra)
                            {
                                var quyenArr = quyen.Split(',');

                                if (quyenArr.Contains(row.IDMenu))
                                {
                                    permissions.Add(Tuple.Create(row.IDMenu, row.ID_NhomQuyen.Value), true);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var quyen in quyensCanKiemTra)
                        {
                            var quyenArr = quyen.Split(',');

                            foreach (var q in quyenArr)
                            {
                                var found = dataQuyen.FirstOrDefault(row => row.IDMenu == q);
                                var permissionKey = Tuple.Create(q, found != null ? found.ID_NhomQuyen.Value : 0);
                                permissions[permissionKey] = found != null;
                            }
                        }
                    }
                }

                return permissions;
            }
            catch (Exception ex)
            {
                logger.Error("Lỗi User_Permissions: ", ex);
                return null;
            }
        }
        public Share_All()
        {
            quyenButtonsDict = new Dictionary<string, bool>();
        }
        public Dictionary<string, bool> GetQuyenButtonsDict(List<string> quyensCanKiemTra, Dictionary<Tuple<string, int>, bool> userPermissionsQuyenDataList, int nhomQuyen)
        {
            try
            {
                foreach (var quyen in quyensCanKiemTra)
                {
                    var quyenArr = quyen.Split(',');

                    foreach (var q in quyenArr)
                    {
                        string viewBagName = "ButtonsOpen" + q;

                        bool result = userPermissionsQuyenDataList.TryGetValue(Tuple.Create(q, nhomQuyen), out bool permissionResult);
                        quyenButtonsDict[viewBagName] = result ? permissionResult : false;
                    }
                }

                return quyenButtonsDict;
            }
            catch (Exception ex)
            {
                logger.Error("Lỗi GetQuyenButtonsDict: ", ex);
                return null;
            }
        }
        //đường dẫn sever
        public string IsInitialCatalogProvided()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["Model_6S"]?.ConnectionString;
                logger.Info("Connect database: " + connectionString);
                if (!string.IsNullOrEmpty(connectionString))
                {
                    var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
                    string valueCatalog = builder.InitialCatalog;
                    if (valueCatalog == "Cham_6S")
                    {
                        string Patch_devPro = "6S_Pro";
                        return Patch_devPro;
                    }
                    else if (valueCatalog == "Cham_6S_test")
                    {
                        string Patch_dev = "6s";
                        return Patch_dev;
                    }
                    else
                    {
                        logger.Error("Lỗi IsInitialCatalogProvided valueCatalog không có giá trị");
                        return null;
                    }
                }
                else
                {
                    logger.Error("Lỗi IsInitialCatalogProvided connectionString rỗng");
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Lỗi IsInitialCatalogProvided: ", ex);
                return null;
            }
        }
    }
}