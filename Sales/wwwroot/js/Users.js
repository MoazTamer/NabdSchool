// ============================================
// Users DataTable & CRUD Operations
// ============================================

"use strict";

var KTUsersList = function () {
    var table;
    var datatable;

    // ============================================
    // Initialize DataTable
    // ============================================
    var initUserTable = function () {
        table = document.querySelector('#kt_table');

        if (!table) {
            return;
        }

        datatable = $(table).DataTable({
            "processing": true,
            "serverSide": true,
            "filter": true,
            "ajax": {
                "url": "/Users/GetData",
                "type": "POST",
                "datatype": "json",
                "error": function (xhr, error, code) {
                    console.log('DataTable Error:', xhr.responseText);
                    toastr.error('حدث خطأ في تحميل البيانات');
                }
            },
            "columnDefs": [
                {
                    "targets": [0],
                    "visible": true,
                    "searchable": false,
                    "orderable": false,
                    "className": "text-center"
                }
            ],
            "columns": [
                {
                    "data": null,
                    "render": function (data, type, row, meta) {
                        return meta.row + meta.settings._iDisplayStart + 1;
                    }
                },
                {
                    "data": "branchName",
                    "name": "branchName",
                    "className": "text-center",
                    "autoWidth": true
                },
                {
                    "data": "userName",
                    "name": "userName",
                    "className": "text-center",
                    "autoWidth": true
                },
                {
                    "data": "userType",
                    "name": "userType",
                    "className": "text-center",
                    "autoWidth": true,
                    "render": function (data) {
                        var badgeClass = '';
                        var displayText = data;

                        switch (data) {
                            case 'Teacher':
                                badgeClass = 'badge-light-primary';
                                displayText = 'مدرس';
                                break;
                            case 'DataEntry':
                                badgeClass = 'badge-light-success';
                                displayText = 'مدخل بيانات';
                                break;
                            case 'Admin':
                                badgeClass = 'badge-light-danger';
                                displayText = 'إدارة';
                                break;
                            default:
                                badgeClass = 'badge-light-info';
                        }
                        return `<span class="badge ${badgeClass} fs-7 fw-bold">${displayText}</span>`;
                    }
                },
                {
                    "data": "id",
                    "orderable": false,
                    "className": "text-center",
                    "render": function (data) {
                        return `<a href="/Users/Permission/${data}" class="btn btn-icon btn-bg-light btn-active-color-primary btn-sm">
                                    <i class="ki-duotone ki-shield-tick fs-2">
                                        <span class="path1"></span>
                                        <span class="path2"></span>
                                    </i>
                                </a>`;
                    }
                },
                {
                    "data": "id",
                    "orderable": false,
                    "className": "text-center",
                    "render": function (data, type, row) {
                        return `<button onclick="EditGet('/Users/Edit/${data}')" class="btn btn-icon btn-bg-light btn-active-color-primary btn-sm me-1">
                                    <i class="ki-duotone ki-pencil fs-2">
                                        <span class="path1"></span>
                                        <span class="path2"></span>
                                    </i>
                                </button>
                                <button onclick="Delete('${data}', '${row.userName}')" class="btn btn-icon btn-bg-light btn-active-color-danger btn-sm">
                                    <i class="ki-duotone ki-trash fs-2">
                                        <span class="path1"></span>
                                        <span class="path2"></span>
                                        <span class="path3"></span>
                                        <span class="path4"></span>
                                        <span class="path5"></span>
                                    </i>
                                </button>`;
                    }
                }
            ],
            "language": {
                "lengthMenu": "عرض _MENU_",
                "zeroRecords": "لم يعثر على أي سجلات",
                "info": "إظهار صفحة _PAGE_ من _PAGES_",
                "infoEmpty": "لا يوجد سجلات متاحة",
                "infoFiltered": "(تصفية من _MAX_ مجموع السجلات)",
                "search": "بحث:",
                "paginate": {
                    "first": "الأول",
                    "last": "الأخير",
                    "next": "التالي",
                    "previous": "السابق"
                },
                "processing": "جارٍ المعالجة..."
            },
            "order": [[2, 'asc']]
        });

        // Search functionality
        const filterSearch = document.querySelector('[data-kt-user-table-filter="search"]');
        if (filterSearch) {
            filterSearch.addEventListener('keyup', function (e) {
                datatable.search(e.target.value).draw();
            });
        }
    }

    return {
        init: function () {
            initUserTable();
        }
    };
}();

// ============================================
// Create User - Get Form
// ============================================
function CreateGet(url) {
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            $('#divModal').html(res);
            $('#kt_modal_add').modal('show');
        },
        error: function (err) {
            console.log('Create Get Error:', err);
            toastr.error('حدث خطأ أثناء تحميل النموذج');
        }
    });
}

// ============================================
// Create User - Post Form
// ============================================
function CreatePost() {
    var form = $('#kt_modal_add_form');
    var submitButton = form.find('[data-kt-modal-action="submit"]');
    var cancelButton = form.find('[data-kt-modal-action="cancel"]');

    if (!form[0].checkValidity()) {
        form[0].reportValidity();
        return false;
    }

    // تحقق من أن الفرع والنوع مختارين - استخدم الأسماء الصحيحة
    var branch = form.find('[name="branchID"]').val();
    var userType = form.find('[name="userType"]').val();
    var userName = form.find('[name="userName"]').val();

    console.log('Form Values:', { branch, userType, userName }); // للتشخيص

    if (!branch || branch === "0" || branch === "") {
        Swal.fire({
            icon: 'warning',
            title: 'تنبيه',
            text: 'من فضلك اختر الفرع أولاً',
            confirmButtonText: 'موافق'
        });
        return false;
    }

    if (!userType || userType === "0" || userType === "") {
        Swal.fire({
            icon: 'warning',
            title: 'تنبيه',
            text: 'من فضلك اختر نوع المستخدم أولاً',
            confirmButtonText: 'موافق'
        });
        return false;
    }

    if (!userName || userName.trim() === "") {
        Swal.fire({
            icon: 'warning',
            title: 'تنبيه',
            text: 'من فضلك أدخل اسم المستخدم',
            confirmButtonText: 'موافق'
        });
        return false;
    }

    // Disable buttons
    submitButton.prop('disabled', true);
    cancelButton.prop('disabled', true);

    // Show SweetAlert loading
    Swal.fire({
        title: 'جاري الحفظ...',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    $.ajax({
        type: 'POST',
        url: form.attr('action'),
        data: form.serialize(),
        success: function (res) {
            Swal.close();

            if (res.isValid) {
                Swal.fire({
                    icon: 'success',
                    title: 'تم الحفظ بنجاح',
                    text: res.message,
                    confirmButtonText: 'موافق'
                }).then(() => {
                    $('#kt_modal_add').modal('hide');
                    $('#kt_table').DataTable().ajax.reload(null, false);
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'فشل الحفظ',
                    text: res.message,
                    confirmButtonText: 'موافق'
                });
            }
        },
        error: function (xhr, status, error) {
            Swal.close();
            console.log('Create Post Error:', xhr.responseText);
            Swal.fire({
                icon: 'error',
                title: 'حدث خطأ',
                text: 'حدث خطأ أثناء الحفظ: ' + (xhr.responseJSON?.message || error),
                confirmButtonText: 'موافق'
            });
        },
        complete: function () {
            submitButton.prop('disabled', false);
            cancelButton.prop('disabled', false);
        }
    });

    return false;
}

// ============================================
// Edit User - Get Form
// ============================================
function EditGet(url) {
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            $('#divModal').html(res);
            $('#kt_modal_edit').modal('show');
        },
        error: function (err) {
            console.log('Edit Get Error:', err);
            toastr.error('حدث خطأ أثناء تحميل بيانات المستخدم');
        }
    });
}

// ============================================
// Edit User - Post Form
// ============================================
function EditPost() {
    var form = $('#kt_modal_edit_form');
    var submitButton = form.find('[data-kt-modal-action="submit"]');
    var cancelButton = form.find('[data-kt-modal-action="cancel"]');

    if (!form[0].checkValidity()) {
        form[0].reportValidity();
        return false;
    }

    // Disable buttons
    submitButton.prop('disabled', true);
    cancelButton.prop('disabled', true);

    // Show SweetAlert loading
    Swal.fire({
        title: 'جاري التعديل...',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    $.ajax({
        type: 'POST',
        url: form.attr('action'),
        data: form.serialize(),
        success: function (res) {
            Swal.close();

            if (res.isValid) {
                Swal.fire({
                    icon: 'success',
                    title: 'تم التعديل بنجاح',
                    text: res.message,
                    confirmButtonText: 'موافق'
                }).then(() => {
                    $('#kt_modal_edit').modal('hide');
                    $('#kt_table').DataTable().ajax.reload(null, false);
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'فشل التعديل',
                    text: res.message,
                    confirmButtonText: 'موافق'
                });
            }
        },
        error: function (xhr, status, error) {
            Swal.close();
            console.log('Edit Post Error:', xhr.responseText);
            Swal.fire({
                icon: 'error',
                title: 'حدث خطأ',
                text: 'حدث خطأ أثناء التعديل: ' + (xhr.responseJSON?.message || error),
                confirmButtonText: 'موافق'
            });
        },
        complete: function () {
            submitButton.prop('disabled', false);
            cancelButton.prop('disabled', false);
        }
    });

    return false;
}

// ============================================
// Delete User
// ============================================
function Delete(id, userName) {
    Swal.fire({
        title: 'هل أنت متأكد؟',
        html: `سيتم حذف المستخدم: <strong>${userName}</strong>`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'نعم، احذف',
        cancelButtonText: 'إلغاء',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: 'POST',
                url: '/Users/Delete',
                data: {
                    id: id,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (res) {
                    if (res.isValid) {
                        toastr.success(res.message);
                        $('#kt_table').DataTable().ajax.reload(null, false);
                    } else {
                        toastr.error(res.message);
                    }
                },
                error: function (err) {
                    console.log('Delete Error:', err);
                    toastr.error('حدث خطأ أثناء الحذف');
                }
            });
        }
    });
}

// ============================================
// Initialize on Page Load
// ============================================
jQuery(document).ready(function () {
    KTUsersList.init();
});