using Microsoft.AspNetCore.Mvc;

namespace RentACar.Controllers
{
    public class ErrorController : Controller
    {
        //Метод за извикване на страница за грешка свързана с вече наета кола
        public IActionResult Rented()
        {
            return View();
        }

        //Метод за извикване на персонализирана страница за грешка за ненамерен обект от базата
        public IActionResult NotFound()
        {
            return View();
        }
    }
}
