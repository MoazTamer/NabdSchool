"use strict";
var datatable;
var rowIndex;
var arrayId = [];


var KTList = function () {
    // Define shared variables
    var table = document.getElementById('kt_table');

    // Private functions
    var initTable = function () {
        var CashbalanceID = $("#CashBalance_ID").val();
        var FromDate = $("#FromDate").val();
        var ToDate = $("#ToDate").val();
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            responsive: true,
            processing: true,
            "ajax": {
                "url": "/CashBalance/GetDetail?CashBalanceID=" + CashbalanceID + "&FromDate=" + FromDate + "&ToDate=" + ToDate
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
                { targets: [0, 5, 6, 7, 8, 9], orderable: false, searchable: false, className: "text-center" },
                { targets: [1, 2, 3], orderable: false, className: "text-center" },
                { targets: [4], orderable: false },
                { targets: [8], visible: false }
            ],
            "columns": [
                { "data": null },
                { "data": "type" },
                { "data": "number" },
                {
                    "data": "date",
                    render: function (data, type, row) {
                        return moment(data).format('YYYY/MM/DD');
                    }
                },
                { "data": "statement" },
                { "data": "incoming" },
                { "data": "outcoming" },
                { "data": "currentBalance" },
                { "data": "statementType" },
                {
                    "data": "id",
                    "render": function (data, type, row) {
                        if (row.statementType == "sale") {
                            return `
                                <a onclick=Detail("/Sales/Detail/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-magnifying-glass text-primary fs-1"></i>
                                </a>
                           `;
                        }
                        else if (row.statementType == "buy") {
                            return `
                                <a onclick=Detail("/Buy/Detail/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-magnifying-glass text-primary fs-1"></i>
                                </a>
                           `;
                        }
                        else if (row.statementType == "payVendor") {
                            return `
                                <a onclick=Detail("/Vendor_Data/Detail/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-magnifying-glass text-primary fs-1"></i>
                                </a>
                           `;
                        }
                        else {
                            return `
                                <a href="#" style="cursor:pointer">
                                   <i class="fa-solid fa-magnifying-glass text-primary fs-1"></i>
                                </a>
                           `;
                        }
                    }
                },
            ]
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

function Detail(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModal').empty();
            $('#divModal').html(response.data);
            $('#kt_modal_add').modal('show');
        })
}