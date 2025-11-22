var StudentAttendanceReport = function () {

    var table = $('#studentAttendanceTable');
    var datatable;

    var initTable = function () {
        datatable = table.DataTable({
            responsive: true,
            processing: true,
            serverSide: false,
            data: [], // البداية فاضية
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
            order: [[0, "desc"]],
            columns: [
                { data: "date", render: d => d ? new Date(d).toLocaleDateString("ar-EG") : "-", className: "text-center" },
                {
                    data: "status",
                    render: function (data) {
                        let className = "", icon = "";
                        if (data === "حضور") { className = "status-present"; icon = '<i class="fas fa-check-circle"></i> '; }
                        else if (data === "متأخر") { className = "status-late"; icon = '<i class="fas fa-clock"></i> '; }
                        else if (data === "غياب") { className = "status-absent"; icon = '<i class="fas fa-times-circle"></i> '; }
                        else { className = "status-nodata"; icon = '<i class="fas fa-minus-circle"></i> '; }
                        return `<span class="${className}">${icon}${data}</span>`;
                    },
                    className: "text-center"
                },
                { data: "time", render: d => d || "-", className: "text-center" },
                { data: "notes", render: d => d || "-", className: "text-center" }
            ]
        });
    };

    // تحميل البيانات بالـ AJAX
    var loadData = function () {
        const studentCode = $("#studentCodeFilter").val().trim();
        const startDate = $("#startDateFilter").val();
        const endDate = $("#endDateFilter").val();

        if (!studentCode) {
            Swal.fire({ text: "⚠️ يرجى إدخال كود الطالب", icon: "warning", confirmButtonText: "موافق" });
            return;
        }

        if (!startDate || !endDate) {
            Swal.fire({ text: "⚠️ يرجى إدخال تاريخ البداية والنهاية", icon: "warning", confirmButtonText: "موافق" });
            return;
        }

        if (new Date(startDate) > new Date(endDate)) {
            Swal.fire({ text: "⚠️ تاريخ البداية يجب أن يكون قبل تاريخ النهاية", icon: "warning", confirmButtonText: "موافق" });
            return;
        }

        $.ajax({
            url: "/Reports/GetStudentAttendanceReport",
            type: "GET",
            data: {
                studentCode: studentCode,
                fromDate: startDate,
                date: endDate
            },
            success: function (json) {
                if (json.success) {
                    // ترتيب الأيام من الأحدث للأقدم
                    json.data.days.sort((a, b) => new Date(b.date) - new Date(a.date));

                    // تحديث الجدول
                    datatable.clear().rows.add(json.data.days).draw();

                    // تحديث الإحصائيات
                    updateStatistics(json.data);
                } else {
                    Swal.fire({ text: json.message, icon: "error", confirmButtonText: "موافق" });
                    datatable.clear().draw();
                    $("#statisticsSection").hide();
                }
            },
            error: function () {
                Swal.fire({ text: "❌ حدث خطأ أثناء جلب البيانات", icon: "error", confirmButtonText: "موافق" });
                datatable.clear().draw();
                $("#statisticsSection").hide();
            }
        });
    };

    var updateStatistics = function (data) {
        $("#totalPresent").text(data.totalPresent || 0);
        $("#totalLate").text(data.totalLate || 0);
        $("#totalAbsent").text(data.totalAbsent || 0);
        $("#consecutiveLate").text(data.consecutiveLate || 0);
        $("#consecutiveAbsent").text(data.consecutiveAbsent || 0);

        // حساب إجمالي الأيام
        const totalDays = (data.totalPresent || 0) + (data.totalLate || 0) + (data.totalAbsent || 0);
        $("#totalDays").text(totalDays);

        $("#statisticsSection").show();
    };

    var handleEvents = function () {
        $("#searchBtn").on("click", loadData);

        // في قسم handleEvents أضف:
        $("#printBtn").on("click", function () {
            const studentCode = $("#studentCodeFilter").val().trim();
            const startDate = $("#startDateFilter").val();
            const endDate = $("#endDateFilter").val();

            if (!studentCode) {
                Swal.fire({ text: "⚠️ يرجى إدخال كود الطالب", icon: "warning", confirmButtonText: "موافق" });
                return;
            }

            if (!startDate || !endDate) {
                Swal.fire({ text: "⚠️ يرجى إدخال تاريخ البداية والنهاية", icon: "warning", confirmButtonText: "موافق" });
                return;
            }

            if (new Date(startDate) > new Date(endDate)) {
                Swal.fire({ text: "⚠️ تاريخ البداية يجب أن يكون قبل تاريخ النهاية", icon: "warning", confirmButtonText: "موافق" });
                return;
            }

            const form = $('<form>', {
                action: '/Reports/PrintStudentAttendancePdf',
                method: 'post',
                target: '_blank'
            });

            form.append($('<input>', { type: 'hidden', name: 'studentCode', value: studentCode }));
            form.append($('<input>', { type: 'hidden', name: 'fromDate', value: startDate }));
            form.append($('<input>', { type: 'hidden', name: 'date', value: endDate }));

            $('body').append(form);
            form.submit();
            form.remove();
        });

        $("#resetBtn").on("click", function () {
            $("#studentCodeFilter").val("");
            $("#startDateFilter").val('@DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd")');
            $("#endDateFilter").val('@DateTime.Today.ToString("yyyy-MM-dd")');
            datatable.clear().draw();
            $("#statisticsSection").hide();
        });

        $("#studentCodeFilter").on("keypress", function (e) {
            if (e.which === 13) loadData();
        });
    };

    return {
        init: function () { initTable(); handleEvents(); },
        loadData: loadData
    };

}();

$(document).ready(function () {
    const studentCode = $("#studentCodeFilter").val();
    const startDate = $("#startDateFilter").val();
    const endDate = $("#endDateFilter").val();

    console.log("Document ready. Checking for pre-fill values...");
    console.log("Pre-filling filters:", { studentCode });

    if (studentCode) {
        $("#studentCodeFilter").val(studentCode);
        $("#startDateFilter").val(startDate);
        $("#endDateFilter").val(endDate);
        StudentAttendanceReport.init(); 
        StudentAttendanceReport.loadData(); 
    } else {
        StudentAttendanceReport.init();
    }
});
