document.addEventListener("DOMContentLoaded", function () {

    // قراءة القيم اللي جاية من السيرفر
    let totalStudents = window.dashboardData.totalStudents;
    let presentCount = window.dashboardData.present;
    let absentCount = window.dashboardData.absent;

    // الوقت والتاريخ
    function updateDateTime() {
        const now = new Date();
        document.getElementById('currentTime').textContent =
            now.toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit', second: '2-digit' });

        document.getElementById('currentDate').textContent =
            now.toLocaleDateString('ar-EG', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
    }

    updateDateTime();
    setInterval(updateDateTime, 1000);

    // الإحصائيات
    function updateStatistics() {
        const presentPercentage = totalStudents > 0 ? Math.round((presentCount / totalStudents) * 100) : 0;
        const absentPercentage = totalStudents > 0 ? Math.round((absentCount / totalStudents) * 100) : 0;

        document.getElementById('attendancePercentage').textContent = presentPercentage + '%';
        document.getElementById('chartPercentage').textContent = presentPercentage + '%';
    }

    // الرسم البياني
    const ctx = document.getElementById('attendanceChart');
    if (ctx) {
        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['حاضر', 'غائب'],
                datasets: [{
                    data: [presentCount, absentCount],
                    backgroundColor: ['#50cd89', '#f1416c'],
                    borderWidth: 0
                }]
            },
            options: {
                legend: { display: false },
                cutout: '75%'
            }
        });
    }

    updateStatistics();
});



function registerAllStudents() {

    Swal.fire({
        title: 'جارٍ التسجيل...',
        text: 'يتم الآن تسجيل حضور جميع الطلاب',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    $.ajax({
        url: '/Home/RegistrationAllStudents',
        type: 'POST',
        success: function (response) {

            if (response.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'تم بنجاح',
                    text: response.message,
                    confirmButtonText: 'تمام'
                }).then(() => {
                    location.reload();
                });

            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'خطأ',
                    text: response.message
                });
            }
        },
        error: function () {
            Swal.fire({
                icon: 'error',
                title: 'خطأ في الاتصال',
                text: 'تأكد من الاتصال وحاول مرة أخرى'
            });
        }
    });
}




$(document).ready(function () {
    initializeTopStudentsTable();

    $('#badgeFilter').on('change', function () {
        const filterValue = $(this).val();
        if (filterValue) {
            $('#topStudentsTable').DataTable().column(4).search(filterValue).draw();
        } else {
            $('#topStudentsTable').DataTable().column(4).search('').draw();
        }
    });

    $('#searchBtn').on('click', function () {
        const searchValue = $('#searchInput').val();
        $('#topStudentsTable').DataTable().search(searchValue).draw();
    });

    $('#searchInput').on('keyup', function (e) {
        if (e.key === 'Enter') {
            $('#topStudentsTable').DataTable().search($(this).val()).draw();
        }
    });
});

function initializeTopStudentsTable() {
    const table = $('#topStudentsTable').DataTable({
        processing: true,
        serverSide: false,
        responsive: true,
        paging: true,
        pageLength: 10,
        lengthMenu: [[5, 10, 25, 50], [5, 10, 25, 50]],
        language: {
            "search": "بحث:",
            "lengthMenu": "عرض _MENU_ طالبة",
            "zeroRecords": "لا توجد بيانات",
            "info": "عرض _START_ إلى _END_ من _TOTAL_ طالبة",
            "infoEmpty": "عرض 0 إلى 0 من 0 طالبة",
            "infoFiltered": "(تمت الفلترة من _MAX_ إجمالي طالبات)",
            "paginate": {
                "first": "الأول",
                "last": "الأخير",
                "next": "التالي",
                "previous": "السابق"
            },
            "processing": "جارٍ التحميل..."
        },
        ajax: {
            url: '/Home/GetTopStudentsData',
            type: 'GET',
            dataSrc: function (response) {
                if (response.success) {
                    updateBadgeStats(response.stats);
                    return response.data;
                } else {
                    console.error('Error loading data:', response.message);
                    showError('حدث خطأ في تحميل البيانات');
                    return [];
                }
            },
            error: function (xhr, error, thrown) {
                console.error('AJAX Error:', error, thrown);
                showError('حدث خطأ في تحميل البيانات');
                return [];
            }
        },
        columns: [
            {
                data: null,
                className: "text-center",
                orderable: false,
                render: function (data, type, row, meta) {
                    const index = meta.row;
                    let rankIcon = '';

                    if (index === 0) {
                        rankIcon = '🥇';
                        return `<span class="badge badge-circle badge-lg badge-light-warning fw-bold fs-4">${rankIcon}</span>`;
                    } else if (index === 1) {
                        rankIcon = '🥈';
                        return `<span class="badge badge-circle badge-lg badge-light-secondary fw-bold fs-4">${rankIcon}</span>`;
                    } else if (index === 2) {
                        rankIcon = '🥉';
                        return `<span class="badge badge-circle badge-lg badge-light-danger fw-bold fs-4">${rankIcon}</span>`;
                    } else {
                        return `<span class="badge badge-circle badge-lg badge-light fw-bold fs-4">${index + 1}</span>`;
                    }
                }
            },
            {
                data: "student_Name", 
                className: "",
                render: function (data, type, row) {
                    const studentName = data || 'غير معروف';
                    const badgeColor = row.badgeColor || '#6c757d'; 
                    const highestBadge = row.highestBadgeLevel || '';
                    const studentId = row.student_ID || 0; 

                    const rowIndex = row.Rank || row.__rowIndex || 0;
                    let badgeClass = 'badge-light-primary';
                    if (rowIndex === 0) badgeClass = 'badge-light-warning';
                    else if (rowIndex === 1) badgeClass = 'badge-light-secondary';
                    else if (rowIndex === 2) badgeClass = 'badge-light-danger';

                    const firstLetter = studentName.trim().substring(0, 1) || 'غ';

                    const hasNegativePoints = row.total_Points < 0;

                    return `
            <div class="d-flex align-items-center">
                <div class="symbol symbol-45px me-3" style="border: 2px solid ${badgeColor};">
                    <div class="symbol-label fs-3 fw-bold" style="background: ${badgeColor}; color: white;">
                        ${firstLetter}
                    </div>
                </div>
                <div>
                    <span class="fw-bold text-gray-800 fs-5 d-block">
                        ${studentName}
                    </span>
                    ${hasNegativePoints ? `
                        <span class="badge badge-light-danger fs-8 d-block mt-1">
                            لم تسجل حضور اليوم
                        </span>
                    ` : highestBadge ? `
                        <span class="badge ${badgeClass} fs-8 d-block mt-1">
                            ${highestBadge}
                        </span>
                    ` : ''}
                </div>
            </div>
        `;
                }
            },
            {
                data: "total_Points",
                className: "text-center",
                render: function (data, type, row) {
                    const points = parseFloat(data) || 0;

                    if (points < 0) {
                        return `
                <span class="badge badge-light-danger fs-6 fw-bold px-4 py-3">
                    ${Math.abs(points)} نقطة سلبية
                </span>
            `;
                    }
                    else if (points === 0) {
                        return `
                <span class="badge badge-light-warning fs-6 fw-bold px-4 py-3">
                    لم تسجل نقاط
                </span>
            `;
                    }
                    else {
                        return `
                <span class="badge badge-light-success fs-4 fw-bold px-4 py-3">
                    ${points} نقطة
                </span>
            `;
                    }
                }
            },
            //{
            //    data: "attendance_Streak",
            //    className: "text-center",
            //    render: function (data) {
            //        const streak = parseInt(data) || 0;

            //        if (streak <= 0) {
            //            return `
            //    <div class="d-flex flex-column align-items-center">
            //        <i class="ki-outline ki-cross-circle text-danger fs-2x mb-1"></i>
            //        <span class="fw-bold text-danger fs-5">0</span>
            //        <span class="text-gray-600 fs-8">يوم</span>
            //    </div>
            //`;
            //        } else {
            //            return `
            //    <div class="d-flex flex-column align-items-center">
            //        <i class="ki-outline ki-chart-simple text-primary fs-2x mb-1"></i>
            //        <span class="fw-bold text-primary fs-5">${streak}</span>
            //        <span class="text-gray-600 fs-8">يوم</span>
            //    </div>
            //`;
            //        }
            //    }
            //},
            {
                data: "badges",
                className: "",
                render: function (data) {
                    if (!data || data.length === 0) {
                        return `
                <div class="text-center">
                    <span class="badge badge-light-warning fs-7">
                        <i class="ki-outline ki-information fs-4 me-1"></i>
                        لا توجد شارات
                    </span>
                </div>
            `;
                    }

                    let badgesHtml = '';
                    const badgesToShow = data.slice(0, 4);

                    badgesToShow.forEach(badge => {
                        const badgeIcon = badge.Badge_Icon || badge.badge_Icon || 'star';
                        const badgeColor = badge.Badge_Color || badge.badge_Color || '#6c757d';
                        const badgeName = badge.Badge_Name || badge.badge_Name || '';
                        const badgeLevel = badge.Badge_Level || badge.badge_Level || '';

                        badgesHtml += `
                <div class="badge-item" style="background: ${badgeColor};"
                     title="${badgeName} - ${badgeLevel}">
                    <i class="ki-outline ki-${badgeIcon} text-white fs-3"></i>
                </div>
            `;
                    });

                    if (data.length > 4) {
                        badgesHtml += `
                <div class="badge badge-light-primary fw-bold">
                    +${data.length - 4}
                </div>
            `;
                    }

                    return `<div class="d-flex flex-wrap justify-content-center gap-2">${badgesHtml}</div>`;
                }
            },
            {
                data: "Student_ID",
                className: "text-center",
                orderable: false,
                render: function (data, type, row) {
                    return `
                        <button onclick="showStudentBadges(${data})" 
                                class="btn btn-sm btn-light-primary">
                            <i class="ki-outline ki-eye"></i>
                            التفاصيل
                        </button>
                    `;
                }
            }
        ],
        order: [[2, 'desc']], 
        initComplete: function () {
            this.api().rows().every(function (rowIdx) {
                const row = this.node();
                if (rowIdx === 0) {
                    $(row).addClass('gold-rank');
                } else if (rowIdx === 1) {
                    $(row).addClass('silver-rank');
                } else if (rowIdx === 2) {
                    $(row).addClass('bronze-rank');
                }
            });
        },
        drawCallback: function () {
            this.api().rows().every(function (rowIdx) {
                const row = this.node();
                $(row).removeClass('gold-rank silver-rank bronze-rank');

                if (rowIdx === 0) {
                    $(row).addClass('gold-rank');
                } else if (rowIdx === 1) {
                    $(row).addClass('silver-rank');
                } else if (rowIdx === 2) {
                    $(row).addClass('bronze-rank');
                }
            });
        }
    });

    new $.fn.dataTable.Buttons(table, {
        buttons: [
            {
                extend: 'excelHtml5',
                text: 'تصدير Excel',
                className: 'btn btn-light-primary',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4]
                }
            },
            {
                extend: 'print',
                text: 'طباعة',
                className: 'btn btn-light-warning',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4]
                }
            }
        ]
    });

    table.buttons().container().appendTo('#topStudentsTable_wrapper .col-md-6:eq(0)');
}

function updateBadgeStats(stats) {
    if (!stats) return;

    const statsHtml = `
        <div class="col-6 col-md-3">
            <div class="badge-stat-box bg-light-primary p-4 rounded text-center">
                <i class="ki-outline ki-award fs-3x text-primary mb-2"></i>
                <div class="fw-bold fs-2 text-primary">${stats.TotalBadgesEarned || 0}</div>
                <div class="text-gray-700 fw-semibold fs-7">إجمالي الشارات</div>
            </div>
        </div>
        <div class="col-6 col-md-3">
            <div class="badge-stat-box p-4 rounded text-center" style="background: linear-gradient(135deg, #e3f2fd 0%, #bbdefb 100%);">
                <div class="fs-2x mb-2">💎</div>
                <div class="fw-bold fs-2" style="color: #1976d2;">${stats.DiamondBadges || 0}</div>
                <div class="text-gray-700 fw-semibold fs-7">شارات ماسية</div>
            </div>
        </div>
        <div class="col-6 col-md-3">
            <div class="badge-stat-box p-4 rounded text-center" style="background: linear-gradient(135deg, #fff9c4 0%, #fff59d 100%);">
                <div class="fs-2x mb-2">🥇</div>
                <div class="fw-bold fs-2" style="color: #f57f17;">${stats.GoldBadges || 0}</div>
                <div class="text-gray-700 fw-semibold fs-7">شارات ذهبية</div>
            </div>
        </div>
        <div class="col-6 col-md-3">
            <div class="badge-stat-box p-4 rounded text-center" style="background: linear-gradient(135deg, #f3e5f5 0%, #e1bee7 100%);">
                <div class="fs-2x mb-2">🥈</div>
                <div class="fw-bold fs-2" style="color: #7b1fa2;">${stats.SilverBadges || 0}</div>
                <div class="text-gray-700 fw-semibold fs-7">شارات فضية</div>
            </div>
        </div>
    `;

    $('#badgeStatsContainer').html(statsHtml);
}

function showError(message) {
    Swal.fire({
        icon: 'error',
        title: 'خطأ',
        text: message,
        confirmButtonText: 'حسناً'
    });
}



function displayBadgesModal(badgesData) {
    const modalHtml = `
        <div class="modal fade" id="badgesModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h3 class="modal-title">
                            <i class="ki-outline ki-award fs-2 text-primary me-2"></i>
                            الشارات المكتسبة
                        </h3>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <div class="row g-4" id="badgesContainer">
                            ${generateBadgesHtml(badgesData)}
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-light" data-bs-dismiss="modal">إغلاق</button>
                    </div>
                </div>
            </div>
        </div>
    `;

    if ($('#badgesModal').length === 0) {
        $('body').append(modalHtml);
    } else {
        $('#badgesContainer').html(generateBadgesHtml(badgesData));
    }

    const modal = new bootstrap.Modal(document.getElementById('badgesModal'));
    modal.show();
}

function generateBadgesHtml(badgesData) {
    if (!badgesData || badgesData.length === 0) {
        return `
            <div class="col-12 text-center py-10">
                <i class="ki-outline ki-information fs-3x text-muted"></i>
                <div class="text-muted fs-4 mt-3">لا توجد شارات مكتسبة</div>
            </div>
        `;
    }

    let html = '';
    badgesData.forEach((badge, index) => {
        const badgeColor = badge.Badge_Color || '#009ef7';
        const badgeIcon = badge.Badge_Icon || 'star';
        const badgeName = badge.Badge_Name || 'شارة';
        const badgeLevel = badge.Badge_Level || '';
        const earnedDate = badge.Earned_Date ? new Date(badge.Earned_Date).toLocaleDateString('ar-EG') : '';

        html += `
            <div class="col-md-4 col-sm-6">
                <div class="card card-flush h-100">
                    <div class="card-body text-center p-5">
                        <div class="badge-display mb-4" style="background: ${badgeColor}; width: 80px; height: 80px; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: 0 auto; box-shadow: 0 8px 16px ${badgeColor}40;">
                            <i class="ki-outline ki-${badgeIcon} text-white fs-2x"></i>
                        </div>
                        <h4 class="fw-bold text-gray-800 mb-2">${badgeName}</h4>
                        <div class="badge badge-light-${getBadgeLevelClass(badgeLevel)} mb-3">${badgeLevel}</div>
                        ${earnedDate ? `<div class="text-muted fs-7"><i class="ki-outline ki-calendar text-muted me-1"></i> ${earnedDate}</div>` : ''}
                    </div>
                </div>
            </div>
        `;
    });

    return html;
}

function getBadgeLevelClass(level) {
    switch (level.toLowerCase()) {
        case 'ماسية':
        case 'diamond':
            return 'primary';
        case 'ذهبية':
        case 'gold':
            return 'warning';
        case 'فضية':
        case 'silver':
            return 'secondary';
        default:
            return 'info';
    }
}

function showStudentBadges(studentId) {
    Swal.fire({
        title: 'جارٍ التحميل...',
        text: 'يتم الآن تحميل الشارات',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    $.ajax({
        url: `/Home/GetStudentBadges/${studentId}`,
        type: 'GET',
        success: function (response) {
            Swal.close();

            if (response.success && response.data) {
                displayBadgesModal(response.data);
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'خطأ',
                    text: response.message || 'لا توجد شارات لهذه الطالبة'
                });
            }
        },
        error: function (xhr, status, error) {
            Swal.close();

            Swal.fire({
                icon: 'error',
                title: 'خطأ في الاتصال',
                text: 'حدث خطأ أثناء تحميل الشارات. يرجى المحاولة مرة أخرى.'
            });
        }
    });
}


//document.addEventListener("DOMContentLoaded", function () {

//	// قراءة القيم اللي جاية من السيرفر
//	let totalStudents = window.dashboardData.totalStudents;
//	let presentCount = window.dashboardData.present;
//	let absentCount = window.dashboardData.absent;

//	// الوقت والتاريخ
//	function updateDateTime() {
//		const now = new Date();
//		document.getElementById('currentTime').textContent =
//			now.toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit', second: '2-digit' });

//		document.getElementById('currentDate').textContent =
//			now.toLocaleDateString('ar-EG', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
//	}

//	updateDateTime();
//	setInterval(updateDateTime, 1000);

//	// الإحصائيات
//	function updateStatistics() {
//		const presentPercentage = totalStudents > 0 ? Math.round((presentCount / totalStudents) * 100) : 0;
//		const absentPercentage = totalStudents > 0 ? Math.round((absentCount / totalStudents) * 100) : 0;

//		document.getElementById('attendancePercentage').textContent = presentPercentage + '%';
//		document.getElementById('chartPercentage').textContent = presentPercentage + '%';
//	}

//	// الرسم البياني
//	const ctx = document.getElementById('attendanceChart');
//	if (ctx) {
//		new Chart(ctx, {
//			type: 'doughnut',
//			data: {
//				labels: ['حاضر', 'غائب'],
//				datasets: [{
//					data: [presentCount, absentCount],
//					backgroundColor: ['#50cd89', '#f1416c'],
//					borderWidth: 0
//				}]
//			},
//			options: {
//				legend: { display: false },
//				cutout: '75%'
//			}
//		});
//	}

//	updateStatistics();
//});



//function registerAllStudents() {

//	Swal.fire({
//		title: 'جارٍ التسجيل...',
//		text: 'يتم الآن تسجيل حضور جميع الطلاب',
//		allowOutsideClick: false,
//		didOpen: () => {
//			Swal.showLoading();
//		}
//	});

//	$.ajax({
//		url: '/Home/RegistrationAllStudents',
//		type: 'POST',
//		success: function (response) {

//			if (response.success) {
//				Swal.fire({
//					icon: 'success',
//					title: 'تم بنجاح',
//					text: response.message,
//					confirmButtonText: 'تمام'
//				}).then(() => {
//					location.reload();
//				});

//			} else {
//				Swal.fire({
//					icon: 'error',
//					title: 'خطأ',
//					text: response.message
//				});
//			}
//		},
//		error: function () {
//			Swal.fire({
//				icon: 'error',
//				title: 'خطأ في الاتصال',
//				text: 'تأكد من الاتصال وحاول مرة أخرى'
//			});
//		}
//	});
//}




