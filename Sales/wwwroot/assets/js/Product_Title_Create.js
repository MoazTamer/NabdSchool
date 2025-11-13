"use strict";
var datatable;
var rowIndex;
var arrayId = [];

// On document ready
var KTAdd = function () {
    // Shared variables
    const form = document.querySelector('#kt_form');

    var blockUI = new KTBlockUI(form, {
        overlayClass: "bg-danger bg-opacity-25",
        message: '<div class="blockui-message"><span class="spinner-border text-primary"></span>من فضلك إنتظر ...</div>',
    });

    // Init add schedule modal
    var initAdd = () => {

        // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
        var validator = FormValidation.formValidation(
            form,
            {
                fields: {
                    'productCategory_ID': {
                        validators: {
                            notEmpty: {
                                message: 'تأكد من إختيار الفئة'
                            }
                        }
                    },
                    'productTitle_Name': {
                        validators: {
                            notEmpty: {
                                message: 'تأكد من تسجيل إسم الصنف'
                            }
                        }
                    },
                    'productTitle_MinimumAmount': {
                        validators: {
                            notEmpty: {
                                message: 'تأكد من تسجيل كمية الحد الأدنى للمخزون'
                            }
                        }
                    },
                    'productTitle_Amount': {
                        validators: {
                            notEmpty: {
                                message: 'تأكد من تسجيل كمية رصيد اول المدة'
                            }
                        }
                    },
                    'productTitle_Date': {
                        validators: {
                            notEmpty: {
                                message: 'تأكد من إختيار التاريخ'
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
        const submitButton = document.querySelector('[data-kt-modal-action="submit"]');
        submitButton.addEventListener('click', e => {
            e.preventDefault();

            // Validate form before submit
            if (validator) {
                validator.validate().then(function (status) {
                    if (status == 'Valid') {
                        blockUI.block();
                        // Show loading indication
                        submitButton.setAttribute('data-kt-indicator', 'on');
                        // Disable button to avoid multiple click 
                        submitButton.disabled = true;

                        axios.post(form.action, new FormData(form))
                            .then(function (response) {
                                if (response.data.isValid) {
                                    swal.fire({
                                        title: response.data.title,
                                        text: response.data.message,
                                        icon: "success",
                                        showConfirmButton: false,
                                        timer: 1500
                                    }).then(function () {
                                        // Hide loading indication
                                        submitButton.removeAttribute('data-kt-indicator');
                                        // Enable button
                                        submitButton.disabled = false;
                                        validator.resetForm(); // Reset formvalidation --- more info: https://formvalidation.io/guide/api/reset-form/                                      
                                        $("#ProductTitle_ID").val(response.data.data);
                                        $("#cardSave").hide();
                                        $("#kt_form_barcode").show();
                                        $("#cardBarcode").show();
                                        KTList.init();
                                        blockUI.release();
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
                                    // Hide loading indication
                                    submitButton.removeAttribute('data-kt-indicator');
                                    // Enable button
                                    submitButton.disabled = false;
                                    blockUI.release();
                                }
                            })
                            .catch(function (error) {
                                swal.fire({
                                    title: "بيانات الأصناف",
                                    text: "من فضلك تأكد من تسجيل البيانات بطريقة صحيحة",
                                    icon: "error",
                                    buttonsStyling: false,
                                    confirmButtonText: "موافق",
                                    customClass: {
                                        confirmButton: "btn fw-bold btn-light-primary"
                                    }
                                });
                                // Hide loading indication
                                submitButton.removeAttribute('data-kt-indicator');
                                // Enable button
                                submitButton.disabled = false;
                                blockUI.release();
                            });
                    } else {
                        KTUtil.scrollTop();
                    }
                });
            }
        });
    }

    return {
        // Public functions
        init: function () {
            initAdd();
        }
    };
}();

var KTAddBarcode = function () {
    // Shared variables
    const form = document.querySelector('#kt_form_barcode');

    var blockUI = new KTBlockUI(form, {
        overlayClass: "bg-danger bg-opacity-25",
        message: '<div class="blockui-message"><span class="spinner-border text-primary"></span>من فضلك إنتظر ...</div>',
    });

    // Init add schedule modal
    var initAdd = () => {

        // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
        var validator = FormValidation.formValidation(
            form,
            {
                fields: {
                    'addProductBarcode_Unit': {
                        validators: {
                            notEmpty: {
                                message: 'تأكد من إختيار الوحدة'
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

        // Submit button handler
        const submitButton = document.querySelector('[data-kt-modal-action-barcode="submit"]');
        submitButton.addEventListener('click', e => {
            e.preventDefault();

            // Validate form before submit
            if (validator) {
                validator.validate().then(function (status) {
                    if (status == 'Valid') {
                        blockUI.block();
                        // Show loading indication
                        submitButton.setAttribute('data-kt-indicator', 'on');
                        // Disable button to avoid multiple click 
                        submitButton.disabled = true;

                        axios.post(form.action, new FormData(form))
                            .then(function (response) {
                                if (response.data.isValid) {
                                    swal.fire({
                                        title: response.data.title,
                                        text: response.data.message,
                                        icon: "success",
                                        showConfirmButton: false,
                                        timer: 1500
                                    }).then(function () {
                                        // Hide loading indication
                                        submitButton.removeAttribute('data-kt-indicator');
                                        // Enable button
                                        submitButton.disabled = false;
                                        validator.resetForm(); // Reset formvalidation --- more info: https://formvalidation.io/guide/api/reset-form/                                      
                                        datatable.row.add(response.data.data).draw();
                                        blockUI.release();
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
                                    // Hide loading indication
                                    submitButton.removeAttribute('data-kt-indicator');
                                    // Enable button
                                    submitButton.disabled = false;
                                    blockUI.release();
                                }
                            })
                            .catch(function (error) {
                                swal.fire({
                                    title: "بيانات الأصناف",
                                    text: "من فضلك تأكد من تسجيل البيانات بطريقة صحيحة",
                                    icon: "error",
                                    buttonsStyling: false,
                                    confirmButtonText: "موافق",
                                    customClass: {
                                        confirmButton: "btn fw-bold btn-light-primary"
                                    }
                                });
                                // Hide loading indication
                                submitButton.removeAttribute('data-kt-indicator');
                                // Enable button
                                submitButton.disabled = false;
                                blockUI.release();
                            });
                    } else {
                        KTUtil.scrollTop();
                    }
                });
            }
        });
    }

    return {
        // Public functions
        init: function () {
            initAdd();
        }
    };
}();

var KTList = function () {
    // Define shared variables
    var table = document.getElementById('kt_table');

    // Private functions
    var initTable = function () {
        var TitleID = $("#ProductTitle_ID").val();
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            responsive: true,
            processing: true,
            "ajax": {
                "url": "/Product_Title/GetBarcode?TitleID=" + TitleID
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
                { targets: [0, 7, 8], orderable: false, searchable: false, className: "text-center" },
                { targets: [2, 3, 4, 5, 6], className: "text-center" }
            ],
            "columns": [
                { "data": null },
                { "data": "productBarcode_Code" },
                { "data": "productBarcode_Unit" },
                { "data": "productBarcode_BuyPrice" },
                { "data": "productBarcode_PayPrice" },
                { "data": "productBarcode_PaySpecial" },
                { "data": "productBarcode_Count" },
                {
                    "data": "productBarcode_ID",
                    "render": function (data) {
                        return `
                                <a onclick=EditGet("/Product_Title/EditBarcode/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-pen-to-square text-success fs-1"></i>
                                </a>
                           `;
                    }
                },
                {
                    "data": "productBarcode_ID",
                    "render": function (data) {
                        return `
                                <a onclick=DeleteBarcode("/Product_Title/DeleteBarcode/${data}") style="cursor:pointer">
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
    $("#kt_form_barcode").hide();
    $("#cardBarcode").hide();
    $("#ProductTitle_Date").flatpickr();
    KTAdd.init();
    KTAddBarcode.init();
});

//-----------------------------------------------

function CreateBarcode(url) {
    axios.post(url)
        .then(function (response) {
            if (response.data.isValid) {
                $("#ProductBarcode_Code").val(response.data.data);
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
                title: "بيانات الأصناف",
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

function CreateAddBarcode(url) {
    axios.post(url)
        .then(function (response) {
            if (response.data.isValid) {
                $("#AddProductBarcode_Code").val(response.data.data);
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
                title: "بيانات الأصناف",
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

function CreateEditdBarcode(url) {
    axios.post(url)
        .then(function (response) {
            if (response.data.isValid) {
                $("#EditProductBarcode_Code").val(response.data.data);
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
                title: "بيانات الأصناف",
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

//-----------------------------------------------

function PayPriceChange() {
    var pay = ($("#ProductBarcode_PayPrice").val() * $("#VatPercent").val()).toFixed(2);
    $("#ProductBarcode_PayPriceVat").val(pay);
}

function PayPricePlusChange() {
    var pay = ($("#ProductBarcode_PayPriceVat").val() / $("#VatPercent").val()).toFixed(2);
    $("#ProductBarcode_PayPrice").val(pay);
}

function PaySpecialChange() {
    var pay = ($("#ProductBarcode_PaySpecial").val() * $("#VatPercent").val()).toFixed(2);
    $("#ProductBarcode_PaySpecialVat").val(pay);
}

function PaySpecialPlusChange() {
    var pay = ($("#ProductBarcode_PaySpecialVat").val() / $("#VatPercent").val()).toFixed(2);
    $("#ProductBarcode_PaySpecial").val(pay);
}

//----------------------------------------------

function AddPayPriceChange() {
    var pay = ($("#AddProductBarcode_PayPrice").val() * $("#VatPercent").val()).toFixed(2);
    $("#AddProductBarcode_PayPriceVat").val(pay);
}

function AddPayPricePlusChange() {
    var pay = ($("#AddProductBarcode_PayPriceVat").val() / $("#VatPercent").val()).toFixed(2);
    $("#AddProductBarcode_PayPrice").val(pay);
}

function AddPaySpecialChange() {
    var pay = ($("#AddProductBarcode_PaySpecial").val() * $("#VatPercent").val()).toFixed(2);
    $("#AddProductBarcode_PaySpecialVat").val(pay);
}

function AddPaySpecialPlusChange() {
    var pay = ($("#AddProductBarcode_PaySpecialVat").val() / $("#VatPercent").val()).toFixed(2);
    $("#AddProductBarcode_PaySpecial").val(pay);
}

//----------------------------------------------

function EditPayPriceChange() {
    var pay = ($("#EditProductBarcode_PayPrice").val() * $("#VatPercent").val()).toFixed(2);
    $("#EditProductBarcode_PayPriceVat").val(pay);
}

function EditPayPricePlusChange() {
    var pay = ($("#EditProductBarcode_PayPriceVat").val() / $("#VatPercent").val()).toFixed(2);
    $("#EditProductBarcode_PayPrice").val(pay);
}

function EditPaySpecialChange() {
    var pay = ($("#EditProductBarcode_PaySpecial").val() * $("#VatPercent").val()).toFixed(2);
    $("#EditProductBarcode_PaySpecialVat").val(pay);
}

function EditPaySpecialPlusChange() {
    var pay = ($("#EditProductBarcode_PaySpecialVat").val() / $("#VatPercent").val()).toFixed(2);
    $("#EditProductBarcode_PaySpecial").val(pay);
}

//----------------------------------------------

function DeleteBarcode(url) {
    Swal.fire({
        title: "هل انت متأكد ؟",
        text: "سيتم حذف وحدة التجزئة",
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
                        title: "بيانات الأصناف",
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
    });

    return false;
}

//-----------------------------------------------

function EditGet(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModal').empty();
            $('#divModal').html(response.data);
            $('#kt_modal_edit').modal('show');
        })
}

function EditPost() {
    const element = document.getElementById('kt_modal_edit');
    const form = element.querySelector('#kt_modal_edit_form');

    // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
    var validator = FormValidation.formValidation(
        form,
        {
            fields: {
                'editProductBarcode_Unit': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار الوحدة'
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
    // Submit button handler
    const submitButton = element.querySelector('[data-kt-modal-action="submit"]');

    // Validate form before submit
    if (validator) {
        validator.validate().then(function (status) {
            if (status == 'Valid') {
                var blockUI = new KTBlockUI(form, {
                    overlayClass: "bg-danger bg-opacity-25",
                    message: '<div class="blockui-message"><span class="spinner-border text-primary"></span>من فضلك إنتظر ...</div>',
                });
                blockUI.block();
                // Show loading indication
                submitButton.setAttribute('data-kt-indicator', 'on');
                // Disable button to avoid multiple click 
                submitButton.disabled = true;

                axios.post(form.action, new FormData(form))
                    .then(function (response) {
                        if (response.data.isValid) {
                            swal.fire({
                                title: response.data.title,
                                text: response.data.message,
                                icon: "success",
                                showConfirmButton: false,
                                timer: 1500
                            }).then(function () {
                                datatable.row(rowIndex).data(response.data.data).draw();
                                // Hide loading indication
                                submitButton.removeAttribute('data-kt-indicator');
                                // Enable button
                                submitButton.disabled = false;
                                validator.resetForm(); // Reset formvalidation --- more info: https://formvalidation.io/guide/api/reset-form/
                                form.reset(); // Reset form
                                $('#kt_modal_edit').modal('hide');
                                blockUI.release();
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
                            // Hide loading indication
                            submitButton.removeAttribute('data-kt-indicator');
                            // Enable button
                            submitButton.disabled = false;
                            blockUI.release();
                        }
                    })
                    .catch(function (error) {
                        swal.fire({
                            title: "بيانات الأصناف",
                            text: "من فضلك تأكد من تسجيل البيانات بطريقة صحيحة",
                            icon: "error",
                            buttonsStyling: false,
                            confirmButtonText: "موافق",
                            customClass: {
                                confirmButton: "btn fw-bold btn-light-primary"
                            }
                        });
                        // Hide loading indication
                        submitButton.removeAttribute('data-kt-indicator');
                        // Enable button
                        submitButton.disabled = false;
                        blockUI.release();
                    });
                blockUI.destroy();
            } else {
                KTUtil.scrollTop();
            }
        });
    }
    return false;
}