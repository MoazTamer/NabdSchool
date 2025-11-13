"use strict";
var datatable;
var rowIndex;

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
                    'productBarcodePrint_Code': {
                        validators: {
                            notEmpty: {
                                message: 'تأكد من تسجيل الباركود'
                            }
                        }
                    },
                    'productTitle_ID': {
                        validators: {
                            notEmpty: {
                                message: 'تأكد من إختيار إسم الصنف'
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

                        var BarcodeID = $("#ProductBarcode_ID").val();
                        var ProductDate = $("#Product_Date").val();
                        var EndDate = $("#End_Date").val();

                        console.log(ProductDate);
                        console.log(EndDate);

                        axios.get("/Product_Title/PrintBarcode?BarcodeID=" + BarcodeID)
                            .then(function (response) {
                                if (response.data.isValid) {
                                    window.open("/Product_Title/Print?BarcodeID=" + BarcodeID + "&ProductDate=" + ProductDate + "&EndDate=" + EndDate);
                                    // Hide loading indication
                                    submitButton.removeAttribute('data-kt-indicator');
                                    // Enable button
                                    submitButton.disabled = false;
                                    validator.resetForm(); // Reset formvalidation --- more info: https://formvalidation.io/guide/api/reset-form/
                                    blockUI.release();
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
                                    title: "طباعة ستيكر باركود",
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

// On document ready
KTUtil.onDOMContentLoaded(function () {
    var d = new Date();
    $("#Product_Date").val(d.getFullYear() + "/" + (d.getMonth() + 1) + "/" + d.getDate());
    $("#End_Date").val(d.getFullYear() + "/" + (d.getMonth() + 1) + "/" + d.getDate());

    $("#Product_Date").flatpickr();
    $("#End_Date").flatpickr();

    FillUnit(0);
    KTAdd.init();

    var input = document.getElementById("ProductBarcodePrint_Code");
    input.addEventListener("keypress", function (event) {
        if (event.key === "Enter") {
            event.preventDefault();

            axios.get('/Product_Title/Barcode_KeyPress?Barcode=' + $("#ProductBarcodePrint_Code").val())
                .then(function (response) {
                    if (response.data.isValid == true) {
                        $.when($('#ProductTitle_ID').val(response.data.title).trigger('change')).then(function () {
                            FillUnit(response.data.titleBarcode);
                        });                       
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

                        $("#ProductBarcodePrint_Code").val('');
                    }
                })
                .catch(function (error) {
                    swal.fire({
                        title: "طباعة ستيكر باركود",
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

});

function FillUnit(titleBarcode) {
    var ProductID = $("#ProductTitle_ID").val();
    axios.get('/Product_Title/GetUnit?ProductID=' + ProductID)
        .then(function (response) {
            var len = response.data.length;
            $("#ProductBarcode_ID").empty();
            for (var i = 0; i < len; i++) {
                var id = response.data[i]['productBarcode_ID'];
                var name = response.data[i]['productBarcode_Unit'];
                $("#ProductBarcode_ID").append("<option value='" + id + "'>" + name + "</option>");
            }
            if (titleBarcode > 0) {
                $("#ProductBarcode_ID").val(titleBarcode);
            }
            UnitData();
        })
        .catch(function (error) {
            swal.fire({
                title: "طباعة ستيكر باركود",
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
    var BarcodeID = $("#ProductBarcode_ID").val();
    axios.get('/Product_Title/GetUnitData?BarcodeID=' + BarcodeID)
        .then(function (response) {
            $("#ProductBarcodePrint_Code").val(response.data.code);
            $("#ProductBarcodePrint_Count").val(1);
            $("#ProductBarcodePrint_Price").val(response.data.price);
        })
        .catch(function (error) {
            swal.fire({
                title: "طباعة ستيكر باركود",
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
