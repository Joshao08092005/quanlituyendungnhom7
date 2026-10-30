
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aznews.Controllers
{
    [Authorize(Roles = "EMPLOYER")]
    public class EmployerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
