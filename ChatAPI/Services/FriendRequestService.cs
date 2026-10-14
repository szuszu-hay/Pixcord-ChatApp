using ChatAPI.Data;
using ChatAPI.DTOs;
using ChatAPI.Models;
using ChatAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatAPI.Services
{
    public class FriendRequestService : IFriendRequestService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notificationService;

        public FriendRequestService(AppDbContext context, UserManager<ApplicationUser> userManager, NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<FriendRequestResponseDto> SendRequestAsync (string senderId, SendFriendRequestDto dto)
        {
            var receiver = await _userManager.FindByIdAsync(dto.ReceiverId);
            if (receiver == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            if (senderId == receiver.Id)
            {
                throw new InvalidOperationException("You cannot send a friend request to yourself.");
            }

            //check existing request
            var existingRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(r => (
                    (r.SenderId == senderId && r.ReceiverId == receiver.Id) || 
                    (r.SenderId == receiver.Id && r.ReceiverId == senderId)
                ) && 
                r.Status == FriendRequestStatus.Pending);
            
            //there is existing request
            if (existingRequest != null)
            {
                if (existingRequest.Status == FriendRequestStatus.Pending)
                    throw new InvalidOperationException("A friend request already exists.");

                if (existingRequest.Status == FriendRequestStatus.Accepted)
                    throw new InvalidOperationException("You are already friends.");

                // Reuse old rejected/cancelled request
                existingRequest.SenderId = senderId;
                existingRequest.ReceiverId = receiver.Id;
                existingRequest.Status = FriendRequestStatus.Pending;
                existingRequest.CreatedAt = DateTime.UtcNow;
                existingRequest.RespondedAt = null;

                await _context.SaveChangesAsync();

                return await GetRequestResponseAsync(existingRequest.Id);
            }

            //no existing request
            var request = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiver.Id,
                Status = FriendRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.FriendRequests.Add(request);

            await _notificationService.CreateNotificationAsync(
                userId: receiver.Id,
                type: NotificationType.FriendRequest,
                message: "sent you a friend request.",
                relatedUserId: senderId
            );

            await _context.SaveChangesAsync();
            return await GetRequestResponseAsync(request.Id);
        }

        public async Task<FriendRequestResponseDto> AcceptRequestAsync(string userId, int requestId)
        {
            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(r =>
                    r.Id == requestId &&
                    r.ReceiverId == userId);

            if (request == null)
                throw new Exception("Friend request not found.");

            if (request.Status != FriendRequestStatus.Pending)
                throw new Exception("This friend request is no longer pending.");

            request.Status = FriendRequestStatus.Accepted;
            request.RespondedAt = DateTime.UtcNow;

            await _notificationService.CreateNotificationAsync(
                userId: request.SenderId,
                type: NotificationType.FriendRequestAccepted,
                message: "accepted your friend request.",
                relatedUserId: userId
            );

            await _context.SaveChangesAsync();

            return await GetRequestResponseAsync(request.Id);
        }

        public async Task<FriendRequestResponseDto> RejectRequestAsync(string userId, int requestId)
        {
            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(r =>
                    r.Id == requestId &&
                    r.ReceiverId == userId);

            if (request == null)
                throw new Exception("Friend request not found.");

            if (request.Status != FriendRequestStatus.Pending)
                throw new Exception("This friend request is no longer pending.");

            request.Status = FriendRequestStatus.Rejected;
            request.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetRequestResponseAsync(request.Id);
        }

        public async Task<FriendRequestResponseDto> CancelRequestAsync(string userId, int requestId)
        {
            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(r =>
                    r.Id == requestId &&
                    r.SenderId == userId);

            if (request == null)
                throw new Exception("Friend request not found.");

            if (request.Status != FriendRequestStatus.Pending)
                throw new Exception("This friend request is no longer pending.");

            request.Status = FriendRequestStatus.Cancelled;
            request.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetRequestResponseAsync(request.Id);
        }

        public async Task<List<FriendRequestResponseDto>> GetIncomingRequestsAsync(string userId)
        {
            var requests = await _context.FriendRequests
                .Include(r => r.Sender)
                .Include(r => r.Receiver)
                .Where(r =>
                    r.ReceiverId == userId &&
                    r.Status == FriendRequestStatus.Pending)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return requests
                .Select(MapToDto)
                .ToList();
        }

        public async Task<List<FriendRequestResponseDto>> GetOutgoingRequestsAsync(string userId)
        {
            var requests = await _context.FriendRequests
                .Include(r => r.Sender)
                .Include(r => r.Receiver)
                .Where(r =>
                    r.SenderId == userId &&
                    r.Status == FriendRequestStatus.Pending)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return requests
                .Select(MapToDto)
                .ToList();
        }

        public async Task<FriendRequestResponseDto> GetRequestResponseAsync(int requestId)
        {
            var request = await _context.FriendRequests
                .Include(r => r.Sender)
                .Include(r => r.Receiver)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                throw new Exception("Friend request not found.");

            return MapToDto(request);
        }

        public async Task<List<FriendsDto>> GetFriendsAsync(string userId)
        {
            var friendRequests = await _context.FriendRequests
                .Include(f => f.Sender)
                .Include(f => f.Receiver)
                .Where(f =>
                    (f.ReceiverId == userId || f.SenderId == userId) &&
                    f.Status == FriendRequestStatus.Accepted)
                .OrderByDescending(f => f.RespondedAt)
                .AsNoTracking()
                .ToListAsync();    

            return friendRequests.Select(f =>
            {
                var friend = f.ReceiverId == userId ? f.Sender : f.Receiver;
                return new FriendsDto
                {
                    FriendId = friend.Id,
                    FriendUsername = friend.UserName ?? string.Empty,
                    FriendDisplayName = friend.DisplayName,
                    FriendProfilePictureUrl = friend.ProfilePictureUrl,
                    FriendBio = friend.Bio,
                    IsOnline = friend.IsOnline,
                    FriendSince = f.RespondedAt
                };
            }).ToList();          
        }

        private static FriendRequestResponseDto MapToDto(FriendRequest request)
        {
            return new FriendRequestResponseDto
            {
                Id = request.Id,

                SenderId = request.SenderId,
                SenderUsername = request.Sender.UserName ?? string.Empty,
                SenderDisplayName = request.Sender.DisplayName,
                SenderProfilePictureUrl =
                    request.Sender.ProfilePictureUrl ?? string.Empty,

                ReceiverId = request.ReceiverId,
                ReceiverUsername = request.Receiver.UserName ?? string.Empty,
                ReceiverDisplayName = request.Receiver.DisplayName,
                ReceiverProfilePictureUrl =
                    request.Receiver.ProfilePictureUrl ?? string.Empty,

                Status = request.Status,

                CreatedAt = request.CreatedAt,
                RespondedAt = request.RespondedAt
            };
        }

        public async Task<List<UserSearchResultDto>> SearchFriendAsync(string query, string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<UserSearchResultDto>();
            }

            query = query.Trim();
            var friendsIds = _context.FriendRequests
                .Where(f => 
                    f.Status == FriendRequestStatus.Accepted &&
                    (f.SenderId == currentUserId || f.ReceiverId == currentUserId))
                .Select(f => f.SenderId == currentUserId ? f.ReceiverId : f.SenderId)
                .Distinct();
            
            return await _context.Users
                .Where(u => u.Id != currentUserId && u.EmailConfirmed && friendsIds.Contains(u.Id) && (
                    u.UserName!.Contains(query) || u.DisplayName.Contains(query)
                ))
                .Select(u => new UserSearchResultDto
                {
                    Id = u.Id,
                    Username = u.UserName!,
                    DisplayName = u.DisplayName,
                    ProfilePictureUrl = u.ProfilePictureUrl ?? "",
                    Bio = u.Bio
                }).Take(20)
                .ToListAsync();
        }

        public async Task<bool> UnfriendAsync(string userId, string friendId)
        {
            var friendships = await _context.FriendRequests
                .Where(f =>
                    f.Status == FriendRequestStatus.Accepted &&
                    ((f.SenderId == userId && f.ReceiverId == friendId) ||
                    (f.SenderId == friendId && f.ReceiverId == userId)))
                .ToListAsync();

            if (friendships.Count == 0) return false;

            _context.FriendRequests.RemoveRange(friendships);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}