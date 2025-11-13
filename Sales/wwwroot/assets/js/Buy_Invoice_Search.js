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
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            responsive: true,
            processing: true,
            "ajax": {
                "url": "/Buy/GetInvoice?FromDate=" + FromDate + "&ToDate=" + ToDate
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
                { targets: [0, 1, 5, 6, 7, 8, 9], orderable: false, searchable: false, className: "text-center" },
                { targets: [2, 3], className: "text-center" },
                { targets: [5, 6, 7], render: $.fn.dataTable.render.number(',', '.', 2) }
            ],
            "columns": [
                { "data": null },
                {
                    "data": "buyInvoice_Date",
                    render: function (data, type, row) {
                        return moment(data).format('YYYY/MM/DD');
                    }
                },
                { "data": "buyInvoice_BillType" },
                { "data": "buyInvoice_Number" },
                { "data": "vendorData_Name" },
                { "data": "buyInvoice_Total" },
                { "data": "buyInvoice_VatMoney" },
                { "data": "buyInvoice_TotalAfterVat" },
                { "data": "userName" },
                {
                    "data": "buyInvoice_ID",
                    "render": function (data) {
                        return `
                                <a onclick=Detail("/Buy/Detail/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-magnifying-glass text-primary fs-1"></i>
                                </a>
                           `;
                    }
                },
            ],
            "footerCallback": function (row, data, start, end, display) {
                var api = this.api(), data;

                // converting to interger to find total
                var intVal = function (i) {
                    return typeof i === 'string' ?
                        i.replace(/[\$,]/g, '') * 1 :
                        typeof i === 'number' ?
                            i : 0;
                };

                var totalBuy = 0;
                var vatBuy = 0;
                var afterVatBuy = 0;

                var totalBack = 0;
                var vatBack = 0;
                var afterVatBack = 0;

                totalBuy = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['buyInvoice_BillType'] === 'فاتورة مشتريات كاش' || data['buyInvoice_BillType'] === 'فاتورة مشتريات أجل') ? intVal(total) + intVal(data['buyInvoice_Total']) : total;
                    }, 0);

                vatBuy = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['buyInvoice_BillType'] === 'فاتورة مشتريات كاش' || data['buyInvoice_BillType'] === 'فاتورة مشتريات أجل') ? intVal(total) + intVal(data['buyInvoice_VatMoney']) : total;
                    }, 0);

                afterVatBuy = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['buyInvoice_BillType'] === 'فاتورة مشتريات كاش' || data['buyInvoice_BillType'] === 'فاتورة مشتريات أجل') ? intVal(total) + intVal(data['buyInvoice_TotalAfterVat']) : total;
                    }, 0);

                totalBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['buyInvoice_BillType'] === 'فاتورة مرتجعات كاش' || data['buyInvoice_BillType'] === 'فاتورة مرتجعات أجل') ? intVal(total) + intVal(data['buyInvoice_Total']) : total;
                    }, 0);

                vatBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['buyInvoice_BillType'] === 'فاتورة مرتجعات كاش' || data['buyInvoice_BillType'] === 'فاتورة مرتجعات أجل') ? intVal(total) + intVal(data['buyInvoice_VatMoney']) : total;
                    }, 0);

                afterVatBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['buyInvoice_BillType'] === 'فاتورة مرتجعات كاش' || data['buyInvoice_BillType'] === 'فاتورة مرتجعات أجل') ? intVal(total) + intVal(data['buyInvoice_TotalAfterVat']) : total;
                    }, 0);
                

                var format = $.fn.dataTable.render.number(',', '.', 2).display;

                $('tr:eq(0) th:eq(4)', api.table().footer()).html('إجمالى المشتريات');
                $('tr:eq(0) th:eq(5)', api.table().footer()).html(format(totalBuy));
                $('tr:eq(0) th:eq(6)', api.table().footer()).html(format(vatBuy));
                $('tr:eq(0) th:eq(7)', api.table().footer()).html(format(afterVatBuy));

                $('tr:eq(1) th:eq(4)', api.table().footer()).html('إجمالى المرتجعات');
                $('tr:eq(1) th:eq(5)', api.table().footer()).html(format(totalBack));
                $('tr:eq(1) th:eq(6)', api.table().footer()).html(format(vatBack));
                $('tr:eq(1) th:eq(7)', api.table().footer()).html(format(afterVatBack));

                $('tr:eq(2) th:eq(4)', api.table().footer()).html('الصافى');
                $('tr:eq(2) th:eq(5)', api.table().footer()).html(format(totalBuy - totalBack));
                $('tr:eq(2) th:eq(6)', api.table().footer()).html(format(vatBuy - vatBack));
                $('tr:eq(2) th:eq(7)', api.table().footer()).html(format(afterVatBuy - afterVatBack));
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
        var BuyInvoice_Number = $("#BuyInvoice_Number").val();
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            responsive: true,
            processing: true,
            "ajax": {
                "url": "/Buy/GetInvoiceNumber?Number=" + BuyInvoice_Number
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
                { targets: [0, 1, 5, 6, 7, 8, 9], orderable: false, searchable: false, className: "text-center" },
                { targets: [2, 3], className: "text-center" },
                { targets: [5, 6, 7], render: $.fn.dataTable.render.number(',', '.', 2) }
            ],
            "columns": [
                { "data": null },
                {
                    "data": "buyInvoice_Date",
                    render: function (data, type, row) {
                        return moment(data).format('YYYY/MM/DD');
                    }
                },
                { "data": "buyInvoice_BillType" },
                { "data": "buyInvoice_Number" },
                { "data": "vendorData_Name" },
                { "data": "buyInvoice_Total" },
                { "data": "buyInvoice_VatMoney" },
                { "data": "buyInvoice_TotalAfterVat" },
                { "data": "userName" },
                {
                    "data": "buyInvoice_ID",
                    "render": function (data) {
                        return `
                                <a onclick=Detail("/Buy/Detail/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-magnifying-glass text-primary fs-1"></i>
                                </a>
                           `;
                    }
                },
            ],
            "footerCallback": function (row, data, start, end, display) {
                var api = this.api(), data;

                // converting to interger to find total
                var intVal = function (i) {
                    return typeof i === 'string' ?
                        i.replace(/[\$,]/g, '') * 1 :
                        typeof i === 'number' ?
                            i : 0;
                };

                var totalBuy = 0;
                var vatBuy = 0;
                var afterVatBuy = 0;

                var totalBack = 0;
                var vatBack = 0;
                var afterVatBack = 0;

                totalBuy = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['buyInvoice_BillType'] === 'فاتورة مشتريات كاش' || data['buyInvoice_BillType'] === 'فاتورة مشتريات أجل') ? intVal(total) + intVal(data['buyInvoice_Total']) : total;
                    }, 0);

                vatBuy = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['buyInvoice_BillType'] === 'فاتورة مشتريات كاش' || data['buyInvoice_BillType'] === 'فاتورة مشتريات أجل') ? intVal(total) + intVal(data['buyInvoice_VatMoney']) : total;
                    }, 0);

                afterVatBuy = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['buyInvoice_BillType'] === 'فاتورة مشتريات كاش' || data['buyInvoice_BillType'] === 'فاتورة مشتريات أجل') ? intVal(total) + intVal(data['buyInvoice_TotalAfterVat']) : total;
                    }, 0);

                totalBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['buyInvoice_BillType'] === 'فاتورة مرتجعات كاش' || data['buyInvoice_BillType'] === 'فاتورة مرتجعات أجل') ? intVal(total) + intVal(data['buyInvoice_Total']) : total;
                    }, 0);

                vatBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['buyInvoice_BillType'] === 'فاتورة مرتجعات كاش' || data['buyInvoice_BillType'] === 'فاتورة مرتجعات أجل') ? intVal(total) + intVal(data['buyInvoice_VatMoney']) : total;
                    }, 0);

                afterVatBack = api
                    .rows()
                    .data()
                    .reduce(function (total, data) {
                        return (data['buyInvoice_BillType'] === 'فاتورة مرتجعات كاش' || data['buyInvoice_BillType'] === 'فاتورة مرتجعات أجل') ? intVal(total) + intVal(data['buyInvoice_TotalAfterVat']) : total;
                    }, 0);


                var format = $.fn.dataTable.render.number(',', '.', 2).display;

                $('tr:eq(0) th:eq(4)', api.table().footer()).html('إجمالى المشتريات');
                $('tr:eq(0) th:eq(5)', api.table().footer()).html(format(totalBuy));
                $('tr:eq(0) th:eq(6)', api.table().footer()).html(format(vatBuy));
                $('tr:eq(0) th:eq(7)', api.table().footer()).html(format(afterVatBuy));

                $('tr:eq(1) th:eq(4)', api.table().footer()).html('إجمالى المرتجعات');
                $('tr:eq(1) th:eq(5)', api.table().footer()).html(format(totalBack));
                $('tr:eq(1) th:eq(6)', api.table().footer()).html(format(vatBack));
                $('tr:eq(1) th:eq(7)', api.table().footer()).html(format(afterVatBack));

                $('tr:eq(2) th:eq(4)', api.table().footer()).html('الصافى');
                $('tr:eq(2) th:eq(5)', api.table().footer()).html(format(totalBuy - totalBack));
                $('tr:eq(2) th:eq(6)', api.table().footer()).html(format(vatBuy - vatBack));
                $('tr:eq(2) th:eq(7)', api.table().footer()).html(format(afterVatBuy - afterVatBack));
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