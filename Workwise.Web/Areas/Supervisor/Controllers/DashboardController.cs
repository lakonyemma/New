using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Workwise.Web.Areas.Supervisor.Controllers;
[Area("Supervisor")][Authorize(Roles="Supervisor")] public class DashboardController : Controller { public IActionResult Index()=>View(); }