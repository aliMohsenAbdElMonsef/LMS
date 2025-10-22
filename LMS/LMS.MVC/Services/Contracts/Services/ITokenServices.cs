namespace LMS.MVC.Services.Contracts.Services
{
    public interface ITokenService
    {
        Task<string?> GetAccessTokenAsync();
    }

}
