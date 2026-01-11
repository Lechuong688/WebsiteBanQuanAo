using Data.Entity;
using Data.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

using Microsoft.Extensions.DependencyInjection;

// Load config
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

// DI container
var services = new ServiceCollection();

// DbContext
services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(
        configuration.GetConnectionString("DefaultConnection"));
});