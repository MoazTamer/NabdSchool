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
                { targets: [0, 4, 5], orderable: false, searchable: false, className: "text-center" },
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
                    "data": "productBarcode_ID",
                    "render": function (data) {
                        return `
                                <a onclick=Delete() style="cursor:pointer">
                                   <i class="fa-solid fa-trash-can text-danger fs-1"></i>
                                </a>
                           `;
                    }
                }
            ]
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
    $("#ProductTransfer_Date").flatpickr();
    FillToBranch();
    FillUnit();
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

            axios.get('/Product_Transfer/Barcode_KeyPress?Barcode=' + $("#ProductBarcode_Code").val())
                .then(function (response) {
                    if (response.data.isValid == true) {
                        $("#ProductBarcode_Code").val('');
                        $(datatable.row.add(response.data.data).draw().node()).find('td').eq(1).addClass('newItem');

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
                        title: "التحويلات بين الفروع",
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

function FillToBranch() {
    var FromBranchID = $("#FromBranchID").val();
    axios.get('/Product_Transfer/GetToBranch?FromBranchID=' + FromBranchID)
        .then(function (response) {
            var len = response.data.length;
            $("#ToBranchID").empty();
            for (var i = 0; i < len; i++) {
                var id = response.data[i]['branch_ID'];
                var name = response.data[i]['branch_Name'];
                $("#ToBranchID").append("<option value='" + id + "'>" + name + "</option>");
            }
        })
        .catch(function (error) {
            swal.fire({
                title: "التحويلات بين الفروع",
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
    axios.get('/Product_Transfer/GetUnit?ProductID=' + ProductID)
        .then(function (response) {
            var len = response.data.length;
            $("#ProductBarcode_Unit").empty();
            for (var i = 0; i < len; i++) {
                var id = response.data[i]['productBarcode_ID'];
                var name = response.data[i]['productBarcode_Unit'];
                $("#ProductBarcode_Unit").append("<option value='" + id + "'>" + name + "</option>");
            }
        })
        .catch(function (error) {
            swal.fire({
                title: "التحويلات بين الفروع",
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
                        "productBarcode_Quantity": $("#ProductBarcode_Count").val()
                    })
                    .draw();
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
        }
    });

    return false;
}

function SaveBill(type) {

    if ($('tbody').length > 0) {
        var transfer = {};
       
        transfer.ProductTransfer_Date = $("#ProductTransfer_Date").val();
        transfer.FromBranchID = $("#FromBranchID").val();
        transfer.ToBranchID = $("#ToBranchID").val();

        transfer.ProductTransferDetail = [];
        $('tbody tr').each(function (i) {
            var obj = {};
            obj.ProductBarcode_ID = datatable.cell(i, 1).data();
            obj.ProductTransferDetail_Quantity = $(datatable.cell(i, 4).node()).find('input').val();
            transfer.ProductTransferDetail.push(obj);
        });

        var element = document.getElementById('divBody');

        var blockUI = new KTBlockUI(element, {
            overlayClass: "bg-danger bg-opacity-25",
            message: '<div class="blockui-message"><span class="spinner-border text-primary"></span>من فضلك إنتظر ...</div>',
        });
        blockUI.block();

        $.post('/Product_Transfer/Create', { modelProduct_Transfer: transfer }, function (data) {
            if (data.isValid == true) {
                swal.fire({
                    title: data.title,
                    text: data.message,
                    icon: "success",
                    showConfirmButton: false,
                    timer: 1500
                });

                if (type == "cashier") {
                    window.open("/Product_Transfer/Print?BillID=" + data.billID);
                }
                if (type == "a4") {
                    window.open("/Product_Transfer/PrintA4?BillID=" + data.billID);
                }

                window.location.href = "/Product_Transfer";
                
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
