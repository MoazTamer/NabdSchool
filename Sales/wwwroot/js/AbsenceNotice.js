    $(document).ready(function() {

        // إخطار لطالبة واحدة
        $('#singleStudentForm').on('submit', function (e) {
            e.preventDefault();

            const studentId = $('#studentId').val();
            const daysCount = $('#daysCount').val();

            if (!studentId) {
                Swal.fire('تنبيه', 'الرجاء اختيار الطالبة', 'warning');
                return;
            }

            // فتح الملف في نافذة جديدة
            window.open(`/Reports/GenerateAbsenceNotice?studentId=${studentId}&daysCount=${daysCount}`, '_blank');
        });

    // إخطارات جماعية
    $('#bulkNoticesForm').on('submit', function(e) {
        e.preventDefault();

    const date = $('#noticeDate').val();
    const classRoomId = $('#classRoomId').val();

    if (!date) {
        Swal.fire('تنبيه', 'الرجاء اختيار التاريخ', 'warning');
    return;
                }

    Swal.fire({
        title: 'جارٍ التوليد...',
    text: 'يتم الآن توليد الإخطارات الجماعية',
    allowOutsideClick: false,
    showConfirmButton: false,
                    didOpen: () => {
        Swal.showLoading();
                    }
                });

    $.ajax({
        url: '/Reports/GenerateAllAbsenceNotices',
    type: 'GET',
    data: {date: date, classRoomId: classRoomId },
    success: function(response) {
        Swal.fire({
            icon: 'success',
            title: 'تم بنجاح',
            text: response,
            confirmButtonText: 'حسناً'
        });
                    },
    error: function(xhr) {
        Swal.fire({
            icon: 'error',
            title: 'خطأ',
            text: xhr.responseText || 'حدث خطأ أثناء التوليد',
            confirmButtonText: 'حسناً'
        });
                    }
                });
            });

    // إخطار حسب فترة زمنية
    $('#dateRangeForm').on('submit', function(e) {
        e.preventDefault();

    const studentId = $('#studentIdRange').val();
    const fromDate = $('#fromDate').val();
    const toDate = $('#toDate').val();

    if (!studentId || !fromDate || !toDate) {
        Swal.fire('تنبيه', 'الرجاء ملء جميع الحقول', 'warning');
    return;
                }

    window.open(`/Reports/GenerateAbsenceNoticeByDate?studentId=${studentId}&fromDate=${fromDate}&toDate=${toDate}`, '_blank');
            });

        });
