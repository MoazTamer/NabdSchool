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
