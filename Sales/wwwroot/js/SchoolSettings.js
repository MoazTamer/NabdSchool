$(document).ready(function () {
    // Handle form submission
    $('#schoolSettingsForm').on('submit', function (e) {
        e.preventDefault();

        var form = $(this);
        var submitBtn = form.find('button[type="submit"]');

        // Disable submit button
        submitBtn.prop('disabled', true);
        submitBtn.html('<span class="spinner-border spinner-border-sm me-2"></span>جاري الحفظ...');

        $.ajax({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            success: function (response) {
                if (response.isValid) {
                    // Update current displays
                    var year = $('#AcademicYear').val();
                    var semester = $('#Semester option:selected').text();
                    var time = $('#AttendanceTime').val();

                    $('#currentYear').text(year + ' هـ');
                    $('#currentSemester').text(semester);
                    $('#currentTime').text(time);

                    // Show success message
                    Swal.fire({
                        icon: 'success',
                        title: response.title,
                        text: response.message,
                        confirmButtonText: 'حسناً',
                        customClass: {
                            confirmButton: 'btn btn-primary'
                        }
                    });
                } else {
                    // Show error message
                    Swal.fire({
                        icon: 'error',
                        title: response.title,
                        text: response.message,
                        confirmButtonText: 'حسناً',
                        customClass: {
                            confirmButton: 'btn btn-danger'
                        }
                    });
                }
            },
            error: function (xhr, status, error) {
                Swal.fire({
                    icon: 'error',
                    title: 'خطأ',
                    text: 'حدث خطأ أثناء الحفظ: ' + error,
                    confirmButtonText: 'حسناً',
                    customClass: {
                        confirmButton: 'btn btn-danger'
                    }
                });
            },
            complete: function () {
                // Re-enable submit button
                submitBtn.prop('disabled', false);
                submitBtn.html('<i class="ki-duotone ki-check fs-2"><span class="path1"></span><span class="path2"></span></i> حفظ جميع الإعدادات');
            }
        });
    });

// Update displays on change
$('#AcademicYear').on('change', function () {
    $('#currentYear').text($(this).val() + ' هـ');
        });

$('#Semester').on('change', function () {
    $('#currentSemester').text($('#Semester option:selected').text());
        });

$('#AttendanceTime').on('change', function () {
    $('#currentTime').text($(this).val());
        });
    });
