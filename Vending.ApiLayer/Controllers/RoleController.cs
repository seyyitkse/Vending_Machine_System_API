using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog.Context;
using Vending.DtoLayer.Dtos.RoleDto;
using Vending.EntityLayer.Concrete;

namespace Vending.ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ILogger<RoleController> _logger;
        public RoleController(RoleManager<AppRole> roleManager, ILogger<RoleController> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            using (LogContext.PushProperty("LogType", "ROLELIST"))
            {
                //hangi kullanıcının listeleme işlemi yaptığı loga eklenecek.Aynı şekilde kim hangi işlemi yaptıysa onun bilgisi loga eklenecek.
                _logger.LogWarning("Rol listeleme işlemi ... kullanıcı tarafından yapıldı.");
            }
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                _logger.LogWarning("Rol ID {roleId} ile eşleşen rol bulunamadı.", id);
                return NotFound(new { message = "Rol bulunamadı." });
            }

            _logger.LogInformation("Rol ID {roleId} başarıyla bulundu: {RoleName}", id, role.Name);
            return Ok(new { id = role.Id, name = role.Name });
        }
        //[HttpPost]
        //public async Task<IActionResult> CreateRole(ApplicationRole roleModel)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Check if the role already exists
        //        bool roleExists = await _roleManager.RoleExistsAsync(roleModel?.RoleName);
        //        if (roleExists)
        //        {
        //            ModelState.AddModelError("", "Role Already Exists");
        //        }
        //        else
        //        {
        //            // Create the role
        //            // We just need to specify a unique role name to create a new role
        //            ApplicationRole identityRole = new ApplicationRole
        //            {
        //                Name = roleModel?.RoleName
        //            };
        //            // Saves the role in the underlying AspNetRoles table
        //            IdentityResult result = await _roleManager.CreateAsync(identityRole);
        //            if (result.Succeeded)
        //            {
        //                return Ok($"'{roleModel.RoleName}' adlı rol başarıyla oluşturuldu.");
        //            }         
        //            return BadRequest("Rol oluşturma başarısız."); 
        //        }
        //    }
        //    return Ok(" Hata!!!");
        //}
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            if (string.IsNullOrEmpty(dto.RoleName))
            {
                ModelState.AddModelError(nameof(dto.RoleName), "Rol ismi boş olamaz!");
                return BadRequest(new { errors = ModelState });
            }

            bool roleExists = await _roleManager.RoleExistsAsync(dto.RoleName);
            if (roleExists)
            {
                ModelState.AddModelError(nameof(dto.RoleName), $"{dto.RoleName} isimli rol daha önce oluşturulmuş.");

                using (LogContext.PushProperty("LogType", "ROLECREATE-FAIL"))
                {
                    _logger.LogWarning("{roleName} isimli rol daha önce oluşturulmuş.", dto.RoleName);
                }
                return BadRequest(new { errors = ModelState });
            }

            AppRole role = new AppRole { Name = dto.RoleName };
            IdentityResult result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                using (LogContext.PushProperty("LogType", "ROLECREATE-SUCCESS"))
                {
                    _logger.LogInformation("{roleName} isimli rol başarıyla oluşturuldu.", dto.RoleName);
                }
                return Ok($"'{dto.RoleName}' isimli rol başarıyla oluşturuldu.");
            }

            ModelState.AddModelError("", "Rol oluşturma hatası! Lütfen tekrar deneyiniz.");
            return BadRequest(new { errors = ModelState });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleDto updatedRoleDto)
        {
            if (string.IsNullOrWhiteSpace(updatedRoleDto.Name))
            {
                using (LogContext.PushProperty("LogType", "ROLEUPDATE"))
                {
                    _logger.LogWarning("Geçersiz rol ismi girildi.");
                }
                return BadRequest(new { message = "Rol ismi boş olamaz." });
            }

            // Retrieve the role by ID
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                using (LogContext.PushProperty("LogType", "ROLEUPDATE"))
                {
                    _logger.LogWarning("Rol ID {roleId} ile eşleşen rol bulunamadı.", id);
                }
                return NotFound(new { message = "Rol bulunamadı." });
            }

            // Save old role name for logging
            var oldRoleName = role.Name;

            // Update role name
            role.Name = updatedRoleDto.Name; // Use updated role name from DTO
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                using (LogContext.PushProperty("LogType", "ROLEUPDATE"))
                {
                    _logger.LogInformation("{OldRoleName} isimli rol, {NewRoleName} olarak başarıyla güncellendi.", oldRoleName, updatedRoleDto.Name);
                }
                return Ok(new { message = $"{oldRoleName} isimli rol, {updatedRoleDto.Name} olarak başarıyla güncellendi." });
            }

            // Log errors and return bad request
            _logger.LogError("Rol güncelleme başarısız oldu: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }
    }
}
