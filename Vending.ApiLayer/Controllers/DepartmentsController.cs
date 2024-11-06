using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Vending.BusinessLayer.Abstract;
using Vending.DataAccessLayer.Concrete;
using Vending.DtoLayer.Dtos.DepartmentsDto;
using Vending.EntityLayer.Concrete;

namespace Vending.ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly VendingContext _context;
        private readonly ILogger<DepartmentsController> _logger;
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(VendingContext context, ILogger<DepartmentsController> logger, IDepartmentService departmentService)
        {
            _context = context;
            _logger = logger;
            _departmentService = departmentService;
        }

        // GET: api/Departments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
            using (LogContext.PushProperty("LogType", "DEPARTMENTLIST"))
            {
                _logger.LogInformation("Departman listeleme işlemi yapıldı.");
            }
            return await _context.Departments.ToListAsync();
        }

        // GET: api/Departments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            _logger.LogInformation($"ID {id} olan departman getiriliyor.");

            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                using (LogContext.PushProperty("LogType", "DEPARTMENTLIST"))
                {
                    _logger.LogWarning($"ID {id} olan departman bulunamadı.");
                }
                return NotFound();
            }

            return department;
        }

        // PUT: api/Departments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] Department department)
        {
            if (id != department.DepartmentID)
            {
                using (LogContext.PushProperty("LogType", "DEPARTMENTUPDATE-FAIL"))
                {
                    _logger.LogWarning("Departman ID uyuşmazlığı. Güncelleme başarısız.");
                }
                return BadRequest();
            }

            // Retrieve the old department for logging purposes
            var oldDepartment = await _context.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.DepartmentID == id);
            if (oldDepartment == null)
            {
                using (LogContext.PushProperty("LogType", "DEPARTMENTUPDATE-FAIL"))
                {
                    _logger.LogWarning("Güncellenecek departman bulunamadı.");
                }
                return NotFound();
            }

            // Update department name only; avoid changing AppUsers
            var oldDepartmentName = oldDepartment.Name;
            var newDepartmentName = department.Name;

            // Track the modified state for the department
            _context.Entry(department).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentExists(id))
                {
                    using (LogContext.PushProperty("LogType", "DEPARTMENTUPDATE-FAIL"))
                    {
                        _logger.LogWarning("Departman güncellenirken ID {id} bulunamadı.", id);
                    }
                    return NotFound();
                }
                else
                {
                    using (LogContext.PushProperty("LogType", "DEPARTMENTUPDATE-FAIL"))
                    {
                        _logger.LogError("{oldDepartmentName} isimli departman güncellenirken bir hata oluştu.", oldDepartmentName);
                    }
                    throw;
                }
            }

            using (LogContext.PushProperty("LogType", "DEPARTMENTUPDATE-SUCCESS"))
            {
                _logger.LogInformation("{oldDepartmentName} isimli departman, {newDepartmentName} olarak başarıyla güncellendi.", oldDepartmentName, newDepartmentName);
            }

            return Ok();
        }

        // POST: api/Departments
        //[HttpPost]
        //public async Task<ActionResult<Department>> PostDepartment(Department department)
        //{
        //    // Check if a department with the same name already exists
        //    var existingDepartment = await _context.Departments
        //        .FirstOrDefaultAsync(d => d.Name == department.Name);

        //    if (existingDepartment != null)
        //    {
        //        using (LogContext.PushProperty("LogType", "DEPARTMENTCREATE"))
        //        {
        //            _logger.LogWarning("{Name} isimli departman zaten mevcut. Oluşturma başarısız.", department.Name);
        //        }

        //        return BadRequest(new { message = "A department with the same name already exists." });
        //    }

        //    // Handle the app users if they exist
        //    if (department.AppUsers != null && department.AppUsers.Any())
        //    {
        //        foreach (var user in department.AppUsers)
        //        {
        //            user.DepartmentID = department.DepartmentID;
        //            _context.Entry(user).State = user.Id == 0 ? EntityState.Added : EntityState.Modified;
        //        }
        //    }

        //    // Add the new department
        //    _context.Departments.Add(department);
        //    await _context.SaveChangesAsync();

        //    using (LogContext.PushProperty("LogType", "DEPARTMENTCREATE"))
        //    {
        //        _logger.LogInformation("{Name} isimli departman başarıyla oluşturuldu.", department.Name);
        //    }

        //    return CreatedAtAction("GetDepartment", new { id = department.DepartmentID }, department);
        //}

        // DELETE: api/Departments/5

        [HttpPost]
        public async Task<ActionResult<Department>> PostDepartment(CreateDepartmentDto departmentDto)
        {
            // Check if a department with the same name already exists
            var existingDepartment = await _context.Departments
                .FirstOrDefaultAsync(d => d.Name == departmentDto.Name);

            if (existingDepartment != null)
            {
                using (LogContext.PushProperty("LogType", "DEPARTMENTCREATE-FAIL"))
                {
                    _logger.LogWarning("{Name} isimli departman zaten mevcut. Oluşturma başarısız.", departmentDto.Name);
                }

                return BadRequest(new { message = "Bu isimli departman daha önce oluşturulmuş." });
            }

            // Map CreateDepartmentDto to Department
            var department = new Department
            {
                Name = departmentDto.Name
            };

            // Add the new department
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            using (LogContext.PushProperty("LogType", "DEPARTMENTCREATE-SUCCESS"))
            {
                _logger.LogInformation("{Name} isimli departman başarıyla oluşturuldu.", department.Name);
            }

            return CreatedAtAction("GetDepartment", new { id = department.DepartmentID }, department);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _context.Departments.Include(d => d.AppUsers).FirstOrDefaultAsync(d => d.DepartmentID == id);
            if (department == null)
            {
                using (LogContext.PushProperty("LogType", "DEPARTMENTDELETE-FAIL"))
                {
                    _logger.LogWarning("ID {id} olan departman bulunamadı.", id);
                }
                return NotFound();
            }

            // Bağlantılı kullanıcıların departmanını kaldırma
            foreach (var user in department.AppUsers)
            {
                user.DepartmentID = null; // Kullanıcıların DepartmentID'sini kaldır
            }

            await _context.SaveChangesAsync(); // Önce kullanıcı güncellemelerini kaydedin

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync(); // Şimdi departmanı silin

            using (LogContext.PushProperty("LogType", "DEPARTMENTDELETE-SUCCESS"))
            {
                _logger.LogInformation("{departmentName} isimli departman silindi.", department.Name);
            }

            return Ok();
        }
        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.DepartmentID == id);
        }
    }
}