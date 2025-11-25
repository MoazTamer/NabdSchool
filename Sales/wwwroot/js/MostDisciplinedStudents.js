var MostDisciplinedStudentsReport = function () {
    var table = $('#disciplinedStudentsTable');
    var datatable;

    var initTable = function () {
        datatable = table.DataTable({
            responsive: true,
            processing: true,
            serverSide: false,
            data: [],
            buttons: [
                {
                    extend: 'excelHtml5',
                    text: '<i class="fas fa-file-excel"></i> Excel',
                    className: 'btn btn-success me-1 excel-btn',
                    title: 'تقرير الطلاب الأكثر انضباطاً',
                    filename: function () {
                        return 'تقرير_الطلاب_الأكثر_انضباطاً';
                    },
                    exportOptions: {
                        columns: ':visible' // يصدر كل الأعمدة الظاهرة
                    }
                }
            ],
            language: {
                search: "البحث ",
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
                { data: "rank", className: "text-center fw-bold" },
                { data: "studentName" },
                { data: "studentCode", className: "text-center" },
                { data: "className", className: "text-center" },
                { data: "classRoomName", className: "text-center" },
                {
                    data: "presentDays",
                    className: "text-center fw-bold",
                    render: function (data) {
                        return `<span class="text-success">${data}</span>`;
                    }
                },
                {
                    data: "lateDays",
                    className: "text-center",
                    render: function (data) {
                        return `<span class="text-warning">${data}</span>`;
                    }
                },
                {
                    data: "absentDays",
                    className: "text-center",
                    render: function (data) {
                        return `<span class="text-danger">${data}</span>`;
                    }
                },
                {
                    data: "disciplinePercentage",
                    className: "text-center fw-bold",
                    render: function (data) {
                        var color = data >= 90 ? 'success' : data >= 80 ? 'info' : data >= 70 ? 'warning' : 'danger';
                        return `<span class="text-${color}">${data}%</span>`;
                    }
                }
            ]
        });
        datatable.buttons().container().appendTo('.filter-section .col-md-12');

    };

    var loadData = function () {
        const classId = $("#classFilter").val();
        const classRoomId = $("#classRoomFilter").val();
        const fromDate = $("#fromDateFilter").val();
        const toDate = $("#toDateFilter").val();
        const topCount = $("#topCountFilter").val();

        $.ajax({
            url: "/Reports/GetMostDisciplinedStudentsReport",
            type: "GET",
            data: {
                classId: classId,
                classRoomId: classRoomId,
                fromDate: fromDate,
                toDate: toDate,
                topCount: topCount
            },
            success: function (json) {
                if (json.success) {
                    // إضافة الترتيب للبيانات
                    var dataWithRank = json.data.students.map((student, index) => {
                        return {
                            ...student,
                            rank: index + 1
                        };
                    });

                    datatable.clear().rows.add(dataWithRank).draw();
                } else {
                    Swal.fire({ text: json.message, icon: "error", confirmButtonText: "موافق" });
                    datatable.clear().draw();
                }
            },
            error: function () {
                Swal.fire({ text: "❌ حدث خطأ أثناء جلب البيانات", icon: "error", confirmButtonText: "موافق" });
                datatable.clear().draw();
            }
        });
    };

    var handleEvents = function () {
        $("#searchBtn").on("click", loadData);

        $("#printBtn").on("click", function () {
            const classId = $("#classFilter").val();
            const classRoomId = $("#classRoomFilter").val();
            const fromDate = $("#fromDateFilter").val();
            const toDate = $("#toDateFilter").val();
            const topCount = $("#topCountFilter").val();

            const form = $('<form>', {
                action: '/Reports/PrintMostDisciplinedStudentsPdf',
                method: 'post',
                target: '_blank'
            });

            form.append($('<input>', { type: 'hidden', name: 'fromDate', value: fromDate }));
            form.append($('<input>', { type: 'hidden', name: 'toDate', value: toDate }));
            form.append($('<input>', { type: 'hidden', name: 'classId', value: classId }));
            form.append($('<input>', { type: 'hidden', name: 'classRoomId', value: classRoomId }));
            form.append($('<input>', { type: 'hidden', name: 'topCount', value: topCount }));

            $('body').append(form);
            form.submit();
            form.remove();
        });

        $("#resetBtn").on("click", function () {
            $("#classFilter").val("");
            $("#classRoomFilter").val("");
            $("#fromDateFilter").val('@DateTime.Today.ToString("yyyy-MM-dd")');
            $("#toDateFilter").val('@DateTime.Today.ToString("yyyy-MM-dd")');
            $("#topCountFilter").val("10");
            datatable.clear().draw();
        });

        // تحميل الصفوف
        $.get("/Reports/GetClasses", function (data) {
            if (data.success) {
                data.data.forEach(function (cls) {
                    $("#classFilter").append(`<option value="${cls.id}">${cls.name}</option>`);
                });
            }
        });

        // عند تغيير الصف
        $("#classFilter").on("change", function () {
            var classId = $(this).val();
            $("#classRoomFilter").html('<option value="">جميع الفصول</option>');

            if (classId) {
                $.get("/Reports/GetClassRooms?classId=" + classId, function (data) {
                    if (data.success) {
                        data.data.forEach(function (room) {
                            $("#classRoomFilter").append(`<option value="${room.id}">${room.name}</option>`);
                        });
                    }
                });
            }
        });
    };

    return {
        init: function () {
            initTable();
            handleEvents();
            loadData(); // تحميل البيانات تلقائياً عند فتح الصفحة
        }
    };
}();

$(document).ready(function () {
    MostDisciplinedStudentsReport.init();
});