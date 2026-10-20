using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using Workwise.Web.Data;
using Workwise.Web.Models;
using Workwise.Web.Services;
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();
var cs=builder.Configuration.GetConnectionString("DefaultConnection")??throw new InvalidOperationException("Missing database connection");
builder.Services.AddDbContext<ApplicationDbContext>(o=>o.UseMySql(cs,new MySqlServerVersion(new Version(8,4,0))));
builder.Services.AddIdentity<ApplicationUser,IdentityRole<long>>(o=>{
 o.Password.RequiredLength=12;o.Password.RequireDigit=true;o.Password.RequireUppercase=true;o.Password.RequireLowercase=true;o.Password.RequireNonAlphanumeric=true;
 o.User.RequireUniqueEmail=true;
 o.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(15);o.Lockout.MaxFailedAccessAttempts=5;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o=>{
 o.LoginPath="/Account/Login";o.AccessDeniedPath="/Account/AccessDenied";
 o.Cookie.HttpOnly=true;o.Cookie.SameSite=SameSiteMode.Lax;o.Cookie.SecurePolicy=CookieSecurePolicy.SameAsRequest;
 o.ExpireTimeSpan=TimeSpan.FromHours(8);o.SlidingExpiration=true;
});
builder.Services.AddRateLimiter(options=>{
 options.RejectionStatusCode=429;
 options.GlobalLimiter=PartitionedRateLimiter.Create<HttpContext,string>(context=>
  RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString()??"unknown",_=>new FixedWindowRateLimiterOptions{PermitLimit=120,Window=TimeSpan.FromMinutes(1),QueueLimit=0,AutoReplenishment=true}));
});
builder.Services.AddScoped<TaskAllocationService>();
builder.Services.AddScoped<DatabaseInitializationService>();
builder.Services.AddSignalR();
var app=builder.Build();
if(builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup")){
 using var scope=app.Services.CreateScope();
 await scope.ServiceProvider.GetRequiredService<DatabaseInitializationService>().InitializeAsync();
}
if(!app.Environment.IsDevelopment()){app.UseExceptionHandler("/Home/Error");app.UseHsts();}
app.UseHttpsRedirection();app.UseStaticFiles();app.UseRouting();app.UseRateLimiter();app.UseAuthentication();app.UseAuthorization();
app.MapControllerRoute("areas","{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}");
app.MapHealthChecks("/health");
app.MapHub<Workwise.Web.Hubs.NotificationHub>("/hubs/notifications");
app.Run();