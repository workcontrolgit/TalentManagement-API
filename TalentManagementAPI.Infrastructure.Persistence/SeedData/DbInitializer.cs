namespace TalentManagementAPI.Infrastructure.Persistence.SeedData
{
    // Static class for database initialization
    public static class DbInitializer
    {
        // Method to seed data into the database
        public static void SeedData(ApplicationDbContext dbContext)
        {
            // Create an instance of DatabaseSeeder
            var databaseSeeder = new DatabaseSeeder();

            // Insert departments data
            dbContext.Departments.AddRange(databaseSeeder.Departments);

            // Insert salary ranges data
            dbContext.SalaryRanges.AddRange(databaseSeeder.SalaryRanges);

            // Insert positions data
            dbContext.Positions.AddRange(databaseSeeder.Positions);

            // Insert employees data
            dbContext.Employees.AddRange(databaseSeeder.Employees);

            dbContext.SaveChanges();
        }
    }
}
