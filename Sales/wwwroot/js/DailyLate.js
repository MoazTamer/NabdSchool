const API_BASE = '/Reports/';
const API_URLS = {
    getClasses: API_BASE + 'GetClasses',
    getClassRooms: API_BASE + 'GetClassRooms',
    getDailyLateReport: API_BASE + 'GetDailyLateReport',
    printDailyLatePdf: API_BASE + 'PrintDailyLatePdf'
};

var datatable;
var tableData = [];

$(document).ready(function () {
    $('#reportDate').val(new Date().toISOString().split('T')[0]);

    initTable();

    loadClasses();

    $('#classFilter').change(function () {
        const classId = $(this).val();
        if (classId) {
            loadClassRooms(classId);
            $('#classRoomFilter').prop('disabled', false);
        } else {
            $('#classRoomFilter').html('<option value="">جميع الفصول</option>').prop('disabled', true);
        }
    });

    $('#btnSearch').click(function () {
        loadReport();
    });

    $('#btnPrintPdf').click(function () {
        printOfficialReport();
    });
});

var initTable = function () {
    datatable = $('#dailyLateTable').DataTable({
        responsive: true,
        processing: true,
        serverSide: false,
        data: [],
        dom: "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        buttons: [
            {
                extend: 'excelHtml5',
                text: '<i class="fas fa-file-excel"></i> Excel',
                className: 'btn btn-success me-1 excel-btn',
                title: 'تقرير التأخر اليومي',
                filename: function () {
                    const date = $('#reportDate').val();
                    return 'تقرير_التأخر_اليومي_' + date;
                },
                exportOptions: {
                    columns: ':visible'
                }
            }
        ],
        language: {
            search: "البحث:",
            emptyTable: "لا توجد بيانات",
            loadingRecords: "جارى التحميل ...",
            processing: "جارى التحميل ...",
            lengthMenu: "عرض _MENU_",
            paginate: {
                first: "الأول",
                last: "الأخير",
                next: "التالى",
                previous: "السابق"
            },
            info: "عرض _START_ الى _END_ من _TOTAL_ مدخل",
            infoFiltered: "(البحث من _MAX_ إجمالى المدخلات)",
            infoEmpty: "لا توجد مدخلات للعرض",
            zeroRecords: "لا توجد مدخلات مطابقة للبحث"
        },
        pageLength: 10,
        order: [[0, "asc"]],
        columns: [
            {
                data: null,
                className: 'text-center',
                render: function (data, type, row, meta) {
                    return meta.row + 1;
                }
            },
            {
                data: 'studentName',
                render: function (data) {
                    return data || '-';
                }
            },
            {
                data: 'studentCode',
                className: 'text-center',
                render: function (data) {
                    return data || '-';
                }
            },
            {
                data: 'className',
                render: function (data) {
                    return data || '-';
                }
            },
            {
                data: 'classRoomName',
                render: function (data) {
                    return data || '-';
                }
            },
            {
                data: 'studentPhone',
                className: 'text-center',
                render: function (data) {
                    return data || '-';
                }
            },
            {
                data: 'attendanceTime',
                className: 'text-center',
                render: function (data) {
                    if (!data) return '-';
                    const timeParts = data.split(':');
                    if (timeParts.length >= 2) {
                        const timeStr = timeParts[0] + ':' + timeParts[1];
                        return `<span class="badge badge-light-primary fw-bold">${timeStr}</span>`;
                    }
                    return '-';
                }
            },
            {
                data: 'consecutiveAbsenceDays',
                className: 'text-center',
                render: function (data) {
                    const badgeClass = data >= 3 ? 'badge-danger' : 'badge-warning';
                    return `<span class="badge ${badgeClass}">${data} يوم</span>`;
                }
            },
            {
                data: 'notes',
                className: 'text-center',
                render: function (data) {
                    return data || '-';
                }
            }
        ],
        rowCallback: function (row, data) {
            if (data.consecutiveAbsenceDays >= 3) {
                $(row).addClass('bg-light-danger');
            }
        }
    });

    datatable.buttons().container().appendTo($('#tableCard .card-toolbar'));
};

function loadClasses() {
    $.get(API_URLS.getClasses, function (response) {
        if (response.success) {
            const select = $('#classFilter');
            response.data.forEach(function (item) {
                select.append(`<option value="${item.id}">${item.name}</option>`);
            });
        }
    }).fail(function () {
        console.error('Failed to load classes');
        showError('حدث خطأ في تحميل الصفوف الدراسية');
    });
}

function loadClassRooms(classId) {
    $.get(API_URLS.getClassRooms, { classId: classId }, function (response) {
        const select = $('#classRoomFilter');
        select.html('<option value="">جميع الفصول</option>');

        if (response.success) {
            response.data.forEach(function (item) {
                select.append(`<option value="${item.id}">${item.name}</option>`);
            });
        }
    }).fail(function () {
        console.error('Failed to load classrooms');
        showError('حدث خطأ في تحميل الفصول الدراسية');
    });
}

function loadReport() {
    const date = $('#reportDate').val();
    const classId = $('#classFilter').val() || null;
    const classRoomId = $('#classRoomFilter').val() || null;

    if (!date) {
        Swal.fire('تنبيه', 'الرجاء اختيار التاريخ', 'warning');
        return;
    }

    Swal.fire({
        title: 'جاري تحميل التقرير...',
        allowOutsideClick: false,
        didOpen: () => { Swal.showLoading(); }
    });

    $.get(API_URLS.getDailyLateReport, {
        date: date,
        classId: classId,
        classRoomId: classRoomId
    }, function (response) {
        Swal.close();

        if (response.success) {
            displayReport(response.data);
        } else {
            Swal.fire('خطأ', response.message, 'error');
        }
    }).fail(function () {
        Swal.close();
        Swal.fire('خطأ', 'حدث خطأ أثناء تحميل التقرير', 'error');
    });
}

function displayReport(data) {
    if (!data.classesAbsence || data.classesAbsence.length === 0) {
        $('#emptyState').show();
        $('#tableCard').hide();
        $('#summaryCards').hide();

        datatable.clear().draw();

        Swal.fire('معلومة', 'لا يوجد طلاب متأخرين في هذا اليوم! 🎉', 'info');
        return;
    }

    $('#totalAbsent').text(data.totalAbsentStudents);
    $('#totalClasses').text(data.totalClasses);
    $('#reportDateDisplay').text(new Date(data.reportDate).toLocaleDateString('ar-EG'));

    tableData = [];
    data.classesAbsence.forEach(function (classData) {
        classData.absentStudentsList.forEach(function (student) {
            tableData.push({
                studentName: student.studentName,
                studentCode: student.studentCode,
                className: classData.className,
                classRoomName: classData.classRoomName,
                studentPhone: student.studentPhone,
                attendanceTime: student.attendanceTime,
                consecutiveAbsenceDays: student.consecutiveAbsenceDays,
                notes: student.notes
            });
        });
    });

    datatable.clear().rows.add(tableData).draw();

    $('#emptyState').hide();
    $('#tableCard').show();
    $('#summaryCards').show();
}

function printOfficialReport() {
    const date = $('#reportDate').val();
    const classId = $('#classFilter').val() || '';
    const classRoomId = $('#classRoomFilter').val() || '';

    if (!date) {
        Swal.fire('تنبيه', 'الرجاء اختيار التاريخ', 'warning');
        return;
    }

    Swal.fire({
        title: 'جاري إنشاء التقرير...',
        text: 'الرجاء الانتظار',
        allowOutsideClick: false,
        didOpen: () => { Swal.showLoading(); }
    });

    const xhr = new XMLHttpRequest();
    xhr.open('POST', API_URLS.printDailyLatePdf, true);
    xhr.responseType = 'blob';
    xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');

    xhr.onload = function () {
        Swal.close();

        if (xhr.status === 200) {
            const contentType = xhr.getResponseHeader('content-type');

            if (contentType && contentType.includes('application/json')) {
                const reader = new FileReader();
                reader.onload = function () {
                    try {
                        const error = JSON.parse(reader.result);
                        Swal.fire('خطأ', error.message || 'حدث خطأ أثناء إنشاء التقرير', 'error');
                    } catch (e) {
                        Swal.fire('خطأ', 'حدث خطأ أثناء إنشاء التقرير', 'error');
                    }
                };
                reader.readAsText(xhr.response);
            } else {
                const blob = xhr.response;
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `تقرير_التأخر_اليومي_${date}.pdf`;
                document.body.appendChild(a);
                a.click();

                setTimeout(() => {
                    window.URL.revokeObjectURL(url);
                    document.body.removeChild(a);
                }, 100);

                Swal.fire({
                    icon: 'success',
                    title: 'تم!',
                    text: 'تم إنشاء التقرير بنجاح',
                    timer: 2000,
                    showConfirmButton: false
                });
            }
        } else {
            Swal.fire('خطأ', 'حدث خطأ أثناء إنشاء التقرير: ' + xhr.statusText, 'error');
        }
    };

    xhr.onerror = function () {
        Swal.close();
        Swal.fire('خطأ', 'حدث خطأ في الاتصال بالسيرفر', 'error');
    };

    const formData = `date=${encodeURIComponent(date)}&classId=${classId}&classRoomId=${classRoomId}`;
    xhr.send(formData);
}

function showError(message) {
    Swal.fire({
        icon: 'error',
        title: 'خطأ',
        text: message,
        timer: 3000
    });
}