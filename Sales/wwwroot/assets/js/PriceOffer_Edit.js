"use strict";
var datatable;
var rowIndex;

// On document ready
var KTList = function () {
    // Define shared variables
    var table = document.getElementById('kt_table');

    // Private functions
    var initTable = function () {
        var OfferID = $("#PriceOffer_ID").val();
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            responsive: true,
            processing: true,
            "ajax": {
                "url": "/PriceOffer/GetOfferProduct/" + OfferID
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
                { targets: [0, 4, 5, 6, 7, 8], orderable: false, searchable: false, className: "text-center" },
                { targets: [1, 7, 8], visible: false },
                { targets: [6, 7], render: $.fn.dataTable.render.number(',', '.', 3) }
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
                    "data": "productBarcode_PayPrice",
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
                { "data": "productBarcode_BuyPrice" },
                { "data": "priceOfferDetail_ID" },
                {
                    "data": "priceOfferDetail_ID",
                    "render": function (data, type, row) {
                        if (row.priceOfferDetail_ID == 0) {
                            return `
                                <a onclick=Delete("0") style="cursor:pointer">
                                   <i class="fa-solid fa-trash-can text-danger fs-1"></i>
                                </a>
                           `;
                        }
                        else {
                            return `
                                <a onclick=Delete("/PriceOffer/DeleteDetail/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-trash-can text-danger fs-1"></i>
                                </a>
                           `;
                        }
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
            },
            "fnInitComplete": function (oSettings, json) {
                CalcTotal();
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
    $("#PriceOffer_Date").flatpickr();
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
                            title: "عرض الأسعار",
                            text: "هذا الصنف غير موجود",
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
                        title: "عرض الأسعار",
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

function FillCustomer() {
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
                title: "عرض الأسعار",
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
            $("#PriceOffere_CustomerName").val(response.data.name);
            $("#PriceOffer_Phone").val(response.data.phone);
            $("#PriceOffer_Address").val(response.data.address);
            $("#PriceOffer_VatNumber").val(response.data.vat);
        })
        .catch(function (error) {
            swal.fire({
                title: "عرض الأسعار",
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
                title: "عرض الأسعار",
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
            if ($("#SalesInvoice_Pay").val() == "نقدى") {
                $("#ProductBarcode_PayPrice").val(format(response.data.price));
            }
            else {
                $("#ProductBarcode_PayPrice").val(format(response.data.special));
            }
            $("#ProductBarcode_BuyPrice").val(response.data.buy);
        })
        .catch(function (error) {
            swal.fire({
                title: "عرض الأسعار",
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
    // Submit button handler
    /*    const submitButton = element.querySelector('[data-kt-modal-action="submit"]');*/

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
                        "productBarcode_PayPrice": $("#ProductBarcode_PayPrice").val(),
                        "productBarcode_Total": $("#ProductBarcode_Count").val() * $("#ProductBarcode_PayPrice").val(),
                        "productBarcode_BuyPrice": $("#ProductBarcode_BuyPrice").val(),
                        "priceOfferDetail_ID": 0
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

function Delete(url) {
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
            if (url == 0) {
                datatable.row(rowIndex).remove().draw();
                CalcTotal();
            }
            else {
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
                                CalcTotal();
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
                            title: "عرض الأسعار",
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
        var offer = {};

        offer.PriceOffer_ID = $("#PriceOffer_ID").val();
        offer.CustomerData_ID = $("#CustomerData_ID").val();
        offer.PriceOffer_CustomerType = $("#PriceOffer_CustomerType").val();
        offer.Branch_ID = 1;
        offer.PriceOffer_Date = $("#PriceOffer_Date").val();
        offer.PriceOffer_CustomerName = $("#PriceOffer_CustomerName").val();
        offer.PriceOffer_Phone = $("#PriceOffer_Phone").val();
        offer.PriceOffer_Address = $("#PriceOffer_Address").val();
        offer.PriceOffer_VatNumber = $("#PriceOffer_VatNumber").val();
        offer.PriceOffer_DiscountType = discountType;
        offer.PriceOffer_Discount = $("#PriceOffer_Discount").val();
        offer.PriceOffer_DiscountMoney = $("#PriceOffer_DiscountMoney").text();
        offer.PriceOffer_VatValue = $("#PriceOffer_VatValue").val();
        offer.PriceOffer_VatMoney = $("#PriceOffer_VatMoney").text();
        offer.PriceOffer_Total = $("#PriceOffer_TotalAfterVat").text();
        offer.PriceOffer_Notes = $("#PriceOffer_Notes").val();

        offer.PriceOfferDetail = [];
        $('tbody tr').each(function (i) {
            var obj = {};
            obj.ProductBarcode_ID = datatable.cell(i, 1).data();
            obj.PriceOfferDetail_Quantity = $(datatable.cell(i, 4).node()).find('input').val();
            obj.PriceOfferDetail_Price = $(datatable.cell(i, 5).node()).find('input').val();
            obj.PriceOfferDetail_Buy = datatable.cell(i, 7).data();
            obj.PriceOfferDetail_ID = datatable.cell(i, 8).data();
            offer.PriceOfferDetail.push(obj);
        });

        var element = document.getElementById('divBody');

        var blockUI = new KTBlockUI(element, {
            overlayClass: "bg-danger bg-opacity-25",
            message: '<div class="blockui-message"><span class="spinner-border text-primary"></span>من فضلك إنتظر ...</div>',
        });
        blockUI.block();

        $.post('/PriceOffer/Edit', { modelPriceOffer: offer, print: type, afterVat: offer.PriceOffer_TotalAfterVat }, function (data) {
            if (data.isValid == true) {
                swal.fire({
                    title: data.title,
                    text: data.message,
                    icon: "success",
                    showConfirmButton: false,
                    timer: 1500
                });

                if (type == "cashier") {
                    window.open("/PriceOffer/Print?BillID=" + data.billID);
                }
                if (type == "a4") {
                    window.open("/PriceOffer/PrintA4?BillID=" + data.billID);
                }

                window.location.href = "/Home";
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
