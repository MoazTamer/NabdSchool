var MostDisciplinedClassesReport = function () {
    var table = $('#disciplinedClassesTable');
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
                    title: 'تقرير الفصول الأكثر انضباطاً',
                    filename: function () {
                        return 'تقرير_الفصول_الأكثر_انضباطاً';
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
                { data: "className", className: "text-center" },
                { data: "classRoomName", className: "text-center" },
                { data: "totalStudents", className: "text-center fw-bold" },
                //{
                //    data: "totalPresentDays",
                //    className: "text-center",
                //    render: function (data) {
                //        return `<span class="text-success fw-bold">${data}</span>`;
                //    }
                //},
                //{
                //    data: "totalLateDays",
                //    className: "text-center",
                //    render: function (data) {
                //        return `<span class="text-warning">${data}</span>`;
                //    }
                //},
                //{
                //    data: "totalAbsentDays",
                //    className: "text-center",
                //    render: function (data) {
                //        return `<span class="text-danger">${data}</span>`;
                //    }
                //},
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

        datatable.buttons().container().appendTo('.buttons');

    };

    var loadData = function () {
        const classId = $("#classFilter").val();
        const fromDate = $("#fromDateFilter").val();
        const toDate = $("#toDateFilter").val();

        $.ajax({
            url: "/Reports/GetMostDisciplinedClassesReport",
            type: "GET",
            data: {
                classId: classId,
                fromDate: fromDate,
                toDate: toDate
            },
            success: function (json) {
                if (json.success) {
                    datatable.clear().rows.add(json.data.classes).draw();
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
            const fromDate = $("#fromDateFilter").val();
            const toDate = $("#toDateFilter").val();

            const form = $('<form>', {
                action: '/Reports/PrintMostDisciplinedClassesPdf',
                method: 'post',
                target: '_blank'
            });

            form.append($('<input>', { type: 'hidden', name: 'fromDate', value: fromDate }));
            form.append($('<input>', { type: 'hidden', name: 'toDate', value: toDate }));
            form.append($('<input>', { type: 'hidden', name: 'classId', value: classId }));

            $('body').append(form);
            form.submit();
            form.remove();
        });

        // تحميل الصفوف
        $.get("/Reports/GetClasses", function (data) {
            if (data.success) {
                data.data.forEach(function (cls) {
                    $("#classFilter").append(`<option value="${cls.id}">${cls.name}</option>`);
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
    MostDisciplinedClassesReport.init();
});