"use strict";
var datatable;
var rowIndex;
var arrayId = [];


var KTList = function () {
    // Define shared variables
    var table = document.getElementById('kt_table');
    var toolbarBase;
    var toolbarSelected;
    var selectedCount;

    var blockUI = new KTBlockUI(table, {
        overlayClass: "bg-danger bg-opacity-25",
        message: '<div class="blockui-message"><span class="spinner-border text-primary"></span>من فضلك إنتظر ...</div>',
    });

    // Private functions
    var initTable = function () {
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            responsive: true,
            processing: true,
            "ajax": {
                "url": "/Vendor_Data/GetVendor"
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
                "info": "عرض _START_ الى _END_ من _TOTAL_ المدخلات",
                "infoFiltered": "(البحث من _MAX_ إجمالى المدخلات)",
                "infoEmpty": "لا توجد مدخلات للعرض",
                "zeroRecords": "لا توجد مدخلات مطابقة للبحث"
            },
            lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
            "pageLength": 100,
            fixedHeader: {
                header: true
            },
            'order': [],
            'columnDefs': [
                { targets: [0, 6, 7, 8], orderable: false, searchable: false, className: "text-center" },
                { targets: [1, 2, 3, 4, 5], className: "text-center" },
                { targets: [5], render: $.fn.dataTable.render.number(',', '.', 2, '') }
            ],
            "columns": [
                { "data": null },
                { "data": "vendorData_Name" },
                { "data": "vendorData_Address" },
                { "data": "vendorData_SenderName" },
                { "data": "vendorData_SenderPhone" },
                { "data": "vendorData_Balance" },
                {
                    "data": "vendorData_ID",
                    "render": function (data) {
                        return `
                                <a href="/Vendor_Data/Detail/${data}" style="cursor:pointer">
                                   <i class="fa-solid fa-magnifying-glass text-primary fs-1"></i>
                                </a>
                           `;
                    }
                },
                {
                    "data": "vendorData_ID",
                    "render": function (data) {
                        return `
                                <a href="/Vendor_Data/Edit/${data}" style="cursor:pointer">
                                   <i class="fa-solid fa-pen-to-square text-success fs-1"></i>
                                </a>
                           `;
                    }
                },
                {
                    "data": "vendorData_ID",
                    "render": function (data) {
                        return `
                                <a onclick=Delete("/Vendor_Data/Delete/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-trash-can text-danger fs-1"></i>
                                </a>
                           `;
                    }
                }
            ],
            "footerCallback": function (row, data, start, end, display) {
                var api = this.api();

                // converting to interger to find total
                var intVal = function (i) {
                    return typeof i === 'string' ?
                        i.replace(/[\$,]/g, '') * 1 :
                        typeof i === 'number' ?
                            i : 0;
                };

                // computing column Total of the complete result 
                var total = api
                    .column(5, { page: 'all', search: 'applied' })
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);

                var format = $.fn.dataTable.render.number(',', '.', 2).display;

                // Update footer by showing the total with the reference of the column index 
                $(api.column(0).footer()).html('');
                $(api.column(1).footer()).html('');
                $(api.column(2).footer()).html('');
                $(api.column(3).footer()).html('');
                $(api.column(4).footer()).html('الإجمالى');
                $(api.column(5).footer()).html(format(total));
                $(api.column(6).footer()).html('');
                $(api.column(7).footer()).html('');
                $(api.column(8).footer()).html('');
            },
        });

        datatable.on('click', 'tr', function () {
            rowIndex = datatable.row(this).index();
        });

        datatable.on('order.dt search.dt', function () {
            datatable.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
                cell.innerHTML = i + 1;
                datatable.cell(cell).invalidate('dom');
            });
        }).draw();
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var handleSearchDatatable = () => {
        const filterSearch = document.querySelector('[data-kt-user-table-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            datatable.search(e.target.value).draw();
        });
    }

    return {
        // Public functions  
        init: function () {
            if (!table) {
                return;
            }

            initTable();
            handleSearchDatatable();
        }
    }
}();

// On document ready
KTUtil.onDOMContentLoaded(function () {
    KTList.init();
});

function Delete(url) {
    Swal.fire({
        title: "هل انت متأكد ؟",
        text: "سيتم حذف المورد",
        icon: "warning",
        showCancelButton: true,
        buttonsStyling: false,
        confirmButtonText: "موافق",
        cancelButtonText: "الغاء",
        customClass: {
            confirmButton: "btn fw-bold btn-danger",
            cancelButton: "btn fw-bold btn-active-light-primary"
        }
    }).then(function (result) {
        if (result.value) {

            axios.post(url)
                .then(function (response) {
                    if (response.data.isValid) {
                        swal.fire({
                            title: response.data.title,
                            text: response.data.message,
                            icon: "success",
                            showConfirmButton: false,
                            timer: 1500
                        }).then(function () {
                            // Remove current row
                            datatable.row(rowIndex).remove().draw();
                        });
                    } else {
                        swal.fire({
                            title: response.data.title,
                            text: response.data.message,
                            icon: "error",
                            buttonsStyling: false,
                            confirmButtonText: "موافق",
                            customClass: {
                                confirmButton: "btn fw-bold btn-light-primary"
                            }
                        });
                    }
                })
                .catch(function (error) {
                    swal.fire({
                        title: "بيانات الموردين",
                        text: "من فضلك تأكد من تسجيل البيانات بطريقة صحيحة",
                        icon: "error",
                        buttonsStyling: false,
                        confirmButtonText: "موافق",
                        customClass: {
                            confirmButton: "btn fw-bold btn-light-primary"
                        }
                    });
                });
        }
    });

    return false;
}