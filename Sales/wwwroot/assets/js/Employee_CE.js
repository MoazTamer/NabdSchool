"use strict";

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
                    'fullName': {
                        validators: {
                            notEmpty: {
                                message: 'تأكد من تسجيل إسم الموظف'
                            }
                        }
                    },
                    'branch_ID': {
                        validators: {
                            notEmpty: {
                                message: 'تأكد من إختيار الفرع'
                            }
                        }
                    },
                    'iDNumber': {
                        validators: {
                            notEmpty: {
                                message: 'تأكد من تسجيل رقم الهوية'
                            },
                            stringLength: {
                                min: 10,
                                max: 10,
                                message: 'رقم الهوية يتكون من 10 ارقام'
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
                                        console.log($("#Id").val());
                                        if ($("#Id").val() == 0) {
                                            form.reset();
                                            $("#Date").flatpickr();
                                            $("#DateEnd").flatpickr();
                                        }
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
                                    title: "الموظفين",
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
    $("#Date").flatpickr();
    $("#DateEnd").flatpickr();
    if ($("#Id").val() != 0) {
        $("#Password").attr("type", "password");
    }
    ChangeUserActive();
    KTAdd.init();
});

function ChangeUserActive() {
    if ($("#Active").prop("checked") == true) {
        $("#Password").attr("disabled", false);
    }
    else if ($("#Active").prop("checked") == false) {
        $("#Password").attr("disabled", "disabled");
    }
}