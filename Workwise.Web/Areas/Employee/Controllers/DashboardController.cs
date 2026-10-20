using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Workwise.Web.Areas.Employee.Controllers;
[Area("Employee")][Authorize(Roles="Employee")] public class DashboardController : Controller { public IActionResult Index()=>View(); }