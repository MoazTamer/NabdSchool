$(document).ready(function () {
    LoadData();
});

function LoadData() {
    $.ajax({
        url: '/Class/GetClasses',
        type: 'GET',
        success: function (response) {
            renderTable(response.data);
        },
        error: function () {
            Swal.fire({
                icon: 'error',
                title: 'خطأ',
                text: 'حدث خطأ أثناء تحميل البيانات'
            });
        }
    });
}

function renderTable(data) {
    let tbody = $('#classTableBody');
    tbody.empty();

    let counter = 1;

    data.forEach(function (classItem) {
        let classRowId = 'class_' + classItem.class_ID;
        let expandIconId = 'expand_' + classItem.class_ID;
        let nestedRowId = 'nested_' + classItem.class_ID;

        let classRow = `
            <tr id="${classRowId}" class="class-row">
                <td class="text-center">${counter++}</td>
                <td class="text-center">
                    <i id="${expandIconId}" class="bi bi-caret-left-fill expand-icon text-primary me-2"></i>
                    ${classItem.class_Name}
                </td>
                <td class="text-center">
                    <button class="btn btn-sm btn-light-primary" onclick="CreateClassRoomGet(${classItem.class_ID})">
                        <i class="bi bi-plus-circle"></i> إضافة فصل
                    </button>
                </td>
                <td class="text-center">
                    <button class="btn btn-icon btn-sm btn-light-success" onclick="EditClassGet(${classItem.class_ID})">
                        <i class="bi bi-pencil-fill"></i>
                    </button>
                </td>
            </tr>
        `;

        tbody.append(classRow);

        // صف الفصول المتداخل
        if (classItem.classRooms && classItem.classRooms.length > 0) {
            let nestedRow = `
                <tr id="${nestedRowId}" class="nested-row" style="display:none;">
                    <td colspan="4" class="p-0">
                        <div class="nested-table p-4">
                            <table class="table table-sm table-row-bordered">
                                <thead>
                                    <tr class="fw-bold fs-7">
                                        <th class="text-center" style="width:10%">م</th>
                                        <th class="text-center" style="width:70%">الفصول</th>
                                        <th class="text-center" style="width:20%">تعديل</th>
                                    </tr>
                                </thead>
                                <tbody>
            `;

            let subCounter = 1;
            classItem.classRooms.forEach(function (room) {
                nestedRow += `
                    <tr>
                        <td class="text-center">${subCounter++}</td>
                        <td class="text-center">
                            ${room.classRoom_Name}
                            <span class="badge badge-light-primary ms-2">${room.studentsCount} طالب</span>
                        </td>
                        <td class="text-center">
                            <button class="btn btn-icon btn-sm btn-light-success" onclick="EditClassRoomGet(${room.classRoom_ID})">
                                <i class="bi bi-pencil-fill"></i>
                            </button>
                        </td>
                    </tr>
                `;
            });

            nestedRow += `
                                </tbody>
                            </table>
                        </div>
                    </td>
                </tr>
            `;

            tbody.append(nestedRow);

            // حدث التوسيع/الطي
            $(`#${expandIconId}`).click(function () {
                $(this).toggleClass('expanded');
                $(`#${nestedRowId}`).toggle();
            });
        }
    });
}

// البحث
$('#searchInput').on('keyup', function () {
    let value = $(this).val().toLowerCase();
    $('.class-row').filter(function () {
        let classMatch = $(this).text().toLowerCase().indexOf(value) > -1;
        let nextRow = $(this).next('.nested-row');

        if (value === '') {
            $(this).show();
            nextRow.hide();
            $(this).find('.expand-icon').removeClass('expanded');
        } else {
            let nestedMatch = nextRow.text().toLowerCase().indexOf(value) > -1;
            $(this).toggle(classMatch || nestedMatch);

            if (nestedMatch && !classMatch) {
                nextRow.show();
                $(this).find('.expand-icon').addClass('expanded');
            }
        }
    });
});

// ================== الصفوف ==================

function CreateClassGet() {
    $.ajax({
        url: '/Class/CreateClass',
        type: 'GET',
        success: function (response) {
            $('#divModal').html(response);
            $('#kt_modal_add').modal('show');
        }
    });
}

function CreateClassPost() {
    event.preventDefault();

    let form = $('#kt_modal_add_form');
    let btn = form.find('[data-kt-modal-action="submit"]');

    btn.attr('data-kt-indicator', 'on');
    btn.prop('disabled', true);

    $.ajax({
        url: form.attr('action'),
        type: 'POST',
        data: form.serialize(),
        success: function (response) {
            btn.removeAttr('data-kt-indicator');
            btn.prop('disabled', false);

            if (response.isValid) {
                $('#kt_modal_add').modal('hide');
                Swal.fire({
                    icon: 'success',
                    title: response.title,
                    text: response.message,
                    timer: 2000
                });
                LoadData();
            } else {
                Swal.fire({
                    icon: 'error',
                    title: response.title,
                    text: response.message
                });
            }
        },
        error: function () {
            btn.removeAttr('data-kt-indicator');
            btn.prop('disabled', false);
            Swal.fire({
                icon: 'error',
                title: 'خطأ',
                text: 'حدث خطأ أثناء الحفظ'
            });
        }
    });

    return false;
}

function EditClassGet(id) {
    $.ajax({
        url: '/Class/EditClass/' + id,
        type: 'GET',
        success: function (response) {
            $('#divModal').html(response);
            $('#kt_modal_edit').modal('show');
        }
    });
}

function EditClassPost() {
    event.preventDefault();

    let form = $('#kt_modal_edit_form');
    let btn = form.find('[data-kt-modal-action="submit"]');

    btn.attr('data-kt-indicator', 'on');
    btn.prop('disabled', true);

    $.ajax({
        url: form.attr('action'),
        type: 'POST',
        data: form.serialize(),
        success: function (response) {
            btn.removeAttr('data-kt-indicator');
            btn.prop('disabled', false);

            if (response.isValid) {
                $('#kt_modal_edit').modal('hide');
                Swal.fire({
                    icon: 'success',
                    title: response.title,
                    text: response.message,
                    timer: 2000
                });
                LoadData();
            } else {
                Swal.fire({
                    icon: 'error',
                    title: response.title,
                    text: response.message
                });
            }
        },
        error: function () {
            btn.removeAttr('data-kt-indicator');
            btn.prop('disabled', false);
            Swal.fire({
                icon: 'error',
                title: 'خطأ',
                text: 'حدث خطأ أثناء الحفظ'
            });
        }
    });

    return false;
}

function DeleteClass(id) {
    Swal.fire({
        title: 'هل أنت متأكد؟',
        text: "سيتم حذف الصف وجميع الفصول التابعة له!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'نعم، احذف',
        cancelButtonText: 'إلغاء'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Class/DeleteClass/' + id,
                type: 'POST',
                success: function (response) {
                    if (response.isValid) {
                        $('#kt_modal_edit').modal('hide');
                        Swal.fire({
                            icon: 'success',
                            title: response.title,
                            text: response.message,
                            timer: 2000
                        });
                        LoadData();
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: response.title,
                            text: response.message
                        });
                    }
                }
            });
        }
    });
}

// ================== الفصول ==================

function CreateClassRoomGet(classId) {
    $.ajax({
        url: '/Class/CreateClassRoom?classId=' + classId,
        type: 'GET',
        success: function (response) {
            $('#divModal').html(response);
            $('#kt_modal_add_room').modal('show');
        }
    });
}

function CreateClassRoomPost() {
    event.preventDefault();

    let form = $('#kt_modal_add_room_form');
    let btn = form.find('[data-kt-modal-action="submit"]');

    btn.attr('data-kt-indicator', 'on');
    btn.prop('disabled', true);

    $.ajax({
        url: form.attr('action'),
        type: 'POST',
        data: form.serialize(),
        success: function (response) {
            btn.removeAttr('data-kt-indicator');
            btn.prop('disabled', false);

            if (response.isValid) {
                $('#kt_modal_add_room').modal('hide');
                Swal.fire({
                    icon: 'success',
                    title: response.title,
                    text: response.message,
                    timer: 2000
                });
                LoadData();
            } else {
                Swal.fire({
                    icon: 'error',
                    title: response.title,
                    text: response.message
                });
            }
        },
        error: function () {
            btn.removeAttr('data-kt-indicator');
            btn.prop('disabled', false);
            Swal.fire({
                icon: 'error',
                title: 'خطأ',
                text: 'حدث خطأ أثناء الحفظ'
            });
        }
    });

    return false;
}

function EditClassRoomGet(id) {
    $.ajax({
        url: '/Class/EditClassRoom/' + id,
        type: 'GET',
        success: function (response) {
            $('#divModal').html(response);
            $('#kt_modal_edit_room').modal('show');
        }
    });
}

function EditClassRoomPost() {
    event.preventDefault();

    let form = $('#kt_modal_edit_room_form');
    let btn = form.find('[data-kt-modal-action="submit"]');

    btn.attr('data-kt-indicator', 'on');
    btn.prop('disabled', true);

    $.ajax({
        url: form.attr('action'),
        type: 'POST',
        data: form.serialize(),
        success: function (response) {
            btn.removeAttr('data-kt-indicator');
            btn.prop('disabled', false);

            if (response.isValid) {
                $('#kt_modal_edit_room').modal('hide');
                Swal.fire({
                    icon: 'success',
                    title: response.title,
                    text: response.message,
                    timer: 2000
                });
                LoadData();
            } else {
                Swal.fire({
                    icon: 'error',
                    title: response.title,
                    text: response.message
                });
            }
        },
        error: function () {
            btn.removeAttr('data-kt-indicator');
            btn.prop('disabled', false);
            Swal.fire({
                icon: 'error',
                title: 'خطأ',
                text: 'حدث خطأ أثناء الحفظ'
            });
        }
    });

    return false;
}

function DeleteClassRoom(id) {
    Swal.fire({
        title: 'هل أنت متأكد؟',
        text: "سيتم حذف الفصل!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'نعم، احذف',
        cancelButtonText: 'إلغاء'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Class/DeleteClassRoom/' + id,
                type: 'POST',
                success: function (response) {
                    if (response.isValid) {
                        $('#kt_modal_edit_room').modal('hide');
                        Swal.fire({
                            icon: 'success',
                            title: response.title,
                            text: response.message,
                            timer: 2000
                        });
                        LoadData();
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: response.title,
                            text: response.message
                        });
                    }
                }
            });
        }
    });
}