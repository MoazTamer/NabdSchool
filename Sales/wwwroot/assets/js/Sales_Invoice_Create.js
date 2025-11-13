"use strict";
var datatable;
var rowIndex;

// On document ready
var KTList = function () {
    // Define shared variables
    var table = document.getElementById('kt_table');

    // Private functions
    var initTable = function () {
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            responsive: true,
            processing: true,
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
                { targets: [0, 6, 7, 8, 9, 10, 11, 12], orderable: false, searchable: false, className: "text-center border border-gray-500" },
                { targets: [1, 4, 5], className: "text-center " },
                { targets: [2, 3, 9], visible: false }
            ],
            "columns": [
                { "data": null },
                { "data": "productBarcode_Code" },
                { "data": "productTitle_ID" },
                { "data": "productBarcode_ID" },
                { "data": "productTitle_Name" },
                { "data": "productBarcode_Unit" },
                {
                    "data": "productBarcode_Quantity",
                    "render": function (data, type, row) {
                        return `
                                <input type="text" class="form-control form-control-solid text-center quantity w-100" value="${data}" />
                           `;
                    }
                },
                {
                    "data": "productBarcode_PayPrice",
                    "render": function (data, type, row) {
                        return `
                                <input type="text" class="form-control form-control-solid text-center price w-100" value="${$.fn.dataTable.render.number('', '.', 2, '').display(data)}" />
                           `;
                    }
                },
                {
                    "data": "productBarcode_Total",
                    "render": function (data, type, row) {
                        return `
                                <span type="text" class="form-control form-control-solid text-center total">${$.fn.dataTable.render.number('', '.', 2, '').display(data)}</span>
                           `;
                    }
                },
                { "data": "productBarcode_BuyPrice" },
                {
                    "data": "productBarcode_PayPriceVat",
                    "render": function (data, type, row) {
                        return `
                                <input type="text" class="form-control form-control-solid text-center priceVat w-100" value="${$.fn.dataTable.render.number('', '.', 2, '').display(data)}" />
                           `;
                    }
                },
                {
                    "data": "productBarcode_TotalVat",
                    "render": function (data, type, row) {
                        return `
                                <span type="text" class="form-control form-control-solid text-center total">${$.fn.dataTable.render.number('', '.', 2, '').display(data)}</span>
                           `;
                    }
                },
                {
                    "data": "productBarcode_ID",
                    "render": function (data) {
                        return `
                                <a onclick=Delete() style="cursor:pointer">
                                   <i class="fa-solid fa-trash-can text-danger fs-1"></i>
                                </a>
                           `;
                    }
                }
            ],
            "drawCallback": function (settings) {
                $(".quantity").on("change", function () {
                    var format3 = $.fn.dataTable.render.number('', '.', 3, '').display;

                    var Quantity = $(datatable.cell(rowIndex, 6).node()).find('input').val();
                    if (Quantity == 0) {
                        $(datatable.cell(rowIndex, 6).node()).find('input').val(1);
                        var Price = $(datatable.cell(rowIndex, 7).node()).find('input').val();
                        var PriceVat = $(datatable.cell(rowIndex, 10).node()).find('input').val();

                        var Total = Price;
                        var TotalVat = PriceVat;

                        datatable.cell(rowIndex, 8).data(format3(Total));
                        datatable.cell(rowIndex, 11).data(format3(TotalVat));
                    }
                    else {
                        var Price = $(datatable.cell(rowIndex, 7).node()).find('input').val();
                        var PriceVat = $(datatable.cell(rowIndex, 10).node()).find('input').val();

                        var Total = Quantity * Price;
                        var TotalVat = Quantity * PriceVat;

                        datatable.cell(rowIndex, 8).data(format3(Total));
                        datatable.cell(rowIndex, 11).data(format3(TotalVat));
                    }

                    CalcTotal();
                })

                $(".price").on("change", function () {
                    var format3 = $.fn.dataTable.render.number('', '.', 3, '').display;

                    var Quantity = $(datatable.cell(rowIndex, 6).node()).find('input').val();
                    var Price = $(datatable.cell(rowIndex, 7).node()).find('input').val();

                    var Total = Quantity * Price;
                    var PriceVat = Price * (($("#SalesInvoice_VatValue").val() / 100) + 1);
                    var TotalVat = Quantity * PriceVat;

                    datatable.cell(rowIndex, 8).data(format3(Total));
                    $(datatable.cell(rowIndex, 10).node()).find('input').val(format3(PriceVat));
                    datatable.cell(rowIndex, 11).data(format3(TotalVat));

                    CalcTotal();
                })

                $(".priceVat").on("change", function () {
                    var format3 = $.fn.dataTable.render.number('', '.', 3, '').display;

                    var Quantity = $(datatable.cell(rowIndex, 6).node()).find('input').val();
                    var PriceVat = $(datatable.cell(rowIndex, 10).node()).find('input').val();

                    var Price = PriceVat / (($("#SalesInvoice_VatValue").val() / 100) + 1);
                    var Total = Quantity * Price;                   
                    var TotalVat = Quantity * PriceVat;

                    $(datatable.cell(rowIndex, 7).node()).find('input').val(format3(Price));
                    datatable.cell(rowIndex, 8).data(format3(Total));
                    datatable.cell(rowIndex, 11).data(format3(TotalVat));

                    CalcTotal();
                })
            }
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
    $("#ProductTitle_Offer").val('no');
    $("#TotalProfit").attr('hidden', 'hidden');
    if ($("#UserCategory").val() != "admin") {
        $("#Branch_ID").attr('disabled', 'disabled');
        $("#ProductTitle_Balance").attr('hidden', 'hidden');
    }
    $("#SalesInvoice_Date").flatpickr();
    $("#lblVat").text("الضريبة " + $("#SalesInvoice_VatValue").val() + " %");
    FillCustomer();
    ChangeBillType();
    FillUnit()
    KTList.init();

    var input = document.getElementById("ProductBarcode_Code");
    input.addEventListener("keypress", function (event) {
        if (event.key === "Enter") {
            event.preventDefault();

            var table = document.getElementById('kt_table');

            var blockUI = new KTBlockUI(table, {
                overlayClass: "bg-danger bg-opacity-25",
                message: '<div class="blockui-message"><span class="spinner-border text-primary"></span>من فضلك إنتظر ...</div>',
            });
            blockUI.block();

            axios.get('/Sales/Barcode_KeyPress?BranchID=' + $("#Branch_ID").val() + '&Barcode=' + $("#ProductBarcode_Code").val() + '&BillType=' + $("#SalesInvoice_BillType").val() + '&Refrence=' + $("#SalesInvoice_Refrence").val())
                .then(function (response) {
                    if (response.data.isValid == true) {
                        //$("#ProductBarcode_Code").val('');
                        $(datatable.row.add(response.data.data).draw().node()).find('td').eq(1).addClass('newItem');
                        CalcTotal();

                        blockUI.release();
                    }
                    else {
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

                        //$("#ProductBarcode_Code").val('');
                        blockUI.release();
                    }
                })
                .catch(function (error) {
                    swal.fire({
                        title: "فواتير المبيعات",
                        text: "من فضلك تأكد من تسجيل البيانات بطريقة صحيحة",
                        icon: "error",
                        buttonsStyling: false,
                        confirmButtonText: "موافق",
                        customClass: {
                            confirmButton: "btn fw-bold btn-light-primary"
                        }
                    });

                    blockUI.release();
                });
            blockUI.destroy();
        }
    });


    //uncomment to prevent on startup
    //removeDefaultFunction();          
    /** Prevents the default function such as the help pop-up **/
    function removeDefaultFunction() {
        window.onhelp = function () { return false; }
    }
    /** use keydown event and trap only the F-key, 
        but not combinations with SHIFT/CTRL/ALT **/
    $(window).bind('keydown', function (e) {
        //This is the F1 key code, but NOT with SHIFT/CTRL/ALT
        var keyCode = e.keyCode || e.which;
        // حفظ بدون طباعة
        if ((keyCode == 112 || e.key == 'F1') &&
            !(event.altKey || event.ctrlKey || event.shiftKey || event.metaKey)) {
            // prevent code starts here:
            removeDefaultFunction();
            e.cancelable = true;
            e.stopPropagation();
            e.preventDefault();
            e.returnValue = false;
            // Open help window here instead of alert
            SaveBill('none');
        }
        // Add other F-keys here: طباعة صغير
        else if ((keyCode == 113 || e.key == 'F2') &&
            !(event.altKey || event.ctrlKey || event.shiftKey || event.metaKey)) {
            // prevent code starts here:
            removeDefaultFunction();
            e.cancelable = true;
            e.stopPropagation();
            e.preventDefault();
            e.returnValue = false;
            // Do something else for F2
            SaveBill('cashier');
        }
        // Add other F-keys here: طباعة A4
        else if ((keyCode == 114 || e.key == 'F3') &&
            !(event.altKey || event.ctrlKey || event.shiftKey || event.metaKey)) {
            // prevent code starts here:
            removeDefaultFunction();
            e.cancelable = true;
            e.stopPropagation();
            e.preventDefault();
            e.returnValue = false;
            // Do something else for F3
            SaveBill('a4');
        }
        // Add other F-keys here: الخصم
        else if ((keyCode == 114 || e.key == 'F4') &&
            !(event.altKey || event.ctrlKey || event.shiftKey || event.metaKey)) {
            // prevent code starts here:
            removeDefaultFunction();
            e.cancelable = true;
            e.stopPropagation();
            e.preventDefault();
            e.returnValue = false;
            // Do something else for F6

            $("#SalesInvoice_Discount").focus();
        }
        // Add other F-keys here: المدفوع نقدا
        else if ((keyCode == 116 || e.key == 'F5') &&
            !(event.altKey || event.ctrlKey || event.shiftKey || event.metaKey)) {
            // prevent code starts here:
            removeDefaultFunction();
            e.cancelable = true;
            e.stopPropagation();
            e.preventDefault();
            e.returnValue = false;
            // Do something else for F5

            var TotalAfterVat = $("#SalesInvoice_TotalAfterVat").text().replace(/,/g, '');
            var PayBank = $("#SalesInvoice_PayBank").val().replace(/,/g, '');

            var format = $.fn.dataTable.render.number('', '.', 2).display;
            var cash = TotalAfterVat - PayBank;

            $("#SalesInvoice_PayCash").val(format(cash));
            $("#SalesInvoice_PayCash").focus();
            CalcTotal();
        }
        // Add other F-keys here: المدفوع شبكة
        else if ((keyCode == 117 || e.key == 'F6') &&
            !(event.altKey || event.ctrlKey || event.shiftKey || event.metaKey)) {
            // prevent code starts here:
            removeDefaultFunction();
            e.cancelable = true;
            e.stopPropagation();
            e.preventDefault();
            e.returnValue = false;
            // Do something else for F6

            var TotalAfterVat = $("#SalesInvoice_TotalAfterVat").text().replace(/,/g, '');
            var PayCash = $("#SalesInvoice_PayCash").val().replace(/,/g, '');

            var format = $.fn.dataTable.render.number('', '.', 2).display;
            var bank = TotalAfterVat - PayCash;

            $("#SalesInvoice_PayBank").val(format(bank));
            $("#SalesInvoice_PayBank").focus();
            CalcTotal();
        }
        // Add other F-keys here: فاتورة جديدة
        else if ((keyCode == 118 || e.key == 'F7') &&
            !(event.altKey || event.ctrlKey || event.shiftKey || event.metaKey)) {
            // prevent code starts here:
            removeDefaultFunction();
            e.cancelable = true;
            e.stopPropagation();
            e.preventDefault();
            e.returnValue = false;
            // Do something else for F7

            window.open("/Sales/Create");
        }
        // Add other F-keys here: الربح
        else if ((keyCode == 119 || e.key == 'F8') &&
            !(event.altKey || event.ctrlKey || event.shiftKey || event.metaKey)) {
            // prevent code starts here:
            removeDefaultFunction();
            e.cancelable = true;
            e.stopPropagation();
            e.preventDefault();
            e.returnValue = false;
            // Do something else for F8

            var attr = $("#TotalProfit").attr('hidden');
            if (typeof attr !== 'undefined' && attr !== false) {
               
                $("#TotalProfit").removeAttr('hidden');
            }
            else {
                $("#TotalProfit").attr('hidden', 'hidden');
            }
        }
        
    });

});

function PayPriceChange() {
    var format = $.fn.dataTable.render.number(',', '.', 3).display;
    var pay = $("#ProductBarcode_PayPrice").val() * (($("#SalesInvoice_VatValue").val() / 100) + 1);
    $("#ProductBarcode_PayPriceVat").val(format(pay));
}

function PayPricePlusChange() {
    var pay = $("#ProductBarcode_PayPriceVat").val() / (($("#SalesInvoice_VatValue").val() / 100) + 1);
    var format = $.fn.dataTable.render.number(',', '.', 3).display;
    $("#ProductBarcode_PayPrice").val(format(pay));
}

function FillCustomer(){
    var Type = $("#SalesInvoice_CustomerType").val();
    if (Type === "عميل نقدى") {
        $("#SalesInvoice_CustomerName").removeAttr('disabled');
        $("#SalesInvoice_Phone").removeAttr('disabled');
        $("#SalesInvoice_Address").removeAttr('disabled');
        $("#SalesInvoice_VatNumber").removeAttr('disabled');
    }
    else {
        $("#SalesInvoice_CustomerName").attr('disabled', 'disabled');
        $("#SalesInvoice_Phone").attr('disabled', 'disabled');
        $("#SalesInvoice_Address").attr('disabled', 'disabled');
        $("#SalesInvoice_VatNumber").attr('disabled', 'disabled');
    }

    axios.get('/Sales/GetCustomer?Type=' + Type)
        .then(function (response) {
            var len = response.data.length;
            $("#CustomerData_ID").empty();
            for (var i = 0; i < len; i++) {
                var id = response.data[i]['customerData_ID'];
                var name = response.data[i]['customerData_Name'];
                $("#CustomerData_ID").append("<option value='" + id + "'>" + name + "</option>");
            }
            CustomerData();
        })
        .catch(function (error) {
            swal.fire({
                title: "المبيعات",
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

function CustomerData() {
    var CustomerID = $("#CustomerData_ID").val();
    axios.get('/Sales/GetCustomerData?CustomerID=' + CustomerID)
        .then(function (response) {
            $("#SalesInvoice_CustomerName").val(response.data.name);
            $("#SalesInvoice_Phone").val(response.data.phone);
            $("#SalesInvoice_Address").val(response.data.address);
            $("#SalesInvoice_VatNumber").val(response.data.vat);
        })
        .catch(function (error) {
            swal.fire({
                title: "المبيعات",
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

function ChangeBillType() {
    var Type = $("#SalesInvoice_BillType").val();
    if (Type === "فاتورة مرتجعات") {
        $("#SalesInvoice_Refrence").removeAttr('disabled');
    }
    else {
        $("#SalesInvoice_Refrence").attr('disabled', 'disabled');
    }
}

function FillUnit() {
    var ProductID = $("#ProductTitle_ID").val();
    axios.get('/Sales/GetUnit?ProductID=' + ProductID)
        .then(function (response) {
            var len = response.data.length;
            $("#ProductBarcode_Unit").empty();
            for (var i = 0; i < len; i++) {
                var id = response.data[i]['productBarcode_ID'];
                var name = response.data[i]['productBarcode_Unit'];
                $("#ProductBarcode_Unit").append("<option value='" + id + "'>" + name + "</option>");
                if (response.data[i]['productBarcode_Unit'] == "عرض") {
                    $("#ProductTitle_Offer").val('yes');
                }
                else {
                    $("#ProductTitle_Offer").val('no');
                }
            }
            UnitData();
        })
        .catch(function (error) {
            swal.fire({
                title: "فواتير المبيعات",
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

function UnitData() {
    var Offer = $("#ProductTitle_Offer").val();
    var BranchID = $("#Branch_ID").val();
    var BarcodeID = $("#ProductBarcode_Unit").val();
    var BillType = $("#SalesInvoice_BillType").val();
    var Refrence = $("#SalesInvoice_Refrence").val();
    axios.get('/Sales/GetUnitData?BranchID=' + BranchID + '&BarcodeID=' + BarcodeID + '&Offer=' + Offer + '&BillType=' + BillType + '&Refrence=' + Refrence)
        .then(function (response) {
            if (response.data.isValid == true) {
                var format2 = $.fn.dataTable.render.number('', '.', 2).display;
                var format3 = $.fn.dataTable.render.number(',', '.', 3).display;

                if (response.data.data.productBarcode_CurrentCount == undefined)
                {
                    $("#ProductTitle_Balance").text(0);
                }
                else {
                    $("#ProductTitle_Balance").text(format2(response.data.data.productBarcode_CurrentCount / response.data.data.productBarcode_Count));
                }
                $("#ProductBarcode_Count").val(1);
                if ($("#SalesInvoice_Pay").val() == "نقدى") {
                    $("#ProductBarcode_PayPrice").val(format3(response.data.data.productBarcode_PayPrice));
                }
                else {
                    $("#ProductBarcode_PayPrice").val(format3(response.data.data.productBarcode_PaySpecial));
                }
                $("#ProductBarcode_Code").val(response.data.data.productBarcode_Code);
                $("#ProductBarcode_BuyPrice").val(response.data.data.productBarcode_BuyPrice);
                PayPriceChange();
            }
            else {
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
                title: "فواتير المبيعات",
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

function AddBill() {
    const element = document.getElementById('divAddBill');
    const form = element.querySelector('#formAddBill');

    // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
    var validator = FormValidation.formValidation(
        form,
        {
            fields: {
                'productTitle_ID': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار الصنف'
                        }
                    }
                },
                'salesInvoice_Pay': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار النوع'
                        }
                    }
                },
                'productBarcode_Unit': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار الوحدة'
                        }
                    }
                },
                'productBarcode_Count': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل العدد'
                        }
                    }
                },
                'productBarcode_PayPrice': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل السعر'
                        }
                    }
                }
            },

            plugins: {
                trigger: new FormValidation.plugins.Trigger(),
                bootstrap: new FormValidation.plugins.Bootstrap5({
                    rowSelector: '.row',
                    eleInvalidClass: '',
                    eleValidClass: ''
                })
            }
        }
    );

    // Validate form before submit
    if (validator) {
        validator.validate().then(function (status) {
            if (status == 'Valid') {
                if (parseFloat($("#ProductBarcode_Count").val()) == 0) {
                    swal.fire({
                        title: "فواتير المبيعات",
                        text: "تأكد من تسجيل عدد الصنف",
                        icon: "error",
                        buttonsStyling: false,
                        confirmButtonText: "موافق",
                        customClass: {
                            confirmButton: "btn fw-bold btn-light-primary"
                        }
                    });
                }
                else {
                    if ($("#SalesInvoice_BillType").val() == "فاتورة مرتجعات") {
                        if ($("#SalesInvoice_Refrence").val() == "" || $("#SalesInvoice_Refrence").val() == null) {
                            swal.fire({
                                title: "مبيعات",
                                text: "تأكد من تسجيل رقم المرجع بشكل صحيح",
                                icon: "error",
                                buttonsStyling: false,
                                confirmButtonText: "موافق",
                                customClass: {
                                    confirmButton: "btn fw-bold btn-light-primary"
                                }
                            });
                        }
                        else {
                            datatable.row
                                .add({
                                    "productBarcode_Code": $("#ProductBarcode_Code").val(),
                                    "productTitle_ID": $("#ProductTitle_ID").val(),
                                    "productBarcode_ID": $("#ProductBarcode_Unit").val(),
                                    "productTitle_Name": $("#ProductTitle_ID option:selected").text(),
                                    "productBarcode_Unit": $("#ProductBarcode_Unit option:selected").text(),
                                    "productBarcode_Quantity": $("#ProductBarcode_Count").val(),
                                    "productBarcode_PayPrice": $("#ProductBarcode_PayPrice").val(),
                                    "productBarcode_Total": $("#ProductBarcode_Count").val() * $("#ProductBarcode_PayPrice").val(),
                                    "productBarcode_BuyPrice": $("#ProductBarcode_BuyPrice").val(),
                                    "productBarcode_PayPriceVat": $("#ProductBarcode_PayPriceVat").val(),
                                    "productBarcode_TotalVat": $("#ProductBarcode_Count").val() * $("#ProductBarcode_PayPriceVat").val()
                                })
                                .draw();
                            CalcTotal();
                        }
                    }
                    else {
                        if ($("#ProductTitle_Offer").val() == "yes") {
                            datatable.row
                                .add({
                                    "productBarcode_Code": $("#ProductBarcode_Code").val(),
                                    "productTitle_ID": $("#ProductTitle_ID").val(),
                                    "productBarcode_ID": $("#ProductBarcode_Unit").val(),
                                    "productTitle_Name": $("#ProductTitle_ID option:selected").text(),
                                    "productBarcode_Unit": $("#ProductBarcode_Unit option:selected").text(),
                                    "productBarcode_Quantity": $("#ProductBarcode_Count").val(),
                                    "productBarcode_PayPrice": $("#ProductBarcode_PayPrice").val(),
                                    "productBarcode_Total": $("#ProductBarcode_Count").val() * $("#ProductBarcode_PayPrice").val(),
                                    "productBarcode_BuyPrice": $("#ProductBarcode_BuyPrice").val(),
                                    "productBarcode_PayPriceVat": $("#ProductBarcode_PayPriceVat").val(),
                                    "productBarcode_TotalVat": $("#ProductBarcode_Count").val() * $("#ProductBarcode_PayPriceVat").val()
                                })
                                .draw();
                            CalcTotal();
                        }
                        else {
                            if (parseFloat($("#ProductTitle_Balance").text()) < parseFloat($("#ProductBarcode_Count").val())) {
                                swal.fire({
                                    title: "فواتير المبيعات",
                                    text: "رصيد الصنف لا يسمح",
                                    icon: "error",
                                    buttonsStyling: false,
                                    confirmButtonText: "موافق",
                                    customClass: {
                                        confirmButton: "btn fw-bold btn-light-primary"
                                    }
                                });
                            }
                            else {
                                datatable.row
                                    .add({
                                        "productBarcode_Code": $("#ProductBarcode_Code").val(),
                                        "productTitle_ID": $("#ProductTitle_ID").val(),
                                        "productBarcode_ID": $("#ProductBarcode_Unit").val(),
                                        "productTitle_Name": $("#ProductTitle_ID option:selected").text(),
                                        "productBarcode_Unit": $("#ProductBarcode_Unit option:selected").text(),
                                        "productBarcode_Quantity": $("#ProductBarcode_Count").val(),
                                        "productBarcode_PayPrice": $("#ProductBarcode_PayPrice").val(),
                                        "productBarcode_Total": $("#ProductBarcode_Count").val() * $("#ProductBarcode_PayPrice").val(),
                                        "productBarcode_BuyPrice": $("#ProductBarcode_BuyPrice").val(),
                                        "productBarcode_PayPriceVat": $("#ProductBarcode_PayPriceVat").val(),
                                        "productBarcode_TotalVat": $("#ProductBarcode_Count").val() * $("#ProductBarcode_PayPriceVat").val()
                                    })
                                    .draw();
                                CalcTotal();
                            }
                        }
                    }
                }
            } else {
                KTUtil.scrollTop();
            }
        });
    }
    return false;
}

function Delete() {
    Swal.fire({
        title: "هل انت متأكد ؟",
        text: "سيتم حذف الصنف",
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

            datatable.row(rowIndex).remove().draw();
            CalcTotal();
        }
    });

    return false;
}

function CalcTotal() {
    var total = 0;
    if ($('tbody').length > 0) {
        $('tbody tr').each(function (i) {
            total += parseFloat(datatable.cell(i, 8).data());
        })
    }

    var discountMoney = 0;
    if ($("#chkAmount").prop("checked") == true) {
        discountMoney = $("#SalesInvoice_Discount").val();
    }
    else {
        discountMoney = (total * $("#SalesInvoice_Discount").val()) / 100;
    }

    var format2 = $.fn.dataTable.render.number(',', '.', 2, '').display;
    var format3 = $.fn.dataTable.render.number(',', '.', 3, '').display;

    var totalBeforeVat = total - discountMoney;
    var vat = $("#SalesInvoice_VatValue").val() / 100;   
    var vatMoney = totalBeforeVat * vat;
    var totalAfterVat = totalBeforeVat + vatMoney;
    var rest = totalAfterVat - $("#SalesInvoice_PayCash").val() - $("#SalesInvoice_PayBank").val();

    $("#totalBill").text(format3(total));
    $("#SalesInvoice_DiscountMoney").text(format3(discountMoney));
    $("#SalesInvoice_TotalBeforeVat").text(format3(totalBeforeVat));
    $("#SalesInvoice_VatMoney").text(format3(vatMoney));
    $("#SalesInvoice_TotalAfterVat").text(format2(totalAfterVat));
    $("#SalesInvoice_CustomerRest").text(format2(rest));
}

function SaveBill(type) {

    var discountType = "مبلغ";
    if ($("#chkAmount").prop("checked") == false) {
        discountType = "نسبة";
    }

    if ($('tbody').length > 0) {
        var sale = {};
       
        sale.CustomerData_ID = $("#CustomerData_ID").val();
        sale.SalesInvoice_CustomerType = $("#SalesInvoice_CustomerType").val();
        sale.SalesInvoice_BillType = $("#SalesInvoice_BillType").val();
        sale.SalesInvoice_Refrence = $("#SalesInvoice_Refrence").val();
        sale.Branch_ID = $("#Branch_ID").val();
        sale.SalesInvoice_Date = $("#SalesInvoice_Date").val();
        sale.SalesInvoice_CustomerName = $("#SalesInvoice_CustomerName").val();
        sale.SalesInvoice_Phone = $("#SalesInvoice_Phone").val();
        sale.SalesInvoice_Address = $("#SalesInvoice_Address").val();
        sale.SalesInvoice_VatNumber = $("#SalesInvoice_VatNumber").val();
        sale.SalesInvoice_DiscountType = discountType;
        sale.SalesInvoice_Discount = $("#SalesInvoice_Discount").val();
        sale.SalesInvoice_DiscountMoney = $("#SalesInvoice_DiscountMoney").text();
        sale.SalesInvoice_VatValue = $("#SalesInvoice_VatValue").val();
        sale.SalesInvoice_VatMoney = $("#SalesInvoice_VatMoney").text();
        sale.SalesInvoice_Bank = $("#SalesInvoice_Bank").val();
        sale.SalesInvoice_PayCash = $("#SalesInvoice_PayCash").val();
        sale.SalesInvoice_PayBank = $("#SalesInvoice_PayBank").val();
        sale.SalesInvoice_CustomerPay = $("#SalesInvoice_PayCash").val();
        sale.SalesInvoice_CustomerRest = $("#SalesInvoice_CustomerRest").text();
        sale.SalesInvoice_Total = $("#SalesInvoice_TotalAfterVat").text();
        sale.SalesInvoice_Notes = $("#SalesInvoice_Notes").val();

        sale.SalesInvoiceDetail = [];
        $('tbody tr').each(function (i) {
            var obj = {};
            obj.ProductTitle_ID = datatable.cell(i, 2).data();
            obj.ProductBarcode_ID = datatable.cell(i, 3).data();
            obj.ProductBarcode_Unit = datatable.cell(i, 5).data();
            obj.SalesInvoiceDetail_Quantity = $(datatable.cell(i, 6).node()).find('input').val();
            obj.SalesInvoiceDetail_Price = $(datatable.cell(i, 7).node()).find('input').val();           
            obj.SalesInvoiceDetail_Buy = datatable.cell(i, 9).data();
            sale.SalesInvoiceDetail.push(obj);
        });

        var element = document.getElementById('divBody');

        var blockUI = new KTBlockUI(element, {
            overlayClass: "bg-danger bg-opacity-25",
            message: '<div class="blockui-message"><span class="spinner-border text-primary"></span>من فضلك إنتظر ...</div>',
        });
        blockUI.block();

        $.post('/Sales/Create', { modelSales_Invoice: sale, print: type, afterVat: sale.SalesInvoice_TotalAfterVat }, function (data) {
            if (data.isValid == true) {
                swal.fire({
                    title: data.title,
                    text: data.message,
                    icon: "success",
                    showConfirmButton: false,
                    timer: 1500
                });

                datatable.rows().remove().draw();

                $("#SalesInvoice_Date").val(data.date);
                $("#SalesInvoice_Date").flatpickr();

                $("#SalesInvoice_CustomerType").val("عميل نقدى");
                FillCustomer();
                CustomerData();
                $("#SalesInvoice_BillType").val("فاتورة مبيعات نقداً");
                ChangeBillType();
                $("#chkAmount").prop("checked", true);

                $("#totalBill").text("0");
                $("#SalesInvoice_Discount").val(0);
                $("#SalesInvoice_DiscountMoney").val(0);
                $("#SalesInvoice_Net").val(0);
                $("#SalesInvoice_PayCash").val(0);
                $("#SalesInvoice_PayBank").val(0);

                CalcTotal();
                blockUI.release();

                if (type == "cashier") {
                    window.open("/Sales/Print?BillID=" + data.billID);
                }
                if (type == "a4") {
                    window.open("/Sales/PrintA4?BillID=" + data.billID);
                }
                
            } else {
                swal.fire({
                    title: data.title,
                    text: data.message,
                    icon: "error",
                    buttonsStyling: false,
                    confirmButtonText: "موافق",
                    customClass: {
                        confirmButton: "btn fw-bold btn-light-primary"
                    }
                });
                blockUI.release();
            }
        })

        blockUI.destroy();
    }
}

function NetChange() {
    if ($("#SalesInvoice_Net").val() == '' || $("#SalesInvoice_Net").val() == 0) {
        $("#chkAmount").prop("checked", true);
        $("#SalesInvoice_Discount").val(0);
        CalcTotal();
    }
    else {
        $("#chkAmount").prop("checked", true);

        var format = $.fn.dataTable.render.number(',', '.', 3).display;

        var totalAfterVat = $("#SalesInvoice_Net").val();
        var vatPercent = $("#SalesInvoice_VatValue").val() / 100;
        var totalBeforeVat = totalAfterVat / (1 + vatPercent);
        var Vat = totalAfterVat - totalBeforeVat;
        var discount = $("#totalBill").text() - totalBeforeVat;

        $("#SalesInvoice_VatMoney").text(format(Vat));
        $("#SalesInvoice_TotalBeforeVat").text(format(totalBeforeVat));
        $("#SalesInvoice_Discount").val(format(discount));
        CalcTotal();
    }
}
