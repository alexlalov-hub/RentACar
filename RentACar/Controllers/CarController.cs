using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentACar.Data;
using RentACar.Models;

namespace RentACar.Controllers
{
    public class CarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Car
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Cars.Include(c => c.Category).Include(i => i.Images);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Car/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Cars == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .Include(c => c.Category)
                .Include(i => i.Images)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        [HttpGet]
        public IActionResult RentACar(int? id)
        {
            if (id == null || _context.Cars == null)
            {
                return NotFound();
            }


            ViewData["Users"] = new SelectList(_context.Users, "Id", "UserName");
            ViewData["Locations"] = new SelectList(_context.Locations, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RentACar([FromForm] RentedCar car)
        {
            car.CarId = int.Parse(Request.RouteValues["id"].ToString());

            var carToRent = await _context.Cars.FindAsync(car.CarId);

            if (ModelState.IsValid)
            {
                carToRent.IsRented = true;
                car.FinalPrice = (car.ReturnedDate - car.RentedDate).Days * carToRent.DailyPrice;
                _context.Add(car);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewData["Users"] = new SelectList(_context.Users, "Id", "UserName");
            ViewData["Locations"] = new SelectList(_context.Locations, "Id", "Name");
            return View();
        }

        public async Task<IActionResult> ReturnACar(int id)
        {
            var car = await _context.Cars.FindAsync(id);

            if (ModelState.IsValid)
            {
                car.IsRented = false;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Car car)
        {
            if (ModelState.IsValid)
            {
                Car newCar = new()
                {
                    Rating = null,
                    Brand = car.Brand,
                    Model = car.Model,
                    ManufactureYear = car.ManufactureYear,
                    DailyPrice = car.DailyPrice,
                    CategoryId = car.CategoryId,
                };

                List<Image> images = new();
                if (car.Files.Count > 0)
                {
                    foreach (var formFile in car.Files)
                    {
                        if (formFile.Length > 0)
                        {
                            using var memoryStream = new MemoryStream();
                            await formFile.CopyToAsync(memoryStream);

                                var newphoto = new Image()
                                {
                                    Bytes = memoryStream.ToArray(),
                                    FileExtension = Path.GetExtension(formFile.FileName),
                                    Size = formFile.Length
                                };
                                images.Add(newphoto);
                        }
                    }
                }

                newCar.Images = images;
                _context.Cars.Add(newCar);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View(car);
        }

        public string GetFileFromBytes(int? id)
        {
            var image = _context.Images.FirstOrDefault(i => i.Id == id);

            string imreBase64Data = Convert.ToBase64String(image.Bytes);

            string imgDataURL = string.Format("data:image;base64,{0}", imreBase64Data);

            return imgDataURL;
        }

        // GET: Car/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Cars == null)
            {
                return NotFound();
            }

            var car = await _context.Cars.Include(c => c.Category).FirstOrDefaultAsync(car => car.Id == id);

            if (car == null)
            {
                return NotFound();
            }

            if (car.IsRented)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", car.CategoryId);
            return View(car);
        }

        // POST: Car/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Brand,Model,ManufactureYear,Rating,DailyPrice,CategoryId")] Car car)
        {
            if (id != car.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(car);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", car.CategoryId);
            return View(car);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null || _context.Cars == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .Include(c => c.Category)
                .Include(i => i.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (car == null)
            {
                return NotFound();
            }

            if (car.IsRented)
            {
                return NotFound();
            }

            _context.Remove(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(int id)
        {
            return (_context.Cars?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
