using LMS.BusinessLogic.DTOs.Notification;
using LMS.BusinessLogic.DTOs.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface INotificationService
    {
        Task<ServiceResponseDTO<ReadNotificationDTO>> CreateNotificationAsync(CreateNotificationDTO dto);
        Task<ServiceResponseDTO<IEnumerable<ReadNotificationDTO>>> GetUserNotificationsAsync(string userId);
        Task<ServiceResponseDTO<IEnumerable<ReadNotificationDTO>>> GetUnreadNotificationsAsync(string userId);
        Task<ServiceResponseDTO<int>> GetUnreadCountAsync(string userId);
        Task<BasicResponseDTO> MarkAsReadAsync(string notificationId);
        Task<BasicResponseDTO> MarkAllAsReadAsync(string userId);
        Task<ServiceResponseDTO<ReadNotificationDTO>> DeleteNotificationAsync(string notificationId);
    }
}
