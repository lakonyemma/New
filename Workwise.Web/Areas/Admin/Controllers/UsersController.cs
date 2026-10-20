using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Workwise.Web.Models;
namespace Workwise.Web.Areas.Admin.Controllers;
[Area("Admin")][Authorize(Roles="Admin")]
public class UsersController(UserManager<ApplicationUser> users,RoleManager<IdentityRole<long>> roles):Controller {
 public IActionResult Index()=>View(users.Users.OrderBy(u=>u.Email).Take(200).ToList());
 [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> SetRole(long userId,string role){
  if(!new[]{"Admin","Supervisor","Employee"}.Contains(role))return BadRequest();
  var user=await users.FindByIdAsync(userId.ToString());if(user==null)return NotFound();
  var existing=await users.GetRolesAsync(user);
  var removed=await users.RemoveFromRolesAsync(user,existing);
  if(!removed.Succeeded)return BadRequest(removed.Errors);
  var result=await users.AddToRoleAsync(user,role);
  if(!result.Succeeded)return BadRequest(result.Errors);
  return RedirectToAction(nameof(Index));
 }
}