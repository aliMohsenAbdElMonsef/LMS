namespace LMS.BusinessLogic.DTOs.Email
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = "noreply@lms.com";
        public string SenderName { get; set; } = "LMS System";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool EnableSsl { get; set; } = true;
        public bool UseMockEmail { get; set; } = true; 
    }
}
