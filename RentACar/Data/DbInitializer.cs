using Microsoft.AspNetCore.Identity;
using RentACar.Models;

namespace RentACar.Data
{
    public static class DbInitializer
    {
        public static long GetSize(string sPath)
        {
            FileInfo fInfo = new(sPath);
            long numBytes = fInfo.Length;

            return numBytes;
        }

        public static byte[] ReadFile(string sPath)
        {
            byte[] data = null;

            FileInfo fInfo = new(sPath);
            long numBytes = fInfo.Length;

            FileStream fStream = new(sPath, FileMode.Open, FileAccess.Read);

            BinaryReader br = new(fStream);

            data = br.ReadBytes((int)numBytes);

            return data;
        }

        public static void Initialize(ApplicationDbContext context)
        {
            if (context.Roles.Any() || context.Users.Any() || context.Cars.Any())
            {
                return;
            }

            //Roles
            IdentityRole[] roles = new IdentityRole[]
            {
                new IdentityRole
                {
                    Id = "4af8f32f-1c3c-43b0-9f5a-216f3df61618",
                    Name = "Client",
                    NormalizedName = "CLIENT",
                },

                new IdentityRole
                {
                    Id = "6bd33099-ebce-44d7-90dc-87312c20ee64",
                    Name = "Employee",
                    NormalizedName = "EMPLOYEE"
                }
            };

            context.Roles.AddRange(roles);
            context.SaveChanges();

            //Users
            ApplicationUser[] users = new ApplicationUser[]
            {
                new ApplicationUser
                {
                    Id = "d504e155-5a72-4da9-8872-71876610436a",
                    UserName = "client",
                    FirstName = "Client",
                    LastName = "Clientov",
                    NormalizedUserName = "CLIENT",
                    Email = "client@client.com",
                    NormalizedEmail = "CLIENT@CLIENT.COM"
                },

                new ApplicationUser
                {
                    Id = "99f7484c-d997-4b6e-87d4-27d59f5c7c6e",
                    UserName = "employee",
                    FirstName = "Empoyee",
                    LastName = "Empoyov",
                    NormalizedUserName = "EMPLOYEE",
                    Email = "employee@employee.com",
                    NormalizedEmail = "EMPLOYEE@EMPLOYEE.COM"
                },
            };

            context.Users.AddRange(users);
            context.SaveChanges();

            PasswordHasher<ApplicationUser> hasher = new();
            users[0].PasswordHash = hasher.HashPassword(users[0], "Client1!");
            users[1].PasswordHash = hasher.HashPassword(users[1], "Employee1!");

            //UserRoles
            IdentityUserRole<string>[] userRoles = new IdentityUserRole<string>[]
            {
                new IdentityUserRole<string>
                {
                    UserId = users[0].Id,
                    RoleId = roles[0].Id
                },

                new IdentityUserRole<string>
                {
                    UserId = users[1].Id,
                    RoleId = roles[1].Id
                }
            };

            context.UserRoles.AddRange(userRoles);
            context.SaveChanges();

            //Categories
            Category[] categories = new Category[]
            {
                new Category
                {
                    Name = "Hybrid"
                },
                new Category
                {
                    Name = "Electric"
                },
                new Category
                {
                    Name = "SUV"
                },
                new Category
                {
                    Name = "Compact"
                },
                new Category
                {
                    Name = "Sport"
                }
            };

            context.Categories.AddRange(categories);
            context.SaveChanges();

            //Locations
            Location[] locations = new Location[]
            {
                new Location
                {
                    Name = "Sofia"
                },
                new Location
                {
                    Name = "New York"
                },
                new Location
                {
                    Name = "Istanbul"
                },
                new Location
                {
                    Name = "Budapest"
                },
            };

            context.Locations.AddRange(locations);
            context.SaveChanges();

            //Cars
            Car[] cars = new Car[]
            {
                new Car
                {
                    Brand = "Honda",
                    Model = "Civic",
                    ManufactureYear = 2023,
                    DailyPrice = 2000,
                    CategoryId = categories[3].Id,
                    Description = "The 2023 Honda Civic sets the standard for the compact car class. It's fun to drive, fuel-efficient, comfortable, well-built and loaded with convenience and safety features.",
                    IsRented = false,
                },

                new Car
                {
                    Brand = "Chevrolet",
                    Model = "Bolt",
                    ManufactureYear = 2023,
                    DailyPrice = 3000,
                    CategoryId = categories[1].Id,
                    Description = "As its name suggests, the 2023 Chevrolet Bolt knows how to get up to speed quickly, but that's far from this Chevy's only strength. " +
                    "The Bolt provides a spacious cabin for such a petite vehicle, and it offers tremendous driving range compared to the competition.",
                    IsRented = false,
                },

                new Car
                {
                    Brand = "Hyundai",
                    Model = "Elantra",
                    ManufactureYear = 2023,
                    DailyPrice = 4000,
                    CategoryId = categories[0].Id,
                    Description = "The 2023 Hyundai Elantra Hybrid offers stellar fuel economy, a peppy powertrain, balanced ride and handling and a feature-packed, spacious interior. " +
                    "The cabin is a bit plasticky, and the gas engine can be noisy, but there's still a lot to like about this Hyundai hybrid.",
                    IsRented = false,
                },

                new Car
                {
                    Brand = "Mazda",
                    Model = "CX-5",
                    ManufactureYear = 2023,
                    DailyPrice = 5000,
                    CategoryId = categories[2].Id,
                    Description = "The 2023 Hyundai Elantra Hybrid offers stellar fuel economy, a peppy powertrain, balanced ride and handling and a feature-packed, spacious interior. " +
                    "The cabin is a bit plasticky, and the gas engine can be noisy, but there's still a lot to like about this Hyundai hybrid.",
                    IsRented = false,
                },

                new Car
                {
                    Brand = "Toyota",
                    Model = "Supra",
                    ManufactureYear = 2019,
                    DailyPrice = 10000,
                    CategoryId = categories[4].Id,
                    Description = "The Toyota Supra blends balance, agility, grip and poise with a punchy six-cylinder motor that delivers a hit of performance and (mostly) the stirring engine note we were after. " +
                    "Whether or not the Toyota collaboration with BMW that produced this Supra affects its authenticity, we’ll leave up to you. But there’s no doubting that this is a coupe with real talent and welcome " +
                    "character at a time when those traits in new cars are becoming less common. ",
                    IsRented = false,
                },
            };

            context.Cars.AddRange(cars);
            context.SaveChanges();

            //Images
            Image[] images = new Image[]
            {
                new Image
                {
                    Bytes = ReadFile("wwwroot/images/honda_civic.jpg"),
                    FileExtension = ".jpg",
                    Size = GetSize("wwwroot/images/honda_civic.jpg"),
                    CarId = cars[0].Id
                },
                new Image
                {
                    Bytes = ReadFile("wwwroot/images/chevrolet_bolt.jpg"),
                    FileExtension = ".jpg",
                    Size = GetSize("wwwroot/images/chevrolet_bolt.jpg"),
                    CarId = cars[1].Id
                },
                new Image
                {
                    Bytes = ReadFile("wwwroot/images/hyundai_elantra.jpg"),
                    FileExtension = ".jpg",
                    Size = GetSize("wwwroot/images/hyundai_elantra.jpg"),
                    CarId = cars[2].Id
                },
                new Image
                {
                    Bytes = ReadFile("wwwroot/images/mazda_cx5.jpg"),
                    FileExtension = ".jpg",
                    Size = GetSize("wwwroot/images/mazda_cx5.jpg"),
                    CarId = cars[3].Id
                },
                new Image
                {
                    Bytes = ReadFile("wwwroot/images/toyota_supra.jpg"),
                    FileExtension = ".jpg",
                    Size = GetSize("wwwroot/images/toyota_supra.jpg"),
                    CarId = cars[4].Id
                }
            };

            context.Images.AddRange(images);
            context.SaveChanges();
        }
    }
}
