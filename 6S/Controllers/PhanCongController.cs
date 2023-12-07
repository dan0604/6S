using _6S.Context;
using _6S.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Web.UI.WebControls;
using iTextSharp.text;
using System.Globalization;
using iTextSharp.text.pdf;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Text;
using System.Data.Entity;
using iTextSharp.text.pdf.parser;
using Path = System.IO.Path;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Newtonsoft.Json;

namespace _6S.Controllers
{
    public class PhanCongController : Controller
    {
        // GET: PhanCong
        Model_6S db = new Model_6S();
        Share_All share_All = new Share_All();
        private ILog logger = LogManager.GetLogger(typeof(PhanCongController));
        [HttpGet]
        public ActionResult Index_PhanCong()
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
                        var query = db.Tbl_PhanCong_H.ToList();
                        logger.Info("Đã lấy được danh sách ở 1000  && !Status: Index_PhanCong" + "User view: " + Session["Username"]?.ToString());
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
                            var query = (nhomQuyenString == "2000") ?
                                db.Tbl_PhanCong_H.Where(x => x.Status != 0).ToList() :
                                db.Tbl_PhanCong_H.Where(x => x.Status != 0 && x.Nguoi_Tao == Username).ToList();
                            logger.Info("Đã lấy được danh sách ở khác 1000 Status != 0: Index_PhanCong" + "User view: " + Session["Username"]?.ToString());
                            Dictionary<string, bool> quyenButtonsDict = share_All.GetQuyenButtonsDict(quyensCanKiemTra, User_Permissions_Quyen_dataList, int.Parse(Session["NhomQuyen"]?.ToString() ?? "0"));
                            foreach (var kvp in quyenButtonsDict)
                            {
                                ViewData[kvp.Key] = kvp.Value;
                            }
                            return View(query);
                        }
                        else
                        {
                            logger.Error("Lỗi: tài khoản " + Session["Username"]?.ToString() +"không có IDMenu quyền 'BC'" + "User_Root_View: "+ User_Root_View);
                            ViewBag.error = "Tài khoản chưa đủ quyền";
                            return View();
                        }
                    }
                    throw new Exception("Bắt đầu gửi danh sách báo cáo phân công");
                }
                catch (Exception ex)
                {
                    logger.Error("Lỗi: ", ex);
                    ViewBag.error = ex.Message;
                    return View("Index_PhanCong", "PhanCong");
                }
            }
            else
            {
                logger.Error("Lỗi: " + "xác thực !checkAccount");
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpPost]
        public ActionResult Add_PhanCong(Tbl_PhanCong_L phancong_L)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                if (ModelState.IsValid)
                {
                    // Lấy chuỗi JSON từ FormData
                    string encodedJson = Request.Form["json"];
                    // Giải mã chuỗi JSON từ Base64
                    byte[] data = Convert.FromBase64String(encodedJson);
                    string jsonData = Encoding.UTF8.GetString(data);
                    // Khởi tạo ba DataTables tương ứng
                    DataTable ThoiGian = new DataTable("ThoiGian");
                    DataTable KhoiSanXuat = new DataTable("KhoiSanXuat");
                    DataTable KhoiVanPhong = new DataTable("KhoiVanPhong");
                    ThoiGian.Columns.Add("6S");
                    ThoiGian.Columns.Add("Chấm nội bộ");
                    ThoiGian.Columns.Add("Chấm chéo");
                    ThoiGian.Columns.Add("Chấm hiệu chỉnh");
                    JObject json = JObject.Parse(jsonData);
                    JArray gioData = (JArray)json["thoigianData"]["gioData"];
                    JArray ngayData = (JArray)json["thoigianData"]["ngayData"];
                    // Thêm dữ liệu từ gioData vào hàng thứ hai của DataTable
                    DataRow rowGioData = ThoiGian.NewRow();
                    for (int i = 0; i < gioData.Count; i++)
                    {
                        rowGioData[i] = gioData[i].ToString();
                    }
                    ThoiGian.Rows.Add(rowGioData);
                    // Thêm dữ liệu từ ngayData vào hàng đầu tiên của DataTable
                    DataRow rowNgayData = ThoiGian.NewRow();
                    for (int i = 0; i < ngayData.Count; i++)
                    {
                        rowNgayData[i] = ngayData[i].ToString();
                    }
                    ThoiGian.Rows.InsertAt(rowNgayData, 1);
                    // Thêm cột cho DataTable KhoiSanXuat
                    KhoiSanXuat.Columns.Add("KHỐI SẢN XUẤT", typeof(string));
                    KhoiSanXuat.Columns.Add("Thành Viên Ban ĐH", typeof(string));
                    KhoiSanXuat.Columns.Add("Tổ Cải Tiến", typeof(string));
                    // Thêm dữ liệu vào DataTable KhoiSanXuat từ JSON
                    JArray sanXuatData = (JArray)json["sanXuatData"];
                    foreach (JArray row in sanXuatData)
                    {
                        DataRow newRow_sanXuatData = KhoiSanXuat.NewRow();
                        for (int i = 0; i < row.Count; i++)
                        {
                            JObject item = (JObject)row[i];
                            string value = item["value"]?.ToString();
                            string text = item["text"]?.ToString();
                            if (i < KhoiSanXuat.Columns.Count)
                            {
                                newRow_sanXuatData[i] = value;
                            }
                        }
                        KhoiSanXuat.Rows.Add(newRow_sanXuatData);
                    }
                    //Tạo cột DataTable KhoiVanPhong
                    KhoiVanPhong.Columns.Add("KHỐI VĂN PHÒNG", typeof(string));
                    KhoiVanPhong.Columns.Add("Thành Viên Ban ĐH", typeof(string));
                    // Thêm dữ liệu vào DataTable KhoiVanPhong từ JSON
                    JArray vanPhongData = (JArray)json["vanPhongData"];
                    foreach (JArray row in vanPhongData)
                    {
                        DataRow newRow_vanPhongData = KhoiVanPhong.NewRow();
                        for (int i = 0; i < row.Count; i++)
                        {
                            JObject item = (JObject)row[i];
                            string value = item["value"]?.ToString();
                            string text = item["text"]?.ToString();
                            if (i < KhoiVanPhong.Columns.Count)
                            {
                                newRow_vanPhongData[i] = value;
                            }
                        }
                        KhoiVanPhong.Rows.Add(newRow_vanPhongData);
                    }
                    // Tạo DataTable mới để lưu trữ thông tin từ cột "Chấm nội bộ"
                    DataTable Time_ChamNoiBo = new DataTable();
                    Time_ChamNoiBo.Columns.Add("Gio");
                    Time_ChamNoiBo.Columns.Add("TuNgay");
                    Time_ChamNoiBo.Columns.Add("DenNgay");
                    Time_ChamNoiBo.Columns.Add("Thangnam");
                    // Lấy dữ liệu từ hàng thứ hai của DataTable ThoiGian
                    string Col_NB_Row_Gio = ThoiGian.Rows[0]["Chấm nội bộ"].ToString();
                    string Col_NB_Row_Ngay = ThoiGian.Rows[1]["Chấm nội bộ"].ToString();
                    bool is_Date_Dash_NB = Col_NB_Row_Ngay.Contains("-");
                    // Kiểm tra nếu chuỗi có dạng "DD/MM/yyyy"
                    if (!is_Date_Dash_NB)
                    {
                        string[] check_Parts = Col_NB_Row_Ngay.Split('/');
                        string tuNgay_Thang = check_Parts[1].Trim();
                        string tuNgay_Nam = check_Parts[2].Trim();
                        string thangNam = tuNgay_Thang + "/" + tuNgay_Nam;
                        DataRow newRow = Time_ChamNoiBo.NewRow();
                        newRow["Gio"] = Col_NB_Row_Gio;
                        newRow["TuNgay"] = Col_NB_Row_Ngay;
                        newRow["DenNgay"] = Col_NB_Row_Ngay;
                        newRow["Thangnam"] = thangNam;
                        Time_ChamNoiBo.Rows.Add(newRow);
                    }
                    // Kiểm tra nếu chuỗi có dạng "DDStart-DDEnd/MM/yyyy"
                    else
                    {
                        string[] check_Parts = Col_NB_Row_Ngay.Split('/');//15-22, 11, 2023
                        string[] check_Parts_Dash = check_Parts[0].Split('-');
                        // check_Parts[0] chứa giá trị phía trước dấu '-'
                        string StartDate = check_Parts_Dash[0];//15
                        // check_Parts[1] chứa giá trị phía sau dấu '-'
                        string EndDate = check_Parts_Dash[1];//22
                        string[] thang = check_Parts[1].Split('/');//11
                        string[] nam = check_Parts[2].Split('/');//2023
                        string tuNgay = StartDate + "/" + thang[0] + "/" + nam[0];
                        string denNgay = EndDate + "/" + thang[0] + "/" + nam[0];
                        string thangNam = thang[0] + "/" + nam[0];
                        DataRow newRow = Time_ChamNoiBo.NewRow();
                        newRow["Gio"] = Col_NB_Row_Gio;
                        newRow["TuNgay"] = tuNgay;
                        newRow["DenNgay"] = denNgay;
                        newRow["Thangnam"] = thangNam;
                        Time_ChamNoiBo.Rows.Add(newRow);
                    }
                    // Tạo DataTable mới để lưu trữ thông tin từ cột "Chấm chéo"
                    DataTable Time_ChamCheo = new DataTable();
                    Time_ChamCheo.Columns.Add("Gio");
                    Time_ChamCheo.Columns.Add("TuNgay");
                    Time_ChamCheo.Columns.Add("DenNgay");
                    Time_ChamCheo.Columns.Add("Thangnam");
                    // Lấy dữ liệu từ hàng thứ hai của DataTable ThoiGian
                    string Col_CH_Row_Gio = ThoiGian.Rows[0]["Chấm chéo"].ToString();
                    string Col_CH_Row_Ngay = ThoiGian.Rows[1]["Chấm chéo"].ToString();
                    bool is_Date_Dash_CH = Col_CH_Row_Ngay.Contains("-");
                    // Kiểm tra nếu chuỗi có dạng "DD/MM/yyyy"
                    if (!is_Date_Dash_CH)
                    {
                        string[] check_Parts = Col_CH_Row_Ngay.Split('/');
                        string tuNgay_Thang = check_Parts[1].Trim();
                        string tuNgay_Nam = check_Parts[2].Trim();
                        string thangNam = tuNgay_Thang + "/" + tuNgay_Nam;
                        DataRow newRow = Time_ChamCheo.NewRow();
                        newRow["Gio"] = Col_CH_Row_Gio;
                        newRow["TuNgay"] = Col_CH_Row_Ngay;
                        newRow["DenNgay"] = Col_CH_Row_Ngay;
                        newRow["Thangnam"] = thangNam;
                        Time_ChamCheo.Rows.Add(newRow);
                    }
                    // Kiểm tra nếu chuỗi có dạng "DDStart-DDEnd/MM/yyyy"
                    else
                    {
                        string[] check_Parts = Col_CH_Row_Ngay.Split('/');//15-22, 11, 2023
                        string[] check_Parts_Dash = check_Parts[0].Split('-');
                        // check_Parts[0] chứa giá trị phía trước dấu '-'
                        string StartDate = check_Parts_Dash[0];//15
                        // check_Parts[1] chứa giá trị phía sau dấu '-'
                        string EndDate = check_Parts_Dash[1];//22
                        string[] thang = check_Parts[1].Split('/');//11
                        string[] nam = check_Parts[2].Split('/');//2023
                        string tuNgay = StartDate + "/" + thang[0] + "/" + nam[0];
                        string denNgay = EndDate + "/" + thang[0] + "/" + nam[0];
                        string thangNam = thang[0] + "/" + nam[0];
                        DataRow newRow = Time_ChamCheo.NewRow();
                        newRow["Gio"] = Col_CH_Row_Gio;
                        newRow["TuNgay"] = tuNgay;
                        newRow["DenNgay"] = denNgay;
                        newRow["Thangnam"] = thangNam;
                        Time_ChamCheo.Rows.Add(newRow);
                    }
                    // Tạo DataTable mới để lưu trữ thông tin từ cột "Chấm hiệu chỉnh"
                    DataTable Time_ChamHieuChinh = new DataTable();
                    Time_ChamHieuChinh.Columns.Add("Gio");
                    Time_ChamHieuChinh.Columns.Add("TuNgay");
                    Time_ChamHieuChinh.Columns.Add("DenNgay");
                    Time_ChamHieuChinh.Columns.Add("Thangnam");
                    // Lấy dữ liệu từ hàng thứ hai của DataTable ThoiGian
                    string Col_HC_Row_Gio = ThoiGian.Rows[0]["Chấm hiệu chỉnh"].ToString();
                    string Col_HC_Row_Ngay = ThoiGian.Rows[1]["Chấm hiệu chỉnh"].ToString();
                    bool is_Date_Dash_HC = Col_HC_Row_Ngay.Contains("-");
                    // Kiểm tra nếu chuỗi có dạng "DD/MM/yyyy"
                    if (!is_Date_Dash_HC)
                    {
                        string[] check_Parts = Col_HC_Row_Ngay.Split('/');
                        string tuNgay_Thang = check_Parts[1].Trim();
                        string tuNgay_Nam = check_Parts[2].Trim();
                        string thangNam = tuNgay_Thang + "/" + tuNgay_Nam;
                        DataRow newRow = Time_ChamHieuChinh.NewRow();
                        newRow["Gio"] = Col_HC_Row_Gio;
                        newRow["TuNgay"] = Col_HC_Row_Ngay;
                        newRow["DenNgay"] = Col_HC_Row_Ngay;
                        newRow["Thangnam"] = thangNam;
                        Time_ChamHieuChinh.Rows.Add(newRow);
                    }
                    // Kiểm tra nếu chuỗi có dạng "DDStart-DDEnd/MM/yyyy"
                    else
                    {
                        string[] check_Parts = Col_HC_Row_Ngay.Split('/');//15-22, 11, 2023
                        string[] check_Parts_Dash = check_Parts[0].Split('-');
                        // check_Parts[0] chứa giá trị phía trước dấu '-'
                        string StartDate = check_Parts_Dash[0];//15
                        // check_Parts[1] chứa giá trị phía sau dấu '-'
                        string EndDate = check_Parts_Dash[1];//22
                        string[] thang = check_Parts[1].Split('/');//11
                        string[] nam = check_Parts[2].Split('/');//2023
                        string tuNgay = StartDate + "/" + thang[0] + "/" + nam[0];
                        string denNgay = EndDate + "/" + thang[0] + "/" + nam[0];
                        string thangNam = thang[0] + "/" + nam[0];
                        DataRow newRow = Time_ChamHieuChinh.NewRow();
                        newRow["Gio"] = Col_HC_Row_Gio;
                        newRow["TuNgay"] = tuNgay;
                        newRow["DenNgay"] = denNgay;
                        newRow["Thangnam"] = thangNam;
                        Time_ChamHieuChinh.Rows.Add(newRow);
                    }
                    // Tạo DataTable tổng mới
                    DataTable TongHop_CH = new DataTable();
                    TongHop_CH.Columns.Add("PhongBan", typeof(string));
                    TongHop_CH.Columns.Add("Username", typeof(string));
                    TongHop_CH.Columns.Add("Gio", typeof(string));
                    TongHop_CH.Columns.Add("TuNgay", typeof(DateTime));
                    TongHop_CH.Columns.Add("DenNgay", typeof(DateTime));
                    TongHop_CH.Columns.Add("ThangNam", typeof(string));
                    TongHop_CH.Columns.Add("Status", typeof(int));
                    TongHop_CH.Columns.Add("Loai_BC", typeof(string));
                    TongHop_CH.Columns.Add("NVCaiTien", typeof(string));
                    TongHop_CH.Columns.Add("MaKhoi", typeof(string));

                    // Duyệt qua các hàng trong DataTable Time_ChamCheo
                    foreach (DataRow row in Time_ChamCheo.Rows)
                    {
                        string Gio = row["Gio"].ToString();
                        string tuNgay = row["TuNgay"].ToString();
                        string denNgay = row["DenNgay"].ToString();
                        string format = "dd/MM/yyyy"; // Định dạng của chuỗi ngày tháng
                        DateTime tuNgayDateTime = DateTime.ParseExact(tuNgay, format, CultureInfo.InvariantCulture);
                        DateTime denNgayDateTime = DateTime.ParseExact(denNgay, format, CultureInfo.InvariantCulture);
                        string thangNam = row["Thangnam"].ToString();
                        // Lấy dữ liệu từ DataTable KhoiSanXuat và thêm vào bảng tổng mới
                        foreach (DataRow sanXuatRow in KhoiSanXuat.Rows)
                        {
                            string phongBan = sanXuatRow["KHỐI SẢN XUẤT"].ToString();
                            string Username = sanXuatRow["Thành Viên Ban ĐH"].ToString();
                            string nvCaiTien = sanXuatRow["Tổ Cải Tiến"].ToString();
                            TongHop_CH.Rows.Add(phongBan, Username, Gio, tuNgayDateTime, denNgayDateTime, thangNam, "1", "CH", nvCaiTien, "K1");
                        }
                        // Lấy dữ liệu từ DataTable KhoiVanPhong và thêm vào bảng tổng mới
                        foreach (DataRow vanPhongRow in KhoiVanPhong.Rows)
                        {
                            string phongBan = vanPhongRow["KHỐI VĂN PHÒNG"].ToString();
                            string Username = vanPhongRow["Thành Viên Ban ĐH"].ToString();
                            TongHop_CH.Rows.Add(phongBan, Username, Gio, tuNgayDateTime, denNgayDateTime, thangNam, "1", "CH", "", "K2");
                        }
                    }
                    // Lấy dữ liệu nhóm chấm hiệu chỉnh
                    string sqlQuery_HC = "SELECT A.ID_PhongBan AS PhongBan, A.Username, CAST(NULL AS VARCHAR) AS Gio, CAST(NULL AS DATETIME) AS TuNgay, CAST(NULL AS DATETIME) AS DenNgay, NULL AS ThangNam, 1 AS Status, 'HC' AS Loai_BC, NULL AS NVCaiTien, NULL AS MaKhoi FROM Tbl_User A INNER JOIN Tbl_NhomQuyen B ON A.NhomQuyen = B.ID_NhomQuyen INNER JOIN Tbl_DMQuyen C ON B.IDMenu = C.ID_Quyen WHERE B.IDMenu = 'CHC' AND A.NhomQuyen = '2000'";
                    List<Tbl_PhanCong_L_View> Tbl_PhanCong_L_View_HC = db.Database.SqlQuery<Tbl_PhanCong_L_View>(sqlQuery_HC).ToList();
                    // Tạo một DataTable mới cho HC
                    DataTable dataTable_HC = new DataTable();
                    // Lấy danh sách các thuộc tính của lớp Tbl_PhanCong_L_View cho HC
                    var properties_HC = typeof(Tbl_PhanCong_L_View).GetProperties();
                    // Tạo các cột trong DataTable cho HC với tên của thuộc tính và kiểu dữ liệu tương ứng
                    foreach (var property in properties_HC)
                    {
                        dataTable_HC.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
                    }
                    // Thêm dữ liệu từ List vào DataTable cho HC
                    foreach (var item in Tbl_PhanCong_L_View_HC)
                    {
                        var values = new object[properties_HC.Length];
                        for (int i = 0; i < properties_HC.Length; i++)
                        {
                            values[i] = properties_HC[i].GetValue(item);
                        }
                        dataTable_HC.Rows.Add(values);
                    }
                    // Số dòng trong DataTable Time_ChamHieuChinh
                    int numRows_HC = Time_ChamHieuChinh.Rows.Count;
                    for (int i = 0; i < numRows_HC; i++)
                    {
                        for (int j = 0; j < dataTable_HC.Rows.Count; j++)
                        {
                            string Gio = Time_ChamHieuChinh.Rows[i]["Gio"].ToString();
                            string tuNgay = Time_ChamHieuChinh.Rows[i]["TuNgay"].ToString();
                            string denNgay = Time_ChamHieuChinh.Rows[i]["DenNgay"].ToString();
                            string thangNam = Time_ChamHieuChinh.Rows[i]["Thangnam"].ToString();
                            string format = "dd/MM/yyyy"; // Định dạng của chuỗi ngày tháng
                            DateTime tuNgayDateTime = DateTime.ParseExact(tuNgay, format, CultureInfo.InvariantCulture);
                            DateTime denNgayDateTime = DateTime.ParseExact(denNgay, format, CultureInfo.InvariantCulture);
                            dataTable_HC.Rows[j]["Gio"] = Gio;
                            dataTable_HC.Rows[j]["TuNgay"] = tuNgayDateTime;
                            dataTable_HC.Rows[j]["DenNgay"] = denNgayDateTime;
                            dataTable_HC.Rows[j]["Thangnam"] = thangNam;
                        }
                    }
                    // Lấy dữ liệu nhóm chấm nội bộ
                    string sqlQuery_NB = "SELECT A.ID_PhongBan AS PhongBan, A.Username, CAST(NULL AS VARCHAR) AS Gio, CAST(NULL AS DATETIME) AS TuNgay, CAST(NULL AS DATETIME) AS DenNgay, NULL AS ThangNam, 1 AS Status, 'NB' AS Loai_BC, NULL AS NVCaiTien, NULL AS MaKhoi FROM Tbl_User A INNER JOIN Tbl_NhomQuyen B ON A.NhomQuyen = B.ID_NhomQuyen INNER JOIN Tbl_DMQuyen C ON B.IDMenu = C.ID_Quyen WHERE B.IDMenu = 'CNB'";
                    List<Tbl_PhanCong_L_View> Tbl_PhanCong_L_View_NB = db.Database.SqlQuery<Tbl_PhanCong_L_View>(sqlQuery_NB).ToList();
                    // Tạo một DataTable mới cho NB
                    DataTable dataTable_NB = new DataTable();
                    // Lấy danh sách các thuộc tính của lớp Tbl_PhanCong_L_View cho NB
                    var properties_NB = typeof(Tbl_PhanCong_L_View).GetProperties();
                    // Tạo các cột trong DataTable cho NB với tên của thuộc tính và kiểu dữ liệu tương ứng
                    foreach (var property in properties_NB)
                    {
                        dataTable_NB.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
                    }
                    // Thêm dữ liệu từ List vào DataTable cho NB
                    foreach (var item in Tbl_PhanCong_L_View_NB)
                    {
                        var values = new object[properties_NB.Length];
                        for (int i = 0; i < properties_NB.Length; i++)
                        {
                            values[i] = properties_NB[i].GetValue(item);
                        }
                        dataTable_NB.Rows.Add(values);
                    }
                    // Số dòng trong DataTable Time_ChamNoiBo
                    int numRows_NB = Time_ChamNoiBo.Rows.Count;
                    for (int i = 0; i < numRows_NB; i++)
                    {
                        for(int j = 0; j < dataTable_NB.Rows.Count; j++)
                        {
                            string Gio = Time_ChamNoiBo.Rows[i]["Gio"].ToString();
                            string tuNgay = Time_ChamNoiBo.Rows[i]["TuNgay"].ToString();
                            string denNgay = Time_ChamNoiBo.Rows[i]["DenNgay"].ToString();
                            string thangNam = Time_ChamNoiBo.Rows[i]["Thangnam"].ToString();
                            string format = "dd/MM/yyyy"; // Định dạng của chuỗi ngày tháng9
                            DateTime tuNgayDateTime = DateTime.ParseExact(tuNgay, format, CultureInfo.InvariantCulture);
                            DateTime denNgayDateTime = DateTime.ParseExact(denNgay, format, CultureInfo.InvariantCulture);
                            dataTable_NB.Rows[j]["Gio"] = Gio;
                            dataTable_NB.Rows[j]["TuNgay"] = tuNgayDateTime;
                            dataTable_NB.Rows[j]["DenNgay"] = denNgayDateTime;
                            dataTable_NB.Rows[j]["Thangnam"] = thangNam;
                        }
                    }
                    // Tạo một DataTable mới để lưu trữ kết quả nối các bảng lại với nhau
                    DataTable combinedDataTable = new DataTable();
                    // Sao chép cấu trúc của dataTable_HC để bắt đầu
                    combinedDataTable = dataTable_HC.Clone();
                    // Thêm cột ID_PhanCong vào đầu của DataTable
                    DataColumn newColumn = new DataColumn("ID_PhanCong", typeof(string)); // Điều chỉnh kiểu dữ liệu tương ứng
                    combinedDataTable.Columns.Add(newColumn);
                    // Di chuyển cột mới tạo vào vị trí đầu tiên của DataTable
                    combinedDataTable.Columns[newColumn.ColumnName].SetOrdinal(0);
                    // Nối tiếp các DataTable theo thứ tự: dataTable_HC -> dataTable_NB -> TongHop_CH
                    combinedDataTable.Merge(dataTable_HC);
                    // Kiểm tra xem TongHop_CH và dataTable_NB có số cột giống nhau hay không trước khi nối
                    if (TongHop_CH.Columns.Count == dataTable_NB.Columns.Count)
                    {
                        combinedDataTable.Merge(dataTable_NB);
                        combinedDataTable.Merge(TongHop_CH);
                    }
                    else
                    {
                        // Nếu số cột không khớp, thực hiện việc nối một cách thủ công (cột của TongHop_CH được thêm vào cuối)
                        DataTable dataTable_NBWithExtraColumns = dataTable_NB.Copy();
                        foreach (DataColumn col in TongHop_CH.Columns)
                        {
                            if (!dataTable_NBWithExtraColumns.Columns.Contains(col.ColumnName))
                            {
                                dataTable_NBWithExtraColumns.Columns.Add(col.ColumnName, col.DataType);
                            }
                        }
                        combinedDataTable.Merge(dataTable_NBWithExtraColumns);
                        combinedDataTable.Merge(TongHop_CH);
                    }
                    //bắt đầu chạy store
                    DateTime date = DateTime.Now;
                    // Lấy ngày từ đối tượng DateTime
                    string formattedDate = date.Date.ToString("yyy-MM-dd");//lấy dạng năm tháng ngày
                    string ID_PhanCong = "";
                    try
                    {
                        using (var db = new Model_6S())
                        {
                            SqlParameter[] parameters_ID_PhanCong = new SqlParameter[]
                            {
                                new SqlParameter("@ma", SqlDbType.NVarChar, 20){ Value = "PC"},
                                new SqlParameter("@namtaoma", SqlDbType.Date){ Value = formattedDate},
                            };
                            ID_PhanCong = db.Database.SqlQuery<string>("EXEC dbo.sp_TaoMaPC @ma, @namtaoma", parameters_ID_PhanCong).FirstOrDefault();
                            Console.WriteLine("ID_PhanCong: " + ID_PhanCong);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Lỗi: ", ex);
                        return Json(new
                        {
                            status = "error",
                            message = ex.Message,
                            redirectUrl = Url.Action("Index_checklist", "Home")
                        });
                    }
                    //lấy mã ID_PhanCong add vào datatable
                    for (int i = 0; i < combinedDataTable.Rows.Count; i++)
                    {
                        combinedDataTable.Rows[i]["ID_PhanCong"] = ID_PhanCong;
                    }
                    logger.Info("ID_PhanCong: " + ID_PhanCong + "User add: " + Session["Username"]?.ToString());
                    //check thời gian và mã để tránh trùng
                    string thangNamValue = "";
                    if (combinedDataTable.Rows.Count > 0)
                    {
                        // Lấy giá trị cột "ThangNam" từ dòng đầu tiên (dòng có chỉ số là 0)
                        thangNamValue = combinedDataTable.Rows[0]["ThangNam"].ToString();
                    }
                    var check_PhanCong = db.Tbl_PhanCong_L.Where(x => x.ThangNam == thangNamValue && x.Status != 0).FirstOrDefault();
                    if(check_PhanCong != null)
                    {
                        logger.Error("Lỗi: " + "Đã có lịch làm việc trong thời gian : " + thangNamValue);
                        var datalist = db.Tbl_PhanCong_L.ToList();
                        return Json(new
                        {
                            success = false,
                            message = "Đã có lịch làm việc trong thời gian : "+ thangNamValue,
                            userList = datalist
                        });
                    }
                    else
                    {
                        try
                        {
                            SqlParameter[] parameters_PhanCong_H = new SqlParameter[]
                            {
                                new SqlParameter("@ID_PhanCong", SqlDbType.VarChar, 20) { Value = ID_PhanCong},
                                new SqlParameter("@Ngay_Tao", SqlDbType.Date) { Value = date },
                                new SqlParameter("@Nguoi_Tao", SqlDbType.NVarChar, 30) { Value = Session["Username"]?.ToString() },
                            };
                            db.Database.ExecuteSqlCommand("EXEC sp_Insert_PhanCong_H @ID_PhanCong, @Ngay_Tao, @Nguoi_Tao", parameters_PhanCong_H);
                            foreach (DataRow row in combinedDataTable.Rows)
                            {
                                // Lấy dữ liệu từ mỗi cột trong hàng hiện tại
                                string ID_PhanCong_Row_combinedDataTable = row["ID_PhanCong"].ToString();
                                string PhongBan = row["PhongBan"].ToString();
                                string Username = row["Username"].ToString();
                                string Gio = row["Gio"].ToString();
                                DateTime TuNgay = (DateTime)row["TuNgay"];
                                DateTime DenNgay = (DateTime)row["DenNgay"];
                                string ThangNam = row["ThangNam"].ToString();
                                string Loai_BC = row["Loai_BC"].ToString();
                                string NVCaiTien = row["NVCaiTien"].ToString();
                                string MaKhoi = row["MaKhoi"].ToString();
                                SqlParameter[] parameters_PhanCong_L = new SqlParameter[]
                                {
                                    new SqlParameter("@ID_PhanCong", SqlDbType.VarChar, 20) { Value = ID_PhanCong_Row_combinedDataTable },
                                    new SqlParameter("@PhongBan", SqlDbType.VarChar, 10) { Value = PhongBan },
                                    new SqlParameter("@Username", SqlDbType.VarChar, 30) { Value = Username },
                                    new SqlParameter("@Gio", SqlDbType.VarChar, 30) { Value = Gio },
                                    new SqlParameter("@TuNgay", SqlDbType.Date) { Value = TuNgay },
                                    new SqlParameter("@DenNgay", SqlDbType.Date) { Value = DenNgay },
                                    new SqlParameter("@ThangNam", SqlDbType.VarChar, 10) { Value = ThangNam },
                                    new SqlParameter("@Loai_BC", SqlDbType.VarChar, 10) { Value = Loai_BC },
                                    new SqlParameter("@NVCaiTien", SqlDbType.NVarChar, 50) { Value = NVCaiTien },
                                    new SqlParameter("@MaKhoi", SqlDbType.VarChar, 10) { Value = MaKhoi },
                                };
                                db.Database.ExecuteSqlCommand("EXEC sp_Insert_PhanCong_L @ID_PhanCong, @PhongBan, @Username, @Gio, @TuNgay, @DenNgay, @ThangNam, @Loai_BC, @NVCaiTien, @MaKhoi", parameters_PhanCong_L);
                                logger.Info("Đã tạo lịch làm  6S thành công :" + ID_PhanCong + "User add: " + Session["Username"]?.ToString()); 
                            }
                            var datalist = db.Tbl_PhanCong_L.ToList();
                            return Json(new
                            {
                                success = true,
                                message = "Đã tạo lịch làm  6S thành công :" + ID_PhanCong,
                                userList = datalist
                            });
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Lỗi: ", ex);
                            var datalist = db.Tbl_PhanCong_L.ToList();
                            return Json(new
                            {
                                success = false,
                                message = "Lỗi :" + ex.Message,
                                userList = datalist
                            });
                        }
                    }
                }
                else
                {
                    logger.Error("Lỗi: " + "ModelState");
                    var datalist = db.Tbl_PhanCong_L.ToList();
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
                logger.Error("Lỗi: " + "xác thực !checkAccount");
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpGet]
        public ActionResult Edit_PhanCong(string ID_PhanCong)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                try
                {
                    var dataPhancong_H = db.Tbl_PhanCong_H.FirstOrDefault(x => x.ID_PhanCong == ID_PhanCong && x.Status == 1);
                    var dataUser_Get_Nguoitao_Phancong_H = db.Tbl_User.Where(x => x.Username == dataPhancong_H.Nguoi_Tao).FirstOrDefault();
                    var dataPhancong_L_NB = db.Tbl_PhanCong_L.Where(x => x.ID_PhanCong == ID_PhanCong && x.Loai_BC == "NB" && x.Status == 1).ToList();
                    var dataPhancong_L_CH = db.Tbl_PhanCong_L.Where(x => x.ID_PhanCong == ID_PhanCong && x.Loai_BC == "CH" && x.Status == 1).ToList();
                    var dataPhancong_L_HC = db.Tbl_PhanCong_L.Where(x => x.ID_PhanCong == ID_PhanCong && x.Loai_BC == "HC" && x.Status == 1).ToList();
                    var dataPhancong_L_CH_K1 = dataPhancong_L_CH.Where(x => x.MaKhoi == "K1")
                        .Select(x => new {ID_PhanCong = x.ID_PhanCong, PhongBan = x.PhongBan, Username = x.Username, NVCaiTien = x.NVCaiTien }).ToList();
                    var dataPhancong_L_CH_K2 = dataPhancong_L_CH.Where(x => x.MaKhoi == "K2")
                        .Select(x => new { ID_PhanCong = x.ID_PhanCong, PhongBan = x.PhongBan, Username = x.Username }).ToList();
                    var dataThangnam = db.Tbl_PhanCong_L.Where(x => x.ID_PhanCong == ID_PhanCong && x.Status == 1).ToList();

                    var gioValue_NB = dataPhancong_L_NB.Select(x => x.Gio).FirstOrDefault();
                    var gioValue_CH = dataPhancong_L_CH.Select(x => x.Gio).FirstOrDefault();
                    var gioValue_HC = dataPhancong_L_HC.Select(x => x.Gio).FirstOrDefault();

                    var tuNgay_NB = dataPhancong_L_NB.Select(x => x.TuNgay).FirstOrDefault();
                    var tuNgay_CH = dataPhancong_L_CH.Select(x => x.TuNgay).FirstOrDefault();
                    var tuNgay_HC = dataPhancong_L_HC.Select(x => x.TuNgay).FirstOrDefault();

                    var denNgay_NB = dataPhancong_L_NB.Select(x => x.DenNgay).FirstOrDefault();
                    var denNgay_CH = dataPhancong_L_CH.Select(x => x.DenNgay).FirstOrDefault();
                    var denNgay_HC = dataPhancong_L_HC.Select(x => x.DenNgay).FirstOrDefault();

                    var thoiGian_NB = GetThoiGian(tuNgay_NB, denNgay_NB);
                    var thoiGian_CH = GetThoiGian(tuNgay_CH, denNgay_CH);
                    var thoiGian_HC = GetThoiGian(tuNgay_HC, denNgay_HC);

                    var thangNam = dataThangnam.Select(x => x.ThangNam).FirstOrDefault();
                    DateTime date = DateTime.ParseExact(thangNam, "MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    string formattedDate = date.ToString("yyyy-MM");

                    string GetThoiGian(DateTime? tuNgay, DateTime? denNgay)
                    {
                        if (tuNgay == denNgay)
                        {
                            return tuNgay?.ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            DateTime tuNgayDateTime = tuNgay ?? default;
                            DateTime denNgayDateTime = denNgay ?? default;
                            return $"{tuNgayDateTime.Day}-{denNgayDateTime.Day}/{tuNgayDateTime.Month}/{tuNgayDateTime.Year}";
                        }
                    }
                    DataTable ThoiGian = new DataTable("ThoiGian");
                    DataTable KhoiSanXuat = new DataTable("KhoiSanXuat");
                    DataTable KhoiVanPhong = new DataTable("KhoiVanPhong");
                    // Tạo cột DataTable ThoiGian
                    ThoiGian.Columns.Add("ID_PhanCong");
                    ThoiGian.Columns.Add("6S");
                    ThoiGian.Columns.Add("Chấm nội bộ");
                    ThoiGian.Columns.Add("Chấm chéo");
                    ThoiGian.Columns.Add("Chấm hiệu chỉnh");
                    var gioValueList = new List<string> { ID_PhanCong, "Giờ", gioValue_NB, gioValue_CH, gioValue_HC };
                    var ngayValueList = new List<string> { ID_PhanCong, "Ngày", thoiGian_NB, thoiGian_CH, thoiGian_HC };
                    // Thêm dữ liệu từ gioData vào hàng thứ hai của DataTable
                    DataRow rowGioData = ThoiGian.NewRow();
                    for (int i = 0; i < gioValueList.Count; i++)
                    {
                        rowGioData[i] = gioValueList[i].ToString();
                    }
                    ThoiGian.Rows.Add(rowGioData);
                    // Thêm dữ liệu từ ngayData vào hàng đầu tiên của DataTable
                    DataRow rowNgayData = ThoiGian.NewRow();
                    for (int i = 0; i < ngayValueList.Count; i++)
                    {
                        rowNgayData[i] = ngayValueList[i].ToString();
                    }
                    ThoiGian.Rows.InsertAt(rowNgayData, 1);
                    //Tạo cột DataTable KhoiVanPhong
                    KhoiVanPhong.Columns.Add("ID_PhanCong", typeof(string));
                    KhoiVanPhong.Columns.Add("valuePhongBan", typeof(string));
                    KhoiVanPhong.Columns.Add("textTenPhongBan", typeof(string));
                    KhoiVanPhong.Columns.Add("valueUsername", typeof(string));
                    KhoiVanPhong.Columns.Add("textUsername", typeof(string));
                    // Thêm dữ liệu từ danh sách vào DataTable
                    foreach (var item in dataPhancong_L_CH_K2)
                    {
                        DataRow row = KhoiVanPhong.NewRow();
                        var dataPhongban = db.Tbl_PhongBan.Where(x => x.ID_PhongBan == item.PhongBan).FirstOrDefault();
                        var dataUser_Get_K2 = db.Tbl_User.Where(x => x.Username == item.Username).FirstOrDefault();
                        row["ID_PhanCong"] = item.ID_PhanCong;
                        row["valuePhongBan"] = item.PhongBan;
                        row["textTenPhongBan"] = dataPhongban.TenPhongBan;
                        row["valueUsername"] = item.Username;
                        row["textUsername"] = dataUser_Get_K2.Fullname;
                        KhoiVanPhong.Rows.Add(row);
                    }
                    // Thêm cột cho DataTable KhoiSanXuat
                    KhoiSanXuat.Columns.Add("ID_PhanCong", typeof(string));
                    KhoiSanXuat.Columns.Add("valuePhongBan", typeof(string));
                    KhoiSanXuat.Columns.Add("textTenPhongBan", typeof(string));
                    KhoiSanXuat.Columns.Add("valueUsername", typeof(string));
                    KhoiSanXuat.Columns.Add("textUsername", typeof(string));
                    KhoiSanXuat.Columns.Add("NVCaiTien", typeof(string));
                    foreach (var item in dataPhancong_L_CH_K1)
                    {
                        DataRow row = KhoiSanXuat.NewRow();
                        var dataPhongban = db.Tbl_PhongBan.Where(x => x.ID_PhongBan == item.PhongBan && x.Ma_Khoi == "K1").FirstOrDefault();
                        var dataUser_Get_K1 = db.Tbl_User.Where(x => x.Username == item.Username).FirstOrDefault();
                        row["ID_PhanCong"] = item.ID_PhanCong;
                        row["valuePhongBan"] = item.PhongBan;
                        row["textTenPhongBan"] = dataPhongban.TenPhongBan;
                        row["valueUsername"] = item.Username;
                        row["textUsername"] = dataUser_Get_K1.Fullname;
                        row["NVCaiTien"] = item.NVCaiTien;
                        KhoiSanXuat.Rows.Add(row);
                    }
                    if (dataPhancong_H != null)
                    {
                        var result = new
                        {
                            ThangNam = formattedDate,
                            ThoiGian = ThoiGian,
                            KhoiSanXuat = KhoiSanXuat,
                            KhoiVanPhong = KhoiVanPhong
                        };
                        var settings = new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        };

                        var json = JsonConvert.SerializeObject(result, settings);
                        return Content(json, "application/json");

                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Lịch làm không tồn tại."
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch(Exception ex)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Lỗi " + ex.Message,
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
        public ActionResult Edit_PhanCong(Tbl_PhanCong_L phancong_L)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                if (ModelState.IsValid)
                {
                    var check_PhanCong = db.Tbl_PhanCong_L.Where(x => x.ID_PhanCong == phancong_L.ID_PhanCong);
                    if (check_PhanCong != null)
                    {
                        try
                        {
                            SqlParameter[] parameters_PhanCong = new SqlParameter[]
                            {
                                new SqlParameter("@ID_PhanCong", SqlDbType.VarChar, 10) { Value = phancong_L.ID_PhanCong },
                                new SqlParameter("@PhongBan", SqlDbType.VarChar, 10) { Value = phancong_L.PhongBan },
                                new SqlParameter("@Username", SqlDbType.VarChar, 20) { Value = phancong_L.Username },
                                new SqlParameter("@TuNgay", SqlDbType.Date) { Value = phancong_L.TuNgay },
                                new SqlParameter("@DenNgay", SqlDbType.Date) { Value = phancong_L.DenNgay },
                                new SqlParameter("@ThangNam", SqlDbType.VarChar, 10) { Value = phancong_L.ThangNam },
                                new SqlParameter("@Loai_BC", SqlDbType.VarChar, 10) { Value = phancong_L.Loai_BC },
                            };
                            db.Database.ExecuteSqlCommand("EXEC sp_Update_PhanCong @ID_PhanCong, @PhongBan, @Username, @TuNgay, @DenNgay, @ThangNam, @Loai_BC, @Status", parameters_PhanCong);
                            logger.Info("Đã sửa Lịch làm  6S thành công :" + phancong_L.ID_PhanCong + "User add: " + Session["Username"]?.ToString());
                            var datalist = db.Tbl_PhanCong_L.ToList();
                            return Json(new
                            {
                                success = true,
                                message = "Đã sửa Lịch làm  6S thành công: " + phancong_L.ID_PhanCong,
                                userList = datalist
                            });
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Lỗi: ", ex);
                            var datalist = db.Tbl_PhanCong_L.ToList();
                            return Json(new
                            {
                                success = false,
                                userList = datalist,
                                message = "Lỗi khi sửa Lịch làm : " + ex.Message
                            });
                        }
                    }
                    else
                    {
                        logger.Error("Lỗi: " + "Lịch làm  không có");
                        ViewBag.error = "Vui lòng kiểm tra lại Lịch làm ";
                        var datalist = db.Tbl_PhanCong_L.ToList();
                        return Json(new
                        {
                            success = false,
                            userList = datalist,
                            message = "Lịch làm  không tồn tại"
                        });
                    }
                }
                else
                {
                    logger.Error("Lỗi: " + "ModelState");
                    var datalist = db.Tbl_PhanCong_L.ToList();
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
                logger.Error("Lỗi: " + "xác thực !checkAccount");
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpPost]
        public ActionResult View_PhanCong(string ID_PhanCong)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                try
                {
                    var item = db.Tbl_PhanCong_H.Find(ID_PhanCong);
                    if (item != null)
                    {
                        var path = @"\\192.168.24.108\6s";
                        var file = Path.Combine(path, item.Duongluu);
                        file = Path.GetFullPath(file);
                        if (file.StartsWith(path) && System.IO.File.Exists(file))
                        {
                            // Return the PDF URL
                            ViewBag.ID_PhanCong = ID_PhanCong;
                            byte[] pdfData = System.IO.File.ReadAllBytes(file);
                            string base64Pdf = Convert.ToBase64String(pdfData);
                            logger.Info("Xem files: " + item.Duongluu);
                            return Json(new { pdfData = base64Pdf, ID_PhanCong = ID_PhanCong }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            logger.Error("Lỗi: " + "Không tìm thấy tệp báo cáo đánh giá 6S " + item.ID_PhanCong);
                            return Json(new { error = "Không tìm thấy tệp báo cáo đánh giá 6S " + item.ID_PhanCong });
                        }
                    }
                    else
                    {
                        logger.Error("Lỗi: " + "Không tìm thấy MÃ báo cáo đánh giá 6S " + item.ID_PhanCong);
                        return Json(new { error = "Không tìm thấy MÃ báo cáo đánh giá 6S " + item.ID_PhanCong });
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Lỗi: ", ex);
                    return Json(new { error = "Lỗi: " + ex.Message });
                }

            }
            else
            {
                logger.Error("Lỗi: " + "xác thực !checkAccount");
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpGet]
        public ActionResult DownloadPDFFile(string ID_PhanCong)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                try
                {
                    var item = db.Tbl_PhanCong_H.Where(x => x.ID_PhanCong == ID_PhanCong).FirstOrDefault();
                    // Đường dẫn đến tệp PDF trên máy chủ mạng
                    string filePath = @"\\192.168.24.108\6s\" + item.Duongluu;
                    if (System.IO.File.Exists(filePath))
                    {
                        // Đọc tệp thành một mảng byte
                        byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                        // Xác định phần mở rộng tệp từ tên tệp
                        string fileExtension = Path.GetExtension(item.Duongluu);
                        // Sử dụng Process.Start để mở thư mục
                        string folderPath = Path.GetDirectoryName(filePath);
                        System.Diagnostics.Process.Start("explorer.exe", folderPath);
                        // Trả về tệp dưới dạng phản hồi HTTP
                        logger.Info("Tải files: " + item.Duongluu);
                        return File(fileBytes, "application/pdf", item.Duongluu);
                    }
                    else
                    {
                        logger.Error("Lỗi: " + "Tệp tin không tồn tại trong thư mục.");
                        return Json(new { success = false, message = "Tệp tin không tồn tại trong thư mục." });
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Lỗi: ", ex);
                    return Json(new { success = false, message = "Lỗi: " + ex.Message });
                }

            }
            else
            {
                logger.Error("Lỗi: " + "xác thực !checkAccount");
                return RedirectToAction("Login", "Login");
            }
        }
        public ActionResult Approve_PhanCong(string ID_PhanCong, int Status, Tbl_PhanCong_L phancong_L)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                try
                {
                    var recordToApprove = db.Tbl_PhanCong_H.Find(ID_PhanCong);
                    var recordToApprove_L = db.Tbl_PhanCong_L.Where(x => x.ID_PhanCong == recordToApprove.ID_PhanCong && x.Status == Status).FirstOrDefault();
                    if (recordToApprove != null || recordToApprove_L == null)
                    {
                        if (recordToApprove.Status == 2)
                        {
                            logger.Error("Lỗi: " + " Lịch làm đã về 2 :" + ID_PhanCong);
                            var datalist = db.Tbl_User.ToList();
                            return Json(new
                            {
                                userList = datalist,
                                success = false,
                                message = "Lịch làm đã được Duyệt trước đó :" + ID_PhanCong,
                                redirectUrl = Url.Action("Index_PhanCong", "User")
                            });
                        }
                        else
                        {
                            var dataPhancong_H = db.Tbl_PhanCong_H.FirstOrDefault(x => x.ID_PhanCong == ID_PhanCong && x.Status == 1);
                            var dataUser_Get_Nguoitao_Phancong_H = db.Tbl_User.Where(x => x.Username == dataPhancong_H.Nguoi_Tao).FirstOrDefault();
                            var dataPhancong_L_NB = db.Tbl_PhanCong_L.Where(x => x.ID_PhanCong == ID_PhanCong && x.Loai_BC == "NB" && x.Status == 1).ToList();
                            var dataPhancong_L_CH = db.Tbl_PhanCong_L.Where(x => x.ID_PhanCong == ID_PhanCong && x.Loai_BC == "CH" && x.Status == 1).ToList();
                            var dataPhancong_L_HC = db.Tbl_PhanCong_L.Where(x => x.ID_PhanCong == ID_PhanCong && x.Loai_BC == "HC" && x.Status == 1).ToList();
                            var dataPhancong_L_CH_K1 = dataPhancong_L_CH.Where(x => x.MaKhoi == "K1")
                                .Select(x => new { PhongBan = x.PhongBan, Username = x.Username, NVCaiTien = x.NVCaiTien }).ToList();
                            var dataPhancong_L_CH_K2 = dataPhancong_L_CH.Where(x => x.MaKhoi == "K2")
                                .Select(x => new { PhongBan = x.PhongBan, Username = x.Username }).ToList();
                            var dataThangnam = db.Tbl_PhanCong_L.Where(x => x.ID_PhanCong == ID_PhanCong && x.Status == 1).ToList();

                            var gioValue_NB = dataPhancong_L_NB.Select(x => x.Gio).FirstOrDefault();
                            var gioValue_CH = dataPhancong_L_CH.Select(x => x.Gio).FirstOrDefault();
                            var gioValue_HC = dataPhancong_L_HC.Select(x => x.Gio).FirstOrDefault();

                            var tuNgay_NB = dataPhancong_L_NB.Select(x => x.TuNgay).FirstOrDefault();
                            var tuNgay_CH = dataPhancong_L_CH.Select(x => x.TuNgay).FirstOrDefault();
                            var tuNgay_HC = dataPhancong_L_HC.Select(x => x.TuNgay).FirstOrDefault();

                            var denNgay_NB = dataPhancong_L_NB.Select(x => x.DenNgay).FirstOrDefault();
                            var denNgay_CH = dataPhancong_L_CH.Select(x => x.DenNgay).FirstOrDefault();
                            var denNgay_HC = dataPhancong_L_HC.Select(x => x.DenNgay).FirstOrDefault();

                            var thoiGian_NB = GetThoiGian(tuNgay_NB, denNgay_NB);
                            var thoiGian_CH = GetThoiGian(tuNgay_CH, denNgay_CH);
                            var thoiGian_HC = GetThoiGian(tuNgay_HC, denNgay_HC);

                            var thangNam = dataThangnam.Select(x => x.ThangNam).FirstOrDefault();

                            string GetThoiGian(DateTime? tuNgay, DateTime? denNgay)
                            {
                                if (tuNgay == denNgay)
                                {
                                    return tuNgay?.ToString("dd/MM/yyyy");
                                }
                                else
                                {
                                    DateTime tuNgayDateTime = tuNgay ?? default;
                                    DateTime denNgayDateTime = denNgay ?? default;
                                    return $"{tuNgayDateTime.Day}-{denNgayDateTime.Day}/{tuNgayDateTime.Month}/{tuNgayDateTime.Year}";
                                }
                            }
                            DataTable ThoiGian = new DataTable("ThoiGian");
                            DataTable KhoiSanXuat = new DataTable("KhoiSanXuat");
                            DataTable KhoiVanPhong = new DataTable("KhoiVanPhong");
                            // Tạo cột DataTable ThoiGian
                            ThoiGian.Columns.Add("6S");
                            ThoiGian.Columns.Add("Chấm nội bộ");
                            ThoiGian.Columns.Add("Chấm chéo");
                            ThoiGian.Columns.Add("Chấm hiệu chỉnh");
                            var gioValueList = new List<string> { "Giờ", gioValue_NB, gioValue_CH, gioValue_HC };
                            var ngayValueList = new List<string> { "Ngày", thoiGian_NB, thoiGian_CH, thoiGian_HC };
                            // Thêm dữ liệu từ gioData vào hàng thứ hai của DataTable
                            DataRow rowGioData = ThoiGian.NewRow();
                            for (int i = 0; i < gioValueList.Count; i++)
                            {
                                rowGioData[i] = gioValueList[i].ToString();
                            }
                            ThoiGian.Rows.Add(rowGioData);
                            // Thêm dữ liệu từ ngayData vào hàng đầu tiên của DataTable
                            DataRow rowNgayData = ThoiGian.NewRow();
                            for (int i = 0; i < ngayValueList.Count; i++)
                            {
                                rowNgayData[i] = ngayValueList[i].ToString();
                            }
                            ThoiGian.Rows.InsertAt(rowNgayData, 1);
                            //Tạo cột DataTable KhoiVanPhong
                            KhoiVanPhong.Columns.Add("KHỐI VĂN PHÒNG", typeof(string));
                            KhoiVanPhong.Columns.Add("Thành Viên Ban ĐH", typeof(string));
                            // Thêm dữ liệu từ danh sách vào DataTable
                            foreach (var item in dataPhancong_L_CH_K2)
                            {
                                DataRow row = KhoiVanPhong.NewRow();
                                var dataPhongban = db.Tbl_PhongBan.Where(x => x.ID_PhongBan == item.PhongBan).FirstOrDefault();
                                var dataUser_Get_K2 = db.Tbl_User.Where(x => x.Username == item.Username).FirstOrDefault();
                                row["KHỐI VĂN PHÒNG"] = dataPhongban.TenPhongBan;
                                row["Thành Viên Ban ĐH"] = dataUser_Get_K2.Fullname;
                                KhoiVanPhong.Rows.Add(row);
                            }
                            // Thêm cột cho DataTable KhoiSanXuat
                            KhoiSanXuat.Columns.Add("KHỐI SẢN XUẤT", typeof(string));
                            KhoiSanXuat.Columns.Add("Thành Viên Ban ĐH", typeof(string));
                            KhoiSanXuat.Columns.Add("Tổ Cải Tiến", typeof(string));
                            foreach (var item in dataPhancong_L_CH_K1)
                            {
                                DataRow row = KhoiSanXuat.NewRow();
                                var dataPhongban = db.Tbl_PhongBan.Where(x => x.ID_PhongBan == item.PhongBan && x.Ma_Khoi == "K1").FirstOrDefault();
                                var dataUser_Get_K1 = db.Tbl_User.Where(x => x.Username == item.Username).FirstOrDefault();
                                row["KHỐI SẢN XUẤT"] = dataPhongban.TenPhongBan;
                                row["Thành Viên Ban ĐH"] = dataUser_Get_K1.Fullname;
                                row["Tổ Cải Tiến"] = item.NVCaiTien;
                                KhoiSanXuat.Rows.Add(row);
                            }
                            //Tạo files pdf
                            string fileName = "";
                            string filePath = "";
                            DateTime date = DateTime.Now;
                            string formattedDate = date.Date.ToString("yyy-MM-dd");//lấy dạng năm tháng ngày
                            try
                            {
                                MemoryStream memoryStream = new MemoryStream();
                                Document document = new Document(PageSize.A4);
                                document.SetMargins(36, 36, 36, 36);
                                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                                document.Open();
                                document.AddTitle("LỊCH PHÂN CÔNG ĐÁNH GIÁ 6S" + ID_PhanCong);
                                document.AddAuthor(Session["Username"].ToString());
                                document.AddSubject("LỊCH PHÂN CÔNG ĐÁNH GIÁ 6S tháng " + formattedDate);
                                document.AddKeywords("Create by Web 6S | Bitis - Tiên Phong");
                                document.AddCreator(Session["Username"].ToString());
                                document.AddLanguage("vi-VN"); // Ngôn ngữ của tài liệu (ví dụ: tiếng Việt)
                                document.AddKeywords("Digital Factory by BITIS, phanmem6s, bitis"); // Các từ khóa khác để tìm kiếm
                                document.AddCreationDate(); // Ngày tạo tài liệu
                                // Thiết lập tiêu đề báo cáo#
                                BaseFont bf = BaseFont.CreateFont("C:/Windows/Fonts/Arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                                // Add the title
                                Font titleFont = new Font(bf, 16, Font.BOLD);
                                Paragraph title = new Paragraph("LỊCH PHÂN CÔNG ĐÁNH GIÁ 6S", titleFont);
                                title.Alignment = Element.ALIGN_CENTER;
                                document.Add(title);
                                // Đảm bảo rằng DataTable có ít nhất một dòng
                                Paragraph title_MM = new Paragraph("THÁNG " + thangNam, titleFont);
                                title_MM.Alignment = Element.ALIGN_CENTER;
                                document.Add(title_MM);
                                document.Add(Chunk.NEWLINE);
                                iTextSharp.text.Image leftImage = iTextSharp.text.Image.GetInstance(Server.MapPath("/Assets/images/Vietnam_value.jpg"));
                                leftImage.SetAbsolutePosition(40, PageSize.A4.Height - 90);///trái phải, trên dưới
                                leftImage.ScaleToFit(65, 65);//chỉnh cao rộng
                                document.Add(leftImage);
                                iTextSharp.text.Image rightImage = iTextSharp.text.Image.GetInstance(Server.MapPath("/Assets/images/Bitis_logo.jpg"));
                                rightImage.SetAbsolutePosition(500, PageSize.A4.Height - 90);
                                rightImage.ScaleToFit(70, 70);
                                document.Add(rightImage);
                                Paragraph paragraph = new Paragraph();
                                paragraph.SpacingBefore = 8f;
                                document.Add(paragraph);
                                Font lineFont = new Font(bf, 12, Font.BOLD);
                                // Tạo một bảng với hai cột
                                PdfPTable infoTable = new PdfPTable(1);
                                infoTable.WidthPercentage = 100;
                                infoTable.TotalWidth = PageSize.A4.Width - 60;
                                // Tạo cột trái và thêm thông tin vào cột này
                                PdfPCell Cell_1 = new PdfPCell();
                                Cell_1.Border = PdfPCell.NO_BORDER; // Loại bỏ viền
                                Cell_1.AddElement(new Paragraph("1. Thời gian đánh giá", lineFont));
                                // Thêm cột trái và cột phải vào bảng
                                infoTable.AddCell(Cell_1);
                                // Thêm bảng vào tài liệu
                                document.Add(infoTable);
                                Font tableFont = new Font(bf, 12, Font.BOLD);
                                // Tạo bảng PDF cho ThoiGian
                                PdfPTable tableThoiGian = new PdfPTable(ThoiGian.Columns.Count);
                                tableThoiGian.WidthPercentage = 100;
                                tableThoiGian.TotalWidth = PageSize.A4.Width - 60;
                                // Thêm tiêu đề cột
                                foreach (DataColumn column in ThoiGian.Columns)
                                {
                                    PdfPCell cell = new PdfPCell(new Phrase(column.ColumnName, tableFont));
                                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                    tableThoiGian.AddCell(cell);
                                }
                                // Thêm dữ liệu từ DataTable ThoiGian vào bảng PDF
                                foreach (DataRow row in ThoiGian.Rows)
                                {
                                    foreach (object item in row.ItemArray)
                                    {
                                        PdfPCell cell = new PdfPCell(new Phrase(item.ToString(), tableFont));
                                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                        tableThoiGian.AddCell(cell);
                                    }
                                }
                                // Thêm bảng PDF vào tài liệu
                                document.Add(tableThoiGian);
                                document.Add(new Paragraph("\n")); // Khoảng cách dòng nhỏ
                                // Tạo cột trái và thêm thông tin vào cột này
                                // Tạo bảng mới để chứa Cell_2 và thêm vào tài liệu
                                PdfPTable infoTableWithCell2 = new PdfPTable(1);
                                infoTableWithCell2.WidthPercentage = 100;
                                infoTableWithCell2.TotalWidth = PageSize.A4.Width - 60;
                                PdfPCell Cell_2 = new PdfPCell();
                                Cell_2.Border = PdfPCell.NO_BORDER;
                                Cell_2.AddElement(new Paragraph("2. Nhân sự đánh giá chấm chéo", lineFont));
                                infoTableWithCell2.AddCell(Cell_2);
                                // Thêm bảng chứa Cell_2 vào tài liệu
                                document.Add(infoTableWithCell2);
                                // Tạo bảng PDF cho KhoiSanXuat
                                PdfPTable tableKhoiSanXuat = new PdfPTable(KhoiSanXuat.Columns.Count);
                                tableKhoiSanXuat.WidthPercentage = 100;
                                tableKhoiSanXuat.TotalWidth = PageSize.A4.Width - 60;
                                // Thêm tiêu đề cột
                                foreach (DataColumn column in KhoiSanXuat.Columns)
                                {
                                    PdfPCell cell = new PdfPCell(new Phrase(column.ColumnName, tableFont));
                                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                    tableKhoiSanXuat.AddCell(cell);
                                }

                                // Thêm dữ liệu từ DataTable KhoiSanXuat vào bảng PDF
                                foreach (DataRow row in KhoiSanXuat.Rows)
                                {
                                    foreach (object item in row.ItemArray)
                                    {
                                        PdfPCell cell = new PdfPCell(new Phrase(item.ToString(), tableFont));
                                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                        tableKhoiSanXuat.AddCell(cell);
                                    }
                                }
                                // Thêm bảng PDF vào tài liệu
                                document.Add(tableKhoiSanXuat);
                                document.Add(paragraph);
                                // Tạo bảng PDF cho KhoiVanPhong
                                PdfPTable tableKhoiVanPhong = new PdfPTable(KhoiVanPhong.Columns.Count);
                                tableKhoiVanPhong.WidthPercentage = 100;
                                tableKhoiVanPhong.TotalWidth = PageSize.A4.Width - 60;
                                // Thêm tiêu đề cột
                                foreach (DataColumn column in KhoiVanPhong.Columns)
                                {
                                    PdfPCell cell = new PdfPCell(new Phrase(column.ColumnName, tableFont));
                                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                    tableKhoiVanPhong.AddCell(cell);
                                }
                                // Thêm dữ liệu từ DataTable KhoiVanPhong vào bảng PDF
                                foreach (DataRow row in KhoiVanPhong.Rows)
                                {
                                    foreach (object item in row.ItemArray)
                                    {
                                        PdfPCell cell = new PdfPCell(new Phrase(item.ToString(), tableFont));
                                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                        tableKhoiVanPhong.AddCell(cell);
                                    }
                                }
                                // Thêm bảng PDF vào tài liệu
                                document.Add(tableKhoiVanPhong);

                                document.Add(paragraph); // Khoảng cách dòng nhỏ
                                // Tạo bảng mới để chứa Cell_3 và thêm vào tài liệu
                                PdfPTable infoTableWithCell3 = new PdfPTable(1);
                                infoTableWithCell3.WidthPercentage = 100;
                                infoTableWithCell3.TotalWidth = PageSize.A4.Width - 60;
                                PdfPCell Cell_3 = new PdfPCell();
                                Cell_3.Border = PdfPCell.NO_BORDER;
                                Cell_3.AddElement(new Paragraph("Lưu ý: Khi đi chấm phải có đại diện đơn vị đi cùng để ghi nhận", lineFont));
                                infoTableWithCell3.AddCell(Cell_3);
                                // Thêm bảng chứa Cell_2 vào tài liệu
                                document.Add(infoTableWithCell3);
                                document.Add(paragraph);
                                PdfPTable table = new PdfPTable(2);
                                table.WidthPercentage = 100;
                                table.TotalWidth = PageSize.A4.Width - 60;
                                table.DefaultCell.Border = Rectangle.NO_BORDER;
                                // Ô "Phê duyệt" - bên phải
                                PdfPCell approvalCell = new PdfPCell(new Phrase("PHÊ DUYỆT", lineFont));
                                approvalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                approvalCell.Border = Rectangle.BOX;
                                table.AddCell(approvalCell);
                                // Ô trống để chứa chữ ký - bên phải
                                PdfPCell signatureCell = new PdfPCell(new Phrase("TRÌNH KÝ", lineFont));
                                signatureCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                table.AddCell(signatureCell);
                                // Ô "Họ và tên" - bên trái
                                PdfPCell nameCell = new PdfPCell(new Phrase());
                                nameCell.FixedHeight = 150;
                                table.AddCell(nameCell);
                                // Ô trống để chứa thông tin "Họ và tên" - bên trái
                                PdfPCell nameInputCell = new PdfPCell();
                                nameInputCell.FixedHeight = 30;
                                table.AddCell(nameInputCell);
                                // Ô "Ngày" - bên phải
                                PdfPCell dateCell = new PdfPCell();
                                dateCell.AddElement(new Paragraph("Họ và tên: " + Session["Fullname"]?.ToString(), lineFont));
                                dateCell.AddElement(new Paragraph("Ngày: " + formattedDate, lineFont));
                                dateCell.HorizontalAlignment = PdfPCell.ALIGN_LEFT; // Căn chỉnh ô bên phải
                                table.AddCell(dateCell);
                                // Ô trống để chứa thông tin "Ngày" - bên phải
                                PdfPCell dateInputCell = new PdfPCell();
                                dateInputCell.AddElement(new Paragraph("Họ và tên: " + dataUser_Get_Nguoitao_Phancong_H.Fullname, lineFont));
                                dateInputCell.AddElement(new Paragraph("Ngày: " + dataPhancong_H.Ngay_Tao.Value.ToString("yyyy-MM-dd"), lineFont));
                                dateInputCell.HorizontalAlignment = PdfPCell.ALIGN_LEFT; // Căn chỉnh ô bên trái
                                dateInputCell.FixedHeight = 30;
                                table.AddCell(dateInputCell);
                                document.Add(table);
                                //tạo đường chân trang
                                PdfTemplate template = writer.DirectContent.CreateTemplate(30, 16);
                                // Lặp qua từng trang
                                for (int i = 1; i <= writer.PageNumber; i++)
                                {
                                    //float xPageNumber = 310;
                                    float xTimestamp = 15;
                                    float y = 20;

                                    PdfContentByte canvas = writer.DirectContent;
                                    canvas.SaveState();
                                    canvas.BeginText();
                                    canvas.SetFontAndSize(BaseFont.CreateFont(), 12);
                                    // Create a Chunk for the report code
                                    Chunk maBaoCaoChunk = new Chunk("Mã báo cáo: " + ID_PhanCong);
                                    // Draw the report code
                                    canvas.ShowTextAligned(Element.ALIGN_LEFT, maBaoCaoChunk.ToString(), xTimestamp, y, 0);
                                    canvas.EndText();
                                    canvas.RestoreState();
                                }
                                // Tạo một template để chứa số trang và thời gian tạo
                                PdfTemplate template_number = writer.DirectContent.CreateTemplate(30, 16);
                                ColumnText.ShowTextAligned(template_number, Element.ALIGN_RIGHT, new Phrase(writer.PageNumber.ToString()), 15, 2, 0);
                                for (int i = 1; i <= writer.PageNumber; i++)
                                {
                                    PdfContentByte canvas = writer.DirectContentUnder;
                                    canvas.AddTemplate(template_number, 310, 20); // Đặt vị trí của template ở chân trang (250, 20)
                                }
                                document.Close();
                                byte[] pdfBytes = memoryStream.ToArray();
                                // Lưu tệp PDF xuống đĩa
                                fileName = "Lich_lam_viec_6S_thang_" + formattedDate + "_" + ID_PhanCong + "_" + Session["Username"]?.ToString() + ".pdf";
                                filePath = Path.Combine("\\\\192.168.24.108\\6s", fileName);
                                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    fileStream.Write(pdfBytes, 0, pdfBytes.Length);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Lỗi: ", ex);
                                Console.WriteLine("Lỗi tạo files: " + ex.Message);
                                return Json(new
                                {
                                    status = "error",
                                    message = ex.Message,
                                    redirectUrl = Url.Action("Index_checklist", "Home")
                                });
                            }
                            try
                            {
                                SqlParameter[] parameters_Approve = new SqlParameter[]
                                {
                                    new SqlParameter("@ID_PhanCong", SqlDbType.VarChar, 20) { Value = ID_PhanCong },
                                    new SqlParameter("@Ngay_Ky", SqlDbType.Date) { Value = DateTime.Now },
                                    new SqlParameter("@Nguoi_Ky", SqlDbType.NVarChar, 30) { Value = Session["Username"]?.ToString() },
                                    new SqlParameter("@Duongluu", SqlDbType.VarChar, 250) { Value = fileName },
                                    new SqlParameter("@Status", SqlDbType.Int) { Value = Status },
                                };
                                db.Database.ExecuteSqlCommand("EXEC sp_Approve_TblPhanCong_H_L @ID_PhanCong, @Ngay_Ky, @Nguoi_Ky, @Duongluu, @Status", parameters_Approve);
                                logger.Info("Duyệt lịch làm: " + ID_PhanCong + " " + "trạng thái: " + Status + " thành công" + "User del: " + Session["Username"]?.ToString());
                                var datalist = db.Tbl_PhanCong_H.ToList();
                                return Json(new
                                {
                                    userList = datalist,
                                    success = true,
                                    message = "Đã duyệt Lịch làm 6S thành công :" + ID_PhanCong,
                                    redirectUrl = Url.Action("Index_PhanCong", "PhanCong")
                                });
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Lỗi: ", ex);
                                var datalist = db.Tbl_PhanCong_H.ToList();
                                return Json(new
                                {
                                    success = false,
                                    userList = datalist,
                                    message = "Lỗi khi sửa tài khoản: " + ex.Message
                                });
                            }
                        }
                    }
                    else
                    {
                        logger.Error("Lỗi: " + "Không có lịch làm :" + ID_PhanCong);
                        var datalist = db.Tbl_PhanCong_H.ToList();
                        return Json(new
                        {
                            userList = datalist,
                            success = false,
                            message = "Không có lịch làm việc :" + ID_PhanCong,
                            redirectUrl = Url.Action("Index_PhanCong", "PhanCong")
                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Lỗi: ", ex);
                    var datalist = db.Tbl_PhanCong_H.ToList();
                    return Json(new
                    {
                        userList = datalist,
                        success = false,
                        message = ex.Message,
                        redirectUrl = Url.Action("Index_PhanCong", "PhanCong")
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
        public ActionResult Delete_PhanCong(string ID_PhanCong, int Status, Tbl_PhanCong_L phancong_L)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                try
                {
                    var recordToApprove = db.Tbl_PhanCong_H.Find(ID_PhanCong);
                    var recordToApprove_L = db.Tbl_PhanCong_L.Where(x => x.ID_PhanCong == recordToApprove.ID_PhanCong && x.Status == recordToApprove.Status).ToList();
                    if (recordToApprove != null && recordToApprove_L != null)
                    {
                        if (recordToApprove.Status == 0)
                        {
                            logger.Error("Lỗi: " + " Lịch làm đã về 0 :" + ID_PhanCong);
                            var datalist = db.Tbl_User.ToList();
                            return Json(new
                            {
                                userList = datalist,
                                success = false,
                                message = "Lịch làm đã được Xóa trước đó :" + ID_PhanCong,
                                redirectUrl = Url.Action("Index_PhanCong", "User")
                            });
                        }
                        else
                        {
                            recordToApprove.Status = Status;
                            foreach (var item in recordToApprove_L)
                            {
                                item.Status = Status;
                                db.Entry(item).State = EntityState.Modified;
                            }
                            db.Entry(recordToApprove).Property(x => x.Status).IsModified = true;
                            db.SaveChanges();
                            logger.Info("Xóa lịch làm: " + ID_PhanCong + " " + "trạng thái: " + Status + " thành công" + "User del: " + Session["Username"]?.ToString());
                            var datalist = db.Tbl_PhanCong_H.ToList();
                            return Json(new
                            {
                                userList = datalist,
                                success = true,
                                message = "Đã Xóa Lịch làm 6S thành công :" + ID_PhanCong,
                                redirectUrl = Url.Action("Index_PhanCong", "PhanCong")
                            });
                        }
                    }
                    else
                    {
                        logger.Error("Lỗi: " + "Không có lịch làm :" + ID_PhanCong);
                        var datalist = db.Tbl_PhanCong_H.ToList();
                        return Json(new
                        {
                            userList = datalist,
                            success = false,
                            message = "Đã Xóa Lịch làm 6S không thành công :" + ID_PhanCong,
                            redirectUrl = Url.Action("Index_PhanCong", "PhanCong")
                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Lỗi: ", ex);
                    var datalist = db.Tbl_PhanCong_H.ToList();
                    return Json(new
                    {
                        userList = datalist,
                        success = false,
                        message = ex.Message,
                        redirectUrl = Url.Action("Index_PhanCong", "PhanCong")
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
        public ActionResult Delete_PhanCong_Edit_Row(string valuePhongBan, int Status, string valueUsername, string ID_PhanCong)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                try
                {
                    var recordToApprove = db.Tbl_PhanCong_L.Where(x => x.ID_PhanCong == ID_PhanCong && x.Username == valueUsername && x.PhongBan == valuePhongBan && x.Status == 1).FirstOrDefault();
                    if (recordToApprove != null)
                    {
                        if (recordToApprove.Status == 0)
                        {
                            logger.Error("Lỗi: " + " Lịch làm đã về 0 :" + ID_PhanCong);
                            var datalist = db.Tbl_User.ToList();
                            return Json(new
                            {
                                userList = datalist,
                                success = false,
                                message = "Lịch làm đã được Xóa trước đó :" + ID_PhanCong,
                                redirectUrl = Url.Action("Index_PhanCong", "User")
                            });
                        }
                        else
                        {
                            recordToApprove.Status = Status;
                            db.Entry(recordToApprove).Property(x => x.Status).IsModified = true;
                            db.SaveChanges();
                            logger.Info("Xóa lịch làm: " + ID_PhanCong + " " + "trạng thái: " + Status + " thành công" + "User del: " + Session["Username"]?.ToString());
                            var datalist = db.Tbl_PhanCong_L.ToList();
                            return Json(new
                            {
                                userList = datalist,
                                success = true,
                                message = "Đã Xóa Lịch làm 6S thành công :" + ID_PhanCong,
                                redirectUrl = Url.Action("Index_PhanCong", "PhanCong")
                            });
                        }
                    }
                    else
                    {
                        logger.Error("Lỗi: " + "Không có lịch làm :" + ID_PhanCong);
                        var datalist = db.Tbl_PhanCong_L.ToList();
                        return Json(new
                        {
                            userList = datalist,
                            success = false,
                            message = "Đã Xóa Lịch làm 6S không thành công :" + ID_PhanCong,
                            redirectUrl = Url.Action("Index_PhanCong", "PhanCong")
                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Lỗi: ", ex);
                    var datalist = db.Tbl_PhanCong_L.ToList();
                    return Json(new
                    {
                        userList = datalist,
                        success = false,
                        message = ex.Message,
                        redirectUrl = Url.Action("Index_PhanCong", "PhanCong")
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