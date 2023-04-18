using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentACar.Data;
using RentACar.Models;
using System.Drawing.Drawing2D;

namespace RentACar.Controllers
{
    public class CarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CarController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Car
        public async Task<IActionResult> Index(string searchValue, int? categoryId, string? priceRange, string? yearRange, string? sort, int? pageNumber, int? pageSize)
        {
            var cars = _context.Cars.Include(c => c.Category).Include(i => i.Images).ToList();

            if (!string.IsNullOrEmpty(searchValue))
            {
                cars = cars
                    .Where(x => x.Description.ToLower().Contains(searchValue.ToLower())
                    || x.Brand.ToLower().Contains(searchValue.ToLower())
                    || x.Model.ToLower().Contains(searchValue.ToLower()))
                    .ToList();
            }

            if (categoryId != null)
            {
                cars = cars.Where(x => x.CategoryId == categoryId).ToList();
            }

            if (!string.IsNullOrEmpty(priceRange))
            {
                string startPriceString = priceRange.ToString().Split("-")[0];
                string endPriceString = priceRange.ToString().Split("-")[1];

                if (endPriceString == "+")
                {
                    cars = cars.Where(x => x.DailyPrice >= 50000).ToList();
                }
                else
                {
                    int startPrice = int.Parse(startPriceString);
                    int endPrice = int.Parse(endPriceString);

                    cars = cars.Where(x => x.DailyPrice >= startPrice && x.DailyPrice <= endPrice).ToList();
                }
            }

            if (!string.IsNullOrEmpty(yearRange))
            {
                string startYearString = yearRange.ToString().Split("-")[0];
                string endYearString = yearRange.ToString().Split("-")[1];

                if (endYearString == "+")
                {
                    cars = cars.Where(x => x.ManufactureYear >= 2010).ToList();
                }
                else
                {
                    int startYear = int.Parse(startYearString);
                    int endYear = int.Parse(endYearString);

                    cars = cars.Where(x => x.ManufactureYear >= startYear && x.ManufactureYear <= endYear).ToList();
                }
            }

            if (!string.IsNullOrEmpty(sort))
            {
                cars = sort switch
                {
                    "Year" => cars.OrderByDescending(car => car.ManufactureYear).ToList(),
                    "Year Descending" => cars.OrderBy(car => car.ManufactureYear).ToList(),
                    "Daily Price" => cars.OrderByDescending(car => car.ManufactureYear).ToList(),
                    "Daily Price Descending" => cars.OrderBy(car => car.ManufactureYear).ToList(),
                    _ => cars.OrderBy(car => car.Brand).ToList(),
                };
            }

            ViewData["Categories"] = new SelectList(_context.Categories.ToList(), "Id", "Name");
            return View(await PaginatedList<Car>.CreateAsync(cars.AsQueryable(), pageNumber ?? 1, pageSize ?? 5));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Cars == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var car = await _context.Cars
                .Include(c => c.Category)
                .Include(i => i.Images)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            return View(car);
        }

        [HttpGet]
        public IActionResult RentACar(int? id)
        {
            if (id == null || _context.Cars == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var clients = _userManager.GetUsersInRoleAsync("Client").Result.ToList();

            ViewData["Users"] = new SelectList(clients, "Id", "UserName");
            ViewData["Locations"] = new SelectList(_context.Locations, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RentACar(int? id, [FromForm] RentedCar car)
        {
            if (id == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            if(car.ReturnedDate < car.RentedDate)
            {
                ModelState.AddModelError("Returned Date", "The return date must not be before the begin date!");
            }

            if (car.RentedDate > car.ReturnedDate)
            {
                ModelState.AddModelError("Rented Date", "The begin date must not be before the return date!");
            }

            var carToRent = await _context.Cars.FindAsync(id);

            if (carToRent == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            if (ModelState.IsValid)
            {
                carToRent.IsRented = true;
                car.FinalPrice = (car.ReturnedDate - car.RentedDate).Days * carToRent.DailyPrice;
                car.CarId = carToRent.Id;
                _context.Add(car);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewData["Users"] = new SelectList(_context.Users, "Id", "UserName");
            ViewData["Locations"] = new SelectList(_context.Locations, "Id", "Name");
            return View();
        }

        public IActionResult ReturnACar(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var car = _context.Cars.Find(id);

            if (car == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var rentedCar = _context.RentedCars.Where(c => c.CarId == id).FirstOrDefault();

            if (rentedCar == null)
            {
                return RedirectToAction("NotFound", "Error");
            }
            else
            {
                car.IsRented = false;
                _context.RentedCars.Remove(rentedCar);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

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
                    Description = car.Description,
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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Cars == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var car = await _context.Cars.Include(c => c.Category).FirstOrDefaultAsync(car => car.Id == id);

            if (car == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            if (car.IsRented)
            {
                return RedirectToAction("Rented", "ErrorController");
            }

            var carToEdit = new CarEditViewModel()
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                ManufactureYear = car.ManufactureYear,
                DailyPrice = car.DailyPrice,
                Description = car.Description,
                CategoryId = car.CategoryId,
            };

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", car.CategoryId);
            return View(carToEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarEditViewModel car)
        {
            if (id != car.Id)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var carToEdit = await _context.Cars.FindAsync(id);

            if (carToEdit == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            carToEdit.Id = car.Id;
            carToEdit.Brand = car.Brand;
            carToEdit.Model = car.Model;
            carToEdit.ManufactureYear = car.ManufactureYear;
            carToEdit.DailyPrice = car.DailyPrice;
            carToEdit.Description = car.Description;
            carToEdit.CategoryId = car.CategoryId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(carToEdit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", car.CategoryId);
            return View(car);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null || _context.Cars == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var car = await _context.Cars
                .Include(i => i.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (car == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            if (car.IsRented)
            {
                return RedirectToAction("NotFound", "Error");
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
