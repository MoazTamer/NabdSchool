        "use strict";
        var MostAbsentReport = function () {
            var table = $('#mostAbsentTable');
            var datatable;

            var initTable = function () {
                datatable = table.DataTable({
                    responsive: true,
                    processing: true,
                    serverSide: false,
                    data: [],
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
                        info: "عرض _START_ الى _END_ من _TOTAL_ طالب",
                        infoFiltered: "(البحث من _MAX_ إجمالى الطلاب)",
                        infoEmpty: "لا توجد طلاب للعرض",
                        zeroRecords: "لا توجد طلاب مطابقة للبحث"
                    },
                    pageLength: 10,
                    order: [[5, "desc"]], // الترتيب حسب أيام الغياب تنازلي
                    columns: [
                        {
                            data: null,
                            render: function (data, type, row, meta) {
                                return meta.row + 1;
                            },
                            className: "text-center"
                        },
                        { data: "studentCode", className: "text-center" },
                        { data: "studentName", className: "text-center" },
                        { data: "className", className: "text-center" },
                        { data: "classRoomName", className: "text-center" },
                        {
                            data: "absentDays",
                            render: function (data) {
                                let className = "high-absence";
                                if (data <= 5) className = "low-absence";
                                else if (data <= 10) className = "medium-absence";

                                return `<span class="${className}">${data} يوم</span>`;
                            },
                            className: "text-center"
                        },
                        {
                            data: "absentPercentage",
                            render: function (data, type, row) {
                                let progressClass = "bg-danger";
                                if (data <= 20) progressClass = "bg-success";
                                else if (data <= 50) progressClass = "bg-warning";

                                return `
                                    <div class="d-flex align-items-center">
                                        <div class="flex-grow-1 me-3">
                                            <div class="progress" style="height: 8px;">
                                                <div class="progress-bar ${progressClass}"
                                                     role="progressbar"
                                                     style="width: ${data}%">
                                                </div>
                                            </div>
                                        </div>
                                        <div class="fw-bold">${data}%</div>
                                    </div>
                                `;
                            },
                            className: "text-center"
                        },
                    ]
                });
            };

            // تحميل قائمة الصفوف
            var loadClasses = function () {
                $.ajax({
                    url: "/Reports/GetClasses", // تحتاج تعمل هذا الـaction
                    type: "GET",
                    success: function (json) {
                        if (json.success) {
                            var classFilter = $('#classFilter');
                            classFilter.empty().append('<option value="">جميع الصفوف</option>');
                            json.data.forEach(function (cls) {
                                classFilter.append(`<option value="${cls.id}">${cls.name}</option>`);
                            });
                        }
                    }
                });
            };

            // تحميل قائمة الفصول بناءً على الصف المحدد
            var loadClassRooms = function (classId) {
                $.ajax({
                    url: "/Reports/GetClassRooms?classId=" + (classId || ''), // تحتاج تعمل هذا الـaction
                    type: "GET",
                    success: function (json) {
                        if (json.success) {
                            var classRoomFilter = $('#classRoomFilter');
                            classRoomFilter.empty().append('<option value="">جميع الفصول</option>');
                            json.data.forEach(function (cr) {
                                classRoomFilter.append(`<option value="${cr.id}">${cr.name}</option>`);
                            });
                        }
                    }
                });
            };

            var loadData = function () {
                const fromDate = $("#fromDate").val();
                const toDate = $("#toDate").val();
                const classId = $("#classFilter").val();
                const classRoomId = $("#classRoomFilter").val();
                const topCount = $("#topCount").val();

                $.ajax({
                    url: "/Reports/GetMostLateStudentsReport",
                    type: "GET",
                    data: {
                        fromDate: fromDate,
                        toDate: toDate,
                        classId: classId,
                        classRoomId: classRoomId,
                        topCount: topCount
                    },
                    success: function (json) {
                        if (json.success) {
                            datatable.clear().rows.add(json.data).draw();
                        } else {
                            Swal.fire({
                                text: json.message,
                                icon: "error",
                                confirmButtonText: "موافق"
                            });
                            datatable.clear().draw();
                        }
                    },
                    error: function () {
                        Swal.fire({
                            text: "❌ حدث خطأ أثناء جلب البيانات",
                            icon: "error",
                            confirmButtonText: "موافق"
                        });
                        datatable.clear().draw();
                    }
                });
            };

       var handleEvents = function () {

                $("#searchBtn").on("click", loadData);
                $("#resetBtn").on("click", function () {
                    $("#fromDate").val('@DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd")');
                    $("#toDate").val('@DateTime.Today.ToString("yyyy-MM-dd")');
                    $("#classFilter").val("");
                    $("#classRoomFilter").val("");
                    $("#topCount").val("10");
                    datatable.clear().draw();
                });

                // عند تغيير الصف، تحديث قائمة الفصول
                $("#classFilter").on("change", function () {
                    loadClassRooms($(this).val());
                });


           // في قسم handleEvents أضف:
           $("#printBtn").on("click", function () {
               const fromDate = $("#fromDate").val();
               const toDate = $("#toDate").val();
               const classId = $("#classFilter").val();
               const classRoomId = $("#classRoomFilter").val();
               const topCount = $("#topCount").val();

               const form = $('<form>', {
                   action: '/Reports/PrintMostLateStudentsPdf',
                   method: 'post',
                   target: '_blank'
               });

               form.append($('<input>', { type: 'hidden', name: 'fromDate', value: fromDate }));
               form.append($('<input>', { type: 'hidden', name: 'toDate', value: toDate }));
               form.append($('<input>', { type: 'hidden', name: 'classId', value: classId || '' }));
               form.append($('<input>', { type: 'hidden', name: 'classRoomId', value: classRoomId || '' }));
               form.append($('<input>', { type: 'hidden', name: 'topCount', value: topCount }));

               $('body').append(form);
               form.submit();
               form.remove();
           });
            };

            return {
                init: function () {
                    initTable();
                    loadClasses();
                    loadClassRooms();
                    handleEvents();
                    loadData(); 
                }
            };
        }();

        $(document).ready(function () {
            MostAbsentReport.init();
        });