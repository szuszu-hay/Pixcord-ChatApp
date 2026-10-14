using ChatAPI.Data;
using ChatAPI.DTOs;
using ChatAPI.Models;
using Microsoft.EntityFrameworkCore;
using ChatAPI.Services.Interfaces;

namespace ChatAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        // Create a new notification
        public async Task CreateNotificationAsync(string userId, NotificationType type, string? message = null, int? conversationId = null, string? relatedUserId = null, string? relatedMessageId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Message = message,
                ConversationId = conversationId,
                RelatedUserId = relatedUserId,
                RelatedMessageId = relatedMessageId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        // Get notifications for a user
        public async Task<List<NotificationDto>> GetNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Include(n => n.RelatedUser)
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type.ToString(),
                    Message = n.Message,
                    ConversationId = n.ConversationId,
                    RelatedUserId = n.RelatedUserId,
                    RelatedUserDisplayName = n.RelatedUser != null ? n.RelatedUser.DisplayName : null,
                    RelatedUserProfilePictureUrl = n.RelatedUser != null ? n.RelatedUser.ProfilePictureUrl : null,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        // get unread notifications count for a user
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        // Mark a notification as read
        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        // Mark all notifications as read for a user
        public async Task MarkAllAsReadAsync(string userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));
        }
    }
}