var studentsTable;

const API_BASE = '/Student/';

const API_URLS = {
    getStudents: API_BASE + 'GetStudents',
    getClassRooms: API_BASE + 'GetClassRoomsByClass',
    createStudent: API_BASE + 'CreateStudent',
    editStudent: API_BASE + 'EditStudent',
    deleteStudent: API_BASE + 'DeleteStudent',
    importStudents: API_BASE + 'ImportStudents',
    printAllCards: API_BASE + 'PrintStudentCards',
    printSingleCard: API_BASE + 'PrintSingleStudentCard'
};

$(document).ready(function () {
    InitializeDataTable();
    LoadStudents();
});

function InitializeDataTable() {
    studentsTable = $('#studentsTable').DataTable({
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
                "data": null,
                "className": "text-center",
                "render": function (data, type, row) {
                    return `
                        <button type="button" class="btn btn-sm btn-info" onclick="EditStudent(${row.student_ID})">
                            <i class="fas fa-edit"></i>
                        </button>

                        <button type="button" class="btn btn-sm" style="background-color:#f5c6e0; color:#fff; border:none;"
                                onclick="ShowQRCode(${row.student_ID}, '${row.student_Name}', '${row.student_Code}', '${row.student_Phone}')">
                            <i class="fas fa-barcode"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-success"
                            onclick="PrintSingleCard(${row.student_ID}, '${row.student_Name}', '${row.student_Code}', '${row.className}', '${row.classRoomName}')">
                        <i class="fas fa-id-card"></i>
                    </button>

                        `;
                }
            },

        ],
        "order": [[1, "asc"]]
    });
}


function PrintAllCards() {
    var classId = $('#filterClass').val() || '';
    var classRoomId = $('#filterClassRoom').val() || '';

    window.open(`${API_URLS.printAllCards}?classId=${classId}&classRoomId=${classRoomId}`, '_blank');

}


function PrintSingleCard(studentId, studentName, studentCode, className, classRoomName) {
    window.open(`${API_URLS.printSingleCard}?studentId=${studentId}`, '_blank');
}


function ShowQRCode(studentId, studentName, studentCode, studentPhone) {
    // تحديث اسم الطالبة
    document.getElementById('qrStudentName').innerText = studentName;

    const canvas = document.getElementById('modalQRCode');
    canvas.getContext('2d').clearRect(0, 0, canvas.width, canvas.height);

    const studentData = studentCode;


    QRCode.toCanvas(canvas, studentData, {
        width: 200,
        margin: 2,
        errorCorrectionLevel: 'H'
    }, function (error) {
        if (error) console.error(error);
    });

    canvas.dataset.filename = `${studentName}-QR.png`;
    var qrModal = new bootstrap.Modal(document.getElementById('qrCodeModal'));
    qrModal.show();
}

function downloadModalQRCode() {
    const canvas = document.getElementById('modalQRCode');
    const link = document.createElement('a');
    link.href = canvas.toDataURL("image/png");
    link.download = canvas.dataset.filename;
    link.click();
}

function LoadStudents() {
    var classId = $('#filterClass').val();
    var classRoomId = $('#filterClassRoom').val();

    $.ajax({
        url: API_URLS.getStudents,
        type: 'GET',
        data: { classId: classId, classRoomId: classRoomId },
        success: function (response) {
            studentsTable.clear();
            studentsTable.rows.add(response.data);
            studentsTable.draw();
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
        LoadStudents();
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
            LoadStudents();
        }
    });
}

function CreateStudent() {
    $.ajax({
        url: API_URLS.createStudent,
        //url: '@Url.Action("CreateStudent", "Student")',
        type: 'GET',
        success: function (html) {
            $('#modalContainer').html(html);
            $('#studentModal').modal('show');
        }
    });
}

function EditStudent(id) {
    $.ajax({
        url: API_URLS.editStudent,
        //url: '@Url.Action("EditStudent", "Student")',
        type: 'GET',
        data: { id: id },
        success: function (html) {
            $('#modalContainer').html(html);
            $('#studentModal').modal('show');
        }
    });
}

function DeleteStudent(id, name) {
    Swal.fire({
        title: 'تأكيد الحذف',
        text: `هل أنت متأكد من حذف الطالب: ${name}؟`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'نعم، احذف',
        cancelButtonText: 'إلغاء'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: API_URLS.deleteStudent,
                //url: '@Url.Action("DeleteStudent", "Student")',
                type: 'POST',
                data: { id: id },
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    if (response.isValid) {
                        ShowToast('success', response.message);
                        LoadStudents();
                    } else {
                        ShowToast('error', response.message);
                    }
                }
            });
        }
    });
}

function ImportStudents() {
    $.ajax({
        url: API_URLS.importStudents,
        type: 'GET',
        success: function (html) {
            $('#modalContainer').html(html);
            $('#importModal').modal('show');
        }
    });
}

function SaveStudentForm(form) {
    $.ajax({
        url: $(form).attr('action'),
        type: 'POST',
        data: new FormData(form),
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.isValid) {
                $('#studentModal').modal('hide');
                ShowToast('success', response.message);
                LoadStudents();
            } else {
                ShowToast('error', response.message);
            }
        }
    });
    return false;
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




