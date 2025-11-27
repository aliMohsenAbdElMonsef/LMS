using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace LMS.BusinessLogic.DTOs.Notification
{
    public class CreateNotificationDTO
    {
        [Required]
        public string UserId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required, MaxLength(1000)]
        public string Message { get; set; }

        public NotificationType Type { get; set; } = NotificationType.General;
    }

    public class ReadNotificationDTO
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public NotificationType Type { get; set; }
    }

    public class MarkAsReadDTO
    {
        [Required]
        public string NotificationId { get; set; }
    }
}
