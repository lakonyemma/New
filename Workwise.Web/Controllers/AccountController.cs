using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Workwise.Web.Models;
namespace Workwise.Web.Controllers;
public class AccountController(SignInManager<ApplicationUser> signInManager,UserManager<ApplicationUser> users) : Controller {
[HttpGet] public IActionResult Login(string? returnUrl=null)=>View(new LoginInput {ReturnUrl=returnUrl});
[HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Login(LoginInput input) {
 if(!ModelState.IsValid)return View(input);
 var account=await users.FindByEmailAsync(input.Email);
 if(account==null||!account.IsActive){ModelState.AddModelError("","Invalid credentials");return View(input);}
 var result=await signInManager.PasswordSignInAsync(account,input.Password,false,true);
 if(!result.Succeeded){ModelState.AddModelError("","Invalid credentials");return View(input);}
 if(Url.IsLocalUrl(input.ReturnUrl))return LocalRedirect(input.ReturnUrl!);
 if(await users.IsInRoleAsync(account,"Admin"))return RedirectToAction("Index","Dashboard",new{area="Admin"});
 if(await users.IsInRoleAsync(account,"Supervisor"))return RedirectToAction("Index","Dashboard",new{area="Supervisor"});
 return RedirectToAction("Index","Dashboard",new{area="Employee"});
}
[HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Logout(){await signInManager.SignOutAsync();return RedirectToAction("Index","Home");}
public IActionResult AccessDenied()=>View();
}
public class LoginInput {
 [System.ComponentModel.DataAnnotations.Required][System.ComponentModel.DataAnnotations.EmailAddress]public string Email{get;set;}="";
 [System.ComponentModel.DataAnnotations.Required][System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]public string Password{get;set;}="";
 public string? ReturnUrl{get;set;}
}