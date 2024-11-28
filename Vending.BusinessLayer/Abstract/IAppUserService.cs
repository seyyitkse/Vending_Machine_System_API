using Vending.DtoLayer.Dtos.AppUserDto;
using Vending.DtoLayer.Response.AppUserResponse;
using Vending.EntityLayer.Concrete;

namespace Vending.BusinessLayer.Abstract
{
    public interface IAppUserService:IGenericService<AppUser>
    {
        Task<AppUserManagerResponse> RegisterUserAsync(CreateAppUserDto createAppUserDto);
        Task<AppUserManagerResponse> LoginUserAsync(LoginAppUserDto loginAppUserDto);
        Task<AppUserManagerResponse> MobileLoginAsync(LoginAppUserDto loginAppUserDto);
        Task<AppUserManagerResponse> MobileLogoutAsync(string email);
    }
}
