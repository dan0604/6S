let selectedMonth = '';
function checkThangNamValue() {
    const ThangNamElement = document.getElementById('ThangNam');
    const dataTableList = document.getElementById('tblMucTieuList');
    const btnSend = document.getElementById('btnGuiDuLieu');

    function hideElements_MucTieu() {
        dataTableList.style.display = 'none';
        btnSend.style.display = 'none';
    }

    function showElements_MucTieu() {
        dataTableList.style.display = 'block';
        btnSend.style.display = 'block';
    }

    ThangNamElement.addEventListener('change', function () {
        selectedMonth = this.value;
        console.log(selectedMonth);
        var currentDate = new Date();
        var year = currentDate.getFullYear();
        var month = ('0' + (currentDate.getMonth() + 1)).slice(-2);
        var currentMonthYear = month + '/' + year;

        if (selectedMonth !== currentMonthYear && selectedMonth !== '') {
            showElements_MucTieu();
        } else {
            hideElements_MucTieu();
        }
    });
    hideElements_MucTieu();
}
checkThangNamValue();

// Hàm xử lý khi tải trang
$(document).ready(function () {
    // Sự kiện xóa dòng khi nhấn nút "Xóa"
    $(document).on('click', '.deleteRowBtn', function () {
        $(this).closest('.row').remove(); // Xóa dòng cha gần nhất
    });
});
function loadMucTieu() {
    $.ajax({
        url: '/MucTieu/Add_MucTieu',
        type: 'GET',
        success: function (result) {
            console.table(result);
            var htmlContent = '';
            result.forEach(function (item) {
                console.table(item);
                htmlContent += '<div class="row">\
                                    <div class="column">\
                                        <div class="phong-ban" data-id="' + item.ID_PhongBan + '">' + item.TenPhongBan + '</div>\
                                    </div>\
                                    <div class="column">\
                                        <input type="number" class="numberInput" name="numberInput">\
                                    </div>\
                                    <div class="column">\
                                        <button class="btn btn-outline-danger deleteRowBtn"><i class="fas fa-trash"></i> Xóa</button>\
                                    </div>\
                                </div>';
            });
            $('#tblMucTieuList').html(htmlContent);
        },
        error: function (error) {
            console.log("Có lỗi xảy ra: ", error);
        }
    });
}
loadMucTieu();

function sendDataToServer() {
    $('#loading').show();
    var dataToSend = [];
    console.log(selectedMonth);
    var splitValue = selectedMonth.split('-');
    var formattedValue = splitValue[1] + '/' + splitValue[0];
    console.log(formattedValue);
    $('#tblMucTieuList .row').each(function () {
        var diemMucTieu = $(this).find('.numberInput').val() || 0;
        var phongBanId = $(this).find('.phong-ban').data('id');
        var item = {
            MucTieu: diemMucTieu,
            ID_PhongBan: phongBanId,
            ThangNam: formattedValue,
            Status: 1
        };
        dataToSend.push(item);
        console.table(diemMucTieu, phongBanId, dataToSend);
    });
    $.ajax({       
        url: 'Add_MucTieu',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(dataToSend),
        success: function (response) {
            console.log(response);
            if (response.success) {
                Swal.fire({
                    title: "Thành công!",
                    text: response.message,
                    icon: "success",
                    timer: 2000,
                    timerProgressBar: true,
                }).then(function () {
                    location.reload();
                });
            } else {
                $('#loading').hide();
                Swal.fire({
                    title: "Lỗi!",
                    text: response.message,
                    icon: "error"
                }).then(function () {
                    if (response.redirectUrl) {
                        window.location.href = response.redirectUrl;
                    }
                });
            }

        },
        error: function (error) {
            console.log("Có lỗi xảy ra: ", error);
            $('#loading').hide();
        }
    });
}
$('#btnGuiDuLieu').click(function () {
    sendDataToServer();
});


