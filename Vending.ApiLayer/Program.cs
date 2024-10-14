using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Filters;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
//Log.Logger = new LoggerConfiguration()
//    .Enrich.FromLogContext()  // Add contextual information like controller
//    .WriteTo.Console()  // Write logs to console
//    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, shared: true)  // Write logs to file
//    .CreateLogger();
// Serilog configuration

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()  // Add contextual information like controller
    .WriteTo.Console()  // Write logs to console
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, shared: true)  // Write logs to file
    .Filter.ByIncludingOnly(Matching.FromSource("Vending.ApiLayer.Controllers.WeatherForecastController")) // Include logs only from WeatherForecastController
    //.Filter.ByIncludingOnly(Matching.FromSource("Vending.ApiLayer.Controllers.TestController")) // Include logs from TestController
    .CreateLogger();

builder.Logging.ClearProviders();  // Clear default log providers
builder.Host.UseSerilog();  // Configure to use Serilog

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Define CORS policy to allow external access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAllOrigins"); // Enable CORS policy

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
