using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // ILogger için gerekli using
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

        public AppUserController(UserManager<AppUser> userManager, ILogger<AppUserController> logger)
        {
            _userManager = userManager;
            _logger = logger; // ILogger bağımlılığının atanması
        }

        [HttpGet]
        public IActionResult GetApplicationUsers()
        {
            _logger.LogInformation("Tüm kullanıcılar alınıyor."); // Log ekleme
            var users = _userManager.Users.ToList();
            return Ok(users);
        }

        [HttpGet("getUserRoles")]
        public async Task<IActionResult> GetUsers()
        {
            _logger.LogInformation("Kullanıcı rolleri alınıyor."); // Log ekleme
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
                _logger.LogWarning($"Kullanıcı bulunamadı. Kullanıcı ID: {userId}"); // Log ekleme
                return NotFound();
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
                    _logger.LogError("Rolleri eklerken hata oluştu: {@Errors}", addResult.Errors); // Log ekleme
                    return BadRequest("Rolleri güncelleme başarısız.");
                }

                _logger.LogInformation("Kullanıcıya eklenen roller: {Roles}", string.Join(", ", rolesToAdd)); // Log ekleme
            }

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    _logger.LogError("Rolleri çıkarırken hata oluştu: {@Errors}", removeResult.Errors); // Log ekleme
                    return BadRequest("Rolleri güncelleme başarısız.");
                }

                _logger.LogInformation("Kullanıcıdan çıkarılan roller: {Roles}", string.Join(", ", rolesToRemove)); // Log ekleme
            }

            _logger.LogInformation("Kullanıcı rolleri başarıyla güncellendi."); // Log ekleme
            return Ok();
        }

        [HttpGet("Customer/{departmentId}")]
        public async Task<IActionResult> GetStudentUsersByDepartment(int departmentId)
        {
            _logger.LogInformation($"Müşteri kullanıcıları alınıyor. Departman ID: {departmentId}"); // Log ekleme
            var customerUsers = await _userManager.GetUsersInRoleAsync("Customer");
            var customerUsersInDepartment = customerUsers.Where(u => u.DepartmentID == departmentId).ToList();

            return Ok(customerUsersInDepartment);
        }

        [HttpGet("Admin/{departmentId}")]
        public async Task<IActionResult> GetTeacherUsersByDepartment(int departmentId)
        {
            _logger.LogInformation($"Admin kullanıcıları alınıyor. Departman ID: {departmentId}"); // Log ekleme
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var adminUsersInDepartment = adminUsers.Where(u => u.DepartmentID == departmentId).ToList();

            return Ok(adminUsersInDepartment);
        }
    }
}
