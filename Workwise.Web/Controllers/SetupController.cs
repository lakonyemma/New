using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workwise.Web.Models;
namespace Workwise.Web.Controllers;
[Route("setup")]
public sealed class SetupController(UserManager<ApplicationUser> users,RoleManager<IdentityRole<long>> roles,IConfiguration config,SignInManager<ApplicationUser> signIn):Controller {
 [HttpGet("")]
 public async Task<IActionResult> Index(){
  if(await users.IsInRoleAsyncForAnyAdmin())return NotFound();
  if(!config.GetValue<bool>("InitialSetup:Enabled")||string.IsNullOrWhiteSpace(config["InitialSetup:Token"]))return NotFound();
  return View(new FirstRunInput());
 }
 [HttpPost("")][ValidateAntiForgeryToken]
 public async Task<IActionResult> Index(FirstRunInput input){
  if(!config.GetValue<bool>("InitialSetup:Enabled"))return NotFound();
  var expected=config["InitialSetup:Token"];
  if(string.IsNullOrWhiteSpace(expected)||expected.Length<32)return NotFound();
  if(!ModelState.IsValid)return View(input);
  var a=SHA256.HashData(Encoding.UTF8.GetBytes(expected));
  var b=SHA256.HashData(Encoding.UTF8.GetBytes(input.SetupToken));
  if(!CryptographicOperations.FixedTimeEquals(a,b)){ModelState.AddModelError(nameof(input.SetupToken),"Invalid setup token.");return View(input);}
  if(await users.IsInRoleAsyncForAnyAdmin())return NotFound();
  if(!await roles.RoleExistsAsync("Admin")){ModelState.AddModelError("","Database roles are not initialized. Contact the system operator.");return View(input);}
  var account=new ApplicationUser{UserName=input.Email.Trim(),Email=input.Email.Trim(),EmailConfirmed=true,FullName=input.FullName.Trim(),IsActive=true};
  var result=await users.CreateAsync(account,input.Password);
  if(!result.Succeeded){foreach(var error in result.Errors)ModelState.AddModelError("",error.Description);return View(input);}
  var roleResult=await users.AddToRoleAsync(account,"Admin");
  if(!roleResult.Succeeded){await users.DeleteAsync(account);ModelState.AddModelError("","Could not assign administrator role.");return View(input);}
  await signIn.SignInAsync(account,false);
  return RedirectToAction("Index","Dashboard",new{area="Admin"});
 }
}
public sealed class FirstRunInput {
 [Required][StringLength(100)]public string FullName{get;set;}="";
 [Required][EmailAddress]public string Email{get;set;}="";
 [Required][MinLength(12)][DataType(DataType.Password)]public string Password{get;set;}="";
 [Required][Compare(nameof(Password))][DataType(DataType.Password)]public string ConfirmPassword{get;set;}="";
 [Required]public string SetupToken{get;set;}="";
}
internal static class AdminSetupExtensions {
 public static async Task<bool> IsInRoleAsyncForAnyAdmin(this UserManager<ApplicationUser> manager){
  var admins=await manager.GetUsersInRoleAsync("Admin");
  return admins.Count>0;
 }
}