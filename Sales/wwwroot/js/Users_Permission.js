"use strict";
var datatable;
var rowIndex;
var arrayId = [];


var KTList = function () {
    // Define shared variables
    var table = document.getElementById('kt_table');

    // Private functions
    var initTable = function () {
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
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
                { targets: [0, 2, 3, 4, 5], orderable: false, searchable: false, className: "text-center" }
            ],
            scrollY: "450px", // Set a fixed height for the table body
            scrollX: true, // Enable horizontal scrolling if needed
            scrollCollapse: true, // Allow the table to be collapsed if the height is less than the defined height
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

var KTAdd = function () {
    // Shared variables
    const form = document.querySelector('#kt_form');

    var blockUI = new KTBlockUI(form, {
        overlayClass: "bg-danger bg-opacity-25",
        message: '<div class="blockui-message"><span class="spinner-border text-primary"></span>من فضلك إنتظر ...</div>',
    });

    // Init add schedule modal
    var initAdd = () => {
        // Submit button handler
        const submitButton = document.querySelector('[data-kt-action="submit"]');
        submitButton.addEventListener('click', e => {
            e.preventDefault();

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
                            window.location.href = "/Users";
                            // Hide loading indication
                            submitButton.removeAttribute('data-kt-indicator');
                            // Enable button
                            submitButton.disabled = false;
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
                });

            blockUI.destroy();
        });
    }

    return {
        // Public functions
        init: function () {
            initAdd();
        }
    };
}();

var KTSelect = function () {
    // Init add schedule modal
    var initAdd = () => {
        // Submit button handler
        const submitButton = document.querySelector('[data-kt-action="selectAll"]');
        submitButton.addEventListener('click', e => {
            e.preventDefault();

            $("#kt_table tbody input[type='checkbox']").prop('checked', true);
        });
    }

    return {
        // Public functions
        init: function () {
            initAdd();
        }
    };
}();

var KTRemove = function () {
    // Shared variables

    // Init add schedule modal
    var initAdd = () => {
        // Submit button handler
        const submitButton = document.querySelector('[data-kt-action="removeAll"]');
        submitButton.addEventListener('click', e => {
            e.preventDefault();
            $("#kt_table tbody input[type='checkbox']").prop('checked', false);
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
    KTList.init();
    KTAdd.init();
    KTSelect.init();
    KTRemove.init();
});
