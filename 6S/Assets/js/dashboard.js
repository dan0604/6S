let selectedValue = '';
const AnotherThangNamElement = document.getElementById('idThangNam');
const displayElement = document.getElementById('displayValue');
const exportDropdown = document.querySelector('.export-dropdown');
const reportContainerone = document.querySelector('.report-container-one');
const reportContainertwo = document.querySelector('.report-container-two');

AnotherThangNamElement?.addEventListener('change', function () {
    selectedValue = this.value;
    const splitValue = selectedValue.split('-');
    //console.log(selectedValue);
    const formattedValue = splitValue[1] + '/' + splitValue[0];
    displayElement.textContent = "THÁNG " + formattedValue;

    if (!selectedValue.trim()) {
        hideElements();
    } else {
        showElements();
        $.ajax({
            url: 'view_Checklist',
            type: 'GET',
            data: { ThangNam: btoa(selectedValue) },
            success: function (response) {
                if (response && response.success) {
                    if (response.data && response.data.length > 0) {
                        // Dữ liệu thành công được trả về từ API
                        var encryptedData = response.data;
                        // Mã hóa dữ liệu sử dụng btoa
                        var decodedJson = atob(encryptedData);
                        var decodedString = decodeURIComponent(escape(decodedJson));
                        var data = JSON.parse(decodedString);
                        if (data && Object.keys(data).length > 0) {
                            //để chart ở đây
                            // Tạo biểu đồ Khối Sản Xuất - Đường và Cột (chart1 và chart3)
                            Chart.register(ChartDataLabels);
                            function createChart(ctx, type, labels, datasets, yAxisTitle) {
                                const chart = new Chart(ctx, {
                                    type: type,
                                    data: {
                                        labels: labels,
                                        datasets: datasets
                                    },
                                    options: {
                                        scales: {
                                            y: {
                                                position: 'left',
                                                title: {
                                                    display: true,
                                                    text: yAxisTitle,
                                                    color: 'red',
                                                    font: {
                                                        size: 16,
                                                        weight: 'bold'
                                                    }
                                                },
                                                ticks: {
                                                    font: {
                                                        size: 14,
                                                        weight: 'bold'
                                                    }
                                                }
                                            }
                                        },
                                        plugins: {
                                            datalabels: {
                                                anchor: 'end',
                                                align: 'center',
                                                font: {
                                                    size: 14,
                                                    weight: 'bold',
                                                    color: 'red'
                                                }
                                            }
                                        }
                                    }
                                });
                            }
                            // Lọc dữ liệu chỉ lấy Khối sản xuất
                            var filteredData_K1 = data.filter(function (item) {
                                //console.log(filteredData_K1);
                                return item.Tenkhoi === "Khối Sản Xuất";
                            });
                            // Tìm và hiển thị giá trị 'DiemTrungBinhKhoi' theo 'Khối sản xuất'
                            var mergedValuesDivthree = document.getElementById('mergedValuesthree');
                            var mergedmergedValuesone = document.getElementById('mergedValuesone');
                            var firstKhoiValue_K1 = filteredData_K1[0].Tenkhoi;
                            //console.log(firstKhoiValue_K1);
                            var firstDiemTBCCValue_K1 = filteredData_K1[0].DiemTrungBinhKhoi;
                            var isErrorFound_K1 = false;
                            filteredData_K1.forEach(function (item) {
                                if (item.DiemTrungBinhKhoi !== firstDiemTBCCValue_K1 && !isErrorFound_K1) {
                                    isErrorFound_K1 = true;
                                    Swal.fire({
                                        icon: 'error',
                                        title: 'Lỗi!',
                                        text: "Giá trị DiemTrungBinhKhoi của 'Khối sản xuất' không đồng nhất."
                                    });
                                    mergedmergedValuesone.innerHTML = '';
                                    mergedValuesDivthree.innerHTML = '';
                                    filteredData_K1 = '';
                                } else {
                                    mergedmergedValuesone.innerHTML = firstKhoiValue_K1 + '<br>';
                                    mergedValuesDivthree.innerHTML = firstDiemTBCCValue_K1 + '<br>';
                                }
                            });

                            // Lọc dữ liệu chỉ lấy Khối văn phòng
                            var filteredData_K2 = data.filter(function (item) {
                                return item.Tenkhoi === "Khối Văn Phòng";
                            });
                            // Tìm và hiển thị giá trị 'DiemTrungBinhKhoi' theo 'Khối văn phòng'
                            var mergedValuesDivtwo = document.getElementById('mergedValuestwo');
                            var mergedValuesDivfour = document.getElementById('mergedValuesfour');
                            var firstKhoiValue_K2 = filteredData_K2[0].Tenkhoi;
                            var firstDiemTBCCValue_K2 = filteredData_K2[0].DiemTrungBinhKhoi;
                            var isErrorFound_K2 = false;
                            filteredData_K2.forEach(function (item) {
                                if (item.DiemTrungBinhKhoi !== firstDiemTBCCValue_K2 && !isErrorFound_K2) {
                                    isErrorFound_K2 = true;
                                    Swal.fire({
                                        icon: 'error',
                                        title: 'Lỗi!',
                                        text: "Giá trị DiemTrungBinhKhoi của 'Khối văn phòng' không đồng nhất."
                                    });
                                    mergedValuesDivtwo.innerHTML = '';
                                    mergedValuesDivfour.innerHTML = '';
                                    filteredData_K2 = '';
                                } else {
                                    mergedValuesDivtwo.innerHTML = firstKhoiValue_K2 + '<br>';
                                    mergedValuesDivfour.innerHTML = firstDiemTBCCValue_K2 + '<br>';
                                }
                            });
                            if (!filteredData_K1 || filteredData_K1.length === 0 || !filteredData_K2 || filteredData_K2.length === 0) {
                                console.error('Lỗi dữ liệu từ data');
                            } else {
                                const chartConfigs = [
                                    {
                                        ctx: document.getElementById('chart1').getContext('2d'),
                                        type: 'bar',
                                        labels: filteredData_K1.map(function (item) {
                                            return item.TenPhongBan;
                                        }),
                                        datasets: [
                                            {
                                                label: 'Điểm chấm nội bộ',
                                                data: filteredData_K1.map(function (item) {
                                                    return item.DiemChamNoiBo;
                                                }),
                                                backgroundColor: 'rgba(0, 0, 255, 1)',
                                                borderColor: 'rgba(0, 0, 255, 1)',
                                                borderWidth: 1
                                            },
                                            {
                                                label: 'Điểm chấm chéo',
                                                data: filteredData_K1.map(function (item) {
                                                    return item.DiemChamCheo;
                                                }),
                                                backgroundColor: 'rgba(255, 255, 0, 1)',
                                                borderColor: 'rgba(255, 255, 0, 1)',
                                                borderWidth: 1
                                            },
                                            {
                                                label: 'Điểm chấm hiệu chỉnh',
                                                data: filteredData_K1.map(function (item) {
                                                    return item.DiemChamHieuChinh;
                                                }),
                                                backgroundColor: 'rgba(0, 128, 0, 1)',
                                                borderColor: 'rgba(0, 128, 0, 1)',
                                                borderWidth: 1
                                            },
                                            {
                                                type: 'line',
                                                label: 'Mục tiêu',
                                                data: filteredData_K1.map(function (item) {
                                                    return item.MucTieu;
                                                }),
                                                fill: false,
                                                borderColor: 'rgba(255, 0, 0, 1)',
                                                borderWidth: 1
                                            }
                                        ],
                                        yAxisTitle: 'Điểm'
                                    },
                                    {
                                        ctx: document.getElementById('chart2').getContext('2d'),
                                        type: 'bar',
                                        labels: filteredData_K1.map(function (item) {
                                            return item.TenPhongBan;
                                        }),
                                        datasets: [
                                            {
                                                label: 'Chấm nội bộ',
                                                data: filteredData_K1.map(function (item) {
                                                    return item.TongHinTongHinhChamCheoamNoiBo;
                                                }),
                                                backgroundColor: 'rgba(0, 0, 255, 1)',
                                                borderColor: 'rgba(0, 0, 255, 1)',
                                                borderWidth: 1
                                            },
                                            {
                                                label: 'Chấm chéo',
                                                data: filteredData_K1.map(function (item) {
                                                    return item.TongHinhChamCheo;
                                                }),
                                                backgroundColor: 'rgba(255, 255, 0, 1)',
                                                borderColor: 'rgba(255, 255, 0, 1)',
                                                borderWidth: 1
                                            },
                                            {
                                                label: 'Chấm hiệu chỉnh',
                                                data: filteredData_K1.map(function (item) {
                                                    return item.TongHinhChamHieuChinh;
                                                }),
                                                backgroundColor: 'rgba(0, 128, 0, 1)',
                                                borderColor: 'rgba(0, 128, 0, 1)',
                                                borderWidth: 1
                                            }
                                        ],
                                        yAxisTitle: 'Hình ảnh'
                                    },
                                    // KHỐI VĂN PHÒNG
                                    {
                                        ctx: document.getElementById('chart3').getContext('2d'),
                                        type: 'bar',
                                        labels: filteredData_K2.map(function (item) {
                                            return item.TenPhongBan;
                                        }),
                                        datasets: [
                                            {
                                                label: 'Điểm chấm nội bộ',
                                                data: filteredData_K2.map(function (item) {
                                                    return item.DiemChamNoiBo;
                                                }),
                                                backgroundColor: 'rgba(0, 0, 255, 1)',
                                                borderColor: 'rgba(0, 0, 255, 1)',
                                                borderWidth: 1
                                            },
                                            {
                                                label: 'Điểm chấm chéo',
                                                data: filteredData_K2.map(function (item) {
                                                    return item.DiemChamCheo;
                                                }),
                                                backgroundColor: 'rgba(255, 255, 0, 1)',
                                                borderColor: 'rgba(255, 255, 0, 1)',
                                                borderWidth: 1
                                            },
                                            {
                                                label: 'Điểm chấm hiệu chỉnh',
                                                data: filteredData_K2.map(function (item) {
                                                    return item.DiemChamHieuChinh;
                                                }),
                                                backgroundColor: 'rgba(0, 128, 0, 1)',
                                                borderColor: 'rgba(0, 128, 0, 1)',
                                                borderWidth: 1
                                            },
                                            {
                                                type: 'line',
                                                label: 'Mục tiêu',
                                                data: filteredData_K2.map(function (item) {
                                                    return item.MucTieu;
                                                }),
                                                fill: false,
                                                borderColor: 'rgba(255, 0, 0, 1)',
                                                borderWidth: 1
                                            }
                                        ],
                                        yAxisTitle: 'Điểm'
                                    },
                                    {
                                        ctx: document.getElementById('chart4').getContext('2d'),
                                        type: 'bar',
                                        labels: filteredData_K2.map(function (item) {
                                            return item.TenPhongBan;
                                        }),
                                        datasets: [
                                            {
                                                label: 'Chấm nội bộ',
                                                data: filteredData_K2.map(function (item) {
                                                    return item.TongHinTongHinhChamCheoamNoiBo;
                                                }),
                                                backgroundColor: 'rgba(0, 0, 255, 1)',
                                                borderColor: 'rgba(0, 0, 255, 1)',
                                                borderWidth: 1
                                            },
                                            {
                                                label: 'Chấm chéo',
                                                data: filteredData_K2.map(function (item) {
                                                    return item.TongHinhChamCheo;
                                                }),
                                                backgroundColor: 'rgba(255, 255, 0, 1)',
                                                borderColor: 'rgba(255, 255, 0, 1)',
                                                borderWidth: 1
                                            },
                                            {
                                                label: 'Chấm hiệu chỉnh',
                                                data: filteredData_K2.map(function (item) {
                                                    return item.TongHinhChamHieuChinh;
                                                }),
                                                backgroundColor: 'rgba(0, 128, 0, 1)',
                                                borderColor: 'rgba(0, 128, 0, 1)',
                                                borderWidth: 1
                                            }
                                        ],
                                        yAxisTitle: 'Hình ảnh'
                                    }
                                ];
                                chartConfigs.forEach(config => {
                                    createChart(config.ctx, config.type, config.labels, config.datasets, config.yAxisTitle);
                                });
                            }
                            var table = document.getElementById("tableDetail");
                            // Hàm mergeCell dùng để merge các ô dữ liệu trùng nhau
                            function mergeCell() {
                                if (typeof table !== 'undefined' && table !== null && table.rows.length > 1) {
                                    var columnIndexToMerge = 0;
                                    var rowCount = table.rows.length;
                                    var currentRowValue = table.rows[1].cells[columnIndexToMerge].innerHTML;
                                    var rowSpanCount = 1;
                                    for (var i = 2; i < rowCount; i++) {
                                        var cellValue = table.rows[i].cells[columnIndexToMerge].innerHTML;

                                        if (cellValue === currentRowValue) {
                                            rowSpanCount++;
                                            table.rows[i].deleteCell(columnIndexToMerge); // Xóa ô dữ liệu trùng lặp
                                        } else {
                                            if (rowSpanCount > 1) {
                                                table.rows[i - rowSpanCount].cells[columnIndexToMerge].rowSpan = rowSpanCount;
                                            }
                                            currentRowValue = cellValue;
                                            rowSpanCount = 1;
                                        }
                                    }
                                    if (rowSpanCount > 1) {
                                        table.rows[rowCount - rowSpanCount].cells[columnIndexToMerge].rowSpan = rowSpanCount;
                                    }
                                } else {
                                    console.error('Bảng không tồn tại hoặc không đủ dữ liệu để merge.');
                                }
                            }
                            // Thêm dữ liệu từ mảng data vào bảng
                            data.forEach(function (row) {
                                var newRow = table.insertRow(-1);
                                Object.values(row).forEach(function (cellData, index) {
                                    var newCell = newRow.insertCell(-1);
                                    if (cellData === null || cellData === undefined || cellData === '') {
                                        newCell.textContent = '-';
                                    } else {
                                        newCell.textContent = cellData;
                                    }
                                    // Ẩn cột "Tổng Điểm Trung Bình Khối" bằng cách thêm class 'hide-column' cho các cell trong cột đó
                                    if (index === 10) { // index 10 tương ứng với cột "Tổng Điểm Trung Bình Khối"
                                        newCell.classList.add('hide-column');
                                    }
                                });
                            });
                            mergeCell();
                            Swal.fire({
                                title: 'Thông báo',
                                title: response.message,
                                icon: 'success',
                                timer: 2000,
                                timerProgressBar: true,
                            });
                        }
                        else {
                            hideElements();
                            Swal.fire({
                                title: 'Cảnh báo',
                                text: 'Không có data báo cáo tháng: ' + selectedValue,
                                icon: 'error',
                                timer: 2000,
                                timerProgressBar: true,
                            }).then(function () {
                                location.reload();
                            });
                        }
                    } else {
                        // Xử lý khi dữ liệu rỗng
                        hideElements();
                        Swal.fire({
                            title: 'Dữ liệu rỗng',
                            text: 'Không có dữ liệu được trả về từ server.',
                            icon: 'error',
                            timer: 2000,
                            timerProgressBar: true,
                        }).then(function () {
                            location.reload();
                        });
                    }
                } else {
                    hideElements();
                    console.error("Lỗi khi gọi API: " + response.message);
                    Swal.fire({
                        title: 'Cảnh báo',
                        text: response.message,
                        icon: 'error',
                        timer: 2000,
                        timerProgressBar: true,
                    }).then(function () {
                        location.reload();
                    });
                }
            },
            error: function (xhr, status, error) {
                hideElements();
                console.error("Lỗi: " + error);
            }
        });
    }
});

function hideElements() {
    exportDropdown.style.display = 'none';
    reportContainerone.style.display = 'none';
    reportContainertwo.style.display = 'none';
}

function showElements() {
    exportDropdown.style.display = 'block';
    reportContainerone.style.display = 'block';
    reportContainertwo.style.display = 'block';
}

// Hàm xử lý sự kiện khi xuất file
function exportFile() {
    const selectedOption = document.getElementById('exportOptions').value;
    if (selectedOption === 'pdf') {
        exportToPDF();
    } else if (selectedOption === 'excel') {
        exportToExcel();
    }
}
// Hàm xuất PDF
function exportToPDF(elementId, fileName) {
    // Khởi tạo một đối tượng jsPDF với orientation là landscape (ngang)
    let pdf = new jsPDF('l', 'pt', 'a2', [300]); // Kích thước A3 theo chiều ngang
    // Hàm để thêm hình ảnh từ canvas vào PDF với kích thước và vị trí tự động
    function addImageToPdfFullPage(canvas, pdf) {
        const imgData = canvas.toDataURL('image/png');
        const pdfWidth = pdf.internal.pageSize.width;
        const pdfHeight = pdf.internal.pageSize.height;
        const imgWidth = canvas.width;
        const imgHeight = canvas.height;
        const ratio = Math.min(pdfWidth / imgWidth, pdfHeight / imgHeight);
        const width = imgWidth * ratio;
        const height = imgHeight * ratio;
        const x = (pdfWidth - width) / 2; // Để căn giữa theo chiều ngang
        const y = (pdfHeight - height) / 2; // Để căn giữa theo chiều dọc
        pdf.addImage(imgData, 'PNG', x, y, width, height);
        pdf.addPage(); // Thêm một trang mới
    }
    // Sử dụng html2canvas để chuyển đổi phần HTML thành hình ảnh và thêm vào PDF
    html2canvas(document.querySelector("#rptpage_number_one"), { dpi: 300, scrollY: -window.scrollY, windowWidth: document.documentElement.clientWidth, windowHeight: document.documentElement.clientHeight }).then(canvas1 => {
        addImageToPdfFullPage(canvas1, pdf);
        // Chuyển đổi phần 2
        html2canvas(document.querySelector("#rptpage_number_two"), { dpi: 300, scrollY: -window.scrollY, windowWidth: document.documentElement.clientWidth, windowHeight: document.documentElement.clientHeight }).then(canvas2 => {
            addImageToPdfFullPage(canvas2, pdf);
            const now = new Date();
            const currentTime = `${now.getHours()}${now.getMinutes()}${now.getSeconds()}`;
            const currentDate = `${now.getDate()}${now.getMonth() + 1}${now.getFullYear()}`;
            const fileNamePdf = `Template_rptPdf_baocao6stongthang_${selectedValue}_${currentDate}${currentTime}.pdf`;
            // Thêm thông báo Swal với timerProgressBar
            Swal.fire({
                icon: 'success',
                title: 'Pdf xuất thành công: ' + fileNamePdf,
                text: 'Đang tải xuống...',
                timerProgressBar: true,
                timer: 2000,
            }).then(() => {
                pdf.save(fileNamePdf);
            });
        });
    });
}
// Hàm xuất Excel
async function exportToExcel() {
    const table = document.getElementById('tableDetail');
    const sheet = XLSX.utils.table_to_sheet(table);
    const range = XLSX.utils.decode_range(sheet['!ref']);
    for (let row = range.s.r; row <= range.e.r; row++) {
        for (let col = range.s.c; col <= range.e.c; col++) {
            const cellAddress = XLSX.utils.encode_cell({ r: row, c: col });
            const cell = sheet[cellAddress];

            if (cell) {
                cell.s = {
                    border: {
                        top: { style: 'thin', color: { rgb: 'FF0000' } },
                        bottom: { style: 'thin', color: { rgb: '00FF00' } },
                        left: { style: 'thin', color: { rgb: '0000FF' } },
                        right: { style: 'thin', color: { rgb: 'FF00FF' } }
                    }
                };
            }
        }
    }
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, sheet, 'Baocao6s');
    const now = new Date();
    const currentTime = `${now.getHours()}${now.getMinutes()}${now.getSeconds()}`;
    const currentDate = `${now.getDate()}${now.getMonth() + 1}${now.getFullYear()}`;
    const fileNameExcel = `Template_rptExcel_baocao6stongthang_${selectedValue}_${currentDate}${currentTime}.xlsx`;
    await new Promise((resolve) => {
        XLSX.writeFile(wb, fileNameExcel);
        setTimeout(() => {
            resolve();
        }, 1000);
    });
    Swal.fire({
        title: 'Excel xuất thành công: ' + fileNameExcel,
        icon: 'success',
        showConfirmButton: true,
        timerProgressBar: true,
        timer: 1500
    });
    location.reload();
}