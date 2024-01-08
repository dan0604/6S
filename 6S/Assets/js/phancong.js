
function loadData() {
    var maKhoi = $('#Ma_Khoi').val();
    $.ajax({
        url: '/DataApi/Get_PhongBanUsers_From_Khoi',
        type: 'GET',
        data: { maKhoi: btoa(maKhoi) },
        success: function (data) {
            var decodedJson = atob(data);
            var decodedString = decodeURIComponent(escape(decodedJson));
            var jsonData = JSON.parse(decodedString);
/*            console.log(jsonData);*/
            var nhomQuyen = jsonData.NhomQuyen;
            updateDateRange(nhomQuyen);
            var PhongBan = $('#PhongBan');
            var Username = $('#Username');
            PhongBan.empty();
            Username.empty();
            if (jsonData.PhongBanList.length > 0) {
                $.each(jsonData.PhongBanList, function (i, item) {
                    PhongBan.append($('<option>').text(item.TenPhongBan).attr('value', item.ID_PhongBan));
                    //    console.log(item.ID_PhongBan, item.TenPhongBan);
                });
            } else {
                PhongBan.append($('<option>').text('-- Không có kết quả --'));
            }
            if (jsonData.UserList.length > 0) {
                $.each(jsonData.UserList, function (i, item) {
                    Username.append($('<option>').text(item.Fullname).attr('value', item.Username));
                    //    console.log(item.Username, item.Fullname);
                });
            } else {
                Username.append($('<option>').text('-- Không có kết quả --'));
            }
            $('#loading').hide();
        },
        error: function (error) {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: 'Có lỗi xảy ra khi lấy dữ liệu từ máy chủ.',
                footer: '<pre>' + error + '</pre>'
            });
        }
    });
}
// Lắng nghe sự kiện thay đổi giá trị của dropdown "Khối"
$('#Ma_Khoi').change(function () {
    var selectedKhoi = $(this).val();
    loadData();
    if (selectedKhoi === 'K2') {
        $('#NVCaiTien').parent().css('display', 'none');
    } else {
        $('#NVCaiTien').parent().css('display', 'block');
    }
    //    console.log(selectedKhoi);
});
// Load dữ liệu khi trang được tải
loadData();
//thực hiện chức năng khóa ngày theo tháng chọn
var flag = false;
var fixedStartDate_Add = null;
var fixedStartDate_Edit = null;
function updateDateRange(nhomQuyen) {
    if (flag) {
        var thangNamChon = $("#idThangNam_BC").val();
        var ngayBatDau = moment(thangNamChon, "YYYY-MM").startOf('month');
        var ngayKetThuc = moment(thangNamChon, "YYYY-MM").endOf('month');
        // Cập nhật phạm vi chọn ngày trong trường ngày
        $('#reservation').daterangepicker({
            startDate: ngayBatDau,
            endDate: ngayKetThuc,
            minDate: ngayBatDau,
            maxDate: ngayKetThuc,
            locale: {
                format: 'DD/MM/YYYY',
                cancelLabel: 'Clear'
            }
        });
        fixedStartDate_Add = ngayBatDau;
/*        console.log(nhomQuyen);*/
        if (nhomQuyen !== '1000') {
            var currentDate = new Date();
            var currentYear = currentDate.getFullYear();
            var currentMonth = currentDate.getMonth() + 1;
            if (currentMonth < 10) {
                currentMonth = '0' + currentMonth;
            }
            var currentFormattedDate = currentYear + '-' + currentMonth;
            $("#idThangNam_BC").attr('min', currentFormattedDate);
        } else {
        }  
    } else {
        // Đánh dấu flag là true sau khi giá trị nhomQuyen được sử dụng lần đầu tiên
        flag = true;
    }
}
// Thiết lập giá trị mặc định cho trường tháng đánh giá
var thangNam = moment().format("YYYY-MM"); // Lấy tháng và năm hiện tại
$("#idThangNam_BC").val(thangNam); // Đặt giá trị tháng đánh giá
// Cập nhật phạm vi ngày ban đầu
updateDateRange();
// Bắt sự kiện thay đổi trường tháng đánh giá
$("#idThangNam_BC").change(function () {
    updateDateRange();
});
//// Khóa các ngày không thuộc tháng đã chọn
//$('#reservation').on('show.daterangepicker', function (ev, picker) {
//    var ngayBatDau = moment(picker.startDate).startOf('day');
//    var ngayKetThuc = moment(picker.endDate).endOf('day');
//    var thangNamChon = $("#idThangNam_BC").val();
//    var thangNamBatDau = moment(thangNamChon, "YYYY-MM").startOf('month');
//    var thangNamKetThuc = moment(thangNamChon, "YYYY-MM").endOf('month');
//    console.log('Ngày bắt đầu trước khi áp dụng:', ngayKetThuc);
//    console.log('Ngày kết thúc trước khi áp dụng:', ngayKetThuc);
//    // Kiểm tra và khóa các ngày không thuộc tháng đã chọn
//    if (ngayBatDau.isBefore(thangNamBatDau)) {
//        picker.setStartDate(thangNamBatDau);
//    }

//    if (ngayKetThuc.isAfter(thangNamKetThuc)) {
//        picker.setEndDate(thangNamKetThuc);
//    }
//});
//lưu table thời gian
var chungTuTonTai = false;
var loaiBaoCaoTruoc = ''; // Thêm biến để theo dõi loại báo cáo trước đó
//document.getElementById('btnLuuTamThoiGian').addEventListener('click', luuTam);
function btnLuuTamThoiGian() {
    var loaiBaoCao = document.getElementById('idLoai_BC').value;
    var gio = document.getElementById('idGio_BC').value;
    var ngay = document.getElementById('reservation').value;
    var [startDate, endDate] = ngay.split(' - ');
    var momentStartDate = moment(startDate, 'DD/MM/YYYY');
    var momentEndDate = moment(endDate, 'DD/MM/YYYY');
    var formattedStartDate = momentStartDate.format('DD/MM/YYYY');
    var formattedEndDate = momentEndDate.format('DD/MM/YYYY');
    var displayDate = '';
    //format theo định dạng dd-dd/MM/yyyy or dd/MM/yyyy
    if (momentStartDate.isSame(momentEndDate)) {
        displayDate = formattedStartDate;
    } else {
        displayDate = momentStartDate.format('DD') + '-' + formattedEndDate;
    }
    // Trừ đi 2 ngày sau khi chọn
    var fixedStartDate = moment(fixedStartDate_Add, 'DD/MM/YYYY'); // Đặt ngày bắt đầu cố định
    momentStartDate = moment.max(momentStartDate.clone().subtract(2, 'days'), fixedStartDate);
    var resultStarDate = momentStartDate.format('DD/MM/YYYY');
    /*        console.log(resultStarDate);*/
    var daterangepickerOptions = {
        startDate: resultStarDate,
        endDate: momentEndDate,
        maxDate: resultStarDate,
        locale: {
            format: 'DD/MM/YYYY',
            cancelLabel: 'Clear'
        },
        isInvalidDate: function (date) {
            var thangNamChon = $("#idThangNam_BC").val();
            var thangNamBatDau = moment(thangNamChon, "YYYY-MM").startOf('month');
            var thangNamKetThuc = moment(thangNamChon, "YYYY-MM").endOf('month');
            // Kiểm tra xem ngày có nằm ngoài tháng được chọn không
            var outsideSelectedMonth = !date.isBetween(thangNamBatDau, thangNamKetThuc, null, '[]');
            // Kiểm tra xem ngày có nằm trong khoảng thời gian đã chọn không
            var insideSelectedRange = !date.isBetween(momentStartDate.clone().add(1, 'days'), momentEndDate.clone().add(1, 'days'), null, '[]');
            //Nếu muốn khóa khác chỉ cần thay đổi dấu " ! "
            return outsideSelectedMonth || !insideSelectedRange;
        }
    };
    // Cập nhật phạm vi chọn ngày trong trường ngày
    $('#reservation').daterangepicker(daterangepickerOptions);

    var cellsGio = document.querySelectorAll("#previewTable_thoigian tbody tr:nth-child(1) td");
    var cellsNgay = document.querySelectorAll("#previewTable_thoigian tbody tr:nth-child(2) td");
    var idThangNam_BC = document.getElementById("idThangNam_BC");
    // Kiểm tra xem có dữ liệu trong cellsGio và cellsNgay hay không
    var hasData = false;
    for (var i = 0; i < cellsGio.length; i++) {
        if (cellsGio[i].textContent.trim() !== "") {
            hasData = true;
            break;
        }
    }
    if (!hasData) {
        for (var j = 0; j < cellsNgay.length; j++) {
            if (cellsNgay[j].textContent.trim() !== "") {
                hasData = true;
                break;
            }
        }
    }
    // Nếu có dữ liệu, khóa idThangNam_BC
    if (hasData) {
        idThangNam_BC.disabled = true;
    } else {
        idThangNam_BC.disabled = false;
    }
    //kiểm tra xem đúng trình tự tạo lịch làm hay chưa Chấm hiệu chỉnh, Chấm chéo, Chấm nội bộ
    if (!chungTuTonTai && loaiBaoCao !== 'HC') {
        swal({
            title: 'Thông báo',
            text: 'Vui lòng thêm dữ liệu thời gian "Chấm hiệu chỉnh" trước.',
            icon: 'warning',
        }).then((value) => {
            location.reload();
        });
        return;
    }
    if (loaiBaoCao === 'NB' && (!chungTuTonTai || loaiBaoCaoTruoc !== 'CH')) {
        // Kiểm tra xem đã có dữ liệu cho Chấm Hiệu Chỉnh và Chấm Chéo chưa
        if (!chungTuTonTai || cellsGio[2].textContent === '' || cellsNgay[2].textContent === '') {
            swal({
                title: 'Thông báo',
                text: 'Vui lòng chọn chấm "Chấm chéo" trước.',
                icon: 'warning',
            });
            return;
        }
        // Đoạn mã xử lý cho Chấm Nội Bộ
        cellsGio[1].textContent = gio;
        cellsNgay[1].textContent = displayDate;
    }
    // Dựa vào giá trị của trường Loại báo cáo để quyết định cập nhật ô nào trong bảng
    switch (loaiBaoCao) {
        case 'HC': // Chấm hiệu chỉnh
            cellsGio[3].textContent = gio;
            cellsNgay[3].textContent = displayDate;
            chungTuTonTai = true;
            break;
        case 'CH': // Chấm chéo
            cellsGio[2].textContent = gio;
            cellsNgay[2].textContent = displayDate;
            chungTuTonTai = true;
            break;
        case 'NB': // Chấm nội bộ
            // Đã xử lý ở phía trên
            break;
        default:
            break;
    }
}
//lưu 2 table bên dưới
function luuTam(tableId) {
    // Lấy giá trị từ dropdown
    var khoiValue = $('#Ma_Khoi').val(); // Lấy giá trị (value) của dropdown "Khối"
    var phongBanValue = $('#PhongBan').val(); // Lấy giá trị (value) của dropdown "Phòng Ban"
    var thanhVienValue = $('#Username').val(); // Lấy giá trị (value) của dropdown "Thành viên Ban ĐH"
    var toCaiTienValue = $('#NVCaiTien').val(); // Lấy giá trị (value) của dropdown "Tổ Cải Tiến"

    var khoiText = $('#Ma_Khoi option:selected').text(); // Lấy nội dung text của dropdown "Khối"
    var phongBanText = $('#PhongBan option:selected').text(); // Lấy nội dung text của dropdown "Phòng Ban"
    var thanhVienText = $('#Username option:selected').text(); // Lấy nội dung text của dropdown "Thành viên Ban ĐH"
    var toCaiTienText = $('#NVCaiTien option:selected').text(); // Lấy nội dung text của dropdown "Tổ Cải Tiến"

    // Tìm bảng và tbody tương ứng
    var table = $('#' + tableId);
    var tbody = table.find('tbody');
    // Tạo hàng mới và thêm ô vào hàng
    var newRow = $('<tr>');
    newRow.append($('<td>').text(phongBanText).data('value', phongBanValue));
    newRow.append($('<td>').text(thanhVienText).data('value', thanhVienValue));
    // Đối với Khối Sản Xuất, thêm cột "Tổ Cải Tiến"
    if (khoiText === 'Khối Sản Xuất') {
        newRow.append($('<td>').text(toCaiTienText).data('value', toCaiTienValue));
    }
    // Tạo nút Xóa Dòng và thêm vào hàng mới
    var deleteButton = $('<button>').text('Xóa').addClass('btn btn-outline-danger delete-row');
    newRow.append($('<td>').append(deleteButton));
    // Thêm hàng mới vào tbody
    tbody.append(newRow);

    //// Đây là giá trị từ các dropdown để gửi đi
    //console.log("Giá trị của Khối: " + khoiValue);
    //console.log("Giá trị của Phòng Ban: " + phongBanValue);
    //console.log("Giá trị của Thành viên Ban ĐH: " + thanhVienValue);
    //console.log("Giá trị của Tổ Cải Tiến: " + toCaiTienValue);
}

// Sự kiện click cho nút "Lưu tạm"
$('#btnLuuTam').on('click', function () {
    var khoiText = $('#Ma_Khoi option:selected').text();

    // Chọn bảng dựa trên giá trị của dropdown "Khối"
    if (khoiText === 'Khối Sản Xuất') {
        luuTam('previewTable_sanxuat');
    } else if (khoiText === 'Khối Văn Phòng') {
        luuTam('previewTable_vanphong');
    }
});

// Xóa dòng khi click vào nút Xóa
$('#previewTable_sanxuat tbody').on('click', '.delete-row', function () {
    $(this).closest('tr').remove();
});
$('#previewTable_vanphong tbody').on('click', '.delete-row', function () {
    $(this).closest('tr').remove();
});

///gửi dữ liệu đi
function gatherSanXuatData() {
    var sanXuatData = [];
    $('#previewTable_sanxuat tbody tr').each(function () {
        var rowData = [];
        $(this).find('td').each(function (index) {
            var cellValue = $(this).data('value'); // Lấy giá trị từ thuộc tính data-value của ô
            var cellText = $(this).text(); // Lấy văn bản từ ô

            // Lưu cả giá trị và văn bản vào mảng
            var cellData = {
                value: cellValue,
                text: cellText
            };

            if (index !== 3) {
                rowData.push(cellData);
            }
        });
        sanXuatData.push(rowData);
    });
    /*        console.log(sanXuatData); // Kiểm tra giá trị rowData*/
    return sanXuatData;
}

function gatherVanPhongData() {
    var vanPhongData = [];
    $('#previewTable_vanphong tbody tr').each(function () {
        var rowData = [];
        $(this).find('td').each(function (index) {
            var cellValue = $(this).data('value'); // Lấy giá trị từ thuộc tính data-value của ô
            var cellText = $(this).text(); // Lấy văn bản từ ô

            // Lưu cả giá trị và văn bản vào mảng
            var cellData = {
                value: cellValue,
                text: cellText
            };

            if (index !== 2) {
                rowData.push(cellData);
            }
        });
        vanPhongData.push(rowData);
    });
    /*        console.log(vanPhongData); // Kiểm tra giá trị rowData*/
    return vanPhongData;
}


function gatherThoiGianData() {
    var gioData = [];
    var ngayData = [];

    $('#previewTable_thoigian tbody tr:nth-child(1) td').each(function () {
        gioData.push($(this).text());
    });

    $('#previewTable_thoigian tbody tr:nth-child(2) td').each(function () {
        ngayData.push($(this).text());
    });

    return {
        gioData: gioData,
        ngayData: ngayData
    };
}
$('#btnGuiDuLieu').on('click', function () {
    $('#loading').show();
    var sanXuatData = gatherSanXuatData();
    var vanPhongData = gatherVanPhongData();
    var thoigianData = gatherThoiGianData();
    /*        console.log(sanXuatData, vanPhongData, thoigianData);*/
    var dataToSend = {
        sanXuatData: sanXuatData,
        vanPhongData: vanPhongData,
        thoigianData: thoigianData
    };
    var dataToSend = {
        sanXuatData: sanXuatData,
        vanPhongData: vanPhongData,
        thoigianData: thoigianData
    };

    // Chuyển đổi dữ liệu JSON thành chuỗi JSON
    var jsonData = JSON.stringify(dataToSend);

    // Chuyển đổi chuỗi JSON thành dạng byte array
    var encoder = new TextEncoder();
    var jsonDataUint8 = encoder.encode(jsonData);

    // Mã hóa dạng byte array thành Base64
    var encodedData = btoa(String.fromCharCode.apply(null, jsonDataUint8));

    // Tạo một đối tượng FormData và thêm dữ liệu Base64 vào đó
    var formData = new FormData();
    formData.append("json", encodedData);

    $.ajax({
        url: 'Add_PhanCong',
        method: 'POST',
        contentType: false, // Không thiết lập contentType
        processData: false, // Không xử lý dữ liệu
        data: formData,
        success: function (response) {
            if (response.success) {
                // Hiển thị thông báo thành công
                Swal.fire({
                    title: 'Success!',
                    text: response.message,
                    icon: 'success',
                    timer: 2000, // Hiển thị trong 2 giây
                    timerProgressBar: true,
                }).then(function () {
                    location.reload();
                });
                //console.log(response.userList);
            } else {
                $('#loading').hide();
                // Hiển thị thông báo lỗi
                Swal.fire({
                    title: 'Error!',
                    text: response.message,
                    icon: 'error',
                    timer: 2000, // Hiển thị trong 2 giây
                    timerProgressBar: true,
                }).then(function () {
                    location.reload();
                });
            }
        },
        error: function (xhr, status, error) {
            $('#loading').hide();
            // Hiển thị thông báo lỗi
            Swal.fire({
                title: 'Error!',
                text: 'Lỗi khi gửi dữ liệu: ' + error,
                icon: 'error',
                timer: 2000, // Hiển thị trong 2 giây
                timerProgressBar: true,
            }).then(function () {
                location.reload();
            });
        }
    });
});
//sửa
function loadData_Edit() {
    var maKhoi_Edit = $('#Ma_Khoi_Edit').val();
    $.ajax({
        url: '/DataApi/Get_PhongBanUsers_From_Khoi',
        type: 'GET',
        data: { maKhoi: btoa(maKhoi_Edit) },
        success: function (data) {
            var decodedJson = atob(data);
            var decodedString = decodeURIComponent(escape(decodedJson));
            var jsonData = JSON.parse(decodedString);
/*            console.log(jsonData);*/
            var nhomQuyen_Edit = jsonData.NhomQuyen;
            updateDateRange_Edit(nhomQuyen_Edit);
            var PhongBan_Edit = $('#PhongBan_Edit');
            var Username_Edit = $('#Username_Edit');
            PhongBan_Edit.empty();
            Username_Edit.empty();
            if (jsonData.PhongBanList.length > 0) {
                $.each(jsonData.PhongBanList, function (i, item) {
                    PhongBan_Edit.append($('<option>').text(item.TenPhongBan).attr('value', item.ID_PhongBan));
                    //    console.log(item.ID_PhongBan, item.TenPhongBan);
                });
            } else {
                PhongBan_Edit.append($('<option>').text('-- Không có kết quả --'));
            }
            if (jsonData.UserList.length > 0) {
                $.each(jsonData.UserList, function (i, item) {
                    Username_Edit.append($('<option>').text(item.Fullname).attr('value', item.Username));
                    //    console.log(item.Username, item.Fullname);
                });
            } else {
                Username_Edit.append($('<option>').text('-- Không có kết quả --'));
            }
            $('#loading').hide();
        },
        error: function (error) {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: 'Có lỗi xảy ra khi lấy dữ liệu từ máy chủ.',
                footer: '<pre>' + error + '</pre>'
            });
        }
    });
}
// Lắng nghe sự kiện thay đổi giá trị của dropdown "Khối"
$('#Ma_Khoi_Edit').change(function () {
    var selectedKhoi_Edit = $(this).val();
    loadData_Edit();
    if (selectedKhoi_Edit === 'K2') {
        $('#NVCaiTien_Edit').parent().css('display', 'none');
    } else {
        $('#NVCaiTien_Edit').parent().css('display', 'block');
    }
    //    console.log(selectedKhoi);
});
// Load dữ liệu khi trang được tải
loadData_Edit();
function updateDateRange_Edit(nhomQuyen_Edit) {
    if (flag) {
        var thangNamChon_Edit = $("#idThangNam_BC_Edit").val();
        var ngayBatDau_Edit = moment(thangNamChon_Edit, "YYYY-MM").startOf('month');
        var ngayKetThuc_Edit = moment(thangNamChon_Edit, "YYYY-MM").endOf('month');
        $('#reservation_PhanCong_Edit').daterangepicker({
            startDate: ngayBatDau_Edit,
            endDate: ngayKetThuc_Edit,
            minDate: ngayBatDau_Edit,
            maxDate: ngayKetThuc_Edit,
            locale: {
                format: 'DD/MM/YYYY',
                cancelLabel: 'Clear'
            }
        });
        fixedStartDate_Edit = ngayBatDau_Edit;
        /*        console.log(nhomQuyen);*/
        if (nhomQuyen_Edit !== '1000') {
            var currentDate = new Date();
            var currentYear = currentDate.getFullYear();
            var currentMonth = currentDate.getMonth() + 1;
            if (currentMonth < 10) {
                currentMonth = '0' + currentMonth;
            }
            var currentFormattedDate = currentYear + '-' + currentMonth;
            $("#idThangNam_BC_Edit").attr('min', currentFormattedDate);
        } else {
        }
    } else {
        flag = true;
    }
}
//// Thiết lập giá trị mặc định cho trường tháng đánh giá
var thangNam_Edit = moment().format("YYYY-MM"); // Lấy tháng và năm hiện tại
$("#idThangNam_BC_Edit").val(thangNam_Edit); // Đặt giá trị tháng đánh giá
// Cập nhật phạm vi ngày ban đầu
updateDateRange_Edit();
// Bắt sự kiện thay đổi trường tháng đánh giá
$("#idThangNam_BC_Edit").change(function () {
    updateDateRange_Edit();
});

function btnLuuTamThoiGian_Edit() {
    var loaiBaoCao_Edit = document.getElementById('idLoai_BC_Edit').value;
    var gio = document.getElementById('idGio_BC').value;
    var ngay = document.getElementById('reservation_PhanCong_Edit').value;
    var [startDate, endDate] = ngay.split(' - ');
    var momentStartDate = moment(startDate, 'DD/MM/YYYY');
    var momentEndDate = moment(endDate, 'DD/MM/YYYY');
    var formattedStartDate = momentStartDate.format('DD/MM/YYYY');
    var formattedEndDate = momentEndDate.format('DD/MM/YYYY');
    var displayDate = '';

    // Format theo định dạng dd-dd/MM/yyyy or dd/MM/yyyy
    if (momentStartDate.isSame(momentEndDate)) {
        displayDate = formattedStartDate;
    } else {
        displayDate = momentStartDate.format('DD') + '-' + formattedEndDate;
    }
    // Trừ đi 2 ngày sau khi chọn
    var fixedStartDate = moment(fixedStartDate_Edit, 'DD/MM/YYYY'); // Đặt ngày bắt đầu cố định
    momentStartDate = moment.max(momentStartDate.clone().subtract(2, 'days'), fixedStartDate);
    var resultStartDate = momentStartDate.format('DD/MM/YYYY');
    var daterangepickerOptions = {
        startDate: resultStartDate,
        endDate: momentEndDate,
        maxDate: resultStartDate,
        locale: {
            format: 'DD/MM/YYYY',
            cancelLabel: 'Clear'
        },
        isInvalidDate: function (date) {
            var thangNamChon = $("#idThangNam_BC").val();
            var thangNamBatDau = moment(thangNamChon, "YYYY-MM").startOf('month');
            var thangNamKetThuc = moment(thangNamChon, "YYYY-MM").endOf('month');
            // Kiểm tra xem ngày có nằm ngoài tháng được chọn không
            var outsideSelectedMonth = !date.isBetween(thangNamBatDau, thangNamKetThuc, null, '[]');
            // Kiểm tra xem ngày có nằm trong khoảng thời gian đã chọn không
            var insideSelectedRange = !date.isBetween(momentStartDate.clone().add(1, 'days'), momentEndDate.clone().add(1, 'days'), null, '[]');
            //Nếu muốn khóa khác chỉ cần thay đổi dấu " ! "
            return outsideSelectedMonth || !insideSelectedRange;
        }
    };

    // Cập nhật phạm vi chọn ngày trong trường ngày
    $('#reservation_PhanCong_Edit').daterangepicker(daterangepickerOptions);

    var cellsGio = document.querySelectorAll("#previewTable_thoigian_Edit tbody tr:nth-child(1) td");
    var cellsNgay = document.querySelectorAll("#previewTable_thoigian_Edit tbody tr:nth-child(2) td");
    var idThangNam_BC = document.getElementById("idThangNam_BC_Edit");
    // Kiểm tra xem có dữ liệu trong cellsGio và cellsNgay hay không
    var hasData = false;
    for (var i = 0; i < cellsGio.length; i++) {
        if (cellsGio[i].textContent.trim() !== "") {
            hasData = true;
            break;
        }
    }
    if (!hasData) {
        for (var j = 0; j < cellsNgay.length; j++) {
            if (cellsNgay[j].textContent.trim() !== "") {
                hasData = true;
                break;
            }
        }
    }
    // Nếu có dữ liệu, khóa idThangNam_BC
    if (hasData) {
        idThangNam_BC.disabled = true;
    } else {
        idThangNam_BC.disabled = false;
    }
    //kiểm tra xem đúng trình tự tạo lịch làm hay chưa Chấm hiệu chỉnh, Chấm chéo, Chấm nội bộ
    if (!chungTuTonTai && loaiBaoCao_Edit !== 'HC') {
        swal({
            title: 'Thông báo',
            text: 'Vui lòng thêm dữ liệu thời gian "Chấm hiệu chỉnh" trước.',
            icon: 'warning',
        }).then((value) => {
            location.reload();
        });
        return;
    }
    if (loaiBaoCao_Edit === 'NB' && (!chungTuTonTai || loaiBaoCaoTruoc !== 'CH')) {
        // Kiểm tra xem đã có dữ liệu cho Chấm Hiệu Chỉnh và Chấm Chéo chưa
        if (!chungTuTonTai || cellsGio[2].textContent === '' || cellsNgay[2].textContent === '') {
            swal({
                title: 'Thông báo',
                text: 'Vui lòng chọn chấm "Chấm chéo" trước.',
                icon: 'warning',
            });
            return;
        }
        cellsGio[1].textContent = gio;
        cellsNgay[1].textContent = displayDate;
    }
    // Dựa vào giá trị của trường Loại báo cáo để quyết định cập nhật ô nào trong bảng
    switch (loaiBaoCao_Edit) {
        case 'HC': // Chấm hiệu chỉnh
            cellsGio[3].textContent = gio;
            cellsNgay[3].textContent = displayDate;
            chungTuTonTai = true;
            break;
        case 'CH': // Chấm chéo
            cellsGio[2].textContent = gio;
            cellsNgay[2].textContent = displayDate;
            chungTuTonTai = true;
            break;
        case 'NB': // Chấm nội bộ
            break;
        default:
            break;
    }
}
//lưu 2 table bên dưới
function luuTam_Edit(tableId_Edit) {
    // Lấy giá trị từ dropdown
    var khoiValue_Edit = $('#Ma_Khoi_Edit').val(); // Lấy giá trị (value) của dropdown "Khối"
    var phongBanValue_Edit = $('#PhongBan_Edit').val(); // Lấy giá trị (value) của dropdown "Phòng Ban"
    var thanhVienValue_Edit = $('#Username_Edit').val(); // Lấy giá trị (value) của dropdown "Thành viên Ban ĐH"
    var toCaiTienValue_Edit = $('#NVCaiTien_Edit').val(); // Lấy giá trị (value) của dropdown "Tổ Cải Tiến"

    var khoiText_Edit = $('#Ma_Khoi_Edit option:selected').text(); // Lấy nội dung text của dropdown "Khối"
    var phongBanText_Edit = $('#PhongBan_Edit option:selected').text(); // Lấy nội dung text của dropdown "Phòng Ban"
    var thanhVienText_Edit = $('#Username_Edit option:selected').text(); // Lấy nội dung text của dropdown "Thành viên Ban ĐH"
    var toCaiTienText_Edit = $('#NVCaiTien_Edit option:selected').text(); // Lấy nội dung text của dropdown "Tổ Cải Tiến"

    // Tìm bảng và tbody tương ứng
    var table = $('#' + tableId_Edit);
    var tbody = table.find('tbody');
    // Tạo hàng mới và thêm ô vào hàng
    var newRow = $('<tr>');
    newRow.append($('<td>').text(phongBanText_Edit).attr('value-phongban', phongBanValue_Edit));
    newRow.append($('<td>').text(thanhVienText_Edit).attr('value-username', thanhVienValue_Edit));
    // Đối với Khối Sản Xuất, thêm cột "Tổ Cải Tiến"
    if (khoiText_Edit === 'Khối Sản Xuất') {
        newRow.append($('<td>').text(toCaiTienText_Edit).attr('nvcaitien', toCaiTienValue_Edit));
    }
    // Tạo nút Xóa Dòng và thêm vào hàng mới
    var deleteButton = $('<button>').text('Xóa').addClass('btn btn-outline-danger delete-row');
    newRow.append($('<td>').append(deleteButton));
    // Thêm hàng mới vào tbody
    tbody.append(newRow);

    //// Đây là giá trị từ các dropdown để gửi đi
    //console.table("Giá trị của Khối: " + khoiValue_Edit, khoiText_Edit);
    //console.table("Giá trị của Phòng Ban: " + phongBanValue_Edit, phongBanText_Edit);
    //console.table("Giá trị của Thành viên Ban ĐH: " + thanhVienValue_Edit, thanhVienText_Edit);
    //console.table("Giá trị của Tổ Cải Tiến: " + toCaiTienValue_Edit, toCaiTienText_Edit);
}

// Sự kiện click cho nút "Lưu tạm"
$('#btnLuuTam_Edit').on('click', function () {
    var khoiText_Edit = $('#Ma_Khoi_Edit option:selected').text();

    // Chọn bảng dựa trên giá trị của dropdown "Khối"
    if (khoiText_Edit === 'Khối Sản Xuất') {
        luuTam_Edit('previewTable_sanxuat_Edit');
    } else if (khoiText_Edit === 'Khối Văn Phòng') {
        luuTam_Edit('previewTable_vanphong_Edit');
    }
});
// Xóa dòng khi click vào nút Xóa
$('#previewTable_sanxuat_Edit tbody').on('click', '.delete-row', function () {
    $(this).closest('tr').remove();
});
$('#previewTable_vanphong_Edit tbody').on('click', '.delete-row', function () {
    $(this).closest('tr').remove();
});
///gửi dữ liệu đi
function gatherSanXuatData_Edit() {
    var table = document.getElementById("previewTable_sanxuat_Edit");
    var sanXuatData_Edit = [];
    for (var i = 0; i < table.rows.length; i++) {
        var row = table.rows[i];
        var rowData = {};

        var cells = row.getElementsByTagName("td");
        if (cells.length > 0) {
            rowData.valuePhongBan = cells[0].getAttribute('value-phongban');
            rowData.valueUsername = cells[1].getAttribute('value-username');
            rowData.toCaiTienValue = cells[2].getAttribute('nvcaitien');
            sanXuatData_Edit.push(rowData);
        }
    }
/*    console.table('sx', sanXuatData_Edit);*/
    return sanXuatData_Edit;
}

function gatherVanPhongData_Edit() {
    var table = document.getElementById("previewTable_vanphong_Edit");
    var vanPhongData_Edit = [];
    for (var i = 0; i < table.rows.length; i++) {
        var row = table.rows[i];
        var rowData = {};

        var cells = row.getElementsByTagName("td");
        if (cells.length > 0) {
            rowData.valuePhongBan = cells[0].getAttribute('value-phongban');
            rowData.valueUsername = cells[1].getAttribute('value-username');
            vanPhongData_Edit.push(rowData);
        }
    }
/*    console.table('vp', vanPhongData_Edit);*/
    return vanPhongData_Edit;
}


function gatherThoiGianData_Edit() {
    var gioData_Edit = [];
    var ngayData_Edit = [];

    $('#previewTable_thoigian_Edit tbody tr:nth-child(1) td').each(function () {
        gioData_Edit.push($(this).text());
    });

    $('#previewTable_thoigian_Edit tbody tr:nth-child(2) td').each(function () {
        ngayData_Edit.push($(this).text());
    });
/*    console.table('gio ngay', gioData_Edit, ngayData_Edit);*/
    return {
        gioData_Edit: gioData_Edit,
        ngayData_Edit: ngayData_Edit
    };    
}
$('#btnGuiDuLieu_Edit').on('click', function () {
    $('#loading').show();
    var formattedDateDisplayText = document.getElementById("selectedThangNam_Edit").textContent.split("Mã phân công: ")[1].trim();
    var sanXuatData_Edit = gatherSanXuatData_Edit();
    var vanPhongData_Edit = gatherVanPhongData_Edit();
    var thoigianData_Edit = gatherThoiGianData_Edit();
    console.log('Data_All',formattedDateDisplayText, sanXuatData_Edit, vanPhongData_Edit, thoigianData_Edit);
    var dataToSend_Edit = {
        sanXuatData_Edit: sanXuatData_Edit,
        vanPhongData_Edit: vanPhongData_Edit,
        thoigianData_Edit: thoigianData_Edit,
        ID_PhanCong: formattedDateDisplayText
    };
    // Chuyển đổi dữ liệu JSON thành chuỗi JSON
    var jsonData_Edit = JSON.stringify(dataToSend_Edit);
    // Chuyển đổi chuỗi JSON thành dạng byte array
    var encoder_Edit = new TextEncoder();
    var jsonDataUint8_Edit = encoder_Edit.encode(jsonData_Edit);
    // Mã hóa dạng byte array thành Base64
    var encodedData_Edit = btoa(String.fromCharCode.apply(null, jsonDataUint8_Edit));
    // Tạo một đối tượng FormData và thêm dữ liệu Base64 vào đó
    var formData_Edit = new FormData();
    formData_Edit.append("json", encodedData_Edit);
    //gửi dữ liệu đi
    $.ajax({
        url: 'Edit_PhanCong',
        method: 'POST',
        contentType: false, // Không thiết lập contentType
        processData: false, // Không xử lý dữ liệu
        data: formData_Edit,
        success: function (response) {
            if (response.success) {
                // Hiển thị thông báo thành công
                Swal.fire({
                    title: 'Success!',
                    text: response.message,
                    icon: 'success',
                    timer: 2000, // Hiển thị trong 2 giây
                    timerProgressBar: true,
                }).then(function () {
                    location.reload();
                });
                //console.log(response.userList);
            } else {
                $('#loading').hide();
                // Hiển thị thông báo lỗi
                Swal.fire({
                    title: 'Error!',
                    text: response.message,
                    icon: 'error',
                    timer: 2000, // Hiển thị trong 2 giây
                    timerProgressBar: true,
                }).then(function () {
                    location.reload();
                });
            }
        },
        error: function (xhr, status, error) {
            $('#loading').hide();
            // Hiển thị thông báo lỗi
            Swal.fire({
                title: 'Error!',
                text: 'Lỗi khi gửi dữ liệu: ' + error,
                icon: 'error',
                timer: 2000, // Hiển thị trong 2 giây
                timerProgressBar: true,
            }).then(function () {
                location.reload();
            });
        }
    });
});



