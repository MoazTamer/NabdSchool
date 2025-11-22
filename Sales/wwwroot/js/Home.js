


KTUtil.onDOMContentLoaded(function () {
   

});


document.addEventListener("DOMContentLoaded", function () {
    loadAttendanceCounts();
});

function loadAttendanceCounts() {
    fetch('/Home/GetTodayAttendanceCount')
        .then(response => response.json())
        .then(data => {
            document.getElementById("presentCount").innerText = data.present;
            document.getElementById("absentCount").innerText = data.absent;
            document.getElementById("totalStudents").innerText = data.totalStudents;
        })
        .catch(err => console.error("Error loading attendance:", err));
}



