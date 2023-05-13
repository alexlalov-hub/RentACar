using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentACar.Data;
using RentACar.Models;

namespace RentACar.Controllers
{
    [Authorize(Roles = "Employee")]
    public class LocationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LocationController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Главен метод за визуализиране на всички локации. В случай че в базата няма категория възниква проблем
        public async Task<IActionResult> Index()
        {
              return _context.Locations != null ? 
                          View(await _context.Locations.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Locations'  is null.");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Locations == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var location = await _context.Locations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (location == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            return View(location);
        }

        //Метод за визуализиране на форма за създаване на локация
        public IActionResult Create()
        {
            return View();
        }

        //Метод за създаване на категория
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Location location)
        {
            //В случай че всички полета са валидни локацията бива създадена
            if (ModelState.IsValid)
            {
                _context.Add(location);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }

        //Метод за визуализиране на форма за редактиране на локация
        public async Task<IActionResult> Edit(int? id)
        {
            //Ако Idто не бъде пратено на контролера или в базата няма локации бива хвърлена грешка
            if (id == null || _context.Locations == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //Намиране на локация, която искаме да редактираме
            var location = await _context.Locations.FindAsync(id);

            //Ако локацията не бъде намерена в базата бива хвърлена грешка
            if (location == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            return View(location);
        }

        //Метод за редактиране на локация
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Location location)
        {
            //Ако двете id-та не са еднакви бива хвърлена грешка
            if (id != location.Id)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //В случай че всички полета са валидни локацията бива редактирана 
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(location);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationExists(location.Id))
                    {
                        return RedirectToAction("NotFound", "Error");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }

        //Метод за изтриване на локация
        public async Task<IActionResult> Delete(int? id)
        {
            //Ако id-то не бъде изпратено бива хвърлена грешка
            if (id == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //Ако не съществуват локации базата бива хвърлена грешка
            if (_context.Locations == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Locations'  is null.");
            }

            //Намиране на всички коли с тази локация
            var carsWithLocation = _context.RentedCars.Where(c => c.LocationReturnedId == id).ToList();

            //Ако има такива не бива позволено да се изтрие локацията
            if (carsWithLocation.Any())
            {
                return RedirectToAction("Rented", "Error");
            }

            //Ако няма такива локацията бива намерена по своето id
            var location = await _context.Locations.FindAsync(id);

            //Ако категорията съществува тя бива изтрита
            if (location != null)
            {
                _context.Locations.Remove(location);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Метод за откриване дали локация съществува в базата
        private bool LocationExists(int id)
        {
          return (_context.Locations?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
