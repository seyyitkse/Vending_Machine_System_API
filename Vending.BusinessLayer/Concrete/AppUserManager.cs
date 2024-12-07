using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Vending.BusinessLayer.Abstract;
using Vending.DataAccessLayer.Abstract;
using Vending.DtoLayer.Dtos.AppUserDto;
using Vending.DtoLayer.Response.AppUserResponse;
using Vending.EntityLayer.Concrete;

namespace Vending.BusinessLayer.Concrete
{
    public class AppUserManager : IAppUserService
    {
        private UserManager<AppUser>? _userManager;

        private SignInManager<AppUser>? _signInManager;
        private IAppUserDal? _applicationUserDal;

        private IConfiguration? _configuration;

        public AppUserManager(UserManager<AppUser>? userManager, SignInManager<AppUser>? signInManager, IAppUserDal? applicationUserDal, IConfiguration? configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _applicationUserDal = applicationUserDal;
            _configuration = configuration;
        }

        public async Task<AppUserManagerResponse> LoginUserAsync(LoginAppUserDto loginAppUserDto)
        {
            var user = await _userManager.FindByEmailAsync(loginAppUserDto.Email);
            if (user == null)
            {
                // Kullanıcı bulunamazsa hata döndür
                return new AppUserManagerResponse
                {
                    Message = "There is no user with that email address",
                    IsSuccess = false
                };
            }

            // Kullanıcının parolasını doğrula
            var result = await _signInManager.PasswordSignInAsync(user, loginAppUserDto.Password, false, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                // Giriş başarılı ise başarılı yanıt döndür
                return new AppUserManagerResponse
                {
                    Message = "Giriş başarılı!",
                    IsSuccess = true
                };
            }
            else
            {
                // Parola doğrulaması başarısız ise hata döndür
                return new AppUserManagerResponse
                {
                    Message = "Geçersiz şifre",
                    IsSuccess = false
                };
            }
        }

        public async Task<AppUserManagerResponse> MobileLoginAsync(LoginAppUserDto loginAppUserDto)
        {
            var user = await _userManager.FindByEmailAsync(loginAppUserDto.Email);
            if (user == null)
            {
                // Kullanıcı bulunamazsa hata döndür
                return new AppUserManagerResponse
                {
                    Message = "Bu maile sahip bir kullanıcı bulunamadı.",
                    IsSuccess = false
                };
            }
            //burası sonradan tekrar açılacak!
            // Kullanıcının başka bir cihazda oturum açıp açmadığını kontrol ediyoruz
            //var existingLogin = await _userManager.GetLoginsAsync(user);
            //if (existingLogin.Any())
            //{
            //    return new AppUserManagerResponse
            //    {
            //        Message = "Lütfen diğer cihazdaki oturumu kapatınız",
            //        IsSuccess = false
            //    };
            //}

            // Kullanıcının parolasını doğruluyoruz
            var result = await _signInManager.PasswordSignInAsync(user, loginAppUserDto.Password, false, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                // Giriş başarılı ise AspNetUserLogins tablosuna bir giriş ekliyoruz
                //var loginEntry = new UserLoginInfo("Mobile", user.Email, "Mobile User");
                //await _userManager.AddLoginAsync(user, loginEntry);

                // Başarılı yanıt döndürüyoruz
                return new AppUserManagerResponse
                {
                    Message = "Giriş başarılı",
                    IsSuccess = true
                };
            }
            else
            {
                // Parola doğrulaması başarısız ise hata döndürüyoruz
                return new AppUserManagerResponse
                {
                    Message = "Geçersiz şifre",
                    IsSuccess = false
                };
            }
        }

        public async Task<AppUserManagerResponse> MobileLogoutAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new AppUserManagerResponse
                {
                    Message = "Bu e-posta adresine sahip bir kullanıcı bulunamadı.",
                    IsSuccess = false
                };
            }

            // Kullanıcının tüm girişlerini silmek için AspNetUserLogins tablosundan girişleri siliyoruz
            var loginInfo = await _userManager.GetLoginsAsync(user);
            foreach (var login in loginInfo)
            {
                await _userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
            }

            // Kullanıcın çıkış yapması için gerekli işlemleri yapıyoruz
            await _signInManager.SignOutAsync();

            return new AppUserManagerResponse
            {
                Message = "Kullanıcı başarıyla çıkış yaptı.",
                IsSuccess = true
            };
        }

        public async Task<AppUserManagerResponse> RegisterUserAsync(CreateAppUserDto createAppUserDto)
        {
            if (createAppUserDto == null)
                throw new NullReferenceException("Boş veriler var");

            if (createAppUserDto.Password != createAppUserDto.ConfirmPassword)
            {
                return new AppUserManagerResponse
                {
                    Message = "Girdiğiniz parolalar eşleşmiyor.",
                    IsSuccess = false,
                };
            }

            var existingUserByEmail = await _userManager.FindByEmailAsync(createAppUserDto.Mail);
            if (existingUserByEmail != null)
            {
                return new AppUserManagerResponse
                {
                    Message = "Bu e-posta adresiyle kayıtlı bir kullanıcı zaten var.",
                    IsSuccess = false
                };
            }

            var identityuser = new AppUser()
            {
                FirstName = createAppUserDto.FirstName,
                LastName = createAppUserDto.LastName,
                Email = createAppUserDto.Mail,
                UserName = createAppUserDto.Mail,
                DepartmentID = createAppUserDto.DepartmentID,
                FullName = createAppUserDto.FirstName + " " + createAppUserDto.LastName
            };

            var result = await _userManager.CreateAsync(identityuser, createAppUserDto.Password);
            if (result.Succeeded)
            {
                //await _userManager.AddToRoleAsync(identityuser, "Ogrenci");

                return new AppUserManagerResponse
                {
                    Message = "Kullanıcı oluşturma işlemi başarıyla gerçekleştirildi.",
                    IsSuccess = true,
                    Errors = result.Errors.Select(e => e.Description)
                };
            }
            return new AppUserManagerResponse
            {
                Message = "Kullanıcı oluşturulamadı!",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description)
            };
        }

        public void TDelete(AppUser entity)
        {
            _applicationUserDal?.Delete(entity);
        }

        public AppUser TGetById(int id)
        {
            return _applicationUserDal.GetById(id);
        }

        public List<AppUser> TGetList()
        {
            return _applicationUserDal.GetAll();
        }

        public void TInsert(AppUser entity)
        {
            _applicationUserDal?.Insert(entity);
        }

        public void TUpdate(AppUser entity)
        {
            _applicationUserDal?.Update(entity);
        }
    }
}
