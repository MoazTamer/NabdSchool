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
                { targets: [0, 5, 6, 7, 8], orderable: false, searchable: false, className: "text-center border border-gray-500" },
                { targets: [1, 3, 4], className: "text-center border border-gray-500" },
                { targets: [2], visible: false }
            ],
            "columns": [
                { "data": null },
                { "data": "productBarcode_Code" },
                { "data": "productBarcode_ID" },
                { "data": "productTitle_Name" },
                { "data": "productBarcode_Unit" },
                {
                    "data": "productOffer_Count",
                    "render": function (data, type, row) {
                        return `
                                <input type="text" class="text-center quantity w-100" value="${data}" />
                           `;
                    }
                },
                { "data": "productBarcode_BuyPrice" },
                { "data": "productBarcode_Total" },
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
                    var format3 = $.fn.dataTable.render.number('', '.', 2, '').display;

                    var count = $(datatable.cell(rowIndex, 5).node()).find('input').val();
                    var buy = $(datatable.cell(rowIndex, 6).node()).text();
                    var Total = count * buy;

                    datatable.cell(rowIndex, 7).data(format3(Total));
                    CalcTotal();
                })
            },
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

            axios.get('/Product_Title/Barcode_KeyPress?Barcode=' + $("#ProductBarcode_Code").val())
                .then(function (response) {
                    if (response.data.isValid == true) {

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

                        blockUI.release();
                    }
                })
                .catch(function (error) {
                    swal.fire({
                        title: "العروض",
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

function PayPriceChange() {
    var format = $.fn.dataTable.render.number(',', '.', 3).display;
    var pay = $("#ProductOffer_Price").val() * (($("#ProductOffer_VatValue").val() / 100) + 1);
    $("#ProductOffer_PriceVat").val(format(pay));
}

function PayPricePlusChange() {
    var pay = $("#ProductOffer_PriceVat").val() / (($("#ProductOffer_VatValue").val() / 100) + 1);
    var format = $.fn.dataTable.render.number(',', '.', 3).display;
    $("#ProductOffer_Price").val(format(pay));
}

function CreateAddBarcode(url) {
    axios.post(url)
        .then(function (response) {
            if (response.data.isValid) {
                $("#ProductOffer_Code").val(response.data.data);
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
                title: "العروض",
                text: "من فضلك تأكد من تسجيل البيانات بطريقة صحيحة",
                icon: "error",
                buttonsStyling: false,
                confirmButtonText: "موافق",
                customClass: {
                    confirmButton: "btn fw-bold btn-light-primary"
                }
            });

        });

    return false;
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
            }
            UnitData();
        })
        .catch(function (error) {
            swal.fire({
                title: "العروض",
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
    axios.get('/Product_Title/GetUnitData?BarcodeID=' + BarcodeID)
        .then(function (response) {
            $("#ProductBarcode_Code").val(response.data.code);
            $("#ProductOffer_Count").val(1);
            $("#ProductBarcode_BuyPrice").text(response.data.buy);
            $("#ProductBarcode_Total").text(response.data.buy);
        })
        .catch(function (error) {
            swal.fire({
                title: "العروض",
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

function ChangeCount() {  
    var format = $.fn.dataTable.render.number(',', '.', 2).display;
    var total = $("#ProductOffer_Count").val() * $("#ProductBarcode_BuyPrice").text();
    $("#ProductBarcode_Total").text(format(total));
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
                'productOffer_Count': {
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
                        "productBarcode_Code": $("#ProductBarcode_Code").val(),
                        "productBarcode_ID": $("#ProductBarcode_Unit").val(),
                        "productTitle_Name": $("#ProductTitle_ID option:selected").text(),
                        "productBarcode_Unit": $("#ProductBarcode_Unit option:selected").text(),
                        "productOffer_Count": $("#ProductOffer_Count").val(),
                        "productBarcode_BuyPrice": $("#ProductBarcode_BuyPrice").text(),
                        "productBarcode_Total": $("#ProductOffer_Count").val() * $("#ProductBarcode_BuyPrice").text()
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
            total += parseFloat(datatable.cell(i, 7).data());
        })
    }

    var format3 = $.fn.dataTable.render.number(',', '.', 3, '').display;

    $("#totalBuy").text(format3(total));
}

function SaveBill() {

    if ($('tbody').length > 0) {
        var offer = {};
       
        offer.ProductCategory_ID = $("#ProductCategory_ID").val();
        offer.ProductOffer_Name = $("#ProductOffer_Name").val();
        offer.ProductOffer_Code = $("#ProductOffer_Code").val();
        offer.ProductOffer_Price = $("#ProductOffer_Price").val();

        offer.productOffer_Detail = [];
        $('tbody tr').each(function (i) {
            var obj = {};
            obj.ProductBarcode_ID = $(datatable.cell(i, 2).node()).text();
            obj.ProductOffer_Count = $(datatable.cell(i, 5).node()).find('input').val();
            obj.ProductOffer_BuyPrice = $(datatable.cell(i, 6).node()).text();
            offer.productOffer_Detail.push(obj);
        });

        var element = document.getElementById('kt_app_content_container');

        var blockUI = new KTBlockUI(element, {
            overlayClass: "bg-danger bg-opacity-25",
            message: '<div class="blockui-message"><span class="spinner-border text-primary"></span>من فضلك إنتظر ...</div>',
        });
        blockUI.block();

        $.post('/Product_Title/Offer_Create', { modelProduct_Offer: offer }, function (data) {
            if (data.isValid == true) {
                swal.fire({
                    title: data.title,
                    text: data.message,
                    icon: "success",
                    showConfirmButton: false,
                    timer: 1500
                });

                window.location.href="/Product_Title/Offer"
                
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
