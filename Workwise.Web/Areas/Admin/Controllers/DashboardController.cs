using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Workwise.Web.Areas.Admin.Controllers;
[Area("Admin")][Authorize(Roles="Admin")] public class DashboardController : Controller { public IActionResult Index()=>View(); }