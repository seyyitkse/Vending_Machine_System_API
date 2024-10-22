using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog.Context;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Vending.BusinessLayer.Abstract;
using Vending.DataAccessLayer.Concrete;
using Vending.DtoLayer.Dtos.AppUserDto;
using Vending.EntityLayer.Concrete;

namespace Vending.ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAppUserService _applicationUserService;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly VendingContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAppUserService applicationUserService, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IConfiguration configuration, VendingContext context, ILogger<AuthController> logger)
        {
            _applicationUserService = applicationUserService;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        private async Task<string> CreateTokenAsync(LoginAppUserDto loginUser)
        {
            AppUser user = _userManager.Users.FirstOrDefault(x => x.Email == loginUser.Email);
            var roles = await _userManager.GetRolesAsync(user);

            string name = $"{user.FirstName} {user.LastName}";
            int userID = user.Id;
            int departmanId = user.DepartmentID;
            Department department = _context.Departments.FirstOrDefault(x => x.DepartmentID == departmanId);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:Token"]));

            var claims = new List<Claim>
            {
                new Claim("Mail", user.Email),
                new Claim("Username", user.UserName),
                new Claim("Name", name),
                new Claim("Department", department.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserID", userID.ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["AuthSettings:Issuer"],
                audience: _configuration["AuthSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                notBefore: DateTime.Now,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            // Kullanıcı için token oluşturuluyor
            //_logger.LogInformation("Kullanıcı için JWT token oluşturuldu: {Email}",user.Email);
            //using (LogContext.PushProperty("LogType", "TOKEN"))
            //{
            //    _logger.LogInformation("Kullanıcı için JWT token oluşturuldu: {Email}", user.Email);
            //}
            using (LogContext.PushProperty("LogType", "TOKEN"))
            {
                _logger.LogInformation("Kullanıcı için JWT token oluşturuldu: {Email}", user.Email);
            }
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("RegisterCustomer")]
        public async Task<IActionResult> RegisterEmployeeAsync([FromBody] CreateAppUserDto model)
        {
            if (ModelState.IsValid)
            {
                var result = await _applicationUserService.RegisterUserAsync(model);
                if (result.IsSuccess)
                {
                    var user = await _userManager.FindByEmailAsync(model.Mail);
                    await _userManager.AddToRoleAsync(user, "Customer");
                    // Yeni kullanıcı kaydedildiğinde log kaydı
                    using (LogContext.PushProperty("LogType", "REGISTERCUSTOMER"))
                    {
                        _logger.LogInformation("Yeni kullanıcı kaydedildi: {Email}", model.Mail);
                    }

                    return Ok(result);
                }
                // Kayıt başarısız olduğunda log kaydı
                //_logger.LogWarning("Kullanıcı kaydı başarısız.");
                using (LogContext.PushProperty("LogType", "REGISTERCUSTOMER-FAIL"))
                {
                    _logger.LogWarning("Kullanıcı {Email} kaydı başarısız.",model.Mail);
                }
                return BadRequest(result);
            }
            return Ok();
        }

        [HttpPost("RegisterAdmin")]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] CreateAppUserDto model)
        {
            if (ModelState.IsValid)
            {
                var result = await _applicationUserService.RegisterUserAsync(model);
                if (result.IsSuccess)
                {
                    var user = await _userManager.FindByEmailAsync(model.Mail);
                    await _userManager.AddToRoleAsync(user, "Admin");
                    // Yeni admin kaydedildiğinde log kaydı
                    //_logger.LogInformation("Yeni admin kaydedildi: {Email}",user.Email);
                    using (LogContext.PushProperty("LogType", "REGISTERADMIN"))
                    {
                        _logger.LogInformation("Yeni admin {Email} kaydedildi.", model.Mail);
                    }
                    return Ok(result);
                }
                // Admin kaydı başarısız olduğunda log kaydı
                //_logger.LogWarning("Admin kaydı başarısız.");
                using (LogContext.PushProperty("LogType", "REGISTERADMIN-FAIL"))
                {
                    _logger.LogWarning("Admin {Email} kaydı başarısız.", model.Mail);
                }
                return BadRequest(result);
            }
            return BadRequest("Bazı değerler girilmedi!");
        }


        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginAppUserDto model)
        {
            if (ModelState.IsValid)
            {
                // Fetch the user by email
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return BadRequest("Invalid login attempt.");
                }

                // Check if the user is locked out
                if (await _userManager.IsLockedOutAsync(user))
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    using (LogContext.PushProperty("LogType", "LOGIN-FAIL-LOCKOUT"))
                    {
                        _logger.LogWarning("Hesabı kilitlenen kullanıcı {Email} giriş yapmaya çalıştı. Hesap {LockoutEnd} tarihine kadar kilitli.", model.Email, lockoutEnd?.DateTime.ToLocalTime());
                    }
                    return BadRequest($"Hesabınız {lockoutEnd?.DateTime.ToLocalTime()} tarihine kadar kilitlidir. Lütfen daha sonra tekrar deneyiniz.");
                }

                // Attempt login
                var result = await _applicationUserService.LoginUserAsync(model);

                if (result.IsSuccess)
                {
                    var token = await CreateTokenAsync(model);

                    // Reset failed login attempts on successful login
                    await _userManager.ResetAccessFailedCountAsync(user);

                    // Log for successful login
                    using (LogContext.PushProperty("LogType", "LOGIN"))
                    {
                        _logger.LogInformation("Kullanıcı {Email} giriş yaptı.", model.Email);
                    }

                    return Ok(new { token });
                }

                // Increment access failed count
                await _userManager.AccessFailedAsync(user);

                // Check if the failed attempts reached the lockout threshold
                int failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
                if (failedAttempts >= _userManager.Options.Lockout.MaxFailedAccessAttempts)
                {
                    // Lock the user out for the defined time
                    var lockoutEnd = DateTimeOffset.UtcNow.AddMinutes(_userManager.Options.Lockout.DefaultLockoutTimeSpan.TotalMinutes);
                    await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);

                    // Log lockout event
                    using (LogContext.PushProperty("LogType", "LOGIN-FAIL-LOCKOUT"))
                    {
                        _logger.LogWarning("Kullanıcı {Email} çok fazla hatalı giriş yaptı ve kilitlendi.", model.Email);
                    }

                    return BadRequest($"Your account has been locked due to too many failed login attempts. Please try again after {lockoutEnd.LocalDateTime}.");
                }

                // Log failed login attempt
                using (LogContext.PushProperty("LogType", "LOGIN-FAIL"))
                {
                    _logger.LogWarning("Kullanıcı {Email} giriş yapmaya çalıştı ama başarısız oldu.", model.Email);
                }

                return BadRequest("Invalid credentials. You have " + (3 - failedAttempts) + " attempt(s) remaining before account lockout.");
            }

            return BadRequest("Some values are invalid.");
        }



        [HttpPost("mobileLogin")]
        public async Task<IActionResult> MobileLoginAsync([FromBody] LoginAppUserDto model)
        {
            if (ModelState.IsValid)
            {
                var response = await _applicationUserService.MobileLoginAsync(model);
                if (response.IsSuccess)
                {
                    var token = await CreateTokenAsync(model);
                    // Başarılı mobil giriş logu
                    using (LogContext.PushProperty("LogType", "MOBILELOGIN"))
                    {
                        _logger.LogInformation("Mobil kullanıcı giriş yaptı: {Email}", model.Email);
                    }

                    return Ok(new { token, response.Message });
                }
                // Başarısız mobil giriş logu
                using (LogContext.PushProperty("LogType", "MOBILELOGIN-FAIL"))
                {
                    _logger.LogWarning("Mobil kullanıcı giriş yapmaya çalıştı ama başarısız oldu: {Email}", model.Email);
                }
                return BadRequest(response);
            }
            return BadRequest("Bazı değerler geçersiz.");
        }

        [HttpPost("mobileLogout")]
        public async Task<IActionResult> LogoutAsync([FromBody] LogoutAppUserDto model)
        {
            if (ModelState.IsValid)
            {
                var response = await _applicationUserService.MobileLogoutAsync(model.Email);
                if (response.IsSuccess)
                {
                    // Başarılı çıkış logu
                    using (LogContext.PushProperty("LogType", "MOBILELOGOUT"))
                    {
                        _logger.LogInformation("Mobil kullanıcı çıkış yaptı: {Email}", model.Email);
                    }
                    return Ok(new { Message = response.Message });
                }
                // Başarısız çıkış logu
                using (LogContext.PushProperty("LogType", "MOBILELOGOUT-FAIL"))
                {
                    _logger.LogWarning("Mobil kullanıcı çıkış yapmaya çalıştı ama başarısız oldu: {Email}", model.Email);
                }
                return BadRequest(response);
            }
            return BadRequest("Bazı değerler geçersiz.");
        }

        //private async Task<string> CreateTokenAsync(LoginAppUserDto loginUser)
        //{
        //    AppUser user = _userManager.Users.FirstOrDefault(x => x.Email == loginUser.Email);
        //    var roles = await _userManager.GetRolesAsync(user);

        //    string name = $"{user.FirstName} {user.LastName}";
        //    int userID = user.Id;

        //    int departmanId = user.DepartmentID;
        //    Department department = _context.Departments.FirstOrDefault(x => x.DepartmentID == departmanId);
        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:Token"]));

        //    var claims = new List<Claim>
        //    {
        //        new Claim("Mail", user.Email),
        //        new Claim("Username", user.UserName),
        //        new Claim("Name", name),
        //        new Claim("Department", department.Name),
        //        //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //        new Claim("UserID", userID.ToString())
        //    };

        //    foreach (var role in roles)
        //    {
        //        claims.Add(new Claim(ClaimTypes.Role, role));
        //    }

        //    var token = new JwtSecurityToken(
        //        issuer: _configuration["AuthSettings:Issuer"],
        //        audience: _configuration["AuthSettings:Audience"],
        //        claims: claims,
        //        expires: DateTime.Now.AddDays(30),
        //        notBefore: DateTime.Now,
        //        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

        //public async Task<List<UserLoginHistory>> GetUserLoginHistory(string userId)
        //{
        //    return await _context.UserLoginHistory
        //        .Where(x => x.UserId == userId)
        //        .OrderByDescending(x => x.LoginTime)
        //        .ToListAsync();
        //}
        // 1. Enable 2FA for the user
        //        var user = await _userManager.FindByEmailAsync(model.Email);
        //        await _userManager.SetTwoFactorEnabledAsync(user, true);

        //        // 2. Generate a 2FA token
        //        var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

        //        // 3. Send the 2FA token to the user
        //        // This will depend on your method of communication, but here's a basic example for email:
        //        await _emailSender.SendEmailAsync(user.Email, "Your authentication code", $"Your 2FA code is: {token}");
    }
}
