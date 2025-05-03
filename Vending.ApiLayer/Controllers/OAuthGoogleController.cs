using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Vending.ApiLayer.Models;
using Vending.BusinessLayer.Abstract;

namespace Vending.ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OAuthGoogleController : ControllerBase
    {
        [HttpGet("login")]
        public IActionResult Login()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // Google giriş cevabı
        private readonly IConfiguration _configuration;
        private readonly IAppUserService _userService; // Kullanıcı varsa ekle, yoksa kaydet

        public OAuthGoogleController(IConfiguration configuration, IAppUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        [HttpPost("google-response")]
        public async Task<IActionResult> GoogleResponse([FromBody] GoogleTokenDto dto)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.Token);

                // Veritabanında kullanıcıyı kontrol et
                var user = await _userService.GetUserByEmailAsync(payload.Email);
                if (user == null)
                {
                    // Kullanıcıyı kaydet (soft kayıt)
                    var fullname = payload.Name ?? "GoogleUser"; // Eğer Name null ise varsayılan bir değer kullanılır
                    user = await _userService.RegisterGoogleUserAsync(payload.Email, fullname);
                }

                // Rolleri al
                var roles = await _userService.GetRolesForUserAsync(user);

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName ?? "GoogleUser"),
        };

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var jwt = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(1),
                    signingCredentials: creds
                );

                var token = new JwtSecurityTokenHandler().WriteToken(jwt);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Token doğrulanamadı", detail = ex.Message });
            }
        }



        //// Çıkış işlemi
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
