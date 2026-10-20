using Microsoft.AspNetCore.Identity;
namespace Workwise.Web.Models;
public class ApplicationUser : IdentityUser<long> { public string FullName { get; set; } = ""; public bool IsActive { get; set; } = true; }