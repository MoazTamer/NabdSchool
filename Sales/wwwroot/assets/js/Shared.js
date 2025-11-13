function EditVatGet(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModelShared').empty();
            $('#divModelShared').html(response.data);
            $('#kt_modal_edit').modal('show');
        })  
}

function EditVatPost() {
    const element = document.getElementById('kt_modal_edit');
    const form = element.querySelector('#kt_modal_edit_form');

    // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
    var validator = FormValidation.formValidation(
        form,
        {
            fields: {
                'vatValue': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل قيمة الضريبة'
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
                                title: "قيمة الضريبة المضافة",
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

//-------------------------------------------------------------------------------

function CreateVendorPaymentGet(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModelShared').empty();
            $('#divModelShared').html(response.data);
            $('#kt_modal_add').modal('show');

            $("#VendorPayment_Date").flatpickr();
            ChangePaymentType();
        })
}

function ChangePaymentType() {
    var Type = $("#VendorPayment_Type").val();
    if (Type === "خصم عام") {
        //$("#CashBalance_ID").val(0);
        $("#CashBalance_ID").attr('disabled', 'disabled');
    }
    else {
        $("#CashBalance_ID").removeAttr('disabled');
    }
}

function CreateVendorPaymentPost() {
    const element = document.getElementById('kt_modal_add');
    const form = element.querySelector('#kt_modal_add_form');

    // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
    var validator = FormValidation.formValidation(
        form,
        {
            fields: {
                'vendorData_ID': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار المورد'
                        }
                    }
                },
                'vendorPayment_Type': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار النوع'
                        }
                    }
                },
                'vendorPayment_Money': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل المبلغ'
                        },
                        greaterThan: {
                            min: 1,
                            message: 'تأكد من تسجيل المبلغ'
                        },
                    }
                },
                'vendorPayment_Number': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل رقم السند'
                        }
                    }
                },
                'cashBalance_ID': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار طريقة الدفع'
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
                                // Hide loading indication
                                submitButton.removeAttribute('data-kt-indicator');
                                // Enable button
                                submitButton.disabled = false;
                                validator.resetForm(); // Reset formvalidation --- more info: https://formvalidation.io/guide/api/reset-form/
                                form.reset(); // Reset form
                                $('#kt_modal_add').modal('hide');
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
                            title: "إضافة سند المورد",
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

//-------------------------------------------------------------------------------

function CreateCustomerPaymentGet(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModelShared').empty();
            $('#divModelShared').html(response.data);
            $('#kt_modal_add').modal('show');

            $("#CustomerPayment_Date").flatpickr();
            ChangePaymentType();
        })
}

function ChangePaymentType() {
    var Type = $("#CustomerPayment_Type").val();
    if (Type === "خصم عام") {
        //$("#CashBalance_ID").val(0);
        $("#CashBalance_ID").attr('disabled', 'disabled');
    }
    else {
        $("#CashBalance_ID").removeAttr('disabled');
    }
}

function CreateCustomerPaymentPost() {
    const element = document.getElementById('kt_modal_add');
    const form = element.querySelector('#kt_modal_add_form');

    // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
    var validator = FormValidation.formValidation(
        form,
        {
            fields: {
                'customerData_ID': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار العميل'
                        }
                    }
                },
                'customerPayment_Type': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار النوع'
                        }
                    }
                },
                'customerPayment_Money': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل المبلغ'
                        },
                        greaterThan: {
                            min: 1,
                            message: 'تأكد من تسجيل المبلغ'
                        },
                    }
                },
                'customerPayment_Number': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل رقم السند'
                        }
                    }
                },
                'cashBalance_ID': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار طريقة الدفع'
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
                                // Hide loading indication
                                submitButton.removeAttribute('data-kt-indicator');
                                // Enable button
                                submitButton.disabled = false;
                                validator.resetForm(); // Reset formvalidation --- more info: https://formvalidation.io/guide/api/reset-form/
                                form.reset(); // Reset form
                                $('#kt_modal_add').modal('hide');
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
                            title: "إضافة سند العميل",
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

//-------------------------------------------------------------------------------

function VatChange() {
    if ($("#moneyVat").prop("checked") == true) {
        var before = ($("#OutcomingDetail_Money").val() / $("#VatPercent").val()).toFixed(2);
        $("#OutcomingDetail_MoneyBeforeVat").val(before);
        var vat = ($("#OutcomingDetail_Money").val() - $("#OutcomingDetail_MoneyBeforeVat").val()).toFixed(2);
        $("#OutcomingDetail_Vat").val(vat);
    }
    else if ($("#moneyVat").prop("checked") == false) {
        $("#OutcomingDetail_Vat").val(0);
        $("#OutcomingDetail_MoneyBeforeVat").val($("#OutcomingDetail_Money").val());
    }
}

function FillOutcomingSub() {
    var MainID = $("#Outcoming_MainID").val();
    axios.get('/Outcoming_Detail/GetOutcoming_Sub?MainID=' + MainID)
        .then(function (response) {
            var len = response.data.length;
            $("#Outcoming_SubID").empty();
            for (var i = 0; i < len; i++) {
                var id = response.data[i]['outcoming_SubID'];
                var name = response.data[i]['outcoming_SubName'];
                $("#Outcoming_SubID").append("<option value='" + id + "'>" + name + "</option>");
            }
        })
        .catch(function (error) {
            swal.fire({
                title: "المصروفات",
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

function CreateOutcomingGet(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModelShared').empty();
            $('#divModelShared').html(response.data);
            $('#kt_modal_add').modal('show');

            $("#OutcomingDetail_Date").flatpickr();
            VatChange();

            $('.decimal').keydown(function (e) {
                //Get the occurence of decimal operator
                var match = $(this).val().match(/\./g);
                if (match != null) {
                    // Allow: backspace, delete, tab, escape and enter 
                    if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110]) !== -1 ||
                        // Allow: Ctrl+A
                        (e.keyCode == 65 && e.ctrlKey === true) ||
                        // Allow: home, end, left, right
                        (e.keyCode >= 35 && e.keyCode <= 39)) {
                        // let it happen, don't do anything
                        return;
                    }  // Ensure that it is a number and stop the keypress
                    else if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105) && (e.keyCode == 190)) {
                        e.preventDefault();
                    }
                }
                else {
                    // Allow: backspace, delete, tab, escape, enter and .
                    if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||
                        // Allow: Ctrl+A
                        (e.keyCode == 65 && e.ctrlKey === true) ||
                        // Allow: home, end, left, right
                        (e.keyCode >= 35 && e.keyCode <= 39)) {
                        // let it happen, don't do anything
                        return;
                    }
                    // Ensure that it is a number and stop the keypress
                    if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                        e.preventDefault();
                    }
                }
            });
            //Allow Upto Two decimal places value only
            $('.decimal').keyup(function () {
                if ($(this).val().indexOf('.') != -1) {
                    if ($(this).val().split(".")[1].length > 2) {
                        if (isNaN(parseFloat(this.value))) return;
                        this.value = parseFloat(this.value).toFixed(2);
                    }
                }
            });

        })
}

function CreateOutcomingPost() {
    const element = document.getElementById('kt_modal_add');
    const form = element.querySelector('#kt_modal_add_form');

    // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
    var validator = FormValidation.formValidation(
        form,
        {
            fields: {
                'outcomingDetail_Money': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل المبلغ'
                        },
                        greaterThan: {
                            min: 1,
                            message: 'تأكد من تسجيل المبلغ'
                        },
                    }
                },
                'outcomingDetail_Date': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار التاريخ'
                        }
                    }
                },
                'outcomingDetail_Number': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل رقم السند'
                        }
                    }
                },
                'outcoming_MainID': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار البند الرئيسى'
                        }
                    }
                },
                'outcoming_SubID': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار البند الفرعى'
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
                'cashBalance_ID': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار طريقة الدفع'
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

                $("#OutcomingDetail_Vat").attr("disabled", false);
                $("#OutcomingDetail_MoneyBeforeVat").attr("disabled", false);

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
                                form.reset(); // Reset form
                                $('#kt_modal_add').modal('hide');
                                blockUI.release();
                                GetTotal();
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

                            $("#OutcomingDetail_Vat").attr("disabled", "disabled");
                            $("#OutcomingDetail_MoneyBeforeVat").attr("disabled", "disabled");

                            // Hide loading indication
                            submitButton.removeAttribute('data-kt-indicator');
                            // Enable button
                            submitButton.disabled = false;
                            blockUI.release();
                        }
                    })
                    .catch(function (error) {
                        swal.fire({
                            title: "المصروفات",
                            text: "من فضلك تأكد من تسجيل البيانات بطريقة صحيحة",
                            icon: "error",
                            buttonsStyling: false,
                            confirmButtonText: "موافق",
                            customClass: {
                                confirmButton: "btn fw-bold btn-light-primary"
                            }
                        });

                        $("#OutcomingDetail_Vat").attr("disabled", "disabled");
                        $("#OutcomingDetail_MoneyBeforeVat").attr("disabled", "disabled");

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

//--------------------------------------------------------------------------------

function EmployeeChange() {
    var EmployeeID = $("#Employee_ID").val();
    axios.get('/Employee_Salary/GetEmployeeSalary?EmployeeID=' + EmployeeID)
        .then(function (response) {
            var len = response.data.length;
            $("#EmployeeSalary_ID").empty();
            for (var i = 0; i < len; i++) {
                var id = response.data[i]['employeeSalary_ID'];
                var name = response.data[i]['employeeSalary_Details'];
                $("#EmployeeSalary_ID").append("<option value='" + id + "'>" + name + "</option>");
            }
        })
        .catch(function (error) {
            swal.fire({
                title: "تفاصيل راتب",
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
function CreateSalaryPayGet(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModal').empty();
            $('#divModal').html(response.data);
            $('#kt_modal_add').modal('show');

            $("#EmployeePay_Date").flatpickr();
            EmployeeChange();
        })
}

function CreateSalaryPayPost() {
    const element = document.getElementById('kt_modal_add');
    const form = element.querySelector('#kt_modal_add_form');

    // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
    var validator = FormValidation.formValidation(
        form,
        {
            fields: {
                'employee_ID': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار إسم الموظف'
                        }
                    }
                },
                'employeeSalary_ID': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار الراتب'
                        }
                    }
                },
                'cashBalance_ID': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار طريقة الدفع'
                        }
                    }
                },
                'employeePay_Money': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل المبلغ'
                        }
                    }
                },
                'employeePay_Date': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار التاريخ'
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
                                // Hide loading indication
                                submitButton.removeAttribute('data-kt-indicator');
                                // Enable button
                                submitButton.disabled = false;
                                validator.resetForm(); // Reset formvalidation --- more info: https://formvalidation.io/guide/api/reset-form/
                                form.reset(); // Reset form
                                $('#kt_modal_add').modal('hide');
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
                            title: "تفاصيل راتب",
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

