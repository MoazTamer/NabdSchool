$(document).ready(function () {

    function updateDateTime() {
        const now = new Date();
        const timeString = now.toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' });
        const dateString = now.toLocaleDateString('ar-EG', {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });

        document.getElementById('currentTime').textContent = timeString;
        document.getElementById('currentDate').textContent = dateString;
    }

    updateDateTime();
    setInterval(updateDateTime, 60000);


    const studentCodeInput = $('#studentCodeInput');
    const alertContainer = $('#alertContainer');
    const recentRegistrations = $('#recentRegistrations');
    const registrationsList = $('#registrationsList');
    let registrations = [];

    // Focus on input
    studentCodeInput.focus();

    // Handle Enter key press
    studentCodeInput.on('keypress', function (e) {
        if (e.which === 13) { // Enter key
            e.preventDefault();
            registerAttendance();
        }
    });

    function registerAttendance() {
        const studentCode = studentCodeInput.val().trim();

        if (!studentCode) {
            showAlert('warning', 'الرجاء إدخال كود الطالب');
            return;
        }

        // Show loading
        studentCodeInput.prop('disabled', true);

        $.ajax({
            url: '/Home/RegisterAttendance',
            type: 'POST',
            data: { studentCode: studentCode },
            success: function (response) {
                if (response.isValid) {
                    // Success
                    showAlert('success', response.message);

                    // Add to recent registrations
                    addToRecentList({
                        studentName: response.studentName,
                        className: response.className,
                        time: response.attendanceTime,
                        lateMinutes: response.lateMinutes,
                        status: response.status
                    });

                    // Clear input
                    studentCodeInput.val('');

                    // Play success sound (optional)
                    playSuccessSound();
                } else {
                    // Error
                    showAlert('danger', response.message);
                    playErrorSound();
                }
            },
            error: function (xhr, status, error) {
                showAlert('danger', 'خطأ في الاتصال بالخادم');
                playErrorSound();
            },
            complete: function () {
                studentCodeInput.prop('disabled', false);
                studentCodeInput.focus();
            }
        });
    }

    function showAlert(type, message) {
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                <div class="d-flex align-items-center">
                    <i class="ki-outline ki-${getAlertIcon(type)} fs-2tx text-${type} me-4"></i>
                    <div class="d-flex flex-column">
                        <span class="fw-bold fs-5">${message}</span>
                    </div>
                </div>
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;

        alertContainer.html(alertHtml);

        // Auto dismiss after 5 seconds
        setTimeout(function () {
            alertContainer.find('.alert').fadeOut(function () {
                $(this).remove();
            });
        }, 5000);
    }

    function getAlertIcon(type) {
        switch (type) {
            case 'success':
                return 'check-circle';
            case 'danger':
                return 'cross-circle';
            case 'warning':
                return 'information';
            default:
                return 'information';
        }
    }

    function addToRecentList(data) {
        // Add to array
        registrations.unshift(data);

        // Keep only last 10
        if (registrations.length > 10) {
            registrations.pop();
        }

        // Update UI
        updateRecentList();
        recentRegistrations.show();
    }

    function updateRecentList() {
        let html = '';

        registrations.forEach(function (reg) {
            const statusClass = reg.lateMinutes > 0 ? 'warning' : 'success';
            const statusText = reg.lateMinutes > 0 ?
                `${reg.status} (${reg.lateMinutes} دقيقة)` :
                reg.status;

            html += `
                <tr>
                    <td>
                        <div class="d-flex flex-column">
                            <span class="fw-bold">${reg.studentName}</span>
                            <span class="text-muted fs-7">${reg.className}</span>
                        </div>
                    </td>
                    <td>
                        <span class="badge badge-light-primary">${reg.time}</span>
                    </td>
                    <td>
                        <span class="badge badge-light-${statusClass}">${statusText}</span>
                    </td>
                </tr>
            `;
        });

        registrationsList.html(html);
    }

    function playSuccessSound() {
        // Optional: Add success sound
        // const audio = new Audio('/sounds/success.mp3');
        // audio.play();
    }

    function playErrorSound() {
        // Optional: Add error sound
        // const audio = new Audio('/sounds/error.mp3');
        // audio.play();
    }

    // Clear alert when typing
    studentCodeInput.on('input', function () {
        if (alertContainer.find('.alert').length > 0) {
            alertContainer.find('.alert').fadeOut(function () {
                $(this).remove();
            });
        }
    });
});