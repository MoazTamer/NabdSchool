$(document).ready(function() {
    // Set today's date as default
    $('#reportDate').val(new Date().toISOString().split('T')[0]);

// Load classes
loadClasses();

// Class change event
$('#classFilter').change(function() {
            const classId = $(this).val();
if (classId) {
    loadClassRooms(classId);
$('#classRoomFilter').prop('disabled', false);
            } else {
    $('#classRoomFilter').html('<option value="">جميع الفصول</option>').prop('disabled', true);
            }
        });

// Search button
$('#btnSearch').click(function() {
    loadReport();
        });

// Print button
$('#btnPrint').click(function() {
    window.print();
        });

// Print PDF button - التقرير الرسمي
$('#btnPrintPdf').click(function() {
    printOfficialReport();
        });

// Export Excel button
$('#btnExportExcel').click(function() {
    exportToExcel();
        });
    });

// Load classes dropdown
function loadClasses() {
    $.get('@Url.Action("GetClasses", "Reports")', function (response) {
        if (response.success) {
            const select = $('#classFilter');
            response.data.forEach(function (item) {
                select.append(`<option value="${item.id}">${item.name}</option>`);
            });
        }
    });
    }

// Load classrooms dropdown
function loadClassRooms(classId) {
    $.get('@Url.Action("GetClassRooms", "Reports")', { classId: classId }, function (response) {
        const select = $('#classRoomFilter');
        select.html('<option value="">جميع الفصول</option>');

        if (response.success) {
            response.data.forEach(function (item) {
                select.append(`<option value="${item.id}">${item.name}</option>`);
            });
        }
    });
    }

// Load report
function loadReport() {
        const date = $('#reportDate').val();
const classId = $('#classFilter').val() || null;
const classRoomId = $('#classRoomFilter').val() || null;

if (!date) {
    Swal.fire('تنبيه', 'الرجاء اختيار التاريخ', 'warning');
return;
        }

// Show loading
Swal.fire({
    title: 'جاري تحميل التقرير...',
allowOutsideClick: false,
            didOpen: () => {Swal.showLoading(); }
        });

$.get('@Url.Action("GetDailyAbsenceReport", "Reports")', {
    date: date,
classId: classId,
classRoomId: classRoomId
        }, function(response) {
    Swal.close();

if (response.success) {
    displayReport(response.data);
            } else {
    Swal.fire('خطأ', response.message, 'error');
            }
        }).fail(function() {
    Swal.close();
Swal.fire('خطأ', 'حدث خطأ أثناء تحميل التقرير', 'error');
        });
    }

// Display report
function displayReport(data) {
        if (!data.classesAbsence || data.classesAbsence.length === 0) {
    $('#emptyState').show();
$('#reportContainer').hide();
$('#summaryCards').hide();
$('#exportButtons').hide();

Swal.fire('معلومة', 'لا يوجد طلاب غائبين في هذا اليوم! 🎉', 'info');
return;
        }

// Update summary
$('#totalAbsent').text(data.totalAbsentStudents);
$('#totalClasses').text(data.totalClasses);
$('#reportDateDisplay').text(new Date(data.reportDate).toLocaleDateString('ar-EG'));

// Build classes cards
let html = '';
data.classesAbsence.forEach(function(classData) {
    html += buildClassCard(classData);
        });

$('#classesContainer').html(html);

// Show report
$('#emptyState').hide();
$('#reportContainer').show();
$('#summaryCards').show();
$('#exportButtons').show();
    }

// Build class card HTML
function buildClassCard(classData) {
    let studentsRows = '';
classData.absentStudentsList.forEach(function(student, index) {
            const warningClass = student.consecutiveAbsenceDays >= 3 ? 'bg-light-danger' : '';
studentsRows += `
<tr class="${warningClass}">
    <td class="text-center">${index + 1}</td>
    <td>${student.studentName}</td>
    <td class="text-center">${student.studentCode || '-'}</td>
    <td class="text-center">${student.studentPhone || '-'}</td>
    <td class="text-center">
        <span class="badge badge-danger">${student.consecutiveAbsenceDays} يوم</span>
    </td>
    <td>${student.notes || '-'}</td>
</tr>
`;
        });

return `
<div class="card shadow-sm mb-5">
    <div class="card-header bg-light-primary">
        <div class="card-title">
            <h3 class="fw-bold text-gray-800">
                <i class="ki-outline ki-element-11 fs-2 text-primary me-2"></i>
                ${classData.className} - ${classData.classRoomName}
            </h3>
        </div>
        <div class="card-toolbar">
            <div class="d-flex gap-5">
                <div class="text-end">
                    <span class="text-gray-600 fs-7 d-block">إجمالي الطلاب</span>
                    <span class="fw-bold text-gray-800 fs-5">${classData.totalStudents}</span>
                </div>
                <div class="text-end">
                    <span class="text-gray-600 fs-7 d-block">الغائبين</span>
                    <span class="fw-bold text-danger fs-5">${classData.absentStudents}</span>
                </div>
                <div class="text-end">
                    <span class="text-gray-600 fs-7 d-block">النسبة</span>
                    <span class="fw-bold text-danger fs-5">${classData.absencePercentage}%</span>
                </div>
            </div>
        </div>
    </div>
    <div class="card-body p-0">
        <div class="table-responsive">
            <table class="table table-row-bordered table-row-gray-100 align-middle gy-4 gs-9 mb-0">
                <thead>
                    <tr class="fw-bold fs-6 text-gray-800 border-bottom-2 border-gray-200">
                        <th class="text-center" width="50">#</th>
                        <th>اسم الطالب</th>
                        <th class="text-center" width="150">كود الطالب</th>
                        <th class="text-center" width="150">رقم الهاتف</th>
                        <th class="text-center" width="150">أيام الغياب المتتالية</th>
                        <th width="200">ملاحظات</th>
                    </tr>
                </thead>
                <tbody>
                    ${studentsRows}
                </tbody>
            </table>
        </div>
    </div>
</div>
`;
    }

// Export to Excel
function exportToExcel() {
        const date = $('#reportDate').val();
const classId = $('#classFilter').val() || null;
const classRoomId = $('#classRoomFilter').val() || null;

window.location.href = `@Url.Action("ExportDailyAbsenceToExcel", "Reports")?date=${date}&classId=${classId}&classRoomId=${classRoomId}`;
    }

// Print Official PDF Report
function printOfficialReport() {
        const date = $('#reportDate').val();
const classId = $('#classFilter').val() || '';
const classRoomId = $('#classRoomFilter').val() || '';

if (!date) {
    Swal.fire('تنبيه', 'الرجاء اختيار التاريخ', 'warning');
return;
        }

console.log('Starting PDF generation...', {date, classId, classRoomId});

// Show loading
Swal.fire({
    title: 'جاري إنشاء التقرير...',
text: 'الرجاء الانتظار',
allowOutsideClick: false,
            didOpen: () => {Swal.showLoading(); }
        });

// طريقة بديلة أبسط - استخدام XMLHttpRequest
const xhr = new XMLHttpRequest();
xhr.open('POST', '@Url.Action("PrintDailyAbsencePdf", "Reports")', true);
xhr.responseType = 'blob';
xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');

xhr.onload = function() {
    console.log('XHR Status:', xhr.status);
Swal.close();

if (xhr.status === 200) {
                // التحقق من نوع الاستجابة
                const contentType = xhr.getResponseHeader('content-type');
console.log('Content-Type:', contentType);

if (contentType && contentType.includes('application/json')) {
                    // الاستجابة JSON (خطأ)
                    const reader = new FileReader();
reader.onload = function() {
                        try {
                            const error = JSON.parse(reader.result);
Swal.fire('خطأ', error.message || 'حدث خطأ أثناء إنشاء التقرير', 'error');
                        } catch(e) {
    Swal.fire('خطأ', 'حدث خطأ أثناء إنشاء التقرير', 'error');
                        }
                    };
reader.readAsText(xhr.response);
                } else {
                    // الاستجابة PDF (نجاح)
                    const blob = xhr.response;
const url = window.URL.createObjectURL(blob);
const a = document.createElement('a');
a.href = url;
a.download = `تقرير_الغياب_اليومي_${date}.pdf`;
document.body.appendChild(a);
a.click();
                        
                    setTimeout(() => {
    window.URL.revokeObjectURL(url);
document.body.removeChild(a);
                    }, 100);

Swal.fire({
    icon: 'success',
title: 'تم!',
text: 'تم إنشاء التقرير بنجاح',
timer: 2000,
showConfirmButton: false
                    });
                }
            } else {
    console.error('XHR Error:', xhr.statusText);
Swal.fire('خطأ', 'حدث خطأ أثناء إنشاء التقرير: ' + xhr.statusText, 'error');
            }
        };

xhr.onerror = function() {
    console.error('XHR Network Error');
Swal.close();
Swal.fire('خطأ', 'حدث خطأ في الاتصال بالسيرفر', 'error');
        };

// إنشاء البيانات للإرسال
const formData = `date=${encodeURIComponent(date)}&classId=${classId}&classRoomId=${classRoomId}`;
console.log('Sending data:', formData);

xhr.send(formData);
    }
