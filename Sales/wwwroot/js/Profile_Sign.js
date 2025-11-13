"use strict";

// Class definition
var KTAccountSettingsSigninMethods = function () {
    var signInForm;
    var signInMainEl;
    var signInEditEl;
    var passwordMainEl;
    var passwordEditEl;
    var signInChangeEmail;
    var signInCancelEmail;
    var passwordChange;
    var passwordCancel;

    var submitSignInButton;

    var toggleChangeEmail = function () {
        signInMainEl.classList.toggle('d-none');
        signInChangeEmail.classList.toggle('d-none');
        signInEditEl.classList.toggle('d-none');
    }

    var toggleChangePassword = function () {
        passwordMainEl.classList.toggle('d-none');
        passwordChange.classList.toggle('d-none');
        passwordEditEl.classList.toggle('d-none');
    }

    // Private functions
    var initSettings = function () {
        if (!signInMainEl) {
            return;
        }

        // toggle UI
        signInChangeEmail.querySelector('button').addEventListener('click', function () {
            toggleChangeEmail();
        });

        signInCancelEmail.addEventListener('click', function () {
            toggleChangeEmail();
        });

        passwordChange.querySelector('button').addEventListener('click', function () {
            toggleChangePassword();
        });

        passwordCancel.addEventListener('click', function () {
            toggleChangePassword();
        });
    }

    var handleChangeEmail = function (e) {
        var validation;

        if (!signInForm) {
            return;
        }

        validation = FormValidation.formValidation(
            signInForm,
            {
                fields: {
                    addNewUserName: {
                        validators: {
                            notEmpty: {
                                message: 'تأكد من تسجيل إسم المستخدم'
                            }
                        }
                    }
                },

                plugins: { //Learn more: https://formvalidation.io/guide/plugins
                    trigger: new FormValidation.plugins.Trigger(),
                    submitSignInButton: new FormValidation.plugins.SubmitButton(),
                    bootstrap: new FormValidation.plugins.Bootstrap5({
                        rowSelector: '.fv-row'
                    })
                }
            }
        );

        submitSignInButton.addEventListener('click', function (e) {
            e.preventDefault();

            validation.validate().then(function (status) {
                if (status == 'Valid') {
                    if (!submitSignInButton.hasAttribute('data-bs-toggle')) {
                        submitSignInButton.setAttribute("data-bs-toggle", "modal");
                        submitSignInButton.setAttribute("data-bs-target", "#kt_modal_change_phone");
                        submitSignInButton.click();
                    }
                }
            });
        });
    }

    var handleConfirmPassword = function (e) {
        var validation;

        // form elements
        var confirmForm = document.getElementById('kt_modal_change_phone_form');
        var submitConfirmButton = confirmForm.querySelector('#kt_confirm_submit');

        var target = document.querySelector("#kt_modal_change_phone");
        var blockUI = new KTBlockUI(target, {
            overlayClass: "bg-danger bg-opacity-25",
            message: '<div class="blockui-message"><span class="spinner-border text-primary"></span>من فضلك إنتظر ...</div>',
        });

        if (!confirmForm) {
            return;
        }

        validation = FormValidation.formValidation(
            confirmForm,
            {
                fields: {
                    signPassword: {
                        validators: {
                            notEmpty: {
                                message: 'تأكد من تسجيل كلمة المرور'
                            }
                        }
                    }
                },

                plugins: { //Learn more: https://formvalidation.io/guide/plugins
                    trigger: new FormValidation.plugins.Trigger(),
                    submitConfirmButton: new FormValidation.plugins.SubmitButton(),
                    bootstrap: new FormValidation.plugins.Bootstrap5({
                        rowSelector: '.fv-row'
                    })
                }
            }
        );

        submitConfirmButton.addEventListener('click', function (e) {
            e.preventDefault();

            validation.validate().then(function (status) {
                if (status == 'Valid') {
                    blockUI.block();

                    // Show loading indication
                    submitConfirmButton.setAttribute('data-kt-indicator', 'on');

                    // Disable button to avoid multiple click 
                    submitConfirmButton.disabled = true;

                    confirmForm.querySelector('#UserNameNew').value = document.getElementById('addNewUserName').value;

                    axios.post(confirmForm.action, new FormData(confirmForm))
                        .then(function (response) {
                            if (response.data.isValid) {
                                swal.fire({
                                    title: response.data.title,
                                    text: response.data.message,
                                    icon: "success",
                                    showConfirmButton: false,
                                    timer: 1500
                                }).then(function () {
                                    $("#SignUserName").text($("#addNewUserName").val())
                                    $("#loggedName").text("مرحبا , " + $("#addNewUserName").val())
                                    // Hide loading indication
                                    submitConfirmButton.removeAttribute('data-kt-indicator');
                                    // Enable button
                                    submitConfirmButton.disabled = false;
                                    confirmForm.reset();
                                    validation.resetForm(); // Reset formvalidation --- more info: https://formvalidation.io/guide/api/reset-form/
                                    $('#kt_modal_change_phone').modal('toggle');
                                    toggleChangeEmail();
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
                                submitConfirmButton.removeAttribute('data-kt-indicator');
                                // Enable button
                                submitConfirmButton.disabled = false;
                                blockUI.release();
                            }
                        })
                        .catch(function (error) {
                            console.log(error.config);
                            swal.fire({
                                title: "تعديل بياناتى",
                                text: "من فضلك تأكد من تسجيل البيانات بطريقة صحيحة",
                                icon: "error",
                                buttonsStyling: false,
                                confirmButtonText: "موافق",
                                customClass: {
                                    confirmButton: "btn fw-bold btn-light-primary"
                                }
                            });
                            // Hide loading indication
                            submitConfirmButton.removeAttribute('data-kt-indicator');
                            // Enable button
                            submitConfirmButton.disabled = false;
                            blockUI.release();
                        });
                }
            });
        });
    }

    var handleChangePassword = function (e) {
        var validation;

        // form elements
        var passwordForm = document.getElementById('kt_signin_change_password');
        var submitPasswordButton = passwordForm.querySelector('#kt_password_submit');

        var target = document.querySelector("#kt_signin_password_edit");
        var blockUI = new KTBlockUI(target, {
            overlayClass: "bg-danger bg-opacity-25",
            message: '<div class="blockui-message"><span class="spinner-border text-primary"></span>من فضلك إنتظر ...</div>',
        });

        if (!passwordForm) {
            return;
        }

        validation = FormValidation.formValidation(
            passwordForm,
            {
                fields: {
                    passwordCurrent: {
                        validators: {
                            notEmpty: {
                                message: 'تأكد من تسجيل كلمة المرور الحالية'
                            }
                        }
                    },

                    passwordNew: {
                        validators: {
                            notEmpty: {
                                message: 'تأكد من تسجيل كلمة المرور الجديدة'
                            },
                            stringLength: {
                                min: 4,
                                message: 'تأكد من تسجيل 4 أحرف على الأقل'
                            }
                        }
                    },

                    passwordConfirm: {
                        validators: {
                            notEmpty: {
                                message: 'تأكد من تسجيل تأكيد كلمة المرور الجديدة'
                            },
                            identical: {
                                compare: function() {
                                    return passwordForm.querySelector('[name="passwordNew"]').value;
                                },
                                message: 'تأكد من تطابق كلمتى المرور'
                            }
                        }
                    },
                },

                plugins: { //Learn more: https://formvalidation.io/guide/plugins
                    trigger: new FormValidation.plugins.Trigger(),
                    submitPasswordButton: new FormValidation.plugins.SubmitButton(),
                    bootstrap: new FormValidation.plugins.Bootstrap5({
                        rowSelector: '.fv-row'
                    })
                }
            }
        );

        submitPasswordButton.addEventListener('click', function (e) {
            e.preventDefault();

            validation.validate().then(function (status) {
                if (status == 'Valid') {
                    blockUI.block();

                    // Show loading indication
                    submitPasswordButton.setAttribute('data-kt-indicator', 'on');

                    // Disable button to avoid multiple click 
                    submitPasswordButton.disabled = true;
                 
                    axios.post(passwordForm.action, new FormData(passwordForm))
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
                                    submitPasswordButton.removeAttribute('data-kt-indicator');
                                    // Enable button
                                    submitPasswordButton.disabled = false;
                                    passwordForm.reset();
                                    validation.resetForm(); // Reset formvalidation --- more info: https://formvalidation.io/guide/api/reset-form/
                                    toggleChangePassword();
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
                                submitPasswordButton.removeAttribute('data-kt-indicator');
                                // Enable button
                                submitPasswordButton.disabled = false;
                                blockUI.release();
                            }
                        })
                        .catch(function (error) {
                            swal.fire({
                                title: "تعديل بياناتى",
                                text: "من فضلك تأكد من تسجيل البيانات بطريقة صحيحة",
                                icon: "error",
                                buttonsStyling: false,
                                confirmButtonText: "موافق",
                                customClass: {
                                    confirmButton: "btn fw-bold btn-light-primary"
                                }
                            });
                            // Hide loading indication
                            submitPasswordButton.removeAttribute('data-kt-indicator');
                            // Enable button
                            submitPasswordButton.disabled = false;
                            blockUI.release();
                        });
                }
            });
        });
    }

    // Public methods
    return {
        init: function () {
            signInForm = document.getElementById('kt_signin_change_email');
            signInMainEl = document.getElementById('kt_signin_email');
            signInEditEl = document.getElementById('kt_signin_email_edit');
            passwordMainEl = document.getElementById('kt_signin_password');
            passwordEditEl = document.getElementById('kt_signin_password_edit');
            signInChangeEmail = document.getElementById('kt_signin_email_button');
            signInCancelEmail = document.getElementById('kt_signin_cancel');
            passwordChange = document.getElementById('kt_signin_password_button');
            passwordCancel = document.getElementById('kt_password_cancel');

            submitSignInButton = signInForm.querySelector('#kt_signin_submit');         
            
            initSettings();
            handleChangeEmail();
            handleConfirmPassword();
            handleChangePassword();
        }
    }
}();

// On document ready
KTUtil.onDOMContentLoaded(function () {
    KTAccountSettingsSigninMethods.init();
});
