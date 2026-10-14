using ChatAPI.DTOs;
using ChatAPI.Models;

namespace ChatAPI.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, NotificationType type, string? message = null, int? conversationId = null, string? relatedUserId = null, string? relatedMessageId = null);
        Task<List<NotificationDto>> GetNotificationsAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
    }
}