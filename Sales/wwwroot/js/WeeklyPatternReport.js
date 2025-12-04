let patternsTable;
let daysChart = null;
const reportType = '@reportType';

$(document).ready(function () {
    loadClasses();
    initializeTable();

    $('#classFilter').change(function () {
        const classId = $(this).val();
        if (classId) {
            loadClassRooms(classId);
            $('#classRoomFilter').prop('disabled', false);
        } else {
            $('#classRoomFilter').html('<option value="">جميع الفصول</option>').prop('disabled', true);
        }
    });

    $('#searchBtn').click(loadReport);
    $('#resetBtn').click(resetFilters);
});

function loadClasses() {
    $.get('/Reports/GetClasses', function (response) {
        if (response.success) {
            response.data.forEach(item => $('#classFilter').append(`<option value="${item.id}">${item.name}</option>`));
        }
    });
}

function loadClassRooms(classId) {
    $.get('/Reports/GetClassRooms', { classId }, function (response) {
        const select = $('#classRoomFilter').html('<option value="">جميع الفصول</option>');
        if (response.success) response.data.forEach(item => select.append(`<option value="${item.id}">${item.name}</option>`));
    });
}

function initializeTable() {
    patternsTable = $('#patternsTable').DataTable({
        language: { url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/ar.json' },
        pageLength: 10,
        order: [[3, 'desc']]
    });
}

function loadReport() {
    const startDate = $('#startDate').val();
    const endDate = $('#endDate').val();
    const classId = $('#classFilter').val() || '';
    const classRoomId = $('#classRoomFilter').val() || '';

    if (!startDate || !endDate) { Swal.fire('تنبيه', 'اختر الفترة', 'warning'); return; }

    Swal.fire({ title: 'جاري التحميل...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });

    $.get('/Reports/GetWeeklyPatternReportStrict', { startDate, endDate, classId, classRoomId, reportType }, function (response) {
        Swal.close();
        if (response.success) displayReport(response.data);
        else { Swal.fire('خطأ', response.message, 'error'); hideSections(); }
    }).fail(() => { Swal.close(); Swal.fire('خطأ', 'حدث خطأ أثناء التحميل', 'error'); hideSections(); });
}

function displayReport(data) {
    if (!data || !data.length) { hideSections(); Swal.fire('معلومة', 'لا توجد أنماط متكررة', 'info'); return; }

    updateSummary(data);
    updateTable(data);
    updateCharts(data);

    $('#emptyState').hide();
    $('#summarySection').show();
    $('#resultsSection').show();
}

function updateSummary(students) {
    $('#studentsWithPatterns').text(students.length);
    const start = $('#startDate').val(), end = $('#endDate').val();
    $('#periodDisplay').text(`${start} - ${end}`);
}

function updateTable(students) {
    patternsTable.clear();
    students.forEach(s => {
        s.dayPatterns.forEach(p => {
            const patternClass = p.PatternType === 'late' ? 'badge-late' :
                p.PatternType === 'absent' ? 'badge-absent' : 'badge-mixed';
            patternsTable.row.add([
                s.StudentName,
                s.StudentCode,
                `${s.ClassName} - ${s.ClassRoomName}`,
                p.DayName,
                `<span class="pattern-badge ${patternClass}">${p.PatternType === 'late' ? 'تأخر' : p.PatternType === 'absent' ? 'غياب' : 'مختلط'}</span>`
            ]);
        });
    });
    patternsTable.draw();
}

function updateCharts(students) {
    if (daysChart) daysChart.destroy();

    const dayCounts = {};
    students.forEach(s => s.dayPatterns.forEach(p => { dayCounts[p.DayName] = (dayCounts[p.DayName] || 0) + 1; }));

    const ctx = document.getElementById('daysChart').getContext('2d');
    daysChart = new Chart(ctx, {
        type: 'bar',
        data: { labels: Object.keys(dayCounts), datasets: [{ label: 'عدد الطلاب', data: Object.values(dayCounts), backgroundColor: ['#ff6384', '#36a2eb', '#cc65fe', '#ffce56', '#4bc0c0', '#8e5ea2', '#3cba9f'] }] },
        options: { responsive: true, plugins: { title: { display: true, text: 'توزيع الأنماط على أيام الأسبوع' }, legend: { display: false } }, scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } } }
    });
}

function resetFilters() {
    $('#startDate').val('@DateTime.Today.AddDays(-60).ToString("yyyy-MM-dd")');
    $('#endDate').val('@DateTime.Today.ToString("yyyy-MM-dd")');
    $('#classFilter').val('');
    $('#classRoomFilter').val('').prop('disabled', true);
    hideSections();
}

function hideSections() {
    $('#summarySection').hide();
    $('#resultsSection').hide();
    $('#emptyState').show();
    if (daysChart) { daysChart.destroy(); daysChart = null; }
}
