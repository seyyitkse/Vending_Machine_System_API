using AutoMapper;
using Vending.DtoLayer.Dtos.AppUserDto;
using Vending.DtoLayer.Dtos.DepartmentsDto;
using Vending.DtoLayer.Dtos.OrderDto;
using Vending.DtoLayer.Dtos.RoleDto;
using Vending.DtoLayer.Dtos.TodoItemDto;

using Vending.EntityLayer.Concrete;

namespace Vending.ApiLayer.Mapping
{
    public class AutoMapperConfig:Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<AppUser, LoginAppUserDto>().ReverseMap();
            CreateMap<CreateAppUserDto, AppUser>().ReverseMap();
            CreateMap<LoginAppUserDto, AppUser>().ReverseMap();
            CreateMap<CreateDepartmentDto, Department>().ReverseMap();
            CreateMap<CreateTodoItemDto, TodoItem>().ReverseMap();
            CreateMap<ResultDepartmentDto, Department>().ReverseMap();
            CreateMap<UpdateRoleDto, AppRole>().ReverseMap();
            CreateMap<ResultAppUserRolesDto, AppUser>().ReverseMap();
            CreateMap<ResultAdminUserDto, AppUser>().ReverseMap();
            CreateMap<ResultCustomerUserDto, AppUser>().ReverseMap();
            CreateMap<CreateOrderDto, AppUser>().ReverseMap();
        }
    }
}
