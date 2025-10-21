namespace LMS.MVC.Services.Contracts
{
    public interface IBaseMVCServices
    {
        Task<T> GetAsync<T>(string url);
        Task<T> PostAsync<T>(string url, HttpContent content);
    }
}
