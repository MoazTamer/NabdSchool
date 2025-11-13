function GetStatisticsBranch() {

    $("#branchChart").empty();
    $("#branchChart").append('<canvas id="kt_charts_widget_branch" class="min-h-auto"></canvas>');

    var ctx = document.getElementById('kt_charts_widget_branch');

    new Chart(ctx, {
        type: 'bar',
        responsive: true,
        processing: true,
        "ajax": {
            "url": "/Home/GetBranchIncom"
        },
        data: {
            labels: ["الرئيسى", "فرعى 1"],
            datasets: [{
                label: 'رسم بيانى لإيرادات الفروع',
                data: [200, 300],
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)',
                    'rgba(255, 159, 64, 0.2)',
                    'rgba(255, 205, 86, 0.2)',
                    'rgba(75, 192, 192, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(153, 102, 255, 0.2)',
                    'rgba(201, 203, 207, 0.2)'
                ],
                borderColor: [
                    'rgb(255, 99, 132)',
                    'rgb(255, 159, 64)',
                    'rgb(255, 205, 86)',
                    'rgb(75, 192, 192)',
                    'rgb(54, 162, 235)',
                    'rgb(153, 102, 255)',
                    'rgb(201, 203, 207)'
                ],
                borderWidth: 1
            }]
        }
    });
};
function GetStatisticsEmployee() {

    $("#employeeChart").empty();
    $("#employeeChart").append('<canvas id="kt_charts_widget_employee" class="min-h-auto"></canvas>');

    var ctx = document.getElementById('kt_charts_widget_employee');

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ["محمد", "احمد", "يوسف"],
            datasets: [{
                label: 'رسم بيانى لإيرادات المستخدمين',
                data: [100, 150, 200],
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)',
                    'rgba(255, 159, 64, 0.2)',
                    'rgba(255, 205, 86, 0.2)',
                    'rgba(75, 192, 192, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(153, 102, 255, 0.2)',
                    'rgba(201, 203, 207, 0.2)'
                ],
                borderColor: [
                    'rgb(255, 99, 132)',
                    'rgb(255, 159, 64)',
                    'rgb(255, 205, 86)',
                    'rgb(75, 192, 192)',
                    'rgb(54, 162, 235)',
                    'rgb(153, 102, 255)',
                    'rgb(201, 203, 207)'
                ],
                borderWidth: 1
            }]
        }
    });
};

KTUtil.onDOMContentLoaded(function () {
    var dt = new Date();

    $("#FromDateBranch").val(dt.toJSON());
    $("#ToDateBranch").val(dt.toJSON());

    $("#FromDateBranch").flatpickr();
    $("#ToDateBranch").flatpickr();

    $("#FromDateEmployee").val(dt.toJSON());
    $("#ToDateEmployee").val(dt.toJSON());

    $("#FromDateEmployee").flatpickr();
    $("#ToDateEmployee").flatpickr();

    GetStatisticsBranch();
    GetStatisticsEmployee();


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
        // فاتورة مبيعات جديدة
        if ((keyCode == 112 || e.key == 'F1') &&
            !(event.altKey || event.ctrlKey || event.shiftKey || event.metaKey)) {
            // prevent code starts here:
            removeDefaultFunction();
            e.cancelable = true;
            e.stopPropagation();
            e.preventDefault();
            e.returnValue = false;
            // Open help window here instead of alert
            window.location.href = "/Sales/Create";
        }
        // Add other F-keys here: سند قبض عميل
        else if ((keyCode == 113 || e.key == 'F2') &&
            !(event.altKey || event.ctrlKey || event.shiftKey || event.metaKey)) {
            // prevent code starts here:
            removeDefaultFunction();
            e.cancelable = true;
            e.stopPropagation();
            e.preventDefault();
            e.returnValue = false;
            // Do something else for F2
            CreateCustomerPaymentGet("/Customer_Data/CreatePayment");
        }
        // Add other F-keys here: فاتورة مشتريات جديدة
        else if ((keyCode == 114 || e.key == 'F3') &&
            !(event.altKey || event.ctrlKey || event.shiftKey || event.metaKey)) {
            // prevent code starts here:
            removeDefaultFunction();
            e.cancelable = true;
            e.stopPropagation();
            e.preventDefault();
            e.returnValue = false;
            // Do something else for F3
            window.location.href = "/Buy/Create";
        }
        // Add other F-keys here: سند صرف مورد
        else if ((keyCode == 114 || e.key == 'F4') &&
            !(event.altKey || event.ctrlKey || event.shiftKey || event.metaKey)) {
            // prevent code starts here:
            removeDefaultFunction();
            e.cancelable = true;
            e.stopPropagation();
            e.preventDefault();
            e.returnValue = false;
            // Do something else for F4
            CreateVendorPaymentGet("/Vendor_Data/CreatePayment");
        }
        // Add other F-keys here: إضافة مصروفات يومية
        else if ((keyCode == 116 || e.key == 'F5') &&
            !(event.altKey || event.ctrlKey || event.shiftKey || event.metaKey)) {
            // prevent code starts here:
            removeDefaultFunction();
            e.cancelable = true;
            e.stopPropagation();
            e.preventDefault();
            e.returnValue = false;
            // Do something else for F5
            CreateOutcomingGet("/Outcoming_Detail/Create");
        }
        else if ((keyCode == 116 || e.key == 'F6') &&
            !(event.altKey || event.ctrlKey || event.shiftKey || event.metaKey)) {
            // prevent code starts here:
            removeDefaultFunction();
            e.cancelable = true;
            e.stopPropagation();
            e.preventDefault();
            e.returnValue = false;
            // Do something else for F6
            CreatePayGet("/Employee_Salary/CreatePay");
        }
    });

});
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
                            title: "تفاصيل الراتب",
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