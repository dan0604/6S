// Hàm để cập nhật giới hạn datetimepicker dựa trên dữ liệu từ GetPhanCongTime
function updateDatetimePickerLimits() {
    $('#loading').show();
    $.ajax({
        url: '/DataApi/Get_PhanCongTime', // Điều chỉnh tên controller và action tương ứng
        type: 'GET',
        success: function (data) {
            if (data.success) {
                var decodedJson = atob(data.data);
                var decodedString = decodeURIComponent(escape(decodedJson));
                var parsedData = JSON.parse(decodedString);
                var loai_BCSelect = document.getElementById("Loai_BC");
                var phongBanSelect = document.getElementById("PhongBan");
                var Datetimepicker = document.getElementById("datetimepicker");
                loai_BCSelect.innerHTML = '';
                phongBanSelect.innerHTML = '';
                if (parsedData.hasOwnProperty('LoaiBCList') && parsedData.hasOwnProperty('PhongBanList')) {
                    parsedData.LoaiBCList.forEach(function (item) {
                        var optionLoai_BC = document.createElement("option");
                        optionLoai_BC.text = item.TenLoai;
                        optionLoai_BC.value = item.Loai_BC;
                        loai_BCSelect.appendChild(optionLoai_BC);
                    });

                    parsedData.PhongBanList.forEach(function (item) {
                        var optionPhongBan = document.createElement("option");
                        optionPhongBan.text = item.PhongBan;
                        optionPhongBan.value = item.ID_PhongBan;
                        phongBanSelect.appendChild(optionPhongBan);
                    });
                    Datetimepicker.disabled = false;
                    loai_BCSelect.disabled = false;
                    phongBanSelect.disabled = false;
                } else {
                    parsedData.forEach(function (item) {
                        var optionLoaiBC = document.createElement("option");
                        optionLoaiBC.text = item.TenLoai;
                        optionLoaiBC.value = item.Loai_BC;
                        loai_BCSelect.appendChild(optionLoaiBC);
                        var optionPhongBan = document.createElement("option");
                        optionPhongBan.text = item.PhongBan;
                        optionPhongBan.value = item.ID_PhongBan;
                        phongBanSelect.appendChild(optionPhongBan);
                        loai_BCSelect.disabled = true;
                        phongBanSelect.disabled = true;
                    });
                }
                $('#loading').hide();
            } else {
                Swal.fire({
                    icon: 'error',
                    text: data.message,
                    timer: 3000,
                    timerProgressBar: true,
                    didOpen: () => {
                        Swal.showLoading();
                        $('#loading').hide();
                    },
                }).then(() => {
                    location.reload();
                });
            }
        },
        error: function (error) {
            $('#loading').hide();
            console.log("Đã xảy ra lỗi: ", error);
        }
    });
}
updateDatetimePickerLimits();

///khai báo các tham số đầu vào cho table để lưu tạm thành mảng để xem trước khi đưa vào db
const addedRows = [];
let selectedHangmuc_Chinh = '';
let selectedHangmuc_Phu = '';
let imageURL; // Declare imageURL variable
// Define and initialize errorCount and pointCount variables
let errorCount = 0;
let pointCount = 0;
let imageCount = 0;

const hangmucChinhCheckboxes = document.querySelectorAll('input[name="Hangmuc_Chinh[]"]');
const hangmucPhuCheckboxes = document.querySelectorAll('input[name="Hangmuc_Phu[]"]');

// Lưu trạng thái ban đầu của tùy chọn trong Hangmuc_Phu
const initialPhuOptions = Array.from(hangmucPhuCheckboxes).map(checkbox => ({
    value: checkbox.value,
    disabled: checkbox.disabled
}));

// Lưu trạng thái ban đầu của tùy chọn trong Hangmuc_Chinh
const initialChinhOptions = Array.from(hangmucChinhCheckboxes).map(checkbox => ({
    value: checkbox.value,
    checked: checkbox.checked
}));

// Xác định xem có ít nhất một giá trị đã được chọn trong Hangmuc_Chinh hay không
const atLeastOneChinhSelected = initialChinhOptions.some(option => option.checked);

hangmucChinhCheckboxes.forEach(checkboxChinh => {
    checkboxChinh.addEventListener('change', () => {
        // Kiểm tra xem có ít nhất một giá trị trong Hangmuc_Chinh đã được chọn hay không
        const atLeastOneChinhSelected = [...hangmucChinhCheckboxes].some(checkbox => checkbox.checked);

        hangmucPhuCheckboxes.forEach(checkboxPhu => {
            if (atLeastOneChinhSelected) {
                // Nếu có ít nhất một giá trị trong Hangmuc_Chinh đã được chọn, mở tùy chọn trong Hangmuc_Phu
                checkboxPhu.disabled = false;

                // Nếu giá trị tương ứng trong Hangmuc_Chinh đã được chọn, vô hiệu hóa và bỏ chọn tùy chọn tương ứng trong Hangmuc_Phu
                if (checkboxChinh.checked && checkboxPhu.value === checkboxChinh.value) {
                    checkboxPhu.disabled = true;
                    checkboxPhu.checked = false;
                }
            } else {
                // Nếu không có giá trị nào trong Hangmuc_Chinh được chọn, vô hiệu hóa tất cả tùy chọn trong Hangmuc_Phu
                checkboxPhu.disabled = true;
                checkboxPhu.checked = false;
            }
        });
    });
});

// Vô hiệu hóa tất cả tùy chọn trong Hangmuc_Phu ban đầu
if (!atLeastOneChinhSelected) {
    hangmucPhuCheckboxes.forEach(checkboxPhu => {
        checkboxPhu.disabled = true;
    });
}

function resetCheckboxes() {
    hangmucChinhCheckboxes.forEach(checkboxChinh => {
        checkboxChinh.checked = false;
    });

    hangmucPhuCheckboxes.forEach(checkboxPhu => {
        const initialPhuOption = initialPhuOptions.find(option => option.value === checkboxPhu.value);
        checkboxPhu.disabled = initialPhuOption.disabled;
        checkboxPhu.checked = false;
    });

    // Đặt lại tất cả các tùy chọn trong Hangmuc_Chinh về trạng thái ban đầu
    initialPhuOptions.forEach(option => {
        const chinhCheckbox = document.querySelector(`input[name="Hangmuc_Chinh[]"][value="${option.value}"]`);
        if (chinhCheckbox) {
            chinhCheckbox.disabled = option.disabled;
        }
    });

}

// Hàm chuyển đổi blob thành định dạng dữ liệu hình ảnh (PNG)
function blobToImageDataURI(blob, callback) {
    const reader = new FileReader();
    reader.onload = function (event) {
        const dataURL = event.target.result;
        callback(dataURL);
    };
    reader.readAsDataURL(blob);
}

///khai báo các giá trị cần lấy và bắt lỗi khi chưa nhập đủ dữ liệu
function saveTemporarily() {
    const KhuVuc = document.getElementById("KhuVuc").value;
    const selectedChinhCheckboxes = document.querySelectorAll('input[name="Hangmuc_Chinh[]"]:checked');
    const MoTa = document.getElementById("MoTa").value;
    const fileInput = document.getElementById("HinhAnh");
    const file = fileInput.files[0];
    const PhongBan = document.getElementById("PhongBan").value;
    const Loai_BC = document.getElementById("Loai_BC").value;
    const Username = document.getElementById("Username").value;

    ///bắt lỗi các giá trị chưa được chọn
    if (!KhuVuc || !MoTa || selectedChinhCheckboxes.length === 0 || PhongBan === "Chọn phòng ban" || Loai_BC === "Chọn loại báo cáo" || Username === "Chọn người đánh giá") {
        let errorCountMessage = "Vui lòng kiểm tra các lỗi sau:\n";
        if (!KhuVuc) {
            errorCountMessage += "- Chưa nhập khu vực\n";
        }
        if (selectedChinhCheckboxes.length === 0) {
            errorCountMessage += "- Chưa chọn ít nhất một hạng mục chính\n";
        }
        if (!MoTa) {
            errorCountMessage += "- Chưa nhập mô tả lỗi\n";
        }
        if (PhongBan === "Chọn phòng ban") {
            errorCountMessage += "- Chưa chọn phòng ban\n";
        }
        if (Loai_BC === "Chọn loại báo cáo") {
            errorCountMessage += "- Chưa chọn loại báo cáo\n";
        }
        if (Username === "Chọn người đánh giá") {
            errorCountMessage += "- Chưa chọn người đánh giá\n";
        }

        Swal.fire({
            icon: 'error',
            title: 'Lưu tạm thất bại',
            text: errorCountMessage,
        });
        return;
    }

    // Khởi tạo biến imageURL
    let imageURL = '';

    // Kiểm tra xem có tệp HinhAnh được chọn hay không
    if (file) {
        // Nếu có tệp HinhAnh, thì thực hiện chuyển đổi và gán imageURL
        blobToImageDataURI(file, function (imageDataURI) {
            imageURL = imageDataURI;
            appendRow(KhuVuc, selectedChinhCheckboxes, MoTa, imageURL);
        });
    } else {
        // Nếu không có tệp HinhAnh, thì gán imageURL thành chuỗi trống
        imageURL = '';
        appendRow(KhuVuc, selectedChinhCheckboxes, MoTa, imageURL);
    }
}

///lấy hình ảnh từng dòng
function getImagesFromPreviewTable() {
    const images = [];
    const rows = document.querySelectorAll('#previewTable tbody tr');

    rows.forEach((row) => {
        const imageURLCell = row.querySelector('td img');
        const imageURL = imageURLCell ? imageURLCell.getAttribute('src') : null; // Kiểm tra nếu có hình ảnh hoặc không
        images.push(imageURL);
    });

    return images;
}
//table
function appendRow(KhuVuc, selectedChinhCheckboxes, MoTa, imageURL) {
    ///table hiển thị dữ liệu
    const newRow = document.createElement("tr");
    newRow.innerHTML = `
                                            <td style="width: 50px;">${addedRows.length + 1}</td>
                                            <td style="width: 100px;">${KhuVuc}</td>
                                            <td style="width: 100px;">${getSelectedHangmuc_Chinh()}</td>
                                            <td style="width: 100px;">${getSelectedHangmuc_Phu()}</td>
                                            <td style="width: 300px;">${MoTa}</td>
                                            ${imageURL ? `<td><img src="${imageURL}" alt="Hình ảnh" style="max-width: 300px; max-height: 300px; width: 80%;display: block;object-fit: cover;margin: 0 auto;"></td>` : '<td></td>'}
                                            <td style="width: 20px;">
                                                <button type="button" class="btn btn-outline-danger" onclick="deleteRow(this)"><i class='fas fa-trash'></i>Xóa</button>
                                            </td>
                                            `;

    document.getElementById("previewTable").getElementsByTagName("tbody")[0].appendChild(newRow);
    addedRows.push(newRow);
    document.getElementById("KhuVuc").value = "";
    document.getElementById("MoTa").value = "";
    document.getElementById("HinhAnh").value = "";
    clearHangmuc_Chinh();
    clearHangmuc_Phu();
    // Sau khi lưu tạm, làm mới giá trị của các tùy chọn
    resetCheckboxes();
    calculateerrorCountsAndpointCounts();
    imageCount++;
}
///tính điểm
function calculateerrorCountsAndpointCounts() {
    errorCount = 0;
    pointCount = 0;
    // Sử dụng một đối tượng Set để theo dõi các giá trị duy nhất
    const uniqueValues = new Set();

    addedRows.forEach((row) => {
        const selectedChinh = row.querySelectorAll('td')[2].textContent;
        const selectedPhu = row.querySelectorAll('td')[3].textContent;

        // Tạo mảng các giá trị cho Hangmuc_Chinh và Hangmuc_Phu
        const chinhValues = selectedChinh.trim() ? selectedChinh.split(', ') : [];
        const phuValues = selectedPhu.trim() ? selectedPhu.split(', ') : [];

        // Duyệt qua các giá trị cho Hangmuc_Chinh
        chinhValues.forEach((value) => {
            if (!uniqueValues.has(value)) {
                errorCount++; // Tăng lỗi tương ứng
                uniqueValues.add(value);
            }
        });

        // Duyệt qua các giá trị cho Hangmuc_Phu
        phuValues.forEach((value) => {
            if (!uniqueValues.has(value)) {
                errorCount++; // Tăng lỗi tương ứng
                uniqueValues.add(value);
            }
        });
    });
    pointCount = 24 - uniqueValues.size;
    document.getElementById("errorCount").value = errorCount;
    document.getElementById("pointCount").value = pointCount;

    const imageCountElements = previewTable.getElementsByTagName("img");
    const imageCount = imageCountElements.length;
    document.getElementById("imageCount").value = imageCount;
    console.log('Số hình có ở giao diện: ', imageCount);
}
///lấy dữ liệu
function confirmAndSend() {
    $('#loading').show();
    // Create a FormData object to capture form data
    const formData = new FormData(document.getElementById("checklistForm"));
    const images = getImagesFromPreviewTable();
    for (let i = 0; i < images.length; i++) {
        formData.append('HinhAnh[]', images[i]);
    }
    // Thêm các giá trị khác vào formData
    formData.append('errorCount', errorCount);
    formData.append('pointCount', pointCount);
    var imageCounter = document.getElementById("imageCount").value
    formData.append('imageCount', imageCounter);
    console.log('Số hình gửi đi: ', imageCounter)
    // Lấy dữ liệu từ bảng xem trước (previewTable)
    const dataToSend = [];
    addedRows.forEach((row) => {
        const cells = row.querySelectorAll('td');
        const imageURLCell = row.querySelector('img');
        const imageURL = imageURLCell ? imageURLCell.getAttribute('src') : null;
        dataToSend.push({
            STT: cells[0].textContent,
            KhuVuc: cells[1].textContent,
            Hangmuc_Chinh: cells[2].textContent,
            Hangmuc_Phu: cells[3].textContent,
            MoTa: cells[4].textContent,
            HinhAnh: imageURL, // Use the imageURL variable
            errorCount: errorCount,
            pointCount: pointCount,
            imageCount: imageCounter
        });
    });

    // Add the dataToSend as a field in the formData object
    formData.append('dataToSend', JSON.stringify(dataToSend));
    // Gửi dữ liệu đến Controller
    $.ajax({
        url: '/Home/Add_checklist',
        type: 'POST',
        data: formData,  // Send the entire form data
        processData: false,
        contentType: false,
        success: function (result) {
            // Kiểm tra xem trả về có tệp PDF hay không
            if (result.contentType === 'application/pdf') {
                // Tạo một Blob từ dữ liệu PDF
                var blob = new Blob([result], { type: 'application/pdf' });
                var url = window.URL.createObjectURL(blob);

                // Tạo một thẻ <a> để mở tệp PDF trong tab mới
                var a = document.createElement('a');
                a.href = url;
                a.download = 'document.pdf';
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
            }

            // Thông báo thành công với icon và thời gian tăng lên 100000ms (100 giây)
            Swal.fire({
                icon: 'success', // Icon thành công
                title: result.message, // Nội dung thông báo thành công
                timer: 1000, // Thời gian hiển thị
                showConfirmButton: false // Ẩn nút OK
            });

            // Redirect to the specified URL after the thời gian đúng hoặc sai tăng lên 100000ms (100 giây)
            if (result.redirectUrl) {
                setTimeout(function () {
                    window.location.href = result.redirectUrl;
                }, 2000); // Tạm dừng sau khi hiển thị thông báo
            }

            clearPreviewTable();
        },
        error: function (xhr, status, error) {
            $('#loading').hide();
            // Hiển thị thông báo lỗi với icon lỗi và thời gian tăng lên 100000ms (100 giây)
            Swal.fire({
                icon: 'error', // Icon lỗi
                title: 'Error: ' + error + status + xhr, // Nội dung thông báo lỗi
                timer: 8000, // Thời gian hiển thị
                showConfirmButton: false // Ẩn nút OK
            });
            setTimeout(function () {
                location.reload();
            }, 2000);
        }
    });
}
//lưu tạm xong thì xóa hết form bắt đầu lại
function clearPreviewTable() {
    const tbody = document.getElementById("previewTable").getElementsByTagName("tbody")[0];
    while (tbody.firstChild) {
        tbody.removeChild(tbody.firstChild);
    }
}

function getSelectedHangmuc_Chinh() {
    const checkboxes = document.querySelectorAll('input[name="Hangmuc_Chinh[]"]:checked');
    const selectedHangmuc_Chinh = [];
    checkboxes.forEach((checkbox) => {
        selectedHangmuc_Chinh.push(checkbox.value);
        // Disable the corresponding option in Hangmuc_Phu
        const phuCheckbox = document.querySelector(`input[name="Hangmuc_Phu[]"][value="${checkbox.value}"]`);
        if (phuCheckbox) {
            phuCheckbox.disabled = true;
        }
    });
    return selectedHangmuc_Chinh.join(', ');
}

function clearHangmuc_Chinh() {
    const checkboxes = document.querySelectorAll('input[name="Hangmuc_Chinh[]"]');
    checkboxes.forEach((checkbox) => {
        checkbox.checked = false;
    });
}

function getSelectedHangmuc_Phu() {
    const checkboxes = document.querySelectorAll('input[name="Hangmuc_Phu[]"]:checked');
    const selectedHangmuc_Phu = [];
    checkboxes.forEach((checkbox) => {
        selectedHangmuc_Phu.push(checkbox.value);
        // Disable the corresponding option in Hangmuc_Chinh
        const chinhCheckbox = document.querySelector(`input[name="Hangmuc_Chinh[]"][value="${checkbox.value}"]`);
        if (chinhCheckbox) {
            chinhCheckbox.disabled = true;
        }
    });
    return selectedHangmuc_Phu.join(', ');
}

function clearHangmuc_Phu() {
    const checkboxes = document.querySelectorAll('input[name="Hangmuc_Phu[]"]');
    checkboxes.forEach((checkbox) => {
        checkbox.checked = false;
    });
}
//copy dòng
function copyRow(button) {
    // Sao chép dòng được chọn
    const rowToCopy = button.parentElement.parentElement;
    const KhuVuc = rowToCopy.querySelector('td:nth-child(2)').textContent;
    const MoTa = rowToCopy.querySelector('td:nth-child(5)').textContent;
    const imageURL = rowToCopy.querySelector('img') ? rowToCopy.querySelector('img').src : "";

    // Gọi hàm appendRow với thông tin sao chép
    appendRow(KhuVuc, [], MoTa, imageURL);
}
//sửa
function editRow(button) {
    // Chỉnh sửa dòng được chọn
    selectedRowToEdit = button.parentElement.parentElement;
    const KhuVuc = selectedRowToEdit.querySelector('td:nth-child(2)').textContent;
    const MoTa = selectedRowToEdit.querySelector('td:nth-child(5)').textContent;
    const imageURL = selectedRowToEdit.querySelector('img') ? selectedRowToEdit.querySelector('img').src : "";

    // Điền thông tin vào các trường nhập
    document.getElementById("KhuVuc").value = KhuVuc;
    document.getElementById("MoTa").value = MoTa;
    document.getElementById("HinhAnh").value = imageURL;

    // Ghi đè các tùy chọn khác (nếu có)

    // Đặt lại nút Lưu để cho phép cập nhật
    document.getElementById("saveButton").textContent = "Cập Nhật";
}
//lưu lại khi sửa
function saveRow() {
    if (selectedRowToEdit) {
        // Lấy thông tin từ các trường nhập và cập nhật dòng
        const KhuVuc = document.getElementById("KhuVuc").value;
        const MoTa = document.getElementById("MoTa").value;
        const imageURL = document.getElementById("HinhAnh").value;

        // Cập nhật dòng
        selectedRowToEdit.querySelector('td:nth-child(2)').textContent = KhuVuc;
        selectedRowToEdit.querySelector('td:nth-child(5)').textContent = MoTa;
        if (imageURL) {
            if (selectedRowToEdit.querySelector('img')) {
                selectedRowToEdit.querySelector('img').src = imageURL;
            } else {
                selectedRowToEdit.querySelector('td:last-child').innerHTML = `<img src="${imageURL}" alt="Hình ảnh" style="max-width: 400px; width: 100%;display: block;object-fit: cover;margin: 0 auto;">`;
            }
        } else if (selectedRowToEdit.querySelector('img')) {
            selectedRowToEdit.querySelector('td:last-child').innerHTML = "";
        }

        // Đặt lại nút Lưu về trạng thái ban đầu
        document.getElementById("saveButton").textContent = "Lưu";

        // Đặt lại các giá trị của trường nhập và các tùy chọn khác
        document.getElementById("KhuVuc").value = "";
        document.getElementById("MoTa").value = "";
        document.getElementById("HinhAnh").value = "";
        clearHangmuc_Chinh();
        clearHangmuc_Phu();

        // Đặt selectedRowToEdit thành null để ngăn cập nhật không mong muốn
        selectedRowToEdit = null;
    }
}

//xóa dòng
function deleteRow(button) {
    const row = button.parentNode.parentNode;
    row.parentNode.removeChild(row);
    const index = addedRows.indexOf(row);
    if (index > -1) {
        addedRows.splice(index, 1);
    }
    calculateerrorCountsAndpointCounts();
}

