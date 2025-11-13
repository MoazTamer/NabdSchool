"use strict";
var datatable;
var datatableStatistics;
var datatableBranch;
var start = 0;


var KTList = function () {
    // Define shared variables
    var table = document.getElementById('kt_table');
    
    // Private functions
    var initTable = function () {
        var ProductID = $("#ProductTitle_ID").val();
        var BranchID = $("#Branch_ID").val();
        var DateFrom = $("#DateFrom").val();
        var DateTo = $("#DateTo").val();
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            responsive: true,
            processing: true,
            "ajax": {
                "url": "/Product_Title/GetFollow?ProductID=" + ProductID + "&BranchID=" + BranchID + "&DateFrom=" + DateFrom + "&DateTo=" + DateTo
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
                { targets: [4, 5, 6, 9], orderable: false, searchable: false, className: "text-center" },
                { targets: [3], className: "text-center" },
                { targets: [8], visible: false }
            ],
            "columns": [
                { "data": "statement" },
                { "data": "itemName" },
                { "data": "number" },
                {
                    "data": "date",
                    render: function (data, type, row) {
                        return moment(data).format('YYYY/MM/DD');
                    }
                },
                { "data": "unit" },
                { "data": "incoming" },
                { "data": "outcoming" },
                { "data": "balance" },
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
                        else if (row.statementType == "transfer") {
                            return `
                                <a onclick=Detail("/Product_Transfer/Detail/${data}") style="cursor:pointer">
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
            ],
            "fnInitComplete": function (oSettings, json) {
                $("#balance").text(json.balanceData);
                $("#buy").text(json.buyData);
                $("#saleBack").text(json.saleBackData);
                $("#check").text(json.checkData);
                $("#sale").text(json.saleData);
                $("#buyBack").text(json.buyBackData);
                $("#currentBalance").text(json.currentBalance);

                if (start == 0) {
                    KTListStatistics.init();
                }
            }
        });
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

var KTListStatistics = function () {
    // Define shared variables
    var table = document.getElementById('kt_table_Statistics');

    // Private functions
    var initTable = function () {
        var ProductID = $("#ProductTitle_ID").val();
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatableStatistics = $(table).DataTable({
            responsive: true,
            processing: true,
            "ajax": {
                "url": "/Product_Title/GetFollowBarcode?ProductID=" + ProductID
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
                { targets: [0, 1, 2, 3, 4], orderable: false, searchable: false, className: "text-center" }
            ],
            "columns": [
                { "data": null },
                { "data": "unitStatistics" },
                { "data": "countStatistics" },
                { "data": "codeStatistics" },
                {
                    "data": null,
                    "render": function (data, type, row) {
                        var format = $.fn.dataTable.render.number('', '.', 2, '').display;
                        return format($("#currentBalance").text() / row.countStatistics);
                    }
                }
            ]
        });

        datatableStatistics.on('order.dt search.dt', function () {
            datatableStatistics.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
                cell.innerHTML = i + 1;
                datatableStatistics.cell(cell).invalidate('dom');
            });
        }).draw();

    }


    return {
        // Public functions  
        init: function () {
            if (!table) {
                return;
            }

            initTable();
            start = 1;
        }
    }
}();

var KTListBranch = function () {
    // Define shared variables
    var table = document.getElementById('kt_table_Branch');

    // Private functions
    var initTable = function () {
        var ProductID = $("#ProductTitle_ID").val();
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatableBranch = $(table).DataTable({
            responsive: true,
            processing: true,
            "ajax": {
                "url": "/Product_Title/GetFollowBranch?ProductID=" + ProductID
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
                { targets: [0, 1, 2], orderable: false, searchable: false, className: "text-center" }
            ],
            "columns": [
                { "data": null },
                { "data": "branch_Name" },
                { "data": "currentBalanceBranch" }
            ]
        });

        datatableBranch.on('order.dt search.dt', function () {
            datatableBranch.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
                cell.innerHTML = i + 1;
                datatableBranch.cell(cell).invalidate('dom');
            });
        }).draw();

    }


    return {
        // Public functions  
        init: function () {
            if (!table) {
                return;
            }

            initTable();
        }
    }
}();

// On document ready
KTUtil.onDOMContentLoaded(function () {
    $("#DateFrom").flatpickr();
    $("#DateTo").flatpickr();
    if ($("#UserCategory").val() != "admin") {
        $("#Branch_ID").attr('disabled', 'disabled');
    }
    KTList.init();    
    KTListBranch.init();
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