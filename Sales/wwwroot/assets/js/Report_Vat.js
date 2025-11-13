"use strict";

// On document ready
KTUtil.onDOMContentLoaded(function () {
    $("#FromDate").flatpickr();
    $("#ToDate").flatpickr();

    jQueryAjaxSearch();
});

function jQueryAjaxSearch() {

    var FromDate = $("#FromDate").val();
    var ToDate = $("#ToDate").val();
    axios.get('/Report/GetReport_Vat?FromDate=' + FromDate + "&ToDate=" + ToDate)
        .then(function (response) {
            $("#TotalSaleAfterVat").text(response.data.totalSaleAfterVat);
            $("#TotalSale").text(response.data.totalSale);
            $("#TotalSaleVat").text(response.data.totalSaleVat);
            $("#TotalSaleBackAfterVat").text(response.data.totalSaleBackAfterVat);
            $("#TotalSaleBack").text(response.data.totalSaleBack);
            $("#TotalSaleBackVat").text(response.data.totalSaleBackVat);
            $("#TotalSaleNetAfterVat").text(response.data.totalSaleNetAfterVat);
            $("#TotalSaleNet").text(response.data.totalSaleNet);
            $("#TotalSaleNetVat").text(response.data.totalSaleNetVat);

            $("#TotalBuyAfterVat").text(response.data.totalBuyAfterVat);
            $("#TotalBuy").text(response.data.totalBuy);
            $("#TotalBuyVat").text(response.data.totalBuyVat);
            $("#TotalOutcomingAfterVat").text(response.data.totalOutcomingAfterVat);
            $("#TotalOutcoming").text(response.data.totalOutcoming);
            $("#TotalOutcomingVat").text(response.data.totalOutcomingVat);
            $("#TotalNetAfterVat").text(response.data.totalNetAfterVat);
            $("#TotalNet").text(response.data.totalNet);
            $("#TotalNetVat").text(response.data.totalNetVat);

            $("#Total").text(response.data.total);
        })
        .catch(function (error) {
            swal.fire({
                title: "تقارير",
                text: "من فضلك تأكد من تسجيل البيانات بطريقة صحيحة",
                icon: "error",
                buttonsStyling: false,
                confirmButtonText: "موافق",
                customClass: {
                    confirmButton: "btn fw-bold btn-light-primary"
                }
            });
        });

    //prevent default form submit event
    return false;
}