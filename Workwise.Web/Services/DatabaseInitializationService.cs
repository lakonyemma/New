using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Workwise.Web.Data;
namespace Workwise.Web.Services;
public sealed class DatabaseInitializationService(ApplicationDbContext db,RoleManager<IdentityRole<long>> roles){
 public async Task InitializeAsync(CancellationToken ct=default){
  await db.Database.MigrateAsync(ct);
  foreach(var name in new[]{"Admin","Supervisor","Employee"}){
   if(await roles.RoleExistsAsync(name))continue;
   var result=await roles.CreateAsync(new IdentityRole<long>(name));
   if(!result.Succeeded)throw new InvalidOperationException("Unable to initialize role "+name+": "+string.Join("; ",result.Errors.Select(e=>e.Description)));
  }
 }
}