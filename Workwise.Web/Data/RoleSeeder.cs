using Microsoft.AspNetCore.Identity;
namespace Workwise.Web.Data;
public static class RoleSeeder {
 public static async Task EnsureRolesAsync(IServiceProvider provider){
  using var scope=provider.CreateScope();
  var manager=scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<long>>>();
  foreach(var role in new[]{"Admin","Supervisor","Employee"})if(!await manager.RoleExistsAsync(role)){
   var result=await manager.CreateAsync(new IdentityRole<long>(role));
   if(!result.Succeeded)throw new InvalidOperationException(string.Join("; ",result.Errors.Select(e=>e.Description)));
  }
 }
}