using DinkToPdf;
using DinkToPdf.Contracts;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

    
    options.AddPolicy("Class_View1", policy => policy.RequireClaim("Class_View1", "True"));
    options.AddPolicy("Class_Create1", policy => policy.RequireClaim("Class_Create1", "True"));
    options.AddPolicy("Class_Edit1", policy => policy.RequireClaim("Class_Edit1", "True"));
    options.AddPolicy("Class_Delete1", policy => policy.RequireClaim("Class_Delete1", "True"));

    options.AddPolicy("ClassRoom_View1", policy => policy.RequireClaim("ClassRoom_View1", "True"));
    options.AddPolicy("ClassRoom_Create1", policy => policy.RequireClaim("ClassRoom_Create1", "True"));
    options.AddPolicy("ClassRoom_Edit1", policy => policy.RequireClaim("ClassRoom_Edit1", "True"));
    options.AddPolicy("ClassRoom_Delete1", policy => policy.RequireClaim("ClassRoom_Delete1", "True"));

    options.AddPolicy("Student_View1", policy => policy.RequireClaim("Student_View1", "True"));
    options.AddPolicy("Student_Create1", policy => policy.RequireClaim("Student_Create1", "True"));
    options.AddPolicy("Student_Edit1", policy => policy.RequireClaim("Student_Edit1", "True"));
    options.AddPolicy("Student_Delete1", policy => policy.RequireClaim("Student_Delete1", "True"));

    options.AddPolicy("SchoolSettings_View", policy => policy.RequireClaim("SchoolSettings_View", "True"));
    options.AddPolicy("SchoolSettings_Edit", policy => policy.RequireClaim("SchoolSettings_Edit", "True"));

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
builder.Services.AddScoped<ReportPdfService>();

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
