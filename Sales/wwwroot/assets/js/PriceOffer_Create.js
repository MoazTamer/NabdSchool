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
                { targets: [0, 5, 6, 7, 8, 9], orderable: false, searchable: false, className: "text-center border border-gray-500" },
                { targets: [1, 3, 4], className: "text-center border border-gray-500" },
                { targets: [2, 8], visible: false },
                { targets: [6, 7], render: $.fn.dataTable.render.number(',', '.', 3) }
            ],
            "columns": [
                { "data": null },
                { "data": "productBarcode_Code" },
                { "data": "productBarcode_ID" },
                { "data": "productTitle_Name" },
                { "data": "productBarcode_Unit" },
                {
                    "data": "productBarcode_Quantity",
                    "render": function (data, type, row) {
                        return `
                                <input type="text" class="text-center quantity w-100" value="${data}" />
                           `;
                    }
                },
                {
                    "data": "productBarcode_PayPrice",
                    "render": function (data, type, row) {
                        return `
                                <input type="text" class="text-center price w-100" value="${data}" />
                           `;
                    }
                },
                {
                    "data": "productBarcode_Total",
                    "render": function (data, type, row) {
                        return `
                                <span type="text" class="fw-semibold fs-5 text-center total">${data}</span>
                           `;
                    }
                },
                { "data": "productBarcode_BuyPrice" },
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
                    var Quantity = $(datatable.cell(rowIndex, 5).node()).find('input').val();
                    var Price = $(datatable.cell(rowIndex, 6).node()).find('input').val();
                    var Total = Quantity * Price;
                    datatable.cell(rowIndex, 7).data(Total);

                    CalcTotal();
                })

                $(".price").on("change", function () {
                    var Quantity = $(datatable.cell(rowIndex, 5).node()).find('input').val();
                    var Price = $(datatable.cell(rowIndex, 6).node()).find('input').val();
                    var Total = Quantity * Price;
                    datatable.cell(rowIndex, 7).data(Total);

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
    $("#TotalProfit").attr('hidden', 'hidden');
    if ($("#UserCategory").val() != "admin") {
        $("#Branch_ID").attr('disabled', 'disabled');
    }
    $("#PriceOffer_Date").flatpickr();
    var vat = $("#lblVat").val() * 100;
    $("#lblVat").text("الضريبة " + $("#PriceOffer_VatValue").val() + " %");
    FillCustomer();
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

            axios.get('/PriceOffer/Barcode_KeyPress?Barcode=' + $("#ProductBarcode_Code").val())
                .then(function (response) {
                    if (response.data.isValid == true) {
                        $("#ProductBarcode_Code").val('');
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

                        $("#ProductBarcode_Code").val('');
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

            $("#PriceOffer_Discount").focus();
        }
        // Add other F-keys here: عرض سعر جديد
        else if ((keyCode == 118 || e.key == 'F7') &&
            !(event.altKey || event.ctrlKey || event.shiftKey || event.metaKey)) {
            // prevent code starts here:
            removeDefaultFunction();
            e.cancelable = true;
            e.stopPropagation();
            e.preventDefault();
            e.returnValue = false;
            // Do something else for F7

            window.open("/PriceOffer/Create");
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
    var pay = $("#ProductBarcode_PayPrice").val() * (($("#PriceOffer_VatValue").val() / 100) + 1);
    $("#ProductBarcode_PayPriceVat").val(format(pay));
}

function PayPricePlusChange() {
    var pay = $("#ProductBarcode_PayPriceVat").val() / (($("#PriceOffer_VatValue").val() / 100) + 1);
    var format = $.fn.dataTable.render.number(',', '.', 3).display;
    $("#ProductBarcode_PayPrice").val(format(pay));
}

function FillCustomer(){
    var Type = $("#PriceOffer_CustomerType").val();
    if (Type === "عميل نقدى") {
        $("#PriceOffer_CustomerName").removeAttr('disabled');
        $("#PriceOffer_Phone").removeAttr('disabled');
        $("#PriceOffer_Address").removeAttr('disabled');
        $("#PriceOffer_VatNumber").removeAttr('disabled');
    }
    else {
        $("#PriceOffer_CustomerName").attr('disabled', 'disabled');
        $("#PriceOffer_Phone").attr('disabled', 'disabled');
        $("#PriceOffer_Address").attr('disabled', 'disabled');
        $("#PriceOffer_VatNumber").attr('disabled', 'disabled');
    }

    axios.get('/PriceOffer/GetCustomer?Type=' + Type)
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
                title: "عرض سعر",
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
    axios.get('/PriceOffer/GetCustomerData?CustomerID=' + CustomerID)
        .then(function (response) {
            $("#PriceOffer_CustomerName").val(response.data.name);
            $("#PriceOffer_Phone").val(response.data.phone);
            $("#PriceOffer_Address").val(response.data.address);
            $("#PriceOffer_VatNumber").val(response.data.vat);
        })
        .catch(function (error) {
            swal.fire({
                title: "عرض سعر",
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

function FillUnit() {
    var ProductID = $("#ProductTitle_ID").val();
    axios.get('/PriceOffer/GetUnit?ProductID=' + ProductID)
        .then(function (response) {
            var len = response.data.length;
            $("#ProductBarcode_Unit").empty();
            for (var i = 0; i < len; i++) {
                var id = response.data[i]['productBarcode_ID'];
                var name = response.data[i]['productBarcode_Unit'];
                $("#ProductBarcode_Unit").append("<option value='" + id + "'>" + name + "</option>");
            }
            UnitData();
        })
        .catch(function (error) {
            swal.fire({
                title: "عرض سعر",
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
    var BarcodeID = $("#ProductBarcode_Unit").val();
    axios.get('/PriceOffer/GetUnitData?BarcodeID=' + BarcodeID)
        .then(function (response) {
            var format = $.fn.dataTable.render.number(',', '.', 3).display;
            $("#ProductBarcode_Count").val(1);
            if ($("#PriceOffer_Pay").val() == "نقدى") {
                $("#ProductBarcode_PayPrice").val(format(response.data.price));
            }
            else {
                $("#ProductBarcode_PayPrice").val(format(response.data.special));
            }
            $("#ProductBarcode_Code").val(response.data.code);
            $("#ProductBarcode_BuyPrice").val(response.data.buy);
            PayPriceChange();
        })
        .catch(function (error) {
            swal.fire({
                title: "عرض سعر",
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
                'priceOffer_Pay': {
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
                datatable.row
                    .add({
                        "productBarcode_Code": $("#ProductBarcode_Code").val(),
                        "productBarcode_ID": $("#ProductBarcode_Unit").val(),
                        "productTitle_Name": $("#ProductTitle_ID option:selected").text(),
                        "productBarcode_Unit": $("#ProductBarcode_Unit option:selected").text(),
                        "productBarcode_Quantity": $("#ProductBarcode_Count").val(),
                        "productBarcode_PayPrice": $("#ProductBarcode_PayPrice").val(),
                        "productBarcode_Total": $("#ProductBarcode_Count").val() * $("#ProductBarcode_PayPrice").val(),
                        "productBarcode_BuyPrice": $("#ProductBarcode_BuyPrice").val()
                    })
                    .draw();

                CalcTotal(); 
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
            total += datatable.cell(i, 7).data();
        })
    }

    var discountMoney = 0;
    if ($("#chkAmount").prop("checked") == true) {
        discountMoney = $("#PriceOffer_Discount").val();
    }
    else {
        discountMoney = (total * $("#PriceOffer_Discount").val()) / 100;
    }

    var format2 = $.fn.dataTable.render.number(',', '.', 2).display;
    var format3 = $.fn.dataTable.render.number(',', '.', 3).display;

    var totalAll = format3(total);
    var discountTotal = format3(discountMoney);

    var totalBeforeVat = totalAll - discountTotal;
    var vat = $("#PriceOffer_VatValue").val() / 100;   
    var vatMoney = totalBeforeVat * vat;
    var totalAfterVat = totalBeforeVat + vatMoney;

    $("#totalBill").text(totalAll);
    $("#PriceOffer_DiscountMoney").text(discountTotal);
    $("#PriceOffer_TotalBeforeVat").text(totalBeforeVat);
    $("#PriceOffer_VatMoney").text(format3(vatMoney));
    $("#PriceOffer_TotalAfterVat").text(format2(totalAfterVat));
}

function SaveBill(type) {

    var discountType = "مبلغ";
    if ($("#chkAmount").prop("checked") == false) {
        discountType = "نسبة";
    }

    if ($('tbody').length > 0) {
        var sale = {};
       
        sale.CustomerData_ID = $("#CustomerData_ID").val();
        sale.PriceOffer_CustomerType = $("#PriceOffer_CustomerType").val();
        sale.Branch_ID = 1;
        sale.PriceOffer_Date = $("#PriceOffer_Date").val();
        sale.PriceOffer_CustomerName = $("#PriceOffer_CustomerName").val();
        sale.PriceOffer_Phone = $("#PriceOffer_Phone").val();
        sale.PriceOffer_Address = $("#PriceOffer_Address").val();
        sale.PriceOffer_VatNumber = $("#PriceOffer_VatNumber").val();
        sale.PriceOffer_DiscountType = discountType;
        sale.PriceOffer_Discount = $("#PriceOffer_Discount").val();
        sale.PriceOffer_DiscountMoney = $("#PriceOffer_DiscountMoney").text();
        sale.PriceOffer_VatValue = $("#PriceOffer_VatValue").val();
        sale.PriceOffer_VatMoney = $("#PriceOffer_VatMoney").text();
        sale.PriceOffer_Total = $("#PriceOffer_TotalAfterVat").text();
        sale.PriceOffer_Notes = $("#PriceOffer_Notes").val();

        sale.PriceOfferDetail = [];
        $('tbody tr').each(function (i) {
            var obj = {};
            obj.ProductBarcode_ID = datatable.cell(i, 2).data();
            obj.PriceOfferDetail_Quantity = $(datatable.cell(i, 5).node()).find('input').val();
            obj.PriceOfferDetail_Price = $(datatable.cell(i, 6).node()).find('input').val();           
            obj.PriceOfferDetail_Buy = datatable.cell(i, 8).data();
            sale.PriceOfferDetail.push(obj);
        });

        var element = document.getElementById('divBody');

        var blockUI = new KTBlockUI(element, {
            overlayClass: "bg-danger bg-opacity-25",
            message: '<div class="blockui-message"><span class="spinner-border text-primary"></span>من فضلك إنتظر ...</div>',
        });
        blockUI.block();

        $.post('/PriceOffer/Create', { modelPriceOffer: sale }, function (data) {
            if (data.isValid == true) {
                swal.fire({
                    title: data.title,
                    text: data.message,
                    icon: "success",
                    showConfirmButton: false,
                    timer: 1500
                });

                datatable.rows().remove().draw();

                $("#PriceOffer_CustomerType").val("عميل نقدى");
                FillCustomer();
                CustomerData();
                $("#PriceOffer_BillType").val("فاتورة مبيعات نقداً");
                $("#chkAmount").prop("checked", true);

                $("#totalBill").text("0");
                $("#PriceOffer_Discount").val(0);
                $("#PriceOffer_DiscountMoney").val(0);

                CalcTotal();
                blockUI.release();

                if (type == "cashier") {
                    window.open("/PriceOffer/Print?BillID=" + data.billID);
                }
                if (type == "a4") {
                    window.open("/PriceOffer/PrintA4?BillID=" + data.billID);
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
