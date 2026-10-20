using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Workwise.Web.Data;
using Workwise.Web.Models;
namespace Workwise.Web.Services;
public sealed class DatabaseInitializationService(ApplicationDbContext db,RoleManager<IdentityRole<long>> roles,UserManager<ApplicationUser> users,IConfiguration config,ILogger<DatabaseInitializationService> logger) {
 public async Task InitializeAsync(CancellationToken ct=default) {
  await db.Database.MigrateAsync(ct);
  foreach(var role in new[]{"Admin","Supervisor","Employee"}) {
   if(await roles.RoleExistsAsync(role)) continue;
   var created=await roles.CreateAsync(new IdentityRole<long>(role));
   if(!created.Succeeded) throw new InvalidOperationException("Unable to create role: "+role);
  }
  var email=config["BootstrapAdmin:Email"];
  var password=config["BootstrapAdmin:Password"];
  if(string.IsNullOrWhiteSpace(email)||string.IsNullOrWhiteSpace(password))return;
  if(!config.GetValue<bool>("BootstrapAdmin:Enabled"))return;
  var existing=await users.FindByEmailAsync(email);
  if(existing!=null){logger.LogInformation("Bootstrap admin already exists; not modifying existing credentials.");return;}
  var user=new ApplicationUser{UserName=email,Email=email,EmailConfirmed=true,FullName="Workwise Administrator"};
  var createdUser=await users.CreateAsync(user,password);
  if(!createdUser.Succeeded)throw new InvalidOperationException("Admin bootstrap failed: "+string.Join("; ",createdUser.Errors.Select(x=>x.Description)));
  var assigned=await users.AddToRoleAsync(user,"Admin");
  if(!assigned.Succeeded)throw new InvalidOperationException("Admin role assignment failed.");
  logger.LogWarning("Initial admin account created. Disable BootstrapAdmin and rotate the bootstrap secret.");
 }
}