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
                "url": "/PriceOffer/GetOffer?FromDate=" + FromDate + "&ToDate=" + ToDate
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
                { targets: [0, 5, 6, 7, 9], orderable: false, searchable: false, className: "text-center" },
                { targets: [1, 2, 3], className: "text-center" },
                { targets: [5, 6, 7], render: $.fn.dataTable.render.number(',', '.', 2) }
            ],
            "columns": [
                { "data": null },
                {
                    "data": "priceOffer_Date",
                    render: function (data, type, row) {
                        return moment(data).format('YYYY/MM/DD');
                    }
                },
                { "data": "time" },
                { "data": "priceOffer_Number" },
                { "data": "customerData_Name" },
                { "data": "priceOffer_Total" },
                { "data": "priceOffer_VatMoney" },
                { "data": "priceOffer_TotalAfterVat" },
                { "data": "userName" },
                {
                    "data": "priceOffer_ID",
                    "render": function (data) {
                        return `
                                <a onclick=Detail("/PriceOffer/Detail/${data}") style="cursor:pointer">
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

                // computing column Total of the complete result 
                var total = api
                    .column(5, { page: 'all', search: 'applied' })
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);

                var vatMoney = api
                    .column(6, { page: 'all', search: 'applied' })
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);

                var afterVat = api
                    .column(7, { page: 'all', search: 'applied' })
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
                $(api.column(4).footer()).html('');
                $(api.column(5).footer()).html(format(total));
                $(api.column(6).footer()).html(format(vatMoney));
                $(api.column(7).footer()).html(format(afterVat));
                $(api.column(8).footer()).html('');
                $(api.column(9).footer()).html('');
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

function Detail(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModal').empty();
            $('#divModal').html(response.data);
            $('#kt_modal_add').modal('show');
        })
}