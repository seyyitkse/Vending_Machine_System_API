using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog.Context;
using Vending.DataAccessLayer.Concrete;
using Vending.DtoLayer.Dtos.AppUserDto;
using Vending.EntityLayer.Concrete;

namespace Vending.ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppUserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AppUserController> _logger; // ILogger bağımlılığı
        private readonly VendingContext _context;

        public AppUserController(UserManager<AppUser> userManager, ILogger<AppUserController> logger, VendingContext context, IMemoryCache cache)
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
            Cache = cache;
        }

        public IMemoryCache Cache { get; }

        /// <summary>
        /// Tüm kullanıcıları ve rollerini getirir.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetApplicationUsers()
        {
            using (LogContext.PushProperty("LogType", "APPUSERROLE-LIST"))
            {
                _logger.LogInformation("Tüm kullanıcılar alınıyor.");
            }

            var users = await _userManager.Users
                .Select(user => new
                {
                    Id = user.UserCode,
                    FullName = user.FullName ?? null, // Ensure null if missing
                    Email = user.Email ?? null, // Ensure null if missing
                    PhoneNumber = user.PhoneNumber ?? null, // Ensure null if missing
                    Role = _userManager.GetRolesAsync(user).Result.FirstOrDefault() ?? null // Ensure null if no role
                })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// Tüm kullanıcıların rollerini getirir.
        /// </summary>
        [HttpGet("getUserRoles")]
        public async Task<IActionResult> GetUserRoles()
        {
            using (LogContext.PushProperty("LogType", "APPUSERROLE-LIST"))
            {
                _logger.LogInformation("Kullanıcı rolleri alınıyor.");
            }
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new List<ResultAppUserRolesDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new ResultAppUserRolesDto
                {
                    UserId = user.Id,
                    Username = user.FirstName + " " + user.LastName, // Kullanıcı adı ve soyadını birleştir
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return Ok(userRoles);
        }

        /// <summary>
        /// Belirtilen kullanıcıya roller ekler veya çıkarır.
        /// </summary>
        /// <param name="userId">Kullanıcı ID'si</param>
        /// <param name="request">Güncellenecek roller</param>
        [HttpPost("getUserRoles/{userId}/roles")]
        public async Task<IActionResult> UpdateUserRoles(string userId, [FromBody] UpdateAppUserRoles request)
        {
            _logger.LogInformation($"Kullanıcı rolleri güncelleniyor. Kullanıcı ID: {userId}"); // Log ekleme
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                using (LogContext.PushProperty("LogType", "APPUSERROLE-FAIL"))
                {
                    _logger.LogWarning($"Kullanıcı bulunamadı. Kullanıcı: {user.UserName}");
                }
                return NotFound($"Kullanıcı bulunamadı. Kullanıcı: {user.UserName}");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = request.Roles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(request.Roles).ToList();

            _logger.LogInformation($"Güncellenen kullanıcı bilgileri: Kullanıcı Adı: {user.FirstName} {user.LastName}, E-posta: {user.Email}"); // Log ekleme

            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    using (LogContext.PushProperty("LogType", "APPUSERROLE-FAIL"))
                    {
                        _logger.LogWarning("Rolleri eklerken hata oluştu: {@Errors}", addResult.Errors);
                    }
                    return BadRequest("Rolleri güncelleme başarısız.");
                }

                _logger.LogInformation("Kullanıcıya eklenen roller: {Roles}", string.Join(", ", rolesToAdd)); // Log ekleme
            }

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    using (LogContext.PushProperty("LogType", "APPUSERROLE-FAIL"))
                    {
                        _logger.LogWarning("Rolleri çıkarırken hata oluştu: {@Errors}", removeResult.Errors);
                    }
                    return BadRequest("Rolleri güncelleme başarısız.");
                }
                using (LogContext.PushProperty("LogType", "APPUSERROLE-SUCCESS"))
                {
                    _logger.LogInformation("Kullanıcıdan çıkarılan roller: {Roles}", string.Join(", ", rolesToRemove));
                }
            }
            using (LogContext.PushProperty("LogType", "APPUSERROLE-SUCCESS"))
            {
                _logger.LogInformation("Kullanıcı rolleri başarıyla güncellendi.");
            }
            return Ok();
        }

        /// <summary>
        /// Belirtilen kullanıcı ID'sine göre kullanıcı rollerini getirir.
        /// </summary>
        /// <param name="userId">Kullanıcı ID'si</param>
        [HttpGet("getUserRoles/{userId}")]
        public async Task<IActionResult> GetUserRolesByUserId(string userId)
        {
            _logger.LogInformation("Kullanıcı rolleri getiriliyor: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Kullanıcı bulunamadı: {UserId}", userId);
                return NotFound(new { Message = "Kullanıcı bulunamadı." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("Kullanıcı rolleri başarıyla getirildi: {UserId}", userId);
            return Ok(roles);
        }

        /// <summary>
        /// Belirtilen departmandaki müşteri kullanıcılarını getirir.
        /// </summary>
        /// <param name="departmentId">Departman ID'si</param>
        [HttpGet("Customer/{departmentId}")]
        public async Task<IActionResult> GetCustomerUsersByDepartment(int departmentId)
        {
            using (LogContext.PushProperty("LogType", "APPUSERROLE-SUCCESS"))
            {
                _logger.LogInformation($"Müşteri kullanıcıları alınıyor. Departman ID: {departmentId}");
            }
            var customerUsers = await _userManager.GetUsersInRoleAsync("Customer");
            var customerUsersInDepartment = customerUsers.Where(u => u.DepartmentID == departmentId).ToList();

            return Ok(customerUsersInDepartment);
        }

        /// <summary>
        /// Belirtilen departmandaki admin kullanıcılarını getirir.
        /// </summary>
        /// <param name="departmentId">Departman ID'si</param>
        [HttpGet("Admin/{departmentId}")]
        public async Task<IActionResult> GetAdminUsersByDepartment(int departmentId)
        {
            using (LogContext.PushProperty("LogType", "APPUSERROLE-SUCCESS"))
            {
                _logger.LogInformation($"Admin kullanıcıları alınıyor. Departman ID: {departmentId}"); // Log ekleme
            }
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var adminUsersInDepartment = adminUsers.Where(u => u.DepartmentID == departmentId).ToList();

            return Ok(adminUsersInDepartment);
        }

        /// <summary>
        /// Tüm müşteri kullanıcılarını listeler.
        /// </summary>
        [HttpGet("getCustomerList")]
        public async Task<IActionResult> GetCustomerUsersList()
        {
            using (LogContext.PushProperty("LogType", "APPUSERLIST-CUSTOMER"))
            {
                _logger.LogInformation("Müşteri kullanıcıları listeleniyor.");
            }

            var allUsers = await _userManager.Users.Include(u => u.Department).ToListAsync();
            var customerUsers = new List<ResultCustomerUserDto>();

            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, "Customer"))
                {
                    customerUsers.Add(new ResultCustomerUserDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        DepartmentName = user.Department != null ? user.Department.Name : "No Department"
                    });
                }
            }

            return Ok(customerUsers);
        }

        /// <summary>
        /// Belirtilen kullanıcı ID'sine göre kullanıcı bilgilerini getirir.
        /// </summary>
        /// <param name="id">Kullanıcı ID'si</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            _logger.LogInformation("Kullanıcı bilgileri getiriliyor: {UserId}", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Kullanıcı bulunamadı: {UserId}", id);
                return NotFound(new { Message = "Kullanıcı bulunamadı." });
            }

            var department = await _context.Departments.FindAsync(user.DepartmentID);

            var userDto = new
            {
                user.Email,
                user.FullName,
                DepartmentId = user.DepartmentID,
                DepartmentName = department?.Name
            };

            _logger.LogInformation("Kullanıcı bilgileri başarıyla getirildi: {UserId}", id);
            return Ok(userDto);
        }

        /// <summary>
        /// Belirtilen kullanıcıyı günceller.
        /// </summary>
        /// <param name="id">Kullanıcı ID'si</param>
        /// <param name="model">Güncellenecek kullanıcı bilgileri</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateAppUserDto model)
        {
            _logger.LogInformation("Kullanıcı bilgileri güncelleniyor: {UserId}", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Kullanıcı bulunamadı: {UserId}", id);
                return NotFound(new { Message = "Kullanıcı bulunamadı." });
            }

            user.FullName = model.FullName;

            // Find the department by name
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.Name == model.DepartmentName);
            if (department == null)
            {
                _logger.LogWarning("Departman bulunamadı: {DepartmentName}", model.DepartmentName);
                return NotFound(new { Message = "Departman bulunamadı." });
            }

            user.DepartmentID = department.DepartmentID;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Kullanıcı bilgileri güncellenemedi: {UserId}", id);
                return BadRequest(new { Message = "Kullanıcı bilgileri güncellenemedi." });
            }

            _logger.LogInformation("Kullanıcı bilgileri başarıyla güncellendi: {UserId}", id);
            return Ok(new { Message = "Kullanıcı bilgileri başarıyla güncellendi." });
        }

        /// <summary>
        /// Tüm admin kullanıcılarını listeler.
        /// </summary>
        [HttpGet("getAdminUsers")]
        public async Task<IActionResult> GetAdminUsersList()
        {
            using (LogContext.PushProperty("LogType", "APPUSERLIST-ADMIN"))
            {
                _logger.LogInformation("Admin kullanıcıları listeleniyor."); // Log ekleme
            }

            var allUsers = await _userManager.Users.Include(u => u.Department).ToListAsync();
            var adminUsers = new List<ResultAdminUserDto>();

            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    adminUsers.Add(new ResultAdminUserDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        DepartmentName = user.Department != null ? user.Department.Name : "Kullanıcının departmanı yok!"
                    });
                }
            }
            return Ok(adminUsers);
        }

        ///// <summary>
        ///// Kullanıcıyı kontrol eder ve bakiye bilgilerini getirir.
        ///// </summary>
        ///// <param name="usercode">Kullanıcı kodu</param>
        //[HttpGet("checkUserAndGetBalance/{usercode}")]
        //public async Task<IActionResult> CheckUserAndGetBalance(string usercode)
        //{
        //    _logger.LogInformation("Kullanıcı kontrol ediliyor ve bakiye bilgisi alınıyor: {UserCode}", usercode);

        //    // Rate limit kontrolü (örnek: 5 saniyede bir istek)
        //    if (!Request.Headers.TryGetValue("X-Client-ID", out var clientId))
        //    {
        //        _logger.LogWarning("Rate limit için X-Client-ID başlığı eksik.");
        //        return BadRequest(new { Message = "Rate limit için X-Client-ID başlığı gereklidir." });
        //    }

        //    var cacheKey = $"RateLimit_{clientId}";
        //    if (_context.Cache.TryGetValue(cacheKey, out _))
        //    {
        //        _logger.LogWarning("Rate limit aşıldı: {ClientId}", clientId);
        //        return StatusCode(429, new { Message = "Çok fazla istek. Lütfen daha sonra tekrar deneyin." });
        //    }

        //    // Rate limit için cache'e ekle (25 saniye süreyle)
        //    _context.Cache.Set(cacheKey, true, TimeSpan.FromSeconds(25));

        //    // usercode'u long'a dönüştür
        //    if (!long.TryParse(usercode, out var usercodeAsLong))
        //    {
        //        _logger.LogWarning("Geçersiz usercode formatı: {UserCode}", usercode);
        //        return BadRequest(new { Exists = false, Message = "Geçersiz usercode formatı." });
        //    }

        //    try
        //    {
        //        // Kullanıcıyı usercode ile bul
        //        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserCode == usercodeAsLong);
        //        if (user == null)
        //        {
        //            _logger.LogWarning("Kullanıcı bulunamadı: {UserCode}", usercode);
        //            return NotFound(new { Exists = false, Message = "Kullanıcı bulunamadı." });
        //        }

        //        // Kullanıcının bilgileri
        //        var balance = user.CurrentBalance;
        //        var fullName = user.FullName ?? $"{user.FirstName} {user.LastName}".Trim();

        //        _logger.LogInformation("Kullanıcı bulundu ve bakiye bilgisi alındı: {UserCode}, Balance: {Balance}, FullName: {FullName}", usercode, balance, fullName);
        //        return Ok(new
        //        {
        //            Exists = true,
        //            UserCode = usercode,
        //            FullName = fullName,
        //            Balance = balance
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Kullanıcı kontrol edilirken bir hata oluştu: {UserCode}", usercode);
        //        return StatusCode(500, new { Exists = false, Message = "Bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
        //    }
        //}
        /// <summary>
        /// Kullanıcıyı kontrol eder ve bakiye bilgilerini getirir.
        /// </summary>
        /// <param name="usercode">Kullanıcı kodu</param>
        [HttpGet("checkUserAndGetBalance/{usercode}")]
        public async Task<IActionResult> CheckUserAndGetBalance(string usercode)
        {
            _logger.LogInformation("Kullanıcı kontrol ediliyor ve bakiye bilgisi alınıyor: {UserCode}", usercode);

            // usercode'u long'a dönüştür
            if (!long.TryParse(usercode, out var usercodeAsLong))
            {
                _logger.LogWarning("Geçersiz usercode formatı: {UserCode}", usercode);
                return BadRequest(new { Exists = false, Message = "Geçersiz kullanıcı kodu formatı." });
            }

            try
            {
                // Kullanıcıyı usercode ile bul
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserCode == usercodeAsLong);
                if (user == null)
                {
                    _logger.LogWarning("Kullanıcı bulunamadı: {UserCode}", usercode);
                    return Ok(new { Exists = false, Message = "Kullanıcı bulunamadı." });
                }

                // Kullanıcının bilgileri
                var balance = user.CurrentBalance;
                var fullName = user.FullName ?? $"{user.FirstName} {user.LastName}".Trim();

                _logger.LogInformation("Kullanıcı bulundu ve bakiye bilgisi alındı: {UserCode}, Balance: {Balance}, FullName: {FullName}", usercode, balance, fullName);
                return Ok(new
                {
                    Exists = true,
                    UserCode = usercode,
                    FullName = fullName,
                    Balance = balance
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı kontrol edilirken bir hata oluştu: {UserCode}", usercode);
                return StatusCode(500, new { Exists = false, Message = "Bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
            }
        }
    }
}