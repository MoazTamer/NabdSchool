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
                { targets: [0, 4, 5, 6, 7, 8, 9], orderable: false, searchable: false, className: "text-center" },
                { targets: [1], visible: false }
            ],
            "columns": [
                { "data": null },
                { "data": "productBarcode_ID" },
                { "data": "productTitle_Name" },
                { "data": "productBarcode_Unit" },
                {
                    "data": "productBarcode_Quantity",
                    "render": function (data, type, row) {
                        return `
                                <input type="text" class="form-control form-control-solid text-center quantity" value="${data}" />
                           `;
                    }
                },
                {
                    "data": "productBarcode_BuyPrice",
                    "render": function (data, type, row) {
                        return `
                                <input type="text" class="form-control form-control-solid text-center price" value="${data}" />
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
                { "data": "freeQuantity" },
                { "data": "productBarcode_PayPrice" },
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
                    var Quantity = $(datatable.cell(rowIndex, 4).node()).find('input').val();
                    var Price = $(datatable.cell(rowIndex, 5).node()).find('input').val();
                    var Total = Quantity * Price;
                    datatable.cell(rowIndex, 6).data(Total);

                    CalcTotal();
                })

                $(".price").on("change", function () {
                    var Quantity = $(datatable.cell(rowIndex, 4).node()).find('input').val();
                    var Price = $(datatable.cell(rowIndex, 5).node()).find('input').val();
                    var Total = Quantity * Price;
                    datatable.cell(rowIndex, 6).data(Total);

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
    $("#BuyInvoice_Date").flatpickr();
    if ($("#UserCategory").val() != "admin") {
        $("#BranchID").attr('disabled', 'disabled');
    }
    GetVendorData();
    FillUnit();
    ChangeBillType();
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

            axios.get('/Buy/Barcode_KeyPress?Barcode=' + $("#ProductBarcode_Code").val())
                .then(function (response) {
                    if (response.data.isValid == true) {
                        $("#ProductBarcode_Code").val('');
                        $(datatable.row.add(response.data.data).draw().node()).find('td').eq(1).addClass('newItem');

                        $("#FreeQuantity").val("0");
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
                        title: "المشتريات",
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
});

function GetVendorData() {
    var VendorID = $("#VendorData_ID").val();
    axios.get('/Buy/GetVendorData?VendorID=' + VendorID)
        .then(function (response) {
            $("#BuyInvoice_SenderName").val(response.data.sender);
        })
        .catch(function (error) {
            swal.fire({
                title: "المشتريات",
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
    axios.get('/Buy/GetUnit?ProductID=' + ProductID)
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
                title: "المشتريات",
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
    axios.get('/Buy/GetUnitData?BarcodeID=' + BarcodeID)
        .then(function (response) {
            $("#ProductBarcode_Count").val(1);
            $("#ProductBarcode_PayPrice").val(response.data.price);
            $("#ProductBarcode_BuyPrice").val(response.data.buy);
        })
        .catch(function (error) {
            swal.fire({
                title: "المشتريات",
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
    var Type = $("#BuyInvoice_BillType").val();
    if (Type === "فاتورة مشتريات كاش" || Type === "فاتورة مرتجعات كاش") {
        $("#CashBalance_ID").removeAttr('disabled');
    }
    else {
        $("#CashBalance_ID").val(0);
        $("#CashBalance_ID").attr('disabled', 'disabled');
    }
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
                'productBarcode_BuyPrice': {
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
                        "productBarcode_ID": $("#ProductBarcode_Unit").val(),
                        "productTitle_Name": $("#ProductTitle_ID option:selected").text(),
                        "productBarcode_Unit": $("#ProductBarcode_Unit option:selected").text(),
                        "productBarcode_Quantity": $("#ProductBarcode_Count").val(),
                        "productBarcode_BuyPrice": $("#ProductBarcode_BuyPrice").val(),
                        "productBarcode_Total": $("#ProductBarcode_Count").val() * $("#ProductBarcode_BuyPrice").val(),
                        "freeQuantity": $("#FreeQuantity").val(),
                        "productBarcode_PayPrice": $("#ProductBarcode_PayPrice").val()
                    })
                    .draw();

                $("#FreeQuantity").val("0");
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
            total += datatable.cell(i, 6).data();
        })
    }

    var discountMoney = 0;
    if ($("#chkAmount").prop("checked") == true) {
        discountMoney = $("#BuyInvoice_Discount").val();
    }
    else {
        discountMoney = (total * $("#BuyInvoice_Discount").val()) / 100;
    }

    var discountTotal = Number(discountMoney).toFixed(3);

    var totalBeforeVat = total - discountTotal;
    var vat = $("#BuyInvoice_VatValue").val() / 100;
    var vatMoney = totalBeforeVat * vat;
    var totalAfterVat = totalBeforeVat + vatMoney;

    $("#totalBill").text(total);
    $("#BuyInvoice_DiscountMoney").text(discountTotal);
    $("#BuyInvoice_TotalBeforeVat").text(totalBeforeVat.toFixed(2));
    $("#BuyInvoice_VatMoney").text(vatMoney.toFixed(2));
    $("#BuyInvoice_TotalAfterVat").text(totalAfterVat.toFixed(2));
}

function SaveBill(type) {

    var discountType = "مبلغ";
    if ($("#chkAmount").prop("checked") == false) {
        discountType = "نسبة";
    }

    if ($('tbody').length > 0) {
        var buy = {};
       
        buy.VendorData_ID = $("#VendorData_ID").val();
        buy.BuyInvoice_BillType = $("#BuyInvoice_BillType").val();
        buy.BuyInvoice_Date = $("#BuyInvoice_Date").val();
        buy.BuyInvoice_Number = $("#BuyInvoice_Number").val();
        buy.BuyInvoice_SenderName = $("#BuyInvoice_SenderName").val();
        buy.BranchID = $("#BranchID").val();
        buy.BuyInvoice_DiscountType = discountType;
        buy.BuyInvoice_Discount = $("#BuyInvoice_Discount").val();
        buy.BuyInvoice_DiscountMoney = $("#BuyInvoice_DiscountMoney").text();
        buy.BuyInvoice_VatValue = $("#BuyInvoice_VatValue").val();
        buy.BuyInvoice_VatMoney = $("#BuyInvoice_VatMoney").text();
        buy.BuyInvoice_TotalAfterVat = $("#BuyInvoice_TotalAfterVat").text();
        buy.CashBalance_ID = $("#CashBalance_ID").val();
        buy.BuyInvoice_Notes = $("#BuyInvoice_Notes").val();

        buy.BuyInvoiceDetail = [];
        $('tbody tr').each(function (i) {
            var obj = {};
            obj.ProductBarcode_ID = datatable.cell(i, 1).data();
            obj.BuyInvoiceDetail_Quantity = $(datatable.cell(i, 4).node()).find('input').val();
            obj.BuyInvoiceDetail_Price = $(datatable.cell(i, 5).node()).find('input').val();   
            obj.BuyInvoiceDetail_FreeQuantity = datatable.cell(i, 7).data();
            obj.BuyInvoiceDetail_PayPrice = datatable.cell(i, 8).data();
            buy.BuyInvoiceDetail.push(obj);
        });

        var element = document.getElementById('divBody');

        var blockUI = new KTBlockUI(element, {
            overlayClass: "bg-danger bg-opacity-25",
            message: '<div class="blockui-message"><span class="spinner-border text-primary"></span>من فضلك إنتظر ...</div>',
        });
        blockUI.block();

        $.post('/Buy/Create', { modelBuy_Invoice: buy, print: type }, function (data) {
            if (data.isValid == true) {
                swal.fire({
                    title: data.title,
                    text: data.message,
                    icon: "success",
                    showConfirmButton: false,
                    timer: 1500
                });

                datatable.rows().remove().draw();

                $("#BuyInvoice_BillType").val("فاتورة مشتريات كاش");
                ChangeBillType();
                $("#FreeQuantity").val("0");
                $("#chkAmount").prop("checked", true);

                $("#totalBill").text("0");
                $("#BuyInvoice_Discount").val(0);
                $("#BuyInvoice_DiscountMoney").val(0);
                $("#BuyInvoice_VatValue").val(0);

                CalcTotal();
                blockUI.release();

                if (type == "cashier") {
                    window.open("/Buy/Print?BillID=" + data.billID);
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
