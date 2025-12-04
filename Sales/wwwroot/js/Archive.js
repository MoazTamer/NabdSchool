var deletedStudentsTable;

const API_BASE = '/Student/';

const API_URLS = {
    getDeletedStudents: API_BASE + 'GetDeletedStudents',
    getClassRooms: API_BASE + 'GetClassRoomsByClass',
    restoreStudent: API_BASE + 'RestoreStudent',
    permanentDeleteStudent: API_BASE + 'PermanentDeleteStudent'
};

$(document).ready(function () {
    InitializeDataTable();
    LoadDeletedStudents();
});

function InitializeDataTable() {
    deletedStudentsTable = $('#deletedStudentsTable').DataTable({
        "language": {
            "url": "//cdn.datatables.net/plug-ins/1.11.5/i18n/ar.json"
        },
        "columns": [
            { "data": "student_Code" },
            { "data": "student_Name" },
            { "data": "student_Phone" },
            { "data": "className" },
            { "data": "classRoomName" },
            {
                "data": "deletedBy",
                "render": function (data) {
                    return '<span class="badge badge-warning">' + data + '</span>';
                }
            },
            {
                "data": "deletedDate",
                "render": function (data) {
                    return '<small class="text-dark">' + data + '</small>';
                }
            },
            {
                "data": null,
                "render": function (data, type, row) {
                    return `
                                <button type="button" class="btn btn-sm btn-success" onclick="RestoreStudent(${row.student_ID}, '${row.student_Name}')" title="استعادة">
                                    <i class="fas fa-undo"></i> استعادة
                                </button>
                            `;
                }
            }
        ],
        "order": [[7, "desc"]] // ترتيب حسب تاريخ الحذف (الأحدث أولاً)
    });
}

function LoadDeletedStudents() {
    var classId = $('#filterClass').val();
    var classRoomId = $('#filterClassRoom').val();

    $.ajax({
        url: API_URLS.getDeletedStudents,
        type: 'GET',
        data: { classId: classId, classRoomId: classRoomId },
        success: function (response) {
            deletedStudentsTable.clear();
            deletedStudentsTable.rows.add(response.data);
            deletedStudentsTable.draw();
        },
        error: function (xhr) {
            ShowToast('error', 'خطأ في تحميل البيانات');
        }
    });
}

function LoadClassRooms(classId) {
    var $classRoomSelect = $('#filterClassRoom');
    $classRoomSelect.html('<option value="">الكل</option>');

    if (!classId) {
        LoadDeletedStudents();
        return;
    }

    $.ajax({
        url: API_URLS.getClassRooms,
        type: 'GET',
        data: { classId: classId },
        success: function (response) {
            $.each(response.data, function (index, item) {
                $classRoomSelect.append($('<option>', {
                    value: item.value,
                    text: item.text
                }));
            });
            LoadDeletedStudents();
        }
    });
}

function RestoreStudent(id, name) {
    Swal.fire({
        title: 'تأكيد الاستعادة',
        text: `هل أنت متأكد من استعادة الطالب: ${name}؟`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#28a745',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'نعم، استعد',
        cancelButtonText: 'إلغاء'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: API_URLS.restoreStudent,
                type: 'POST',
                data: { id: id },
                success: function (response) {
                    if (response.isValid) {
                        ShowToast('success', response.message);
                        LoadDeletedStudents();
                    } else {
                        ShowToast('error', response.message);
                    }
                }
            });
        }
    });
}

function PermanentDeleteStudent(id, name) {
    Swal.fire({
        title: 'تحذير: حذف نهائي!',
        html: `
                    <p class="text-danger"><strong>تحذير:</strong> سيتم حذف الطالب نهائياً من قاعدة البيانات!</p>
                    <p>هل أنت متأكد من حذف الطالب: <strong>${name}</strong> نهائياً؟</p>
                    <p class="text-muted"><small>لن تتمكن من استعادة البيانات بعد هذا الإجراء!</small></p>
                `,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'نعم، احذف نهائياً',
        cancelButtonText: 'إلغاء',
        input: 'checkbox',
        inputPlaceholder: 'أنا متأكد من الحذف النهائي',
        inputValidator: (result) => {
            return !result && 'يجب تأكيد الحذف النهائي!'
        }
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: API_URLS.permanentDeleteStudent,
                type: 'POST',
                data: { id: id },
                success: function (response) {
                    if (response.isValid) {
                        ShowToast('success', response.message);
                        LoadDeletedStudents();
                    } else {
                        ShowToast('error', response.message);
                    }
                }
            });
        }
    });
}

function ShowToast(type, message) {
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true
    });

    Toast.fire({
        icon: type,
        title: message
    });
}
