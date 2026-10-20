using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace Workwise.Web.Data;
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext> {
 public ApplicationDbContext CreateDbContext(string[] args) {
  var configuration=new ConfigurationBuilder().AddJsonFile("appsettings.json",optional:true)
    .AddEnvironmentVariables().Build();
  var cs=configuration.GetConnectionString("DefaultConnection")
    ??throw new InvalidOperationException("Set ConnectionStrings__DefaultConnection before running EF Core commands.");
  var options=new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseMySql(cs,new MySqlServerVersion(new Version(8,4,0))).Options;
  return new ApplicationDbContext(options);
 }
}