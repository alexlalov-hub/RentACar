using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentACar.Data;
using RentACar.Models;

namespace RentACar.Controllers
{
    [Authorize(Roles = "Employee")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Главен метод за визуализиране на всички категории. В случай че в базата няма категория възниква проблем
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Index()
        {
              return _context.Categories != null ? 
                          View(await _context.Categories.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
        }

        //Метод за визуализиране на форма за създаване на категория
        [Authorize(Roles = "Employee")]
        public IActionResult Create()
        {
            return View();
        }

        //Метод за създаване на категория
        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Category category)
        {
            //В случай че всички полета са валидни категорията бива създадена
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        //Метод за визуализиране на форма за редактиране на категория
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Edit(int? id)
        {
            //Ако Idто не бъде пратено на контролера или в базата няма категории бива хвърлена грешка
            if (id == null || _context.Categories == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //Намиране на категорията, която искаме да редактираме
            var category = await _context.Categories.FindAsync(id);

            //Ако категорията не бъде намерена в базата бива хвърлена грешка
            if (category == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            return View(category);
        }

        //Метод за редактиране на категорията
        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Category category)
        {
            //Ако двете id-та не са еднакви бива хвърлена грешка
            if (id != category.Id)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //В случай че всички полета са валидни категорията бива редактирана 
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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

            return View(category);
        }

        //Метод за изтриване на категория
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Delete(int? id)
        {
            //Ако id-то не бъде изпратено бива хвърлена грешка
            if (id == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //Ако не съществуват категории в базата бива хвърлена грешка
            if (_context.Categories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }

            //Намиране на всички коли с тази категория
            var carsWithCategory = _context.Cars.Where(c => c.CategoryId == id).ToList();

            //Ако има такива не бива позволено да се изтрие категорията
            if (carsWithCategory.Any())
            {
                return RedirectToAction("Rented", "Error");
            }

            //Ако няма такива категорията бива намерена по своето id
            var category = await _context.Categories.FindAsync(id);

            //Ако категорията съществува тя бива изтрита
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Метод за откриване дали категория съществува в базата
        private bool CategoryExists(int id)
        {
          return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
