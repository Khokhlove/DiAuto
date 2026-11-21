using DiAvto.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=../../data/diavto.db;Cache=Shared";

builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
builder.Services.AddDiAvtoInfrastructure(builder.Configuration, Directory.GetCurrentDirectory());

var host = builder.Build();
await DatabaseInitializer.InitializeAsync(host.Services);
Console.WriteLine("DiAvto: миграции применены, публичный контент загружен.");
