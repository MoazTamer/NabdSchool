"use strict";
var datatable;
var rowIndex;
var total;

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
                "url": "/Report/GetReport_Safe"
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
                { targets: [0], orderable: false, searchable: false, className: "text-center" },
                { targets: [1, 2], className: "text-center" },
                { targets: [2], render: $.fn.dataTable.render.number(',', '.', 2, '') }
            ],
            "columns": [
                { "data": null },
                { "data": "cashBalance_Name" },
                { "data": "currentBalance" },
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
                total = api
                    .column(2, { page: 'all', search: 'applied' })
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);

                var format = $.fn.dataTable.render.number(',', '.', 2).display;
                var totalFormat = format(total);

                // Update footer by showing the total with the reference of the column index 
                $(api.column(0).footer()).html('');
                $(api.column(1).footer()).html('الإجمالى');
                $(api.column(2).footer()).html(totalFormat);
            },
            "fnInitComplete": function (oSettings, json) {

                var cash = parseFloat(total);
                var productAfterVat = parseFloat(json.productAfterVat);
                var customer = parseFloat(json.customer);
                var vendor = parseFloat(json.vendor);

                var format = $.fn.dataTable.render.number(',', '.', 2).display;

                $("#Product").text(format(json.product));
                $("#ProductAfterVat").text(format(productAfterVat));
                $("#Customer").text(format(customer));
                $("#Vendor").text(format(vendor));

                var net = cash + productAfterVat + customer - vendor;               
                $("#Net").text(format(net));
            }
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
    KTList.init();
});