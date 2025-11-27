namespace LMS.MVC.Models.ViewModels.Notification
{
    public class NotificationViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = string.Empty; // e.g., "Info", "Warning", "Success"
        public string RelatedEntityId { get; set; } = string.Empty;
        public string RelatedEntityType { get; set; } = string.Empty;
    }
}
