"use strict";
var datatable;
var rowIndex;

function FillOutcomingSub() {
    var MainID = $("#Outcoming_MainSearch").val();
    axios.get('/Outcoming_Detail/GetOutcoming_Sub?MainID=' + MainID)
        .then(function (response) {
            var len = response.data.length;
            $("#Outcoming_SubSearch").empty();
            $("#Outcoming_SubSearch").append("<option selected value='all'>بنود المصروفات الفرعية</option>");
            for (var i = 0; i < len; i++) {
                var id = response.data[i]['outcoming_SubID'];
                var name = response.data[i]['outcoming_SubName'];
                $("#Outcoming_SubSearch").append("<option value='" + name + "'>" + name + "</option>");
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

var KTList = function () {
    // Define shared variables
    var table = document.getElementById('kt_table');

    // Private functions
    var initTable = function () {
        var MainID = $("#Outcoming_MainSearch").val();
        var DateFrom = $("#OutcomingDetail_DateFrom").val();
        var DateTo = $("#OutcomingDetail_DateTo").val();
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            responsive: true,
            processing: true,
            "ajax": {
                "url": "/Outcoming_Detail/GetOutcoming?MainID=" + MainID + "&DateFrom=" + DateFrom + "&DateTo=" + DateTo
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
                { targets: [0, 9, 10], orderable: false, searchable: false, className: "text-center" },
                { targets: [5, 6, 7, 8], className: "text-center" },
                { targets: [11], visible: false }
            ],
            "columns": [
                { "data": null },
                { "data": "outcoming_MainName" },
                { "data": "outcoming_SubName" },
                { "data": "branch_Name" },
                { "data": "cashBalance_Name" },
                { "data": "outcomingDetail_Number" },
                {
                    "data": "outcomingDetail_Date",
                    render: function (data, type, row) {
                        return moment(data).format('YYYY/MM/DD');
                    }
                },
                { "data": "outcomingDetail_Money" },
                { "data": "outcomingDetail_Vat" },
                {
                    "data": "outcomingDetail_ID",
                    "render": function (data) {
                        return `
                                <a onclick=EditGet("/Outcoming_Detail/Edit/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-pen-to-square text-success fs-1"></i>
                                </a>
                           `;
                    }
                },
                {
                    "data": "outcomingDetail_ID",
                    "render": function (data) {
                        return `
                                <a onclick=Delete("/Outcoming_Detail/Delete/${data}") style="cursor:pointer">
                                   <i class="fa-solid fa-trash-can text-danger fs-1"></i>
                                </a>
                           `;
                    }
                },
                { "data": "cashBalanceType_Name" },
            ],
            "footerCallback": function (row, data, start, end, display) {
                var api = this.api();

                // converting to interger to find total
                var intVal = function (i) {
                    return typeof i === 'string' ?
                        i.replace(/[\$,]/g, '') * 1 :
                        typeof i === 'number' ?
                            i : 0;
                };

                // computing column Total of the complete result 
                var outcome = api
                    .column(7, { page: 'all', search: 'applied' })
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0).toFixed(2);

                // Update footer by showing the total with the reference of the column index 
                $(api.column(0).footer()).html('');
                $(api.column(1).footer()).html('');
                $(api.column(2).footer()).html('');
                $(api.column(3).footer()).html('');
                $(api.column(4).footer()).html('');
                $(api.column(5).footer()).html('');
                $(api.column(6).footer()).html('الإجمالى');
                $(api.column(7).footer()).html(outcome);
                $(api.column(8).footer()).html('');
                $(api.column(9).footer()).html('');
                $(api.column(10).footer()).html('');
                $(api.column(11).footer()).html('');
            },
        });

        // Re-init functions on every table re-draw -- more info: https://datatables.net/reference/event/draw
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

    // Handle Active filter dropdown
    var handleBranchFilter = () => {
        const filterBranch = document.querySelector('[data-kt-filter="branch"]');
        $(filterBranch).on('change', e => {
            let value = e.target.value;
            if (value === 'all') {
                value = '';
            }
            datatable.column(3).search(value).draw();
        });
    }

    // Handle Active filter dropdown
    var handleSubFilter = () => {
        const filterSub = document.querySelector('[data-kt-filter="sub"]');
        $(filterSub).on('change', e => {
            let value = e.target.value;
            if (value === 'all') {
                value = '';
            }
            datatable.column(2).search(value).draw();
        });
    }

    // Handle Center filter dropdown
    var handleCashFilter = () => {
        const filterCash = document.querySelector('[data-kt-filter="cash"]');
        $(filterCash).on('change', e => {
            let value = e.target.value;
            if (value === 'all') {
                value = '';
            }
            datatable.column(4).search(value).draw();
        });
    }

    // Handle Center filter dropdown
    var handleCashTypeFilter = () => {
        const filterCashType = document.querySelector('[data-kt-filter="cashType"]');
        $(filterCashType).on('change', e => {
            let value = e.target.value;
            if (value === 'all') {
                value = '';
            }
            datatable.column(11).search(value).draw();
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
            handleBranchFilter();
            handleSubFilter();
            handleCashFilter();
            handleCashTypeFilter();
        }
    }
}();

// On document ready
KTUtil.onDOMContentLoaded(function () {
    $("#OutcomingDetail_DateFrom").flatpickr();
    $("#OutcomingDetail_DateTo").flatpickr();
    KTList.init();
});

function jQueryAjaxSearch() {
    datatable.destroy();
    KTList.init();
    FillOutcomingSub();
    //prevent default form submit event
    return false;
}

function EditGet(url) {
    axios.get(url)
        .then(function (response) {
            $('#divModal').empty();
            $('#divModal').html(response.data);
            $('#kt_modal_edit').modal('show');

            $("#OutcomingDetail_Date").flatpickr();

            if ($("#OutcomingDetail_Vat").val() == 0) {
                $("#moneyVat").attr("checked", false);
            }
            FillOutcomingSubEdit();

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

function EditPost() {
    const element = document.getElementById('kt_modal_edit');
    const form = element.querySelector('#kt_modal_edit_form');

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

function FillOutcomingSubEdit() {
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

function Delete(url) {
    Swal.fire({
        title: "هل انت متأكد ؟",
        text: "سيتم حذف المصروف",
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
    });

    return false;
}