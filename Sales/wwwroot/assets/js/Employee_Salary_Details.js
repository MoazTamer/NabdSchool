"use strict";

var datatablePay;
var rowIndexPay;

var datatableBonus;
var rowIndexBonus;

var datatableDiscount;
var rowIndexDiscount;

function GetTotal() {
    axios.get('/Employee_Salary/GetTotal?SalaryID=' + $("#EmployeeSalary_ID").val())
        .then(function (response) {
            $("#EmployeeSalary_Money").text(response.data.salary.employeeSalary_Money);
            $("#EmployeeSalary_CommissionHouse").text(response.data.salary.employeeSalary_CommissionHouse);
            $("#EmployeeSalary_CommissionOther").text(response.data.salary.employeeSalary_CommissionOther);
            $("#TotalBonus").text(response.data.salary.totalBonus);
            $("#TotalDiscount").text(response.data.salary.totalDiscount);
            $("#CurrentSalary").text(response.data.salary.currentSalary);
            $("#TotalPayMonth").text(response.data.salary.totalPayMonth);
            $("#Rest").text(response.data.salary.rest);
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
        });
};

// On document ready
KTUtil.onDOMContentLoaded(function () {
    GetTotal();
    KTBonus.init();
    KTDiscount.init();
    KTPay.init();
});

//---------------------------------------------------------------------------

var KTPay = function () {
    // Define shared variables
    var table = document.getElementById('kt_pay');

    // Private functions
    var initTable = function () {
        var SalaryID = $("#EmployeeSalary_ID").val();
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatablePay = $(table).DataTable({
            responsive: true,
            processing: true,
            "ajax": {
                "url": "/Employee_Salary/GetEmployeePay?SalaryID=" + SalaryID
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
                { targets: [0, 5, 6], orderable: false, searchable: false, className: "text-center" },
                { targets: [1, 3], className: "text-center" }
            ],
            "columns": [
                { "data": null },
                {
                    "data": "employeePay_Date",
                    render: function (data, type, row) {
                        return moment(data).format('YYYY/MM/DD');
                    }
                },
                { "data": "employeePay_Details" },
                { "data": "employeePay_Money" },
                { "data": "fullName" },
                {
                    "data": "employeePay_ID",
                    "render": function (data) {
                        return `
                                <a onclick=EditSalaryPayGet("/Employee_Salary/EditPay/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-pen-to-square text-success fs-1"></i>
                                </a>
                           `;
                    }
                },
                {
                    "data": "employeePay_ID",
                    "render": function (data) {
                        return `
                                <a onclick=DeleteSalaryPay("/Employee_Salary/DeletePay/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-trash-can text-danger fs-1"></i>
                                </a>
                           `;
                    }
                }
            ]
        });

        datatablePay.on('click', 'tr', function () {
            rowIndexPay = datatablePay.row(this).index();
        });

        datatablePay.on('order.dt search.dt', function () {
            datatablePay.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
                cell.innerHTML = i + 1;
                datatablePay.cell(cell).invalidate('dom');
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
                                GetTotal();
                                datatablePay.row.add(response.data.data).draw();
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

function EditSalaryPayGet(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModal').empty();
            $('#divModal').html(response.data);
            $('#kt_modal_edit').modal('show');

            $("#EmployeePay_Date").flatpickr();
            EmployeeChange();
        })
}

function EditSalaryPayPost() {
    const element = document.getElementById('kt_modal_edit');
    const form = element.querySelector('#kt_modal_edit_form');

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
                                GetTotal();
                                datatablePay.row(rowIndexPay).data(response.data.data).draw();                               
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

function DeleteSalaryPay(url) {
    Swal.fire({
        title: "هل انت متأكد ؟",
        text: "سيتم حذف المبلغ المستلم",
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
                            datatablePay.row(rowIndexPay).remove().draw();
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
                });
        }
    });

    return false;
}

//---------------------------------------------------------------------------------

function EditSalaryGet(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModal').empty();
            $('#divModal').html(response.data);
            $('#kt_modal_edit').modal('show');

            $("#EmployeeSalary_Date").flatpickr();
        })
}

function EditSalaryPost() {
    const element = document.getElementById('kt_modal_edit');
    const form = element.querySelector('#kt_modal_edit_form');

    // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
    var validator = FormValidation.formValidation(
        form,
        {
            fields: {
                'employeeSalary_Money': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل الراتب'
                        }
                    }
                },
                'employeeSalary_CommissionHouse': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل بدل السكن'
                        }
                    }
                },
                'employeeSalary_CommissionOther': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل بدلات أخرى'
                        }
                    }
                },
                'employeeSalary_Date': {
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
                                GetTotal();
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

//---------------------------------------------------------------------------------

var KTBonus= function () {
    // Define shared variables
    var table = document.getElementById('kt_bonus');

    // Private functions
    var initTable = function () {
        var SalaryID = $("#EmployeeSalary_ID").val();
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatableBonus = $(table).DataTable({
            responsive: true,
            processing: true,
            "ajax": {
                "url": "/Employee_Salary/GetEmployeeBonus?SalaryID=" + SalaryID
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
                { targets: [0, 5, 6], orderable: false, searchable: false, className: "text-center" },
                { targets: [1, 3], className: "text-center" }
            ],
            "columns": [
                { "data": null },
                {
                    "data": "employeeBonus_Date",
                    render: function (data, type, row) {
                        return moment(data).format('YYYY/MM/DD');
                    }
                },
                { "data": "employeeBonus_Details" },
                { "data": "employeeBonus_Money" },
                { "data": "fullName" },
                {
                    "data": "employeeBonus_ID",
                    "render": function (data) {
                        return `
                                <a onclick=EditSalaryBonusGet("/Employee_Salary/EditBonus/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-pen-to-square text-success fs-1"></i>
                                </a>
                           `;
                    }
                },
                {
                    "data": "employeeBonus_ID",
                    "render": function (data) {
                        return `
                                <a onclick=DeleteSalaryBonus("/Employee_Salary/DeleteBonus/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-trash-can text-danger fs-1"></i>
                                </a>
                           `;
                    }
                }
            ]
        });

        datatableBonus.on('click', 'tr', function () {
            rowIndexBonus = datatableBonus.row(this).index();
        });

        datatableBonus.on('order.dt search.dt', function () {
            datatableBonus.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
                cell.innerHTML = i + 1;
                datatableBonus.cell(cell).invalidate('dom');
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

function CreateSalaryBonusGet(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModal').empty();
            $('#divModal').html(response.data);
            $('#kt_modal_add').modal('show');

            $("#EmployeeBonus_Date").flatpickr();
        })
}

function CreateSalaryBonusPost() {
    const element = document.getElementById('kt_modal_add');
    const form = element.querySelector('#kt_modal_add_form');

    // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
    var validator = FormValidation.formValidation(
        form,
        {
            fields: {
                'employeeBonus_Money': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل المبلغ'
                        }
                    }
                },
                'employeeBonus_Date': {
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
                                GetTotal();
                                datatableBonus.row.add(response.data.data).draw();
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

function EditSalaryBonusGet(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModal').empty();
            $('#divModal').html(response.data);
            $('#kt_modal_edit').modal('show');

            $("#EmployeeBonus_Date").flatpickr();
        })
}

function EditSalaryBonusPost() {
    const element = document.getElementById('kt_modal_edit');
    const form = element.querySelector('#kt_modal_edit_form');

    // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
    var validator = FormValidation.formValidation(
        form,
        {
            fields: {
                'employeeBonus_Money': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل المبلغ'
                        }
                    }
                },
                'employeeBonus_Date': {
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
                                GetTotal();
                                datatableBonus.row(rowIndexBonus).data(response.data.data).draw();
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

function DeleteSalaryBonus(url) {
    Swal.fire({
        title: "هل انت متأكد ؟",
        text: "سيتم حذف المكافأة",
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
                            datatableBonus.row(rowIndexBonus).remove().draw();
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
                });
        }
    });

    return false;
}

//---------------------------------------------------------------------------------

var KTDiscount = function () {
    // Define shared variables
    var table = document.getElementById('kt_discount');

    // Private functions
    var initTable = function () {
        var SalaryID = $("#EmployeeSalary_ID").val();
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatableDiscount = $(table).DataTable({
            responsive: true,
            processing: true,
            "ajax": {
                "url": "/Employee_Salary/GetEmployeeDiscount?SalaryID=" + SalaryID
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
                { targets: [0, 5, 6], orderable: false, searchable: false, className: "text-center" },
                { targets: [1, 3], className: "text-center" }
            ],
            "columns": [
                { "data": null },
                {
                    "data": "employeeDiscount_Date",
                    render: function (data, type, row) {
                        return moment(data).format('YYYY/MM/DD');
                    }
                },
                { "data": "employeeDiscount_Details" },
                { "data": "employeeDiscount_Money" },
                { "data": "fullName" },
                {
                    "data": "employeeDiscount_ID",
                    "render": function (data) {
                        return `
                                <a onclick=EditSalaryDiscountGet("/Employee_Salary/EditDiscount/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-pen-to-square text-success fs-1"></i>
                                </a>
                           `;
                    }
                },
                {
                    "data": "employeeDiscount_ID",
                    "render": function (data) {
                        return `
                                <a onclick=DeleteSalaryDiscount("/Employee_Salary/DeleteDiscount/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-trash-can text-danger fs-1"></i>
                                </a>
                           `;
                    }
                }
            ]
        });

        datatableDiscount.on('click', 'tr', function () {
            rowIndexDiscount = datatableDiscount.row(this).index();
        });

        datatableDiscount.on('order.dt search.dt', function () {
            datatableDiscount.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
                cell.innerHTML = i + 1;
                datatableDiscount.cell(cell).invalidate('dom');
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

function CreateSalaryDiscountGet(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModal').empty();
            $('#divModal').html(response.data);
            $('#kt_modal_add').modal('show');

            $("#EmployeeDiscount_Date").flatpickr();
        })
}

function CreateSalaryDiscountPost() {
    const element = document.getElementById('kt_modal_add');
    const form = element.querySelector('#kt_modal_add_form');

    // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
    var validator = FormValidation.formValidation(
        form,
        {
            fields: {
                'employeeDiscount_Money': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل المبلغ'
                        }
                    }
                },
                'employeeDiscount_Date': {
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
                                GetTotal();
                                datatableDiscount.row.add(response.data.data).draw();
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

function EditSalaryDiscountGet(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModal').empty();
            $('#divModal').html(response.data);
            $('#kt_modal_edit').modal('show');

            $("#EmployeeDiscount_Date").flatpickr();
        })
}

function EditSalaryDiscountPost() {
    const element = document.getElementById('kt_modal_edit');
    const form = element.querySelector('#kt_modal_edit_form');

    // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
    var validator = FormValidation.formValidation(
        form,
        {
            fields: {
                'employeeDiscount_Money': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من تسجيل المبلغ'
                        }
                    }
                },
                'employeeDiscount_Date': {
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
                                GetTotal();
                                datatableDiscount.row(rowIndexDiscount).data(response.data.data).draw();
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

function DeleteSalaryDiscount(url) {
    Swal.fire({
        title: "هل انت متأكد ؟",
        text: "سيتم حذف الخصم",
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
                            datatableDiscount.row(rowIndexDiscount).remove().draw();
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
                });
        }
    });

    return false;
}