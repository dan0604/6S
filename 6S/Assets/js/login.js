$(document).ready(function () {
    function functionLogin() {
        $('form').submit(function (e) {
            e.preventDefault();
            // Hiển thị div loading
            $('#loading').show();
            var url = btoa("/Login/Login");
            var decodedUrl = atob(url);
            // Chặn việc gửi form
            $('button[type=submit]', this).prop('disabled', true);
            var Username = $('#Username').val();
            var Password = $('#Password').val();
            $.ajax({
                url: decodedUrl,
                method: 'POST',
                data: {
                    Username: btoa(Username),
                    Password: btoa(Password)
                },
                success: function (data) {
                    if (data.status === 'success') {
                        swal({
                            title: '',
                            text: data.message,
                            icon: 'success',
                            timer: 2000
                        }).then(function () {
                            window.location.href = data.redirectUrl;
                        });
                    } else {
                        $('#loading').hide();
                        swal({
                            title: 'Lỗi',
                            text: data.message,
                            icon: 'error',
                            timer: 2000
                        }).then(function () {
                            window.location.href = data.redirectUrl;
                        });
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    $('#loading').hide();
                    console.log('Lỗi AJAX: ' + textStatus + ', ' + errorThrown);
                    swal({
                        title: textStatus,
                        text: 'Lỗi: ' + errorThrown + '. Đăng nhập thất bại',
                        icon: 'error',
                        timer: 2000
                    }).then(function () {
                        window.location.href = data.redirectUrl;
                    });
                }
            });
        });
    }
    // Gọi hàm functionLogin khi tài liệu (document) đã sẵn sàng
    $(document).ready(function () {
        functionLogin();
    });

    $(document).ready(function () {
        // Kiểm tra xem WURFL và thuộc tính complete_device_name có tồn tại hay không
        if (typeof WURFL !== 'undefined' && WURFL.complete_device_name) {
            // Hiển thị thông tin từ WURFL lên thẻ span có class là "device-name"
            $('.device-name span').html(WURFL.complete_device_name);
        } else {
            // Nếu không có thông tin từ WURFL, sử dụng hàm getDeviceName()
            var userDevice = getDeviceName();
            $('.device-name span').html(userDevice);
        }
    });

    // Hàm lấy thông tin hệ điều hành từ User Agent
    function getDeviceName() {
        var userAgent = navigator.userAgent;
        var deviceName;

        // Các điều kiện để xác định hệ điều hành từ User Agent
        if (userAgent.match(/Windows/i)) {
            deviceName = 'Windows';
        } else if (userAgent.match(/Mac/i)) {
            deviceName = 'Mac OS';
        } else if (userAgent.match(/Linux/i)) {
            deviceName = 'Linux';
        } else if (userAgent.match(/iPhone|iPad|iPod/i)) {
            deviceName = 'iOS';
        } else if (userAgent.match(/Android/i)) {
            deviceName = 'Android';
        } else {
            deviceName = 'Unknown';
        }

        return deviceName;
    }    
});
