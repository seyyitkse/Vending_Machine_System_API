using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Filters;
using System.Text;
using Vending.BusinessLayer.Abstract;
using Vending.BusinessLayer.Concrete;
using Vending.DataAccessLayer.Abstract;
using Vending.DataAccessLayer.Concrete;
using Vending.DataAccessLayer.EntityFramework;
using Vending.EntityLayer.Concrete;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext() // To capture LogContext properties like LogType
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{LogType:-5} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, shared: true,
                  outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{LogType:-5} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Filter.ByIncludingOnly(Matching.FromSource("Vending.ApiLayer.Controllers"))
    .CreateLogger();

builder.Logging.ClearProviders();  // Clear default log providers
builder.Host.UseSerilog();  // Configure to use Serilog

builder.Services.AddIdentity<AppUser, AppRole>()
         .AddEntityFrameworkStores<VendingContext>()
         .AddDefaultTokenProviders();

builder.Services.AddAuthentication(auth =>
{
    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var tokenKey = builder.Configuration["AuthSettings:Token"];
    var issuer = builder.Configuration["AuthSettings:Issuer"];
    var audience = builder.Configuration["AuthSettings:Audience"];

    if (string.IsNullOrEmpty(tokenKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
    {
        throw new ArgumentNullException("JWT configuration values are missing in appsettings.json");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
        ValidateIssuer = false,
        ValidIssuer = issuer,
        ValidateAudience = false,
        ValidAudience = audience,
        RequireExpirationTime = false,
    };
});


builder.Services.AddDbContext<VendingContext>();


builder.Services.AddScoped<IDepartmentDal, EfDepartmentDal>();
builder.Services.AddScoped<IDepartmentService, DepartmentManager>();

builder.Services.AddScoped<IAppUserDal, EfAppUserDal>();
builder.Services.AddScoped<IAppUserService, AppUserManager>();

builder.Services.AddScoped<IVendDal, EfVendDal>();
builder.Services.AddScoped<IVendService, VendManager>();

builder.Services.AddScoped<ICategoryDal, EfCategoryDal>();
builder.Services.AddScoped<ICategoryService, CategoryManager>();

builder.Services.AddScoped<IProductDal, EfProductDal>();
builder.Services.AddScoped<IProductService, ProductManager>();


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(Program));


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



// Google Authentication Configuration
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
//})
//.AddCookie()
//.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
//{
//    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
//    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
//    options.CallbackPath = "/api/OAuthGoogle/google-response";  // Bu geri dönüþ yolu URI'niz olmalý
//});






var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAllOrigins"); // Enable CORS policy

app.UseHttpsRedirection();

//app.UseAuthentication(); // Add authentication middleware
//app.UseAuthorization();
app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();  // Add authorization middleware

app.MapControllers();

app.Run();
