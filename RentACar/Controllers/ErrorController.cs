using Microsoft.AspNetCore.Mvc;

namespace RentACar.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Rented()
        {
            return View();
        }

        public IActionResult NotFound()
        {
            return View();
        }
    }
}
