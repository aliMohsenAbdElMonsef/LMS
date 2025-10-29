namespace LMS.MVC.Services.Response
{
    public class SuccessServiceResult<T>: BasicServiceResult
    {
        public T? Data { get; set; }
    }
}
