var KTDailyAbsenceReport = function () {
    // Shared variables
    var table;
    var datatable;

    // Private functions
    var initFilters = function () {
        // Set today's date as default
        $('#reportDate').val(new Date().toISOString().split('T')[0]);

        // Load classes
        loadClasses();

        // Class change event
        $('#classFilter').change(function () {
            const classId = $(this).val();
            if (classId) {
                loadClassRooms(classId);
                $('#classRoomFilter').prop('disabled', false);
            } else {
                $('#classRoomFilter').html('<option value="">جميع الفصول</option>').prop('disabled', true);
            }
        });

        // Search button
        $('#btnSearch').click(function () {
            loadReport();
        });

        // Print button
        $('#btnPrint').click(function () {
            window.print();
        });

        // Print PDF button
        $('#btnPrintPdf').click(function () {
            printOfficialReport();
        });

        // Export Excel button
        $('#btnExportExcel').click(function () {
            exportToExcel();
        });
    };

    var initDatatable = function () {
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        table = document.getElementById('kt_absence_table');

        if (!table) {
            console.error('Table element not found');
            return;
        }

        datatable = $(table).DataTable({
            responsive: true,
            processing: true,
            serverSide: true,
            "ajax": {
                "url": "/Reports/GetDailyAbsenceReportData",
                "type": "GET",
                "data": function (d) {
                    d.date = $('#reportDate').val();
                    d.classId = $('#classFilter').val() || null;
                    d.classRoomId = $('#classRoomFilter').val() || null;
                }
            },
            "language": {
                "search": "البحث ",
                "emptyTable": "لا توجد بيانات",
                "loadingRecords": "جارى التحميل ...",
                "processing": "جارى التحميل ...",
                "lengthMenu": "عرض _MENU_",
                "paginate": {
                    "first": "الأول",
                    "last": "الأخير",
                    "next": "التالى",
                    "previous": "السابق"
                },
                "info": "عرض _START_ الى _END_ من _TOTAL_ طالب غائب",
                "infoFiltered": "(البحث من _MAX_ إجمالى الطلاب)",
                "infoEmpty": "لا توجد طلاب غائبين للعرض",
                "zeroRecords": "لا توجد طلاب غائبين مطابقة للبحث"
            },
            lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "الكل"]],
            "pageLength": 25,
            fixedHeader: {
                header: true
            },
            'order': [[1, 'asc']],
            'columnDefs': [
                {
                    targets: [0, 2, 3, 4, 5],
                    orderable: false,
                    searchable: false,
                    className: "text-center"
                },
                {
                    targets: [1],
                    className: "text-start"
                }
            ],
            "columns": [
                {
                    "data": null,
                    "render": function (data, type, row, meta) {
                        return meta.row + 1;
                    }
                },
                {
                    "data": "studentName",
                    "render": function (data, type, row) {
                        return `<span class="fw-bold text-gray-800">${data}</span>`;
                    }
                },
                {
                    "data": "className",
                    "render": function (data, type, row) {
                        return `<span class="badge badge-light-primary">${data}</span>`;
                    }
                },
                {
                    "data": "classRoomName",
                    "render": function (data, type, row) {
                        return `<span class="badge badge-light-info">${data}</span>`;
                    }
                },
                {
                    "data": "consecutiveAbsenceDays",
                    "render": function (data, type, row) {
                        const badgeClass = data >= 3 ? 'badge-danger' : 'badge-warning';
                        return `<span class="badge ${badgeClass}">${data} يوم</span>`;
                    }
                },
                {
                    "data": "studentPhone",
                    "render": function (data, type, row) {
                        return data ? `<span class="text-gray-600">${data}</span>` : '-';
                    }
                },
                {
                    "data": "studentCode",
                    "render": function (data, type, row) {
                        return data ? `<span class="text-gray-700">${data}</span>` : '-';
                    }
                },
                {
                    "data": "notes",
                    "render": function (data, type, row) {
                        return data ? `<span class="text-muted">${data}</span>` : '-';
                    }
                }
            ],
            "drawCallback": function (settings) {
                // Re-init functions on every table re-draw
                var api = this.api();
                api.column(0, { page: 'current' }).nodes().each(function (cell, i) {
                    cell.innerHTML = i + 1;
                });
            }
        });

        // Handle row click
        datatable.on('click', 'tr', function () {
            $(this).toggleClass('selected');
        });
    };

    // Load classes dropdown
    var loadClasses = function () {
        $.get('/Reports/GetClasses', function (response) {
            if (response.success) {
                const select = $('#classFilter');
                select.html('<option value="">جميع الصفوف</option>');
                response.data.forEach(function (item) {
                    select.append(`<option value="${item.id}">${item.name}</option>`);
                });
            }
        }).fail(function () {
            console.error('Failed to load classes');
        });
    };

    // Load classrooms dropdown
    var loadClassRooms = function (classId) {
        $.get('/Reports/GetClassRooms', { classId: classId }, function (response) {
            const select = $('#classRoomFilter');
            select.html('<option value="">جميع الفصول</option>');

            if (response.success) {
                response.data.forEach(function (item) {
                    select.append(`<option value="${item.id}">${item.name}</option>`);
                });
            }
        }).fail(function () {
            console.error('Failed to load classrooms');
        });
    };

    // Load report
    var loadReport = function () {
        const date = $('#reportDate').val();

        if (!date) {
            Swal.fire('تنبيه', 'الرجاء اختيار التاريخ', 'warning');
            return;
        }

        // Show loading
        KTApp.showLoading('#reportContainer');

        // Reload datatable with new parameters
        if (datatable) {
            datatable.ajax.reload(function (json) {
                KTApp.hideLoading('#reportContainer');
                updateSummary(json.summary);
            }, false);
        }
    };

    // Update summary cards
    var updateSummary = function (summary) {
        if (summary) {
            $('#totalAbsent').text(summary.totalAbsentStudents || 0);
            $('#totalClasses').text(summary.totalClasses || 0);
            $('#reportDateDisplay').text(summary.reportDate ?
                new Date(summary.reportDate).toLocaleDateString('ar-EG') : '--/--/----');

            $('#summaryCards').show();
            $('#exportButtons').show();
        } else {
            $('#summaryCards').hide();
            $('#exportButtons').hide();
        }
    };

    // Export to Excel
    var exportToExcel = function () {
        const date = $('#reportDate').val();
        const classId = $('#classFilter').val() || null;
        const classRoomId = $('#classRoomFilter').val() || null;

        window.location.href = `/Reports/ExportDailyAbsenceToExcel?date=${date}&classId=${classId}&classRoomId=${classRoomId}`;
    };

    // Print Official PDF Report
    var printOfficialReport = function () {
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
            didOpen: () => { Swal.showLoading(); }
        });

        // Using XMLHttpRequest for better error handling
        const xhr = new XMLHttpRequest();
        xhr.open('POST', '/Reports/PrintDailyAbsencePdf', true);
        xhr.responseType = 'blob';
        xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');

        xhr.onload = function () {
            Swal.close();

            if (xhr.status === 200) {
                const contentType = xhr.getResponseHeader('content-type');

                if (contentType && contentType.includes('application/json')) {
                    // Handle JSON error response
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
                    // Success - download PDF
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
                Swal.fire('خطأ', 'حدث خطأ أثناء إنشاء التقرير', 'error');
            }
        };

        xhr.onerror = function () {
            Swal.close();
            Swal.fire('خطأ', 'حدث خطأ في الاتصال بالسيرفر', 'error');
        };

        const formData = `date=${encodeURIComponent(date)}&classId=${classId}&classRoomId=${classRoomId}`;
        xhr.send(formData);
    };

    // Public methods
    return {
        init: function () {
            initFilters();
            initDatatable();
        },

        reload: function () {
            if (datatable) {
                datatable.ajax.reload();
            }
        },

        getSelectedRows: function () {
            if (datatable) {
                return datatable.rows('.selected').data().toArray();
            }
            return [];
        }
    };
}();

// Initialize on document ready
if (typeof jQuery !== 'undefined') {
    jQuery(document).ready(function () {
        KTDailyAbsenceReport.init();
    });
}