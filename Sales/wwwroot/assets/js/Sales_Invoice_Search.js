"use strict";
var datatable;
var rowIndex;

var KTList = function () {
    // Define shared variables
    var table = document.getElementById('kt_table');

    // Private functions
    var initTable = function () {
        var FromDate = $("#FromDate").val();
        var ToDate = $("#ToDate").val();
        var BranchID = $("#Branch_ID").val();
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            responsive: true,
            processing: true,
            "ajax": {
                "url": "/Sales/GetInvoice?BranchID=" + BranchID + "&FromDate=" + FromDate + "&ToDate=" + ToDate
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
                { targets: [0, 6, 7, 8, 9, 10, 11, 13], orderable: false, searchable: false, className: "text-center" },
                { targets: [1, 2, 3, 4], className: "text-center" },
                { targets: [6, 7, 8, 9, 10, 11], render: $.fn.dataTable.render.number(',', '.', 2) }
            ],
            "columns": [
                { "data": null },
                {
                    "data": "salesInvoice_Date",
                    render: function (data, type, row) {
                        return moment(data).format('YYYY/MM/DD');
                    }
                },
                { "data": "time" },
                { "data": "salesInvoice_BillType" },
                { "data": "salesInvoice_Number" },
                { "data": "customerData_Name" },
                { "data": "salesInvoice_Total" },
                { "data": "salesInvoice_VatMoney" },
                { "data": "salesInvoice_TotalAfterVat" },
                { "data": "salesInvoice_PayCash" },
                { "data": "salesInvoice_PayBank" },
                { "data": "salesInvoice_CustomerRest" },
                { "data": "userName" },
                {
                    "data": "salesInvoice_ID",
                    "render": function (data) {
                        return `
                                <a onclick=Detail("/Sales/Detail/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-magnifying-glass text-primary fs-1"></i>
                                </a>
                           `;
                    }
                },
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

                var totalSale = 0;
                var vatSale = 0;
                var afterVatSale = 0;
                var cashSale = 0;
                var bankSale = 0;
                var restSale = 0;

                var totalBack = 0;
                var vatBack = 0;
                var afterVatBack = 0;
                var cashBack = 0;
                var bankBack = 0;
                var restBack = 0;

                totalSale = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مبيعات نقداً' || data['salesInvoice_BillType'] === 'فاتورة مبيعات أجل') ? intVal(total) + intVal(data['salesInvoice_Total']) : total;
                    }, 0);

                vatSale = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مبيعات نقداً' || data['salesInvoice_BillType'] === 'فاتورة مبيعات أجل') ? intVal(total) + intVal(data['salesInvoice_VatMoney']) : total;
                    }, 0);

                afterVatSale = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مبيعات نقداً' || data['salesInvoice_BillType'] === 'فاتورة مبيعات أجل') ? intVal(total) + intVal(data['salesInvoice_TotalAfterVat']) : total;
                    }, 0);

                cashSale = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مبيعات نقداً' || data['salesInvoice_BillType'] === 'فاتورة مبيعات أجل') ? intVal(total) + intVal(data['salesInvoice_PayCash']) : total;
                    }, 0);

                bankSale = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مبيعات نقداً' || data['salesInvoice_BillType'] === 'فاتورة مبيعات أجل') ? intVal(total) + intVal(data['salesInvoice_PayBank']) : total;
                    }, 0);

                restSale = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مبيعات نقداً' || data['salesInvoice_BillType'] === 'فاتورة مبيعات أجل') ? intVal(total) + intVal(data['salesInvoice_CustomerRest']) : total;
                    }, 0);




                totalBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مرتجعات') ? intVal(total) + intVal(data['salesInvoice_Total']) : total;
                    }, 0);

                vatBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مرتجعات') ? intVal(total) + intVal(data['salesInvoice_VatMoney']) : total;
                    }, 0);

                afterVatBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مرتجعات') ? intVal(total) + intVal(data['salesInvoice_TotalAfterVat']) : total;
                    }, 0);

                cashBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مرتجعات') ? intVal(total) + intVal(data['salesInvoice_PayCash']) : total;
                    }, 0);

                bankBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مرتجعات') ? intVal(total) + intVal(data['salesInvoice_PayBank']) : total;
                    }, 0);

                restBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مرتجعات') ? intVal(total) + intVal(data['salesInvoice_CustomerRest']) : total;
                    }, 0);




                var format = $.fn.dataTable.render.number(',', '.', 2).display;

                $('tr:eq(0) th:eq(5)', api.table().footer()).html('إجمالى المبيعات');
                $('tr:eq(0) th:eq(6)', api.table().footer()).html(format(totalSale));
                $('tr:eq(0) th:eq(7)', api.table().footer()).html(format(vatSale));
                $('tr:eq(0) th:eq(8)', api.table().footer()).html(format(afterVatSale));
                $('tr:eq(0) th:eq(9)', api.table().footer()).html(format(cashSale));
                $('tr:eq(0) th:eq(10)', api.table().footer()).html(format(bankSale));
                $('tr:eq(0) th:eq(11)', api.table().footer()).html(format(restSale));

                $('tr:eq(1) th:eq(5)', api.table().footer()).html('إجمالى المرتجعات');
                $('tr:eq(1) th:eq(6)', api.table().footer()).html(format(totalBack));
                $('tr:eq(1) th:eq(7)', api.table().footer()).html(format(vatBack));
                $('tr:eq(1) th:eq(8)', api.table().footer()).html(format(afterVatBack));
                $('tr:eq(1) th:eq(9)', api.table().footer()).html(format(cashBack));
                $('tr:eq(1) th:eq(10)', api.table().footer()).html(format(bankBack));
                $('tr:eq(1) th:eq(11)', api.table().footer()).html(format(restBack));

                $('tr:eq(2) th:eq(5)', api.table().footer()).html('الصافى');
                $('tr:eq(2) th:eq(6)', api.table().footer()).html(format(totalSale - totalBack));
                $('tr:eq(2) th:eq(7)', api.table().footer()).html(format(vatSale - vatBack));
                $('tr:eq(2) th:eq(8)', api.table().footer()).html(format(afterVatSale - afterVatBack));
                $('tr:eq(2) th:eq(9)', api.table().footer()).html(format(cashSale - cashBack));
                $('tr:eq(2) th:eq(10)', api.table().footer()).html(format(bankSale - bankBack));
                $('tr:eq(2) th:eq(11)', api.table().footer()).html(format(restSale - restBack));
            },
        });

        // Re-init functions on every table re-draw -- more info: https://datatables.net/reference/event/draw
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

var KTListNumber = function () {
    // Define shared variables
    var table = document.getElementById('kt_table');

    // Private functions
    var initTable = function () {
        var SalesInvoice_Number = $("#SalesInvoice_Number").val();
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            responsive: true,
            processing: true,
            "ajax": {
                "url": "/Sales/GetInvoiceNumber?Number=" + SalesInvoice_Number
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
                { targets: [0, 6, 7, 8, 9, 10, 11, 13], orderable: false, searchable: false, className: "text-center" },
                { targets: [1, 2, 3, 4], className: "text-center" },
                { targets: [6, 7, 8, 9, 10, 11], render: $.fn.dataTable.render.number(',', '.', 2) }
            ],
            "columns": [
                { "data": null },
                {
                    "data": "salesInvoice_Date",
                    render: function (data, type, row) {
                        return moment(data).format('YYYY/MM/DD');
                    }
                },
                { "data": "time" },
                { "data": "salesInvoice_BillType" },
                { "data": "salesInvoice_Number" },
                { "data": "customerData_Name" },
                { "data": "salesInvoice_Total" },
                { "data": "salesInvoice_VatMoney" },
                { "data": "salesInvoice_TotalAfterVat" },
                { "data": "salesInvoice_PayCash" },
                { "data": "salesInvoice_PayBank" },
                { "data": "salesInvoice_CustomerRest" },
                { "data": "userName" },
                {
                    "data": "salesInvoice_ID",
                    "render": function (data) {
                        return `
                                <a onclick=Detail("/Sales/Detail/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-magnifying-glass text-primary fs-1"></i>
                                </a>
                           `;
                    }
                },
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

                var totalSale = 0;
                var vatSale = 0;
                var afterVatSale = 0;
                var cashSale = 0;
                var bankSale = 0;
                var restSale = 0;

                var totalBack = 0;
                var vatBack = 0;
                var afterVatBack = 0;
                var cashBack = 0;
                var bankBack = 0;
                var restBack = 0;

                totalSale = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مبيعات نقداً' || data['salesInvoice_BillType'] === 'فاتورة مبيعات أجل') ? intVal(total) + intVal(data['salesInvoice_Total']) : total;
                    }, 0);

                vatSale = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مبيعات نقداً' || data['salesInvoice_BillType'] === 'فاتورة مبيعات أجل') ? intVal(total) + intVal(data['salesInvoice_VatMoney']) : total;
                    }, 0);

                afterVatSale = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مبيعات نقداً' || data['salesInvoice_BillType'] === 'فاتورة مبيعات أجل') ? intVal(total) + intVal(data['salesInvoice_TotalAfterVat']) : total;
                    }, 0);

                cashSale = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مبيعات نقداً' || data['salesInvoice_BillType'] === 'فاتورة مبيعات أجل') ? intVal(total) + intVal(data['salesInvoice_PayCash']) : total;
                    }, 0);

                bankSale = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مبيعات نقداً' || data['salesInvoice_BillType'] === 'فاتورة مبيعات أجل') ? intVal(total) + intVal(data['salesInvoice_PayBank']) : total;
                    }, 0);

                restSale = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مبيعات نقداً' || data['salesInvoice_BillType'] === 'فاتورة مبيعات أجل') ? intVal(total) + intVal(data['salesInvoice_CustomerRest']) : total;
                    }, 0);




                totalBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مرتجعات') ? intVal(total) + intVal(data['salesInvoice_Total']) : total;
                    }, 0);

                vatBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مرتجعات') ? intVal(total) + intVal(data['salesInvoice_VatMoney']) : total;
                    }, 0);

                afterVatBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مرتجعات') ? intVal(total) + intVal(data['salesInvoice_TotalAfterVat']) : total;
                    }, 0);

                cashBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مرتجعات') ? intVal(total) + intVal(data['salesInvoice_PayCash']) : total;
                    }, 0);

                bankBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مرتجعات') ? intVal(total) + intVal(data['salesInvoice_PayBank']) : total;
                    }, 0);

                restBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['salesInvoice_BillType'] === 'فاتورة مرتجعات') ? intVal(total) + intVal(data['salesInvoice_CustomerRest']) : total;
                    }, 0);




                var format = $.fn.dataTable.render.number(',', '.', 2).display;

                $('tr:eq(0) th:eq(5)', api.table().footer()).html('إجمالى المبيعات');
                $('tr:eq(0) th:eq(6)', api.table().footer()).html(format(totalSale));
                $('tr:eq(0) th:eq(7)', api.table().footer()).html(format(vatSale));
                $('tr:eq(0) th:eq(8)', api.table().footer()).html(format(afterVatSale));
                $('tr:eq(0) th:eq(9)', api.table().footer()).html(format(cashSale));
                $('tr:eq(0) th:eq(10)', api.table().footer()).html(format(bankSale));
                $('tr:eq(0) th:eq(11)', api.table().footer()).html(format(restSale));

                $('tr:eq(1) th:eq(5)', api.table().footer()).html('إجمالى المرتجعات');
                $('tr:eq(1) th:eq(6)', api.table().footer()).html(format(totalBack));
                $('tr:eq(1) th:eq(7)', api.table().footer()).html(format(vatBack));
                $('tr:eq(1) th:eq(8)', api.table().footer()).html(format(afterVatBack));
                $('tr:eq(1) th:eq(9)', api.table().footer()).html(format(cashBack));
                $('tr:eq(1) th:eq(10)', api.table().footer()).html(format(bankBack));
                $('tr:eq(1) th:eq(11)', api.table().footer()).html(format(restBack));

                $('tr:eq(2) th:eq(5)', api.table().footer()).html('الصافى');
                $('tr:eq(2) th:eq(6)', api.table().footer()).html(format(totalSale - totalBack));
                $('tr:eq(2) th:eq(7)', api.table().footer()).html(format(vatSale - vatBack));
                $('tr:eq(2) th:eq(8)', api.table().footer()).html(format(afterVatSale - afterVatBack));
                $('tr:eq(2) th:eq(9)', api.table().footer()).html(format(cashSale - cashBack));
                $('tr:eq(2) th:eq(10)', api.table().footer()).html(format(bankSale - bankBack));
                $('tr:eq(2) th:eq(11)', api.table().footer()).html(format(restSale - restBack));
            },
        });

        // Re-init functions on every table re-draw -- more info: https://datatables.net/reference/event/draw
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
    $("#FromDate").flatpickr();
    $("#ToDate").flatpickr();
    if ($("#Branch_ID").val() != 0) {
        $("#Branch_ID").attr('disabled', 'disabled');
    }
    KTList.init();
});

function jQueryAjaxSearch() {
    datatable.destroy();
    KTList.init();

    //prevent default form submit event
    return false;
}

function jQueryAjaxSearchNumber() {
    datatable.destroy();
    KTListNumber.init();

    //prevent default form submit event
    return false;
}

function Detail(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModal').empty();
            $('#divModal').html(response.data);
            $('#kt_modal_add').modal('show');
        })
}