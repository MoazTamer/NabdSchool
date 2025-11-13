"use strict";
var datatable;
var rowIndex;
var datatableemployee;
var rowIndexemployee;

// On document ready
KTUtil.onDOMContentLoaded(function () {
    KTList.init();
});

var KTList = function () {
    // Define shared variables
    var table = document.getElementById('kt_table');

    // Private functions
    var initTable = function () {
        var Month = $("#SearchMonth").val();
        var Year = $("#SearchYear").val();
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            responsive: true,
            processing: true,
            "ajax": {
                "url": "/Employee_Salary/GetSalary?Month=" + Month + "&Year=" + Year
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
                { targets: [0, 2, 3, 4, 5, 6, 7, 8, 9, 10], orderable: false, searchable: false, className: "text-center" }
            ],
            "columns": [
                { "data": null },
                { "data": "employee_Name" },
                { "data": "employeeSalary_Money" },
                { "data": "employeeSalary_CommissionHouse" },
                { "data": "employeeSalary_CommissionOther" },
                { "data": "totalBonus" },
                { "data": "totalDiscount" },
                { "data": "currentSalary" },
                { "data": "totalPayMonth" },
                { "data": "rest" },
                {
                    "data": "employeeSalary_ID",
                    "render": function (data) {
                        return `
                                <a href="/Employee_Salary/Details/${data}" style="cursor:pointer">
                                   <i class="fa-solid fa-note-sticky text-info fs-1"></i>
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

function jQueryAjaxSearch() {
    datatable.destroy();
    KTList.init();

    //prevent default form submit event
    return false;
}

function CreateGet(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModal').empty();
            $('#divModal').html(response.data);
            $('#kt_modal_add').modal('show');


            var table = document.getElementById('kt_tablecreat');

            datatableemployee = $(table).DataTable({
                responsive: true,
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
                    { targets: [0, 1], orderable: false, searchable: false, className: "text-center" }
                ]
            });

            datatableemployee.on('order.dt search.dt', function () {
                datatableemployee.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
                    cell.innerHTML = i + 1;
                    datatableemployee.cell(cell).invalidate('dom');
                });
            }).draw();


            //KTSelect.init();
            //KTRemove.init();
        })
}

function CreatePost() {
    const element = document.getElementById('kt_modal_add');
    const form = element.querySelector('#kt_modal_add_form');

    // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
    var validator = FormValidation.formValidation(
        form,
        {
            fields: {
                'searchMonth': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار الشهر'
                        }
                    }
                },
                'searchYear': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار العام'
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
                                //datatable.row.add(response.data.data).draw();
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
                            title: "حسابات الموظفين",
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

function SelectAll() {
    const element = document.getElementById('kt_modal_add');
    const form = element.querySelector('#kt_modal_add_form');
    // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
    var validator = FormValidation.formValidation(
        form,
        {
            fields: {
                'searchMonth': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار الشهر'
                        }
                    }
                },
                'searchYear': {
                    validators: {
                        notEmpty: {
                            message: 'تأكد من إختيار العام'
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
    const submitButton = element.querySelector('[data-kt-action="selectAll"]');
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

                axios.post("/Employee_Salary/SelectAll", new FormData(form))
                    .then(function (response) {
                        if (response.data.isValid) {
                            swal.fire({
                                title: response.data.title,
                                text: response.data.message,
                                icon: "success",
                                showConfirmButton: false,
                                timer: 1500
                            }).then(function () {
                                //datatable.row.add(response.data.data).draw();
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
                            title: "الصلاحيات",
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
                    });;
            } else {
                KTUtil.scrollTop();
            }
        });
    }
    return false;
}



