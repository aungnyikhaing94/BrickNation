using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MyProj_DataAccess;
using MyProj_DataAccess.Data.Repository;
using MyProj_DataAccess.Data.Repository.IRepository;
using MyProj_DataAccess.Initializer;
using MyProj_Utility;
using MyProj_Utility.BrainTree;

var builder = WebApplication.CreateBuilder(args);
//var provider = builder.Services.BuildServiceProvider();
//var configuration = provider.GetService<IConfiguration>();

// Add services to the container.
builder.Services.AddControllersWithViews();

//Register DBContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDBContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddDefaultTokenProviders().AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDBContext>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();// add a package called Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation in NuGet Package Manager
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IApplicationTypeRepository, ApplicationTypeRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IInquiryHeaderRepository, InquiryHeaderRepository>();
builder.Services.AddScoped<IInquiryDetailRepository, InquiryDetailRepository>();
builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
builder.Services.AddScoped<IOrderHeaderRepository, OrderHeaderRepository>();
builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
builder.Services.AddSession(Options =>
{
    Options.IdleTimeout = TimeSpan.FromMinutes(10);
    Options.Cookie.HttpOnly = true;
    Options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication().AddFacebook(Options =>
{
    Options.AppId = "1188447355191386";
    Options.AppSecret = "f5ad33ed6408d4c8280725fba88f97f3";
});

//builder.Services.Configure<BrainTreeSettings>(builder.Configuration.GetSection("BrainTree"));
//builder.Services.AddSingleton<IBrainTreeGate, BrainTreeGate>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetService<IDbInitializer>();
    dbInitializer.Initialize();
}

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();