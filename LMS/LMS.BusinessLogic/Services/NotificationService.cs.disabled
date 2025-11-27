using Domain.Entities.MainEntities;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Notification;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.DataAccess.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Services
{
    internal class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponseDTO<ReadNotificationDTO>> CreateNotificationAsync(CreateNotificationDTO dto)
        {
            try
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = dto.UserId,
                    Title = dto.Title,
                    Message = dto.Message,
                    Type = dto.Type,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                // Add to DbSet
                var dbSet = _unitOfWork.GetQueryable<Notification>() as DbSet<Notification>;
                if (dbSet != null)
                {
                    await dbSet.AddAsync(notification);
                }
                else
                {
                    throw new Exception("Cannot save notification: Notification DbSet not found.");
                }

                await _unitOfWork.SaveChangesAsync();

                return new ServiceResponseDTO<ReadNotificationDTO>
                {
                    Data = MapToReadDTO(notification),
                    Success = true,
                    Message = "Notification created successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<ReadNotificationDTO>
                {
                    Success = false,
                    Message = $"Error creating notification: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponseDTO<IEnumerable<ReadNotificationDTO>>> GetUserNotificationsAsync(string userId)
        {
            try
            {
                var notifications = await _unitOfWork.GetQueryable<Notification>()
                    .Where(n => n.UserId == userId && !n.IsDeleted)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                var dtos = notifications.Select(MapToReadDTO);

                return new ServiceResponseDTO<IEnumerable<ReadNotificationDTO>>
                {
                    Data = dtos,
                    Success = true,
                    Message = "Notifications retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<IEnumerable<ReadNotificationDTO>>
                {
                    Success = false,
                    Message = $"Error retrieving notifications: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponseDTO<IEnumerable<ReadNotificationDTO>>> GetUnreadNotificationsAsync(string userId)
        {
            try
            {
                var notifications = await _unitOfWork.GetQueryable<Notification>()
                    .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                var dtos = notifications.Select(MapToReadDTO);

                return new ServiceResponseDTO<IEnumerable<ReadNotificationDTO>>
                {
                    Data = dtos,
                    Success = true,
                    Message = "Unread notifications retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<IEnumerable<ReadNotificationDTO>>
                {
                    Success = false,
                    Message = $"Error retrieving unread notifications: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponseDTO<int>> GetUnreadCountAsync(string userId)
        {
            try
            {
                var count = await _unitOfWork.GetQueryable<Notification>()
                    .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);

                return new ServiceResponseDTO<int>
                {
                    Data = count,
                    Success = true,
                    Message = "Unread count retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<int>
                {
                    Success = false,
                    Message = $"Error retrieving unread count: {ex.Message}"
                };
            }
        }

        public async Task<BasicResponseDTO> MarkAsReadAsync(string notificationId)
        {
            try
            {
                var notification = await _unitOfWork.GetQueryable<Notification>()
                    .FirstOrDefaultAsync(n => n.Id == notificationId && !n.IsDeleted);

                if (notification == null)
                {
                    return new BasicResponseDTO
                    {
                        Success = false,
                        Message = "Notification not found."
                    };
                }

                notification.IsRead = true;
                await _unitOfWork.SaveChangesAsync();

                return new BasicResponseDTO
                {
                    Success = true,
                    Message = "Notification marked as read."
                };
            }
            catch (Exception ex)
            {
                return new BasicResponseDTO
                {
                    Success = false,
                    Message = $"Error marking notification as read: {ex.Message}"
                };
            }
        }

        public async Task<BasicResponseDTO> MarkAllAsReadAsync(string userId)
        {
            try
            {
                var notifications = await _unitOfWork.GetQueryable<Notification>()
                    .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }

                await _unitOfWork.SaveChangesAsync();

                return new BasicResponseDTO
                {
                    Success = true,
                    Message = "All notifications marked as read."
                };
            }
            catch (Exception ex)
            {
                return new BasicResponseDTO
                {
                    Success = false,
                    Message = $"Error marking all notifications as read: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponseDTO<ReadNotificationDTO>> DeleteNotificationAsync(string notificationId)
        {
            try
            {
                var notification = await _unitOfWork.GetQueryable<Notification>()
                    .FirstOrDefaultAsync(n => n.Id == notificationId && !n.IsDeleted);

                if (notification == null)
                {
                    return new ServiceResponseDTO<ReadNotificationDTO>
                    {
                        Success = false,
                        Message = "Notification not found."
                    };
                }

                notification.IsDeleted = true;
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResponseDTO<ReadNotificationDTO>
                {
                    Success = true,
                    Message = "Notification deleted successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<ReadNotificationDTO>
                {
                    Success = false,
                    Message = $"Error deleting notification: {ex.Message}"
                };
            }
        }

        private ReadNotificationDTO MapToReadDTO(Notification notification)
        {
            return new ReadNotificationDTO
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                Type = notification.Type
            };
        }
    }
}
