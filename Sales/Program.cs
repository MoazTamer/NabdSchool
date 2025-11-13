using DinkToPdf;
using DinkToPdf.Contracts;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Sales.Helper;

using SalesModel.IRepository;
using SalesModel.Models;

using SalesRepository.Data;
using SalesRepository.Repository;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("SalesConnection");
builder.Services.AddDbContext<SalesDBContext>(options =>
    options.UseSqlServer(connectionString));
//// 1) Hangfire
builder.Services.AddHangfire(cfg =>
{
    cfg.UseSqlServerStorage(connectionString);
});


builder.Services.AddIdentity<ApplicationUser, ApplicationRole>((options) =>
{
    options.User.AllowedUserNameCharacters = null;

    options.Password.RequiredLength = 4;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;

    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<SalesDBContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();


//builder.Services.Configure<SecurityStampValidatorOptions>(options =>
//{
//    options.ValidationInterval = TimeSpan.Zero; // ⏱️ No delay
//});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Users_View", policy => policy.RequireClaim("UsersView", "True"));
    options.AddPolicy("Users_Create", policy => policy.RequireClaim("UsersCreate", "True"));
    options.AddPolicy("Users_Edit", policy => policy.RequireClaim("UsersEdit", "True"));
    options.AddPolicy("Users_Delete", policy => policy.RequireClaim("UsersDelete", "True"));

    options.AddPolicy("UsersPermission_View", policy => policy.RequireClaim("UsersPermissionView", "True"));
    options.AddPolicy("UsersPermission_Edit", policy => policy.RequireClaim("UsersPermissionEdit", "True"));

    options.AddPolicy("CashBalanceType_View", policy => policy.RequireClaim("CashBalanceTypeView", "True"));
    options.AddPolicy("CashBalanceType_Create", policy => policy.RequireClaim("CashBalanceTypeCreate", "True"));
    options.AddPolicy("CashBalanceType_Edit", policy => policy.RequireClaim("CashBalanceTypeEdit", "True"));
    options.AddPolicy("CashBalanceType_Delete", policy => policy.RequireClaim("CashBalanceTypeDelete", "True"));

    options.AddPolicy("OutcomingMain_View", policy => policy.RequireClaim("OutcomingMainView", "True"));
    options.AddPolicy("OutcomingMain_Create", policy => policy.RequireClaim("OutcomingMainCreate", "True"));
    options.AddPolicy("OutcomingMain_Edit", policy => policy.RequireClaim("OutcomingMainEdit", "True"));
    options.AddPolicy("OutcomingMain_Delete", policy => policy.RequireClaim("OutcomingMainDelete", "True"));

    options.AddPolicy("OutcomingSub_View", policy => policy.RequireClaim("OutcomingSubView", "True"));
    options.AddPolicy("OutcomingSub_Create", policy => policy.RequireClaim("OutcomingSubCreate", "True"));
    options.AddPolicy("OutcomingSub_Edit", policy => policy.RequireClaim("OutcomingSubEdit", "True"));
    options.AddPolicy("OutcomingSub_Delete", policy => policy.RequireClaim("OutcomingSubDelete", "True"));

    options.AddPolicy("Vat_Edit", policy => policy.RequireClaim("VatEdit", "True"));

    options.AddPolicy("CashBalance_View", policy => policy.RequireClaim("CashBalanceView", "True"));
    options.AddPolicy("CashBalance_Create", policy => policy.RequireClaim("CashBalanceCreate", "True"));
    options.AddPolicy("CashBalance_Edit", policy => policy.RequireClaim("CashBalanceEdit", "True"));
    options.AddPolicy("CashBalance_Delete", policy => policy.RequireClaim("CashBalanceDelete", "True"));

    options.AddPolicy("CashBalanceDetail_View", policy => policy.RequireClaim("CashBalanceDetailView", "True"));
    options.AddPolicy("CashBalanceUser_View", policy => policy.RequireClaim("CashBalanceUserView", "True"));

    options.AddPolicy("CashBalanceOperation_Create", policy => policy.RequireClaim("CashBalanceOperationCreate", "True"));
    options.AddPolicy("CashBalanceOperation_Edit", policy => policy.RequireClaim("CashBalanceOperationEdit", "True"));
    options.AddPolicy("CustomerPaymentOperation_Delete", policy => policy.RequireClaim("CashBalanceOperationDelete", "True"));

    options.AddPolicy("CustomerData_View", policy => policy.RequireClaim("CustomerDataView", "True"));
    options.AddPolicy("CustomerData_Create", policy => policy.RequireClaim("CustomerDataCreate", "True"));
    options.AddPolicy("CustomerData_Edit", policy => policy.RequireClaim("CustomerDataEdit", "True"));
    options.AddPolicy("CustomerData_Delete", policy => policy.RequireClaim("CustomerDataDelete", "True"));

    options.AddPolicy("CustomerDataDetail_View", policy => policy.RequireClaim("CustomerDataDetailView", "True"));
    options.AddPolicy("CustomerDataBranch_View", policy => policy.RequireClaim("CustomerDataBranchView", "True"));
    options.AddPolicy("CustomerDataBranch_Create", policy => policy.RequireClaim("CustomerDataBranchCreate", "True"));

    options.AddPolicy("CustomerSale_View", policy => policy.RequireClaim("CustomerSaleView", "True"));
    options.AddPolicy("CustomerSale_Create", policy => policy.RequireClaim("CustomerSaleCreate", "True"));
    options.AddPolicy("CustomerSale_Edit", policy => policy.RequireClaim("CustomerSaleEdit", "True"));
    options.AddPolicy("CustomerSale_Delete", policy => policy.RequireClaim("CustomerSaleDelete", "True"));
    options.AddPolicy("CustomerSaleDate_View", policy => policy.RequireClaim("CustomerSaleDateView", "True"));

    options.AddPolicy("CustomerSale_Buy", policy => policy.RequireClaim("CustomerSaleBuy", "True"));
    options.AddPolicy("CustomerSale_Kemia", policy => policy.RequireClaim("CustomerSaleKemia", "True"));
    options.AddPolicy("CustomerSale_Cash", policy => policy.RequireClaim("CustomerSaleCash", "True"));
    options.AddPolicy("CustomerSale_Branch", policy => policy.RequireClaim("CustomerSaleBranch", "True"));
    options.AddPolicy("CustomerSale_PayPrice", policy => policy.RequireClaim("CustomerSalePayPrice", "True"));
    options.AddPolicy("CustomerSale_SpecialPrice", policy => policy.RequireClaim("CustomerSaleSpecialPrice", "True"));
    options.AddPolicy("CustomerSale_BuyPrice", policy => policy.RequireClaim("CustomerSaleBuyPrice", "True"));
    options.AddPolicy("CustomerSale_AnyPrice", policy => policy.RequireClaim("CustomerSaleAnyPrice", "True"));

    options.AddPolicy("CustomerSaleReport_View", policy => policy.RequireClaim("CustomerSaleReportView", "True"));

    options.AddPolicy("CustomerSaleReportProduct_View", policy => policy.RequireClaim("CustomerSaleReportProductView", "True"));

    options.AddPolicy("CustomerPayment_View", policy => policy.RequireClaim("CustomerPaymentView", "True"));
    options.AddPolicy("CustomerPayment_Create", policy => policy.RequireClaim("CustomerPaymentCreate", "True"));
    options.AddPolicy("CustomerPayment_Edit", policy => policy.RequireClaim("CustomerPaymentEdit", "True"));
    options.AddPolicy("CustomerPayment_Delete", policy => policy.RequireClaim("CustomerPaymentDelete", "True"));

    options.AddPolicy("VendorData_View", policy => policy.RequireClaim("VendorDataView", "True"));
    options.AddPolicy("VendorData_Create", policy => policy.RequireClaim("VendorDataCreate", "True"));
    options.AddPolicy("VendorData_Edit", policy => policy.RequireClaim("VendorDataEdit", "True"));
    options.AddPolicy("VendorData_Delete", policy => policy.RequireClaim("VendorDataDelete", "True"));

    options.AddPolicy("VendorDataDetail_View", policy => policy.RequireClaim("VendorDataDetailView", "True"));
    options.AddPolicy("VendorDataBranch_View", policy => policy.RequireClaim("VendorDataBranchView", "True"));

    options.AddPolicy("VendorBuy_View", policy => policy.RequireClaim("VendorBuyView", "True"));
    options.AddPolicy("VendorBuy_Create", policy => policy.RequireClaim("VendorBuyCreate", "True"));
    options.AddPolicy("VendorBuy_Edit", policy => policy.RequireClaim("VendorBuyEdit", "True"));
    options.AddPolicy("VendorBuy_Delete", policy => policy.RequireClaim("VendorBuyDelete", "True"));

    options.AddPolicy("VendorPayment_View", policy => policy.RequireClaim("VendorPaymentView", "True"));
    options.AddPolicy("VendorPayment_Create", policy => policy.RequireClaim("VendorPaymentCreate", "True"));
    options.AddPolicy("VendorPayment_Edit", policy => policy.RequireClaim("VendorPaymentEdit", "True"));
    options.AddPolicy("VendorPayment_Delete", policy => policy.RequireClaim("VendorPaymentDelete", "True"));

    options.AddPolicy("OutcomingDetail_View", policy => policy.RequireClaim("OutcomingDetailView", "True"));
    options.AddPolicy("OutcomingDetail_Create", policy => policy.RequireClaim("OutcomingDetailCreate", "True"));
    options.AddPolicy("OutcomingDetail_Edit", policy => policy.RequireClaim("OutcomingDetailEdit", "True"));
    options.AddPolicy("OutcomingDetail_Delete", policy => policy.RequireClaim("OutcomingDetailDelete", "True"));

    options.AddPolicy("ProductCategory_View", policy => policy.RequireClaim("ProductCategoryView", "True"));
    options.AddPolicy("ProductCategory_Create", policy => policy.RequireClaim("ProductCategoryCreate", "True"));
    options.AddPolicy("ProductCategory_Edit", policy => policy.RequireClaim("ProductCategoryEdit", "True"));
    options.AddPolicy("ProductCategory_Delete", policy => policy.RequireClaim("ProductCategoryDelete", "True"));

    options.AddPolicy("ProductUnit_View", policy => policy.RequireClaim("ProductUnitView", "True"));
    options.AddPolicy("ProductUnit_Create", policy => policy.RequireClaim("ProductUnitCreate", "True"));
    options.AddPolicy("ProductUnit_Edit", policy => policy.RequireClaim("ProductUnitEdit", "True"));
    options.AddPolicy("ProductUnit_Delete", policy => policy.RequireClaim("ProductUnitDelete", "True"));

    options.AddPolicy("ProductTitle_View", policy => policy.RequireClaim("ProductTitleView", "True"));
    options.AddPolicy("ProductTitle_Create", policy => policy.RequireClaim("ProductTitleCreate", "True"));
    options.AddPolicy("ProductTitle_Edit", policy => policy.RequireClaim("ProductTitleEdit", "True"));
    options.AddPolicy("ProductTitle_Delete", policy => policy.RequireClaim("ProductTitleDelete", "True"));

    options.AddPolicy("ProductInventory_View", policy => policy.RequireClaim("ProductInventoryView", "True"));
    options.AddPolicy("ProductInventory_Create", policy => policy.RequireClaim("ProductInventoryCreate", "True"));
    options.AddPolicy("ProductInventory_Edit", policy => policy.RequireClaim("ProductInventoryEdit", "True"));
    options.AddPolicy("ProductInventory_Delete", policy => policy.RequireClaim("ProductInventoryDelete", "True"));

    options.AddPolicy("ProductInventoryDetail_View", policy => policy.RequireClaim("ProductInventoryDetailView", "True"));

    options.AddPolicy("ProductTitleFollow_View", policy => policy.RequireClaim("ProductTitleFollowView", "True"));

    options.AddPolicy("ProductBarcodePrint_View", policy => policy.RequireClaim("ProductBarcodePrintView", "True"));

    options.AddPolicy("ProductOffer_View", policy => policy.RequireClaim("ProductOfferView", "True"));
    options.AddPolicy("ProductOffer_Create", policy => policy.RequireClaim("ProductOfferCreate", "True"));
    options.AddPolicy("ProductOffer_Edit", policy => policy.RequireClaim("ProductOfferEdit", "True"));
    options.AddPolicy("ProductOffer_Delete", policy => policy.RequireClaim("ProductOfferDelete", "True"));

    options.AddPolicy("ProductTransfer_View", policy => policy.RequireClaim("ProductTransferView", "True"));
    options.AddPolicy("ProductTransfer_Create", policy => policy.RequireClaim("ProductTransferCreate", "True"));
    options.AddPolicy("ProductTransfer_Edit", policy => policy.RequireClaim("ProductTransferEdit", "True"));
    options.AddPolicy("ProductTransfer_Delete", policy => policy.RequireClaim("ProductTransferDelete", "True"));

    options.AddPolicy("ProductDamaged_View", policy => policy.RequireClaim("ProductDamagedView", "True"));
    options.AddPolicy("ProductDamaged_Create", policy => policy.RequireClaim("ProductDamagedCreate", "True"));
    options.AddPolicy("ProductDamaged_Edit", policy => policy.RequireClaim("ProductDamagedEdit", "True"));
    options.AddPolicy("ProductDamaged_Delete", policy => policy.RequireClaim("ProductDamagedDelete", "True"));

    options.AddPolicy("ProductReceipt_View", policy => policy.RequireClaim("ProductReceiptView", "True"));
    options.AddPolicy("ProductReceipt_Create", policy => policy.RequireClaim("ProductReceiptCreate", "True"));
    options.AddPolicy("ProductReceipt_Edit", policy => policy.RequireClaim("ProductReceiptEdit", "True"));
    options.AddPolicy("ProductReceipt_Delete", policy => policy.RequireClaim("ProductReceiptDelete", "True"));

    options.AddPolicy("PriceOffer_View", policy => policy.RequireClaim("PriceOfferView", "True"));
    options.AddPolicy("PriceOffer_Create", policy => policy.RequireClaim("PriceOfferCreate", "True"));
    options.AddPolicy("PriceOffer_Edit", policy => policy.RequireClaim("PriceOfferEdit", "True"));
    options.AddPolicy("PriceOffer_Delete", policy => policy.RequireClaim("PriceOfferDelete", "True"));

    options.AddPolicy("ReportProfit_View", policy => policy.RequireClaim("ReportProfitView", "True"));

    options.AddPolicy("ReportSafe_View", policy => policy.RequireClaim("ReportSafeView", "True"));

    options.AddPolicy("ReportVat_View", policy => policy.RequireClaim("ReportVatView", "True"));

    options.AddPolicy("ReportDaily_View", policy => policy.RequireClaim("ReportDailyView", "True"));

	options.AddPolicy("ReportStoreCost_View", policy => policy.RequireClaim("ReportStoreCostView", "True"));

	options.AddPolicy("ReportStatistics_View", policy => policy.RequireClaim("ReportStatisticsView", "True"));

    options.AddPolicy("EmployeeData_View", policy => policy.RequireClaim("EmployeeDataView", "True"));
    options.AddPolicy("EmployeeData_Create", policy => policy.RequireClaim("EmployeeDataCreate", "True"));
    options.AddPolicy("EmployeeData_Edit", policy => policy.RequireClaim("EmployeeDataEdit", "True"));
    options.AddPolicy("EmployeeData_Delete", policy => policy.RequireClaim("EmployeeDataDelete", "True"));

    options.AddPolicy("EmployeeSalary_View", policy => policy.RequireClaim("EmployeeSalaryView", "True"));
    options.AddPolicy("EmployeeSalary_Edit", policy => policy.RequireClaim("EmployeeSalaryEdit", "True"));
    options.AddPolicy("EmployeeSalary_Delete", policy => policy.RequireClaim("EmployeeSalaryDelete", "True"));

    options.AddPolicy("EmployeePay_Create", policy => policy.RequireClaim("EmployeePayCreate", "True"));
    options.AddPolicy("EmployeePay_Edit", policy => policy.RequireClaim("EmployeePayEdit", "True"));
    options.AddPolicy("EmployeePay_Delete", policy => policy.RequireClaim("EmployeePayDelete", "True"));

    options.AddPolicy("EmployeeBonus_Create", policy => policy.RequireClaim("EmployeeBonusCreate", "True"));
    options.AddPolicy("EmployeeBonus_Edit", policy => policy.RequireClaim("EmployeeBonusEdit", "True"));
    options.AddPolicy("EmployeeBonus_Delete", policy => policy.RequireClaim("EmployeeBonusDelete", "True"));

    options.AddPolicy("EmployeeDiscount_Create", policy => policy.RequireClaim("EmployeeDiscountCreate", "True"));
    options.AddPolicy("EmployeeDiscount_Edit", policy => policy.RequireClaim("EmployeeDiscountEdit", "True"));
    options.AddPolicy("EmployeeDiscount_Delete", policy => policy.RequireClaim("EmployeeDiscountDelete", "True"));

    options.AddPolicy("Employee_Archives_View", policy => policy.RequireClaim("EmployeeArchivesView", "True"));
    options.AddPolicy("Employee_Archives_Delete", policy => policy.RequireClaim("EmployeeArchivesDelete", "True"));

    options.AddPolicy("Class_View", policy => policy.RequireClaim("Permission", "Class_View"));
    options.AddPolicy("Class_Create", policy => policy.RequireClaim("Permission", "Class_Create"));
    options.AddPolicy("Class_Edit", policy => policy.RequireClaim("Permission", "Class_Edit"));
    options.AddPolicy("Class_Delete", policy => policy.RequireClaim("Permission", "Class_Delete"));

    options.AddPolicy("ClassRoom_Create", policy => policy.RequireClaim("Permission", "ClassRoom_Create"));
    options.AddPolicy("ClassRoom_Edit", policy => policy.RequireClaim("Permission", "ClassRoom_Edit"));
    options.AddPolicy("ClassRoom_Delete", policy => policy.RequireClaim("Permission", "ClassRoom_Delete"));

    options.AddPolicy("Student_View", policy => policy.RequireClaim("Permission", "Student_View"));
    options.AddPolicy("Student_Create", policy => policy.RequireClaim("Permission", "Student_Create"));
    options.AddPolicy("Student_Edit", policy => policy.RequireClaim("Permission", "Student_Edit"));
    options.AddPolicy("Student_Delete", policy => policy.RequireClaim("Permission", "Student_Delete"));



    options.AddPolicy("HangfirePolicy", p =>
        p.RequireAuthenticatedUser()
         .RequireRole("admin"));

});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = false;
    options.ExpireTimeSpan = TimeSpan.FromDays(365);

    options.LoginPath = $"/Login";
    options.AccessDeniedPath = $"/Home/Authorized";
    options.SlidingExpiration = true;
});

builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

builder.Services.AddHangfireServer();

var app = builder.Build();



if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

var dashboardPath = "/admin/hg";
app.MapHangfireDashboard(dashboardPath, new DashboardOptions
{
    // القراءة فقط لغير الأدمن
    IsReadOnlyFunc = ctx => !ctx.GetHttpContext().User.IsInRole("admin")
})
.RequireAuthorization("HangfirePolicy"); // 🔒 السماح فقط لسياسة HangfirePolicy


app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
