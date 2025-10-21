namespace LMS.MVC.Services.Response
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
        public string? UserId { get; set; }
        public string? Token { get; set; }
    }
}
