using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public AppUserController(UserManager<AppUser> userManager, ILogger<AppUserController> logger, VendingContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

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
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.PhoneNumber,
                    Role = _userManager.GetRolesAsync(user).Result.FirstOrDefault()
                })
                .ToListAsync();

            return Ok(users);
        }

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


    }
}
