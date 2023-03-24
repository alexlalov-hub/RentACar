namespace RentACar.Data
{
    public static class DbInitiliazerExtension
    {
        public static IApplicationBuilder Seed(this IApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));

            using var scope = builder.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                DbInitializer.Initialize(context);
            }
            catch (Exception ex)
            {
                ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occured seeding users.");
            }

            return builder;
        }
    }
}
