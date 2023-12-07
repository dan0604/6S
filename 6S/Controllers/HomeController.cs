using _6S.Context;
using _6S.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using static iTextSharp.text.pdf.AcroFields;
using Path = System.IO.Path;

namespace _6S.Controllers
{
    public class HomeController : Controller
    {
        Model_6S db = new Model_6S();
        Share_All share_All = new Share_All();
        private ILog logger = LogManager.GetLogger(typeof(HomeController));
        [HttpGet]
        public ActionResult Index_checklist()
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
                        var query = from baocao_h in db.Tbl_BaoCao_Cham6S_H
                                    from phongban in db.Tbl_PhongBan
                                    from khoi in db.Tbl_Khoi
                                    where baocao_h.PhongBan == phongban.ID_PhongBan && phongban.Ma_Khoi == khoi.Ma_Khoi
                                    select new Tbl_Join_ALL { baocao_H = baocao_h, PhongBan = phongban, Khoi = khoi };
                        logger.Info("Đã lấy được danh sách ở 1000  && !Status: Index_checklist" + "User view: " + Session["Username"]?.ToString());
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
                            var query = from baocao_h in db.Tbl_BaoCao_Cham6S_H
                                        from phongban in db.Tbl_PhongBan
                                        from khoi in db.Tbl_Khoi
                                        where baocao_h.PhongBan == phongban.ID_PhongBan && phongban.Ma_Khoi == khoi.Ma_Khoi && baocao_h.Status == 1
                                        select new Tbl_Join_ALL { baocao_H = baocao_h, PhongBan = phongban, Khoi = khoi };
                            logger.Info("Đã lấy được danh sách ở khác 1000 Status 1: Index_checklist" + "User view: " + Session["Username"]?.ToString());
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
        public ActionResult Add_checklist(string dataToSend)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var phongBan = Request.Form["PhongBan"]; // Lấy giá trị từ field "PhongBan"
                        var loaiBC = Request.Form["Loai_BC"]; // Lấy giá trị từ field "Loai_BC"
                        var username = Session["Username"]?.ToString(); // Lấy giá trị từ field "Username"fixbug 061223
                        var ngayBC = Request.Form["Ngay_BC"]; // Lấy giá trị từ field "Ngay_BC"
                        var error = Request.Form["errorCount"]; // Lấy giá trị từ field "errorCount"
                        var point = Request.Form["pointCount"]; // Lấy giá trị từ field "pointCount"
                        var image = Request.Form["imageCount"]; // Lấy giá trị từ field "imageCount"
                        // Tạo đối tượng DateTime từ chuỗi thời gian
                        DateTime date = DateTime.Parse(ngayBC);
                        // Lấy ngày từ đối tượng DateTime
                        string formattedDate = date.Date.ToString("yyy-MM-dd");//lấy dạng năm tháng ngày
                        string formattedDate_MM = date.Date.ToString("MM");// tháng dạng tháng
                        string fileName = "";
                        string filePath = "";
                        // Lấy dữ liệu JSON từ FormData và deserialize nó thành một đối tượng
                        List<Tbl_BaoCao_Cham6S_L> dataList = JsonConvert.DeserializeObject<List<Tbl_BaoCao_Cham6S_L>>(dataToSend);
                        //Tạo table chứa danh sách lấy được từ dataToSend
                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("STT", typeof(string));
                        dataTable.Columns.Add("Hangmuc_Chinh", typeof(string));
                        dataTable.Columns.Add("Hangmuc_Phu", typeof(string));
                        dataTable.Columns.Add("KhuVuc", typeof(string));
                        dataTable.Columns.Add("MoTa", typeof(string));
                        dataTable.Columns.Add("HinhAnh", typeof(string));
                        dataTable.Columns.Add("Ma_BC", typeof(string));
                        //chạy vòng thêm để thêm giá trị vào từng cột
                        foreach (var item in dataList)
                        {
                            dataTable.Rows.Add(item.STT, item.Hangmuc_Chinh, item.HangMuc_Phu, item.KhuVuc, item.MoTa, item.HinhAnh, loaiBC);
                        }
                        string Ma_BC = "";
                        logger.Info("Ma_BC: " + Ma_BC + "User add: " + Session["Username"]?.ToString());
                        try
                        {
                            using (var db = new Model_6S())
                            {
                                SqlParameter[] parameters_Ma_BC = new SqlParameter[]
                                {
                                    new SqlParameter("@ma", SqlDbType.NVarChar, 20){ Value = loaiBC},
                                    new SqlParameter("@namtaoma", SqlDbType.Date){ Value = formattedDate},
                                };
                                Ma_BC = db.Database.SqlQuery<string>("EXEC dbo.sp_TaoMaBC @ma, @namtaoma", parameters_Ma_BC).FirstOrDefault();
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Lỗi: ", ex);
                            Console.WriteLine("Lỗi sp_TaoMaBC: " + ex.Message);
                            return Json(new
                            {
                                status = "error",
                                message = ex.Message,
                                redirectUrl = Url.Action("Index_checklist", "Home")
                            });
                        }
                        List<Tbl_BaoCao_Cham6S_L> dataList_rpt = JsonConvert.DeserializeObject<List<Tbl_BaoCao_Cham6S_L>>(dataToSend);
                        //Tạo table chứa danh sách lấy được từ dataToSend
                        DataTable dataTable_rpt = new DataTable();
                        dataTable_rpt.Columns.Add("STT", typeof(string));
                        dataTable_rpt.Columns.Add("Khu vực", typeof(string));
                        dataTable_rpt.Columns.Add("Hạng mục 6S", typeof(string));
                        dataTable_rpt.Columns.Add("Mô tả vấn đề", typeof(string));
                        dataTable_rpt.Columns.Add("Hình ảnh ghi nhận", typeof(string));
                        //chạy vòng thêm để thêm giá trị vào từng cột
                        foreach (var item in dataList_rpt)
                        {
                            string combinedHangmuc = $"{item.Hangmuc_Chinh}, {item.HangMuc_Phu}";
                            dataTable_rpt.Rows.Add(item.STT, item.KhuVuc, combinedHangmuc, item.MoTa, item.HinhAnh);
                        }
                        var query_phongBan = db.Tbl_PhongBan.Where(x => x.ID_PhongBan == phongBan).FirstOrDefault();
                        var username_Fullname = db.Tbl_User.Where(x => x.Username == username).FirstOrDefault();
                        try
                        {
                            MemoryStream memoryStream = new MemoryStream();
                            Document document = new Document(PageSize.A4);
                            document.SetMargins(36, 36, 36, 36);
                            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                            document.Open();
                            document.AddTitle("Báo cáo đánh giá 6S" + Ma_BC);
                            document.AddAuthor(Session["Username"].ToString());
                            document.AddSubject("Báo cáo đánh giá 6S tháng " + formattedDate_MM);
                            document.AddKeywords("Create by Web 6S | Bitis - Tiên Phong");
                            document.AddCreator(Session["Username"].ToString());
                            document.AddLanguage("vi-VN"); // Ngôn ngữ của tài liệu (ví dụ: tiếng Việt)
                            document.AddKeywords("Digital Factory by BITIS, phanmem6s, bitis"); // Các từ khóa khác để tìm kiếm
                            document.AddCreationDate(); // Ngày tạo tài liệu
                            // Thiết lập tiêu đề báo cáo#
                            BaseFont bf = BaseFont.CreateFont("C:/Windows/Fonts/Arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                            // Add the title
                            Font titleFont = new Font(bf, 16, Font.BOLD);
                            Paragraph title = new Paragraph("BÁO CÁO ĐÁNH GIÁ 6S", titleFont);
                            title.Alignment = Element.ALIGN_CENTER;
                            document.Add(title);
                            Paragraph title_MM = new Paragraph("Tháng " + formattedDate_MM, titleFont);
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
                            Font lineFont = new Font(bf, 12, Font.BOLD);
                            // Tạo một bảng với hai cột
                            PdfPTable infoTable = new PdfPTable(2);
                            infoTable.WidthPercentage = 100;
                            infoTable.TotalWidth = PageSize.A4.Width - 60; // Đặt chiều rộng của bảng (trừ lề trái và lề phải)
                            // Tạo cột trái và thêm thông tin vào cột này
                            PdfPCell leftCell = new PdfPCell();
                            leftCell.Border = PdfPCell.NO_BORDER; // Loại bỏ viền
                            leftCell.AddElement(new Paragraph("Xưởng/ Văn: " + query_phongBan.TenPhongBan, lineFont));
                            leftCell.AddElement(new Paragraph("Ngày chấm: " + formattedDate, lineFont));
                            leftCell.AddElement(new Paragraph("Người Đánh Giá: " + username_Fullname.Fullname, lineFont));
                            // Tạo cột phải và thêm thông tin vào cột này
                            PdfPCell rightCell = new PdfPCell();
                            rightCell.Border = PdfPCell.NO_BORDER; // Loại bỏ viền
                            rightCell.VerticalAlignment = Element.ALIGN_RIGHT;
                            rightCell.AddElement(new Paragraph("Tổng lỗi: " + error, lineFont));
                            rightCell.AddElement(new Paragraph("Tổng điểm: " + point, lineFont));
                            rightCell.AddElement(new Paragraph("Tổng hình ảnh lỗi: " + image, lineFont));
                            // Thêm cột trái và cột phải vào bảng
                            infoTable.AddCell(leftCell);
                            infoTable.AddCell(rightCell);
                            // Thêm bảng vào tài liệu
                            document.Add(infoTable);
                            // Số lượng cột
                            int numberOfColumns = dataTable_rpt.Columns.Count;

                            // Mảng chứa chiều rộng của các cột
                            float[] columnWidths = new float[numberOfColumns];

                            // Thiết lập chiều rộng cho từng cột (ví dụ: 3 cột đầu lớn hơn)
                            columnWidths[0] = 70f; // Chiều rộng của cột đầu
                            columnWidths[1] = 100f; // Chiều rộng của cột thứ hai
                            columnWidths[2] = 150f; // Chiều rộng của cột thứ ba (lớn hơn)
                            columnWidths[3] = 300f; // Chiều rộng của cột thứ tư
                            columnWidths[4] = 400f; // Chiều rộng của cột thứ năm

                            PdfPTable table = new PdfPTable(numberOfColumns);
                            table.SetWidths(columnWidths);
                            table.WidthPercentage = 100;

                            foreach (DataColumn column in dataTable_rpt.Columns)
                            {
                                Font tableFont = new Font(bf, 12, Font.BOLD);
                                PdfPCell cell = new PdfPCell(new Phrase(column.ColumnName, tableFont));
                                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(cell);
                            }
                            foreach (DataRow row in dataTable_rpt.Rows)
                            {
                                foreach (DataColumn column in dataTable_rpt.Columns)
                                {
                                    PdfPCell cell;
                                    if (column.ColumnName == "Hình ảnh ghi nhận")
                                    {
                                        string imageURL = row[column.ColumnName].ToString();

                                        if (!string.IsNullOrEmpty(imageURL))
                                        {
                                            try
                                            {
                                                if (imageURL.Contains(","))
                                                {
                                                    imageURL = imageURL.Split(',')[1];
                                                }
                                                byte[] imageBytes = Convert.FromBase64String(imageURL);
                                                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imageBytes);
                                                PdfPCell imageCell = new PdfPCell(img, true);
                                                table.AddCell(imageCell);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine("Lỗi hình: " + ex.Message);
                                                return Json(new
                                                {
                                                    status = false,
                                                    message = ex.Message,
                                                    redirectUrl = Url.Action("Index_checklist", "Home")
                                                });
                                            }
                                        }
                                        else
                                        {
                                            // Nếu không có đường dẫn hình ảnh, thêm một ô trống
                                            Font tableFont = new Font(bf, 12, Font.BOLD);
                                            cell = new PdfPCell(new Phrase("", tableFont));
                                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                            table.AddCell(cell);
                                        }
                                    }
                                    else
                                    {
                                        Font tableFont = new Font(bf, 12, Font.BOLD);
                                        cell = new PdfPCell(new Phrase(row[column].ToString(), tableFont));
                                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                        table.AddCell(cell);
                                    }
                                }
                            }
                            document.Add(table);

                            int tongGhiNhan = dataTable_rpt.AsEnumerable().Max(row => int.Parse(row.Field<string>("STT")));
                            PdfPTable infoTableFooter = new PdfPTable(2);
                            infoTableFooter.WidthPercentage = 100;

                            // Tính tổng chiều rộng của 3 cột đầu và 2 cột cuối
                            float leftCellWidth = columnWidths[0] + columnWidths[1] + columnWidths[2];
                            float rightCellWidth = columnWidths[3] + columnWidths[4];

                            // Tạo cột trái và thêm thông tin vào cột này
                            PdfPCell leftCellFooter = new PdfPCell(new Paragraph("TỔNG GHI NHẬN", lineFont));
                            leftCellFooter.HorizontalAlignment = Element.ALIGN_CENTER;
                            leftCellFooter.VerticalAlignment = Element.ALIGN_MIDDLE;

                            // Tạo cột phải và thêm thông tin vào cột này
                            PdfPCell rightCellFooter = new PdfPCell(new Paragraph(tongGhiNhan.ToString(), lineFont));
                            rightCellFooter.HorizontalAlignment = Element.ALIGN_CENTER;
                            rightCellFooter.VerticalAlignment = Element.ALIGN_MIDDLE;

                            // Thiết lập phần trăm chiều rộng cho cột trái và cột phải
                            float[] widths = new float[] { leftCellWidth, rightCellWidth };
                            infoTableFooter.SetWidths(widths);

                            // Thêm cột trái và cột phải vào bảng
                            infoTableFooter.AddCell(leftCellFooter);
                            infoTableFooter.AddCell(rightCellFooter);

                            // Thêm bảng vào tài liệu và căn giữa bảng trong tài liệu
                            document.Add(infoTableFooter);
                            infoTableFooter.HorizontalAlignment = Element.ALIGN_CENTER;
                            PdfPCell tableCell = new PdfPCell(infoTableFooter);
                            tableCell.HorizontalAlignment = Element.ALIGN_CENTER;
                            tableCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            document.Add(tableCell);
                            // Tạo đoạn văn bản mới
                            // Tạo bảng với 3 ô
                            PdfPTable infoTableFooter_newSignature = new PdfPTable(3);
                            infoTableFooter_newSignature.WidthPercentage = 100;

                            // Tạo ô đầu tiên
                            PdfPCell cell1 = new PdfPCell();
                            cell1.HorizontalAlignment = Element.ALIGN_LEFT;
                            cell1.Border = PdfPCell.NO_BORDER;
                            infoTableFooter_newSignature.AddCell(cell1);

                            // Tạo ô thứ hai
                            PdfPCell cell2 = new PdfPCell();
                            cell2.HorizontalAlignment = Element.ALIGN_LEFT;
                            cell2.Border = PdfPCell.NO_BORDER;
                            infoTableFooter_newSignature.AddCell(cell2);

                            PdfPCell cell3 = new PdfPCell();
                            cell3.Border = PdfPCell.NO_BORDER;
                            Paragraph paragraph1 = new Paragraph("Người đánh giá", lineFont);
                            paragraph1.SpacingAfter = 100f; // Khoảng cách sau dòng (đơn vị là điểm)
                            paragraph1.Alignment = Element.ALIGN_CENTER; // Căn giữa cho dòng này
                            cell3.AddElement(paragraph1);

                            Paragraph paragraph2 = new Paragraph(username_Fullname.Fullname, lineFont);
                            paragraph2.SpacingAfter = 5f;  // Khoảng cách sau dòng (đơn vị là điểm)
                            paragraph2.Alignment = Element.ALIGN_CENTER; // Căn giữa cho dòng này
                            cell3.AddElement(paragraph2);

                            Paragraph paragraph3 = new Paragraph("Thời gian:" + DateTime.Now.ToString("HH:mm dd-MM-yyyy"), lineFont);
                            paragraph3.Alignment = Element.ALIGN_CENTER; // Căn giữa cho dòng này
                            cell3.AddElement(paragraph3);
                            infoTableFooter_newSignature.AddCell(cell3);
                            // Thêm bảng vào tài liệu
                            document.Add(infoTableFooter_newSignature);
                            // Tạo một template để chứa số trang và thời gian tạo
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
                                Chunk maBaoCaoChunk = new Chunk("Mã báo cáo: " + Ma_BC);

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
                            fileName = "Template_bao_cao_6S_thang" + formattedDate_MM + "_" + Ma_BC + "_" + username + ".pdf";
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
                        //thêm dbo.Tbl_BaoCao_Cham6S_H
                        try
                        {
                            ///thêm bảng H
                            SqlParameter[] parameters_H = new SqlParameter[]
                            {
                                new SqlParameter("@Mabc", SqlDbType.NVarChar){ Value = Ma_BC },
                                new SqlParameter("@Loaibc", SqlDbType.NVarChar){ Value = loaiBC },
                                new SqlParameter("@Username", SqlDbType.VarChar){ Value = username },
                                new SqlParameter("@Ngaybc", SqlDbType.Date){ Value = formattedDate },
                                new SqlParameter("@loi", SqlDbType.Int){ Value = error },
                                new SqlParameter("@diem", SqlDbType.Int){ Value = point },
                                new SqlParameter("@phongban", SqlDbType.NVarChar){ Value = phongBan },
                                new SqlParameter("@duongluu", SqlDbType.VarChar){ Value = fileName },
                                new SqlParameter("@tonghinh", SqlDbType.Int){ Value = image },
                                new SqlParameter("@ngaytao_bc", SqlDbType.Date){ Value = DateTime.Now },
                            };
                            // Thực hiện truy vấn SQL mà không cần trả về dữ liệu
                            db.Database.ExecuteSqlCommand("EXEC sp_Insert_H @Mabc,@Loaibc, @Username, @Ngaybc, @loi, @diem, @phongban, @duongluu, @tonghinh, @ngaytao_bc", parameters_H);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Lỗi: ", ex);
                            Console.WriteLine("Lỗi sp_Insert_H: " + ex.Message);
                            return Json(new
                            {
                                status = false,
                                message = ex.Message,
                                redirectUrl = Url.Action("Index_checklist", "Home")
                            });
                        }
                        //thêm dbo.Tbl_BaoCao_Cham6S_L
                        try
                        {
                            //khai bái giá trị STT
                            foreach (DataRow row in dataTable.Rows)
                            {
                                // Lấy dữ liệu từ mỗi dòng của DataTable
                                string STT = row["STT"].ToString();
                                string hangMucChinh = row["Hangmuc_Chinh"].ToString();
                                string hangMucPhu = row["Hangmuc_Phu"].ToString();
                                string khuVuc = row["KhuVuc"].ToString();
                                string moTa = row["MoTa"].ToString();
                                string hinhAnh = row["HinhAnh"].ToString();
                                //Thêm bảng L
                                SqlParameter[] parameters_L = new SqlParameter[]
                                {
                                    new SqlParameter("@ma", SqlDbType.NVarChar, 20) { Value = Ma_BC },
                                    new SqlParameter("@STT", SqlDbType.Int) { Value = STT }, // Sử dụng rowCount
                                    new SqlParameter("@khuvuc", SqlDbType.NVarChar, 20) { Value = khuVuc },
                                    new SqlParameter("@hangmucchinh", SqlDbType.NVarChar, 30) { Value = hangMucChinh },
                                    new SqlParameter("@hangmucphu", SqlDbType.NVarChar, 30) { Value = hangMucPhu },
                                    new SqlParameter("@mota", SqlDbType.NVarChar, 255) { Value = moTa },
                                    new SqlParameter("@hinhanh", SqlDbType.VarChar, 100) { Value = hinhAnh },
                                };
                                // Thực hiện truy vấn SQL mà không cần trả về dữ liệu
                                db.Database.ExecuteSqlCommand("EXEC sp_Insert_L @ma, @STT, @khuvuc, @hangmucchinh, @hangmucphu, @mota, @hinhanh", parameters_L);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Lỗi: ", ex);
                            Console.WriteLine("Lỗi sp_Insert_L: " + ex.Message);
                            return Json(new
                            {
                                status = false,
                                message = ex.Message,
                                redirectUrl = Url.Action("Index_checklist", "Home")
                            });
                        }
                        // Sử dụng Process.Start để mở thư mục
                        string folderPath = Path.GetDirectoryName(filePath);
                        System.Diagnostics.Process.Start("explorer.exe", folderPath);
                        logger.Info("Đã tạo đánh giá thành công: " + Ma_BC + "User add: " + Session["Username"]?.ToString());
                        return Json(new
                        {
                            status = true,
                            message = "Đã tạo checklist 6S thành công :" + Ma_BC,
                            redirectUrl = Url.Action("Index_checklist", "Home")
                        });
                    }
                    catch (Exception ex)
                    {
                        //lỗi ex
                        logger.Error("Lỗi: ", ex);
                        Console.WriteLine("Lỗi chung: " + ex.Message);
                        return Json(new
                        {
                            status = false,
                            message = ex.Message,
                            redirectUrl = Url.Action("Index_checklist", "Home")
                        });
                    }
                }
                else
                {
                    //lỗi ModelState giá trị có vấn đề
                    logger.Error("Lỗi: " + "ModelState");
                    Console.WriteLine("Lỗi ModelState");
                    return Json(new
                    {
                        status = false,
                        message = "Giá trị chưa hợp lệ",
                        redirectUrl = Url.Action("Index_checklist", "Home")
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
        public ActionResult View_checklist(string Ma_BC)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                try
                {
                    var item = db.Tbl_BaoCao_Cham6S_H.Find(Ma_BC);
                    if (item != null)
                    {
                        var path = @"\\192.168.24.108\6s";
                        var file = Path.Combine(path, item.Duongluu);
                        file = Path.GetFullPath(file);
                        if (file.StartsWith(path) && System.IO.File.Exists(file))
                        {
                            // Return the PDF URL
                            ViewBag.Ma_BC = Ma_BC;
                            byte[] pdfData = System.IO.File.ReadAllBytes(file);
                            string base64Pdf = Convert.ToBase64String(pdfData);
                            logger.Info("Xem files: " + item.Duongluu);
                            return Json(new { pdfData = base64Pdf, Ma_BC = Ma_BC }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            logger.Error("Lỗi: " + "Không tìm thấy tệp báo cáo đánh giá 6S " + item.Ma_BC);
                            return Json(new { error = "Không tìm thấy tệp báo cáo đánh giá 6S " + item.Ma_BC });
                        }
                    }
                    else
                    {
                        logger.Error("Lỗi: " + "Không tìm thấy MÃ báo cáo đánh giá 6S " + item.Ma_BC);
                        return Json(new { error = "Không tìm thấy MÃ báo cáo đánh giá 6S " + item.Ma_BC });
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
        public ActionResult DownloadPDFFile(string Ma_BC)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                try
                {
                    var item = db.Tbl_BaoCao_Cham6S_H.Where(x => x.Ma_BC == Ma_BC).FirstOrDefault();
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
        [HttpPost]
        public ActionResult Delete_checklist(string Ma_BC , int Status)
        {
            var checkAccount = share_All.CheckAccount(Session["Username"]?.ToString());
            if (checkAccount == true)
            {
                try
                {
                    var recordToDelete_H = db.Tbl_BaoCao_Cham6S_H.Find(Ma_BC);
                    if (recordToDelete_H != null)
                    {
                        var recordToDelete_L = db.Tbl_BaoCao_Cham6S_L.Where(x => x.Ma_BC == Ma_BC).FirstOrDefault();
                        if (recordToDelete_L != null)
                        {
                            if (recordToDelete_H.Status == 0 && recordToDelete_L.Status == 0)
                            {
                                logger.Error("Lỗi: " + " Ma_BC đã về 0 :" + Ma_BC);
                                return Json(new
                                {
                                    status = "error",
                                    message = "Báo cáo đã được xóa trước đó :" + Ma_BC,
                                    redirectUrl = Url.Action("Index_checklist", "Home")
                                });
                            }
                            else
                            {
                                recordToDelete_L.Status = Status;
                                recordToDelete_H.Status = Status;
                                db.Entry(recordToDelete_H).Property(x => x.Status).IsModified = true;
                                db.Entry(recordToDelete_L).State = EntityState.Modified;
                                logger.Info("Xóa đánh giá: " + Ma_BC + " " + "trạng thái: " + Status + " thành công" + "User del: " + Session["Username"]?.ToString());
                                db.SaveChanges();
                                return Json(new
                                {
                                    status = "success",
                                    message = "Đã xóa checklist 6S thành công :" + Ma_BC,
                                    redirectUrl = Url.Action("Index_checklist", "Home")
                                });
                            }
                        }
                        else
                        {
                            logger.Error("Lỗi: " + "Không có dữ liệu L :" + Ma_BC);
                            return Json(new
                            {
                                status = "error",
                                message = "Không có dữ liệu L :" + Ma_BC,
                                redirectUrl = Url.Action("Index_checklist", "Home")
                            });
                        }
                    }
                    else
                    {
                        logger.Error("Lỗi: " + "Không có dữ liệu H :" + Ma_BC);
                        return Json(new
                        {
                            status = "error",
                            message = "Không có dữ liệu H :" + Ma_BC,
                            redirectUrl = Url.Action("Index_checklist", "Home")
                        });
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

            }
            else
            {
                logger.Error("Lỗi: " + "xác thực !checkAccount");
                return RedirectToAction("Login", "Login");
            }
        }
    }
}

