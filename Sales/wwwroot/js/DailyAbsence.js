    var datatable;
    var tableData = [];

    $(document).ready(function() {
        // Set today's date as default
        $('#reportDate').val(new Date().toISOString().split('T')[0]);

    // Initialize DataTable
    initTable();

    // Load classes
    loadClasses();

    // Class change event
    $('#classFilter').change(function() {
             const classId = $(this).val();
    if (classId) {
        loadClassRooms(classId);
    $('#classRoomFilter').prop('disabled', false);
             } else {
        $('#classRoomFilter').html('<option value="">جميع الفصول</option>').prop('disabled', true);
             }
         });

    // Search button
    $('#btnSearch').click(function() {
        loadReport();
         });

    // Print PDF button
    $('#btnPrintPdf').click(function() {
        printOfficialReport();
         });
     });

    // Initialize DataTable
    var initTable = function () {
        datatable = $('#dailyAbsenceTable').DataTable({
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
                    title: 'تقرير الغياب اليومي',
                    filename: function () {
                        const date = $('#reportDate').val();
                        return 'تقرير_الغياب_اليومي_' + date;
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

    // Add Excel button to toolbar
    datatable.buttons().container().appendTo($('#tableCard .card-toolbar'));
     };

    // Load classes dropdown
    function loadClasses() {
        $.get('@Url.Action("GetClasses", "Reports")', function (response) {
            if (response.success) {
                const select = $('#classFilter');
                response.data.forEach(function (item) {
                    select.append(`<option value="${item.id}">${item.name}</option>`);
                });
            }
        });
     }

    // Load classrooms dropdown
    function loadClassRooms(classId) {
        $.get('@Url.Action("GetClassRooms", "Reports")', { classId: classId }, function (response) {
            const select = $('#classRoomFilter');
            select.html('<option value="">جميع الفصول</option>');

            if (response.success) {
                response.data.forEach(function (item) {
                    select.append(`<option value="${item.id}">${item.name}</option>`);
                });
            }
        });
     }

    // Load report
    function loadReport() {
         const date = $('#reportDate').val();
    const classId = $('#classFilter').val() || null;
    const classRoomId = $('#classRoomFilter').val() || null;

    if (!date) {
        Swal.fire('تنبيه', 'الرجاء اختيار التاريخ', 'warning');
    return;
         }

    // Show loading
    Swal.fire({
        title: 'جاري تحميل التقرير...',
    allowOutsideClick: false,
             didOpen: () => {Swal.showLoading(); }
         });

    $.get('@Url.Action("GetDailyAbsenceReport", "Reports")', {
        date: date,
    classId: classId,
    classRoomId: classRoomId
         }, function(response) {
        Swal.close();

    if (response.success) {
        displayReport(response.data);
             } else {
        Swal.fire('خطأ', response.message, 'error');
             }
         }).fail(function() {
        Swal.close();
    Swal.fire('خطأ', 'حدث خطأ أثناء تحميل التقرير', 'error');
         });
     }

    // Display report
    function displayReport(data) {
         if (!data.classesAbsence || data.classesAbsence.length === 0) {
        $('#emptyState').show();
    $('#tableCard').hide();
    $('#summaryCards').hide();

    // Clear DataTable
    datatable.clear().draw();

    Swal.fire('معلومة', 'لا يوجد طلاب غائبين في هذا اليوم! 🎉', 'info');
    return;
         }

    // Update summary
    $('#totalAbsent').text(data.totalAbsentStudents);
    $('#totalClasses').text(data.totalClasses);
    $('#reportDateDisplay').text(new Date(data.reportDate).toLocaleDateString('ar-EG'));

    // Prepare data for DataTable
    tableData = [];
    data.classesAbsence.forEach(function(classData) {
        classData.absentStudentsList.forEach(function (student) {
            tableData.push({
                studentName: student.studentName,
                studentCode: student.studentCode,
                className: classData.className,
                classRoomName: classData.classRoomName,
                studentPhone: student.studentPhone,
                consecutiveAbsenceDays: student.consecutiveAbsenceDays,
                notes: student.notes
            });
        });
         });

    // Update DataTable
    datatable.clear().rows.add(tableData).draw();

    // Show report
    $('#emptyState').hide();
    $('#tableCard').show();
    $('#summaryCards').show();
     }

    // Print Official PDF Report
    function printOfficialReport() {
         const date = $('#reportDate').val();
    const classId = $('#classFilter').val() || '';
    const classRoomId = $('#classRoomFilter').val() || '';

    if (!date) {
        Swal.fire('تنبيه', 'الرجاء اختيار التاريخ', 'warning');
    return;
         }

    // Show loading
    Swal.fire({
        title: 'جاري إنشاء التقرير...',
    text: 'الرجاء الانتظار',
    allowOutsideClick: false,
             didOpen: () => {Swal.showLoading(); }
         });

    const xhr = new XMLHttpRequest();
    xhr.open('POST', '@Url.Action("PrintDailyAbsencePdf", "Reports")', true);
    xhr.responseType = 'blob';
    xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');

    xhr.onload = function() {
        Swal.close();

    if (xhr.status === 200) {
                 const contentType = xhr.getResponseHeader('content-type');

    if (contentType && contentType.includes('application/json')) {
                     const reader = new FileReader();
    reader.onload = function() {
                         try {
                             const error = JSON.parse(reader.result);
    Swal.fire('خطأ', error.message || 'حدث خطأ أثناء إنشاء التقرير', 'error');
                         } catch(e) {
        Swal.fire('خطأ', 'حدث خطأ أثناء إنشاء التقرير', 'error');
                         }
                     };
    reader.readAsText(xhr.response);
                 } else {
                     const blob = xhr.response;
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `تقرير_الغياب_اليومي_${date}.pdf`;
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

    xhr.onerror = function() {
        Swal.close();
    Swal.fire('خطأ', 'حدث خطأ في الاتصال بالسيرفر', 'error');
         };

    const formData = `date=${encodeURIComponent(date)}&classId=${classId}&classRoomId=${classRoomId}`;
    xhr.send(formData);
     }
