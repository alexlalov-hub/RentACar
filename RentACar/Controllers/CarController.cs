using Microsoft.AspNetCore.Authorization;
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

        // Главен метод за извеждане на всички автомобили, приема текст по който да търси(по описание, модел и марка), приема категория по която да филтрира
        // , диапазон от години на производство по който да филтрира, диапазон от цени по който да сортира, има възможност за сортиране по години във възходящ и низходящ ред
        // и възможност за сортиране по цена на ден във възходящ и низходящ ред. Чрез финалния ред се създава и обект благодарение на който има възможност и за странициране
        public async Task<IActionResult> Index(string searchValue, int? categoryId, string? priceRange, string? yearRange, string? sort, int? pageNumber, int? pageSize)
        {
            // Първоначално зареждане на всички коли от базата като заедно с тях зареждаме категорията и снимки на всяка една
            var cars = _context.Cars.Include(c => c.Category).Include(i => i.Images).ToList();

            // Ако текстът за търсене не е празен  търсим в листът от коли всички тези, които отговарят на условието за търсене
            if (!string.IsNullOrEmpty(searchValue))
            {
                cars = cars
                    .Where(x => x.Description.ToLower().Contains(searchValue.ToLower())
                    || x.Brand.ToLower().Contains(searchValue.ToLower())
                    || x.Model.ToLower().Contains(searchValue.ToLower()))
                    .ToList();
            }

            // Ако категорията не е празна търсим сред всички коли тези които имат тази специална категория
            if (categoryId != null)
            {
                cars = cars.Where(x => x.CategoryId == categoryId).ToList();
            }

            // Ако диапазонът от цени не е празен се търси всички коли принадлежащи на диапазона
            if (!string.IsNullOrEmpty(priceRange))
            {
                // Така се намира стартът на диапазона
                string startPriceString = priceRange.ToString().Split("-")[0];
                // Така се намира краят на диапазона
                string endPriceString = priceRange.ToString().Split("-")[1];

                // Ако крайната цена е плюс намираме тези с дневна цена над 50000 иначе ще получим грешка
                if (endPriceString == "+")
                {
                    cars = cars.Where(x => x.DailyPrice >= 50000).ToList();
                }
                else
                {
                    //Превръщаме резултатите в числа и намираме всички коли в диапазона
                    int startPrice = int.Parse(startPriceString);
                    int endPrice = int.Parse(endPriceString);

                    cars = cars.Where(x => x.DailyPrice >= startPrice && x.DailyPrice <= endPrice).ToList();
                }
            }

            // Ако диапазонът от години не е празен се търси всички коли принадлежащи на диапазона
            if (!string.IsNullOrEmpty(yearRange))
            {
                // Така се намира стартът на диапазона
                string startYearString = yearRange.ToString().Split("-")[0];
                // Така се намира краят на диапазона
                string endYearString = yearRange.ToString().Split("-")[1];

                // Ако крайната година е плюс намираме тези коли направени след 2010 иначе ще получим грешка
                if (endYearString == "+")
                {
                    cars = cars.Where(x => x.ManufactureYear >= 2010).ToList();
                }
                else
                {
                    // Така се намира стартът на диапазона
                    int startYear = int.Parse(startYearString);
                    // Така се намира краят на диапазона
                    int endYear = int.Parse(endYearString);

                    cars = cars.Where(x => x.ManufactureYear >= startYear && x.ManufactureYear <= endYear).ToList();
                }
            }

            // Ако полето за сортиране не празен сортираме всички коли по избраният от потребителя начин
            if (!string.IsNullOrEmpty(sort))
            {
                cars = sort switch
                {
                    //Сортиране по година възходящ ред
                    "Year" => cars.OrderBy(car => car.ManufactureYear).ToList(),
                    //Сортиране по година низходящ ред
                    "Year Descending" => cars.OrderByDescending(car => car.ManufactureYear).ToList(),
                    //Сортиране по цена възходящ ред
                    "Daily Price" => cars.OrderBy(car => car.DailyPrice).ToList(),
                    //Сортиране по цена низходящ ред
                    "Daily Price Descending" => cars.OrderByDescending(car => car.DailyPrice).ToList(),
                    _ => cars.OrderBy(car => car.ManufactureYear).ToList(),
                };
            }

            //Зареждане на всички категории от базата за да е възможно филтрирането по категория
            ViewData["Categories"] = new SelectList(_context.Categories.ToList(), "Id", "Name");

            //Изпращане на всички коли към сайта с помощта на модел, който позволява страницирането им като му се задават номер на страница и колко коли да бъдат показани на всяка страница
            return View(await PaginatedList<Car>.CreateAsync(cars.AsQueryable(), pageNumber ?? 1, pageSize ?? 5));
        }

        //Метод за показване на детайлен изглед за кола
        public async Task<IActionResult> Details(int? id)
        {
            //Ако Idто не бъде пратено на контролера или в базата няма коли бива хвърлена грешка
            if (id == null || _context.Cars == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //Зареждане на колата от базата по нейното Id като биват заредени и нейните категория и снимки
            var car = await _context.Cars
                .Include(c => c.Category)
                .Include(i => i.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            //Ако колата не бъде намерена в базата бива хвърлена 
            if (car == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            return View(car);
        }

        //Метод за зареждане на страницата за наемане на кола който е наличен само за служители
        [Authorize(Roles = "Employee")]
        [HttpGet]
        public IActionResult RentACar(int? id)
        {
            //Ако Idто не бъде пратено на контролера или в базата няма коли бива хвърлена грешка
            if (id == null || _context.Cars == null)
            {
                return RedirectToAction("NotFound", "Error");
            }
           
            //Зареждане на всички потребители, които имат роля клиент
            var clients = _userManager.GetUsersInRoleAsync("Client").Result.ToList();

            //Изпращане на тези клиенти към сайта
            ViewData["Users"] = new SelectList(clients, "Id", "UserName");
            //Зареждане на всички локации и изпращането им за избор от служителя
            ViewData["Locations"] = new SelectList(_context.Locations, "Id", "Name");
            return View();
        }

        //Метод за наемане на кола достъпен само за служители
        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> RentACar(int? id, [FromForm] RentedCar car)
        {
            //Ако id не съществува бива хвърлена грешка
            if (id == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //Ако датата на връщане е преди датата на наемане бива хвърлена грешка
            if (car.ReturnedDate < car.RentedDate)
            {
                ModelState.AddModelError("Returned Date", "The return date must not be before the begin date!");
            }

            //Ако датата на наемане е след датата на връщане бива хвърлена грешка
            if (car.RentedDate > car.ReturnedDate)
            {
                ModelState.AddModelError("Rented Date", "The begin date must not be before the return date!");
            }

            //Зареждане на колата отговаряща на това id
            var carToRent = await _context.Cars.FindAsync(id);

            //Ако колата не бъде намерена в базата бива хвърлена грешка
            if (carToRent == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //Ако няма грешки при въведената информация колата бива запазена в базата като и ѝ се задават крайна цена, дата на наемане и дата на връщане
            if (ModelState.IsValid)
            {
                carToRent.IsRented = true;
                car.FinalPrice = (car.ReturnedDate - car.RentedDate).Days * carToRent.DailyPrice;
                car.CarId = carToRent.Id;
                _context.Add(car);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            //Повторно изпращане на всички потребители към сайта в случай на грешка
            ViewData["Users"] = new SelectList(_context.Users, "Id", "UserName");
            // Повторно зареждане на всички локации и изпращането им за избор от служителя в случай на грешка
            ViewData["Locations"] = new SelectList(_context.Locations, "Id", "Name");
            return View();
        }

        //Метод за връщане на кола достъпен само за служители
        [Authorize(Roles = "Employee")]
        public IActionResult ReturnACar(int? id)
        {
            //Ако id не съществува бива хвърлена грешка
            if (id == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //Зареждане на колата отговаряща на това id
            var car = _context.Cars.Find(id);

            //Ако колата не бъде намерена в базата бива хвърлена грешка
            if (car == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //Ако колата която търсим е наета зарежаме модела от базата
            var rentedCar = _context.RentedCars.Where(c => c.CarId == id).FirstOrDefault();

            //Ако наетата кола не бъде намерена в базата бива хвърлена грешка
            if (rentedCar == null)
            {
                return RedirectToAction("NotFound", "Error");
            }
            else
            {
                //Променяме статуса на колата от наета на свободна
                car.IsRented = false;
                //Премахваме наетата кола от таблицата за наети коли
                _context.RentedCars.Remove(rentedCar);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

        }

        //Метод за зареждане на форма за създаване на кола
        [Authorize(Roles = "Employee")]
        [HttpGet]
        public IActionResult Create()
        {
            //Зареждане на всички категории и изпращането им към сайта
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        //Метод за създаване на кола
        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Car car)
        {
            //Ако във формата няма празни полета или грешни данни влизаме в if
            if (ModelState.IsValid)
            {
                //Създаване на нова инстанция на модела Car и записване на данните от формата в него
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

                //Създаване на празен лист от снимки
                List<Image> images = new();

                //Проверка дали са качени снимки от формата
                if (car.Files.Count > 0)
                {
                    //За всеки един файл се прави едно и също нещо
                    foreach (var formFile in car.Files)
                    {
                        if (formFile.Length > 0)
                        {
                            using var memoryStream = new MemoryStream();
                            await formFile.CopyToAsync(memoryStream);

                            //Създаване на инстанция на модела Image и записване на нужните данни
                            var newphoto = new Image()
                            {
                                // Масив от байтове чрез който снимката бива визуализирана в сайта
                                Bytes = memoryStream.ToArray(),
                                // Разширението на снимката(jpg, webp ....)
                                FileExtension = Path.GetExtension(formFile.FileName),
                                //Големината на снимката
                                Size = formFile.Length
                            };
                            //Добавяне на снимката в листа с всички
                            images.Add(newphoto);
                        }
                    }
                }

                //Запълване на листа със снимки и задаването му на модела и добавянето на колата в базата
                newCar.Images = images;
                _context.Cars.Add(newCar);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // Повторно зареждане на всички категории и изпращането им за избор от служителя в случай на грешка
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View(car);
        }
        
        //Метод за визуализиране на снимка
        public string GetFileFromBytes(int? id)
        {
            //Намиране на всички снимки на зададената кола
            var image = _context.Images.FirstOrDefault(i => i.Id == id);

            //Превръщане на снимката в четим за сайта формат чрез масивът ѝ от байтове
            string imreBase64Data = Convert.ToBase64String(image.Bytes);

            //Създаване на URL благодарение на който снимката може да бъде показана в сайта
            string imgDataURL = string.Format("data:image;base64,{0}", imreBase64Data);

            return imgDataURL;
        }

        //Метод за зареждане на форма за редактиране на автомобил
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Edit(int? id)
        {
            //Ако Idто не бъде пратено на контролера или в базата няма коли бива хвърлена грешка
            if (id == null || _context.Cars == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //Зареждане на колата със зададеното id
            var car = await _context.Cars.Include(c => c.Category).FirstOrDefaultAsync(car => car.Id == id);

            //Ако колата не бъде намерена в базата бива хвърлена грешка
            if (car == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //Ако колата е наета бива хвърлена грешка
            if (car.IsRented)
            {
                return RedirectToAction("Rented", "ErrorController");
            }

            //Създаване на инстанция на помощен модел благодарение на който става възможно зареждането на досегашните данни за колата за да могат след това да бъдат редактирани
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

            // Повторно зареждане на всички категории и изпращането им за избор от служителя в случай на грешка
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", car.CategoryId);
            return View(carToEdit);
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarEditViewModel car)
        {
            //Ако двете idта не са еднакви бива хвърлена грешка
            if (id != car.Id)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //Зареждане на колата която ще бъде редактирана
            var carToEdit = await _context.Cars.FindAsync(id);

            //Ако колата не бъде намерена в базата бива хвърлена грешка
            if (carToEdit == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //Презаписване на данните които ще бъдат редактирани
            carToEdit.Id = car.Id;
            carToEdit.Brand = car.Brand;
            carToEdit.Model = car.Model;
            carToEdit.ManufactureYear = car.ManufactureYear;
            carToEdit.DailyPrice = car.DailyPrice;
            carToEdit.Description = car.Description;
            carToEdit.CategoryId = car.CategoryId;

            //Ако всички полета са валидни колата бива редактирана
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
            // Повторно зареждане на всички категории и изпращането им за избор от служителя в случай на грешка
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", car.CategoryId);
            return View(car);
        }

        //Метод за изтриване на кола от базата
        [Authorize(Roles = "Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            //Ако Idто не бъде пратено на контролера или в базата няма коли бива хвърлена грешка
            if (id == null || _context.Cars == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //Зареждане на колата която искаме да изтрием
            var car = await _context.Cars
                .Include(i => i.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            //Ако колата не бъде намерена в базата бива хвърлена грешка
            if (car == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            //Ако колата е наета бива хвърлена грешка
            if (car.IsRented)
            {
                return RedirectToAction("NotFound", "Error");
            }

            _context.Remove(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Метод за откриване дали кола съществува в базата
        private bool CarExists(int id)
        {
            return (_context.Cars?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
