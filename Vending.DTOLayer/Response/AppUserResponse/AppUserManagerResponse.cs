using Vending.EntityLayer.Concrete;

namespace Vending.DtoLayer.Response.AppUserResponse
{
    public class AppUserManagerResponse
    {
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        public DateTime? ExpireDate { get; set; }
        public AppUser? Data { get; set; }
    }
}
