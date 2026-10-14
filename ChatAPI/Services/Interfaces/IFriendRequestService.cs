using ChatAPI.DTOs;

namespace ChatAPI.Services.Interfaces
{
    public interface IFriendRequestService
    {
        Task<FriendRequestResponseDto> SendRequestAsync(string senderId, SendFriendRequestDto dto);
        Task<FriendRequestResponseDto> AcceptRequestAsync(string userId, int requestId);
        Task<FriendRequestResponseDto> RejectRequestAsync(string userId, int requestId);
        Task<FriendRequestResponseDto> CancelRequestAsync(string userId, int requestId);
        Task<List<FriendRequestResponseDto>> GetIncomingRequestsAsync(string userId);
        Task<List<FriendRequestResponseDto>> GetOutgoingRequestsAsync(string userId);
        Task<FriendRequestResponseDto> GetRequestResponseAsync(int requestId);
        Task<List<FriendsDto>> GetFriendsAsync(string userId);
        Task<List<UserSearchResultDto>> SearchFriendAsync(string query, string currentUserId);
        Task<bool> UnfriendAsync(string userId, string friendId);
    }
}