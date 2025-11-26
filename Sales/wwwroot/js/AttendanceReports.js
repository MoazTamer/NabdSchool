let reportTable;
let attendanceChart;

$(document).ready(function () {
    initializePage();
    initializeDataTable();
});

function initializePage() {
    // تعيين التاريخ الحالي
    const today = new Date();
    document.getElementById('weekStartDate').valueAsDate = getLastSaturday(today);

    // تعيين الشهر والسنة الحالية
    document.getElementById('monthSelect').value = today.getMonth() + 1;

    // ملء قائمة السنوات
    const yearSelect = document.getElementById('yearSelect');
    const currentYear = today.getFullYear();
    for (let year = currentYear; year >= currentYear - 5; year--) {
        const option = document.createElement('option');
        option.value = year;
        option.text = year;
        if (year === currentYear) option.selected = true;
        yearSelect.appendChild(option);
    }

    // عند تغيير نوع التقرير
    $('#reportType').on('change', function () {
        if ($(this).val() === 'weekly') {
            $('#weeklyDatePicker').show();
            $('#monthlyPicker, #yearPicker').hide();
        } else {
            $('#weeklyDatePicker').hide();
            $('#monthlyPicker, #yearPicker').show();
        }
    });
}

function getLastSaturday(date) {
    const day = date.getDay();
    const diff = day === 6 ? 0 : day + 1; // Saturday is 6
    const saturday = new Date(date);
    saturday.setDate(date.getDate() - diff);
    return saturday;
}

function initializeDataTable() {
    reportTable = $('#reportTable').DataTable({
        "language": {
            "url": "//cdn.datatables.net/plug-ins/1.13.7/i18n/ar.json"
        },
        "order": [[0, "asc"]],
        "pageLength": 31,
        "columns": [
            { "data": "date" },
            { "data": "dayName", "className": "fw-bold" },
            {
                "data": "present",
                "className": "text-center fw-bold text-success",
                "render": function (data) {
                    return `<span class="badge badge-light-success fs-5">${data}</span>`;
                }
            },
            {
                "data": "onTime",
                "className": "text-center fw-bold text-primary"
            },
            {
                "data": "late",
                "className": "text-center fw-bold text-warning",
                "render": function (data) {
                    return data > 0 ? `<span class="badge badge-warning">${data}</span>` : data;
                }
            },
            {
                "data": "absent",
                "className": "text-center fw-bold text-danger",
                "render": function (data) {
                    return data > 0 ? `<span class="badge badge-danger">${data}</span>` : data;
                }
            },
            {
                "data": "excused",
                "className": "text-center fw-bold text-info",
                "render": function (data) {
                    return data > 0 ? `<span class="badge badge-info">${data}</span>` : data;
                }
            },
            {
                "data": "attendancePercentage",
                "className": "text-center fw-bold",
                "render": function (data) {
                    const color = data >= 90 ? 'success' : data >= 75 ? 'warning' : 'danger';
                    return `<span class="badge badge-light-${color} fs-6">${data}%</span>`;
                }
            }
        ]
    });
}

async function loadReport() {
    const reportType = $('#reportType').val();

    try {
        showLoader();

        if (reportType === 'weekly') {
            await loadWeeklyReport();
        } else {
            await loadMonthlyReport();
        }

        // إظهار الأقسام
        $('#summarySection, #chartSection, #tableSection').fadeIn();

    } catch (error) {
        console.error('Error loading report:', error);
        Swal.fire({
            icon: 'error',
            title: 'خطأ',
            text: 'حدث خطأ أثناء تحميل التقرير'
        });
    } finally {
        hideLoader();
    }
}

async function loadWeeklyReport() {
    const startDate = $('#weekStartDate').val();

    const response = await $.ajax({
        url: '/AttendanceReports/GetWeeklyReport',
        type: 'GET',
        data: { startDate: startDate }
    });

    if (response.success) {
        updateTable(response.data);
        updateChart(response.data, 'أسبوعي');
        await updateSummary(response.weekStart, response.weekEnd);
    }
}

async function loadMonthlyReport() {
    const month = $('#monthSelect').val();
    const year = $('#yearSelect').val();

    const response = await $.ajax({
        url: '/AttendanceReports/GetMonthlyReport',
        type: 'GET',
        data: { month: month, year: year }
    });

    if (response.success) {
        updateTable(response.data);
        updateChart(response.data, response.monthName);
        await updateSummary(response.monthStart, response.monthEnd);
    }
}

function updateTable(data) {
    reportTable.clear();
    reportTable.rows.add(data);
    reportTable.draw();
}

function updateChart(data, label) {
    const ctx = document.getElementById('attendanceChart').getContext('2d');

    if (attendanceChart) {
        attendanceChart.destroy();
    }

    attendanceChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.map(d => d.dayName + ' ' + d.dayNumber),
            datasets: [
                {
                    label: 'الحضور الكلي',
                    data: data.map(d => d.present),
                    borderColor: 'rgb(50, 205, 50)',
                    backgroundColor: 'rgba(50, 205, 50, 0.1)',
                    tension: 0.4,
                    fill: true
                },
                {
                    label: 'في الوقت',
                    data: data.map(d => d.onTime),
                    borderColor: 'rgb(0, 158, 247)',
                    backgroundColor: 'rgba(0, 158, 247, 0.1)',
                    tension: 0.4
                },
                {
                    label: 'متأخرين',
                    data: data.map(d => d.late),
                    borderColor: 'rgb(255, 193, 7)',
                    backgroundColor: 'rgba(255, 193, 7, 0.1)',
                    tension: 0.4
                },
                {
                    label: 'غياب',
                    data: data.map(d => d.absent),
                    borderColor: 'rgb(244, 67, 54)',
                    backgroundColor: 'rgba(244, 67, 54, 0.1)',
                    tension: 0.4
                },
                {
                    label: 'مستئذنين',
                    data: data.map(d => d.excused),
                    borderColor: 'rgb(23, 162, 184)',
                    backgroundColor: 'rgba(23, 162, 184, 0.1)',
                    tension: 0.4
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                title: {
                    display: true,
                    text: `تقرير ${label}`,
                    font: { size: 16, weight: 'bold' }
                },
                legend: {
                    position: 'bottom',
                    rtl: true,
                    labels: { font: { size: 12 } }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: { font: { size: 12 } }
                },
                x: {
                    ticks: { font: { size: 11 } }
                }
            }
        }
    });
}

async function updateSummary(startDate, endDate) {
    const response = await $.ajax({
        url: '/AttendanceReports/GetReportSummary',
        type: 'GET',
        data: { startDate: startDate, endDate: endDate }
    });

    if (response.success) {
        const summary = response.summary;
        $('#summaryTotalStudents').text(summary.totalStudents);
        $('#summaryPresent').text(summary.totalPresent);
        $('#summaryAbsent').text(summary.totalAbsent);
        $('#summaryAverage').text(summary.averageAttendance + '%');
    }
}

function showLoader() {
    Swal.fire({
        title: 'جاري التحميل...',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
}

function hideLoader() {
    Swal.close();
}