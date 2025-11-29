var DailyEarlyExitReport = function () {
    var table;
    var datatable;


    var initTable = function () {
        table = $('#earlyExitTable');

        datatable = table.DataTable({
            responsive: true,
            processing: true,
            serverSide: false,
            data: [],
            dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
                '<"row"<"col-sm-12"tr>>' +
                '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>' +
                '<"row"<"col-sm-12"B>>',
            buttons: [
                {
                    extend: 'excelHtml5',
                    text: '<i class="fas fa-file-excel me-2"></i> تصدير Excel',
                    className: 'btn btn-success me-2 excel-btn',
                    title: 'تقرير الخروج المبكر',
                    filename: function () {
                        var date = $("#dateFilter").val();
                        return 'تقرير_الخروج_المبكر_' + date;
                    },
                    exportOptions: {
                        columns: ':visible'
                    }
                },
               
            ],
            language: {
                search: "البحث:",
                searchPlaceholder: "ابحث في التقرير...",
                emptyTable: "لا توجد بيانات للعرض",
                loadingRecords: "جارٍ التحميل...",
                processing: "جارٍ المعالجة...",
                lengthMenu: "عرض _MENU_ سجل",
                paginate: {
                    first: "الأول",
                    last: "الأخير",
                    next: "التالى",
                    previous: "السابق"
                },
                info: "عرض _START_ إلى _END_ من _TOTAL_ سجل",
                infoFiltered: "(تمت تصفية من _MAX_ إجمالي السجلات)",
                infoEmpty: "لا توجد سجلات للعرض",
                zeroRecords: "لا توجد سجلات مطابقة للبحث"
            },
            pageLength: 10,
            lengthMenu: [[5, 10, 25, 50, -1], [5, 10, 25, 50, "الكل"]],
            order: [[1, "asc"]],
            columns: [
                {
                    data: null,
                    className: "text-center fw-bold",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        return meta.row + 1;
                    }
                },
                {
                    data: "studentName",
                    title: "اسم الطالبة"
                },
                {
                    data: "studentCode",
                    className: "text-center",
                    title: "ةكود الطالب"
                },
                {
                    data: "studentPhone",
                    className: "text-center",
                    title: "جوال الطالبة"
                },
                {
                    data: "exitTime",
                    className: "text-center fw-bold",
                    title: "وقت الخروج",
                    render: function (data) {
                        if (!data) return '<span class="text-muted">-</span>';
                        var parts = data.split(':');
                        if (parts.length >= 2) {
                            var h = parseInt(parts[0]);
                            var m = parts[1];
                            var ampm = h >= 12 ? 'م' : 'ص';
                            var displayH = h % 12 || 12;
                            return `<span class="badge bg-danger text-white">${displayH}:${m} ${ampm}</span>`;
                        }
                        return data;
                    }
                },
                {
                    data: "className",
                    className: "text-center",
                    title: "الصف"
                },
                {
                    data: "classRoomName",
                    className: "text-center",
                    title: "الفصل"
                }
            ],
            initComplete: function () {
                $('.dataTables_filter input').addClass('form-control form-control-solid');
                $('.dataTables_length select').addClass('form-select form-select-solid');
            }
        });

        datatable.buttons().container().appendTo('#exportButtonsContainer');
    };

    // ============================
    var loadData = function () {
        const classId = $("#classFilter").val();
        const classRoomId = $("#classRoomFilter").val();
        const date = $("#dateFilter").val();


        $.ajax({
            url: "/Reports/GetDailyEarlyExitReport",
            type: "GET",
            data: { classId, classRoomId, date },
            beforeSend: function () {
                $('#searchBtn').prop('disabled', true).html('<i class="ki-outline ki-loading fs-2 spinner"></i> جاري التحميل...');
            },
            success: function (json) {

                $('#emptyState').hide();
                $('#reportTableCard').show();
                $('#exportButtons').show();
                $('#summaryCards').show();

                $('#searchBtn').prop('disabled', false).html('<i class="ki-outline ki-magnifier fs-2"></i> عرض التقرير');

                console.log("Response JSON:", json); 

                if (json.success) {
                    var allStudents = [];
                    var totalEarlyExit = 0;
                    var totalClasses = 0;

                    if (json.data && json.data.classesReport) {
                        totalClasses = json.data.classesReport.length;

                        json.data.classesReport.forEach(function (classData) {
                            if (classData.earlyExitStudentsList && classData.earlyExitStudentsList.length > 0) {
                                totalEarlyExit += classData.earlyExitStudentsList.length;

                                classData.earlyExitStudentsList.forEach(function (student) {
                                    allStudents.push({
                                        ...student,
                                        className: classData.className || 'غير محدد',
                                        classRoomName: classData.classRoomName || 'غير محدد'
                                    });
                                });
                            }
                        });
                    }

                    $('#totalEarlyExit').text(totalEarlyExit);
                    $('#totalClasses').text(totalClasses);

                    if (json.data && json.data.reportDate) {
                        $('#reportDateDisplay').text(new Date(json.data.reportDate).toLocaleDateString('ar-EG'));
                    } else {
                        $('#reportDateDisplay').text(new Date().toLocaleDateString('ar-EG'));
                    }

                    datatable.clear().rows.add(allStudents).draw();

                    if (allStudents.length === 0) {
                        $('#emptyState').show();
                        $('#reportTableCard').hide();
                        $('#exportButtons').hide();
                        $('#summaryCards').hide();
                        Swal.fire({
                            text: "⚠️ لا توجد بيانات للعرض في التاريخ المحدد",
                            icon: "info",
                            confirmButtonText: "حسناً"
                        });
                    } else {
                        $('#emptyState').hide();
                        $('#reportTableCard').show();
                        $('#exportButtons').show();
                        $('#summaryCards').show();

                        Swal.fire({
                            text: `✅ تم تحميل ${allStudents.length} سجل بنجاح`,
                            icon: "success",
                            timer: 2000,
                            showConfirmButton: false
                        });
                    }
                } else {
                    Swal.fire({
                        text: json.message || "حدث خطأ غير معروف",
                        icon: "error",
                        confirmButtonText: "حسناً"
                    });
                    datatable.clear().draw();
                    $('#emptyState').show();
                    $('#reportTableCard').hide();
                    $('#exportButtons').hide();
                    $('#summaryCards').hide();
                }
            },
            error: function (xhr, status, error) {
                $('#searchBtn').prop('disabled', false).html('<i class="ki-outline ki-magnifier fs-2"></i> عرض التقرير');

                console.error("AJAX Error:", error, xhr.responseText);

                Swal.fire({
                    text: "❌ حدث خطأ أثناء جلب البيانات: " + error,
                    icon: "error",
                    confirmButtonText: "حسناً"
                });
                datatable.clear().draw();
                $('#emptyState').show();
                $('#reportTableCard').hide();
                $('#exportButtons').hide();
                $('#summaryCards').hide();
            }
        });


    };

    var handleEvents = function () {
        $("#searchBtn").on("click", loadData);

        $("#printBtn").on("click", function () {
            const date = $("#dateFilter").val();
            const classId = $("#classFilter").val();
            const classRoomId = $("#classRoomFilter").val();

            if (datatable.rows().count() === 0) {
                Swal.fire({
                    text: "لا توجد بيانات لطباعة التقرير",
                    icon: "warning",
                    confirmButtonText: "حسناً"
                });
                return;
            }

            const form = $('<form>', {
                action: '/Reports/PrintDailyEarlyExitPdf',
                method: 'post',
                target: '_blank'
            }).appendTo('body');

            form.append(`<input type="hidden" name="date" value="${date}">`);
            form.append(`<input type="hidden" name="classId" value="${classId || ''}">`);
            form.append(`<input type="hidden" name="classRoomId" value="${classRoomId || ''}">`);

            form.submit();
            form.remove();
        });

        $("#previewBtn").on("click", function () {
            if (datatable.rows().count() > 0) {
                Swal.fire({
                    title: 'معاينة التقرير',
                    html: `تم تحميل <strong>${datatable.rows().count()}</strong> سجل للطلاب المستأذنين`,
                    icon: 'info',
                    confirmButtonText: 'حسناً'
                });
            } else {
                Swal.fire({
                    text: 'لا توجد بيانات للمعاينة',
                    icon: 'warning',
                    confirmButtonText: 'حسناً'
                });
            }
        });

        $.get("/Reports/GetClasses", function (data) {
            if (data.success) {
                data.data.forEach(cls =>
                    $("#classFilter").append(`<option value="${cls.id}">${cls.name}</option>`)
                );
            } else {
                console.error("Error loading classes:", data.message);
            }
        }).fail(function (xhr, status, error) {
            console.error("Failed to load classes:", error);
        });

        $("#classFilter").on("change", function () {
            var classId = $(this).val();
            var classRoomFilter = $("#classRoomFilter");

            classRoomFilter.html('<option value="">جميع الفصول</option>');

            if (classId) {
                classRoomFilter.prop('disabled', false);
                $.get("/Reports/GetClassRooms?classId=" + classId, function (data) {
                    if (data.success) {
                        data.data.forEach(room =>
                            classRoomFilter.append(`<option value="${room.id}">${room.name}</option>`)
                        );
                    } else {
                        console.error("Error loading classrooms:", data.message);
                    }
                }).fail(function (xhr, status, error) {
                    console.error("Failed to load classrooms:", error);
                });
            } else {
                classRoomFilter.prop('disabled', true);
            }
        });

        $("#dateFilter").on('keypress', function (e) {
            if (e.which === 13) {
                loadData();
            }
        });
    };

    return {
        init: function () {
            initTable();
            handleEvents();
        }
    };
}();

$(document).ready(function () {
    DailyEarlyExitReport.init();
});