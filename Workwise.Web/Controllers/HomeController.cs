using Microsoft.AspNetCore.Mvc;
namespace Workwise.Web.Controllers;
public class HomeController : Controller { public IActionResult Index()=>View(); public IActionResult Error()=>View(); }