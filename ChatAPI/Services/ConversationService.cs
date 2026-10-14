using ChatAPI.Data;
using ChatAPI.DTOs;
using ChatAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ChatAPI.Helpers;
using ChatAPI.Services.Interfaces;

namespace ChatAPI.Services
{
    public class ConversationService : IConversationService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notificationService;
        public ConversationService(AppDbContext context, UserManager<ApplicationUser> userManager, NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // system msg
        private void SystemMessage(int conversationId, string actorUserId, string text)
        {
            _context.Messages.Add(new Message
            {
                ConversationId = conversationId,
                SenderId = actorUserId,
                Content = text,
                Type = MessageType.System,
                SentAt = DateTime.UtcNow
            });
        }

        public async Task<ConversationDto?> CreateDirectConversationAsync(string currentUserId, string otherUserId)
        {
            if (currentUserId == otherUserId)
            {
                return null;
            }

            var otherUser = await _userManager.FindByIdAsync(otherUserId);
            if (otherUser == null)
            {
                return null;
            }

            var areFriends = await _context.FriendRequests
                .AnyAsync(f => 
                    (
                        (f.SenderId == currentUserId && f.ReceiverId == otherUserId) || 
                        (f.SenderId == otherUserId && f.ReceiverId == currentUserId)
                    ) &&
                    f.Status == FriendRequestStatus.Accepted);
            if (!areFriends)
                return null;

            var existingConversation = await _context.Conversations
                .Where(c => c.Type == ConversationType.Direct)
                .Where(c => c.Members.Any(m => m.UserId == currentUserId))
                .Where(c => c.Members.Any(m => m.UserId == otherUserId))
                .FirstOrDefaultAsync();

            if (existingConversation != null)
            {
                return new ConversationDto
                {
                    ConversationId = existingConversation.Id,
                    Type = existingConversation.Type.ToString(),
                    Name = existingConversation.Name,
                    IconUrl = existingConversation.IconUrl
                };
            }

            var conversation = new Conversation
            {
                Type = ConversationType.Direct
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var currentUserMember = new ConversationMember
            {
                ConversationId = conversation.Id,
                UserId = currentUserId
            };

            var otherUserMemeber = new ConversationMember
            {
                ConversationId = conversation.Id,
                UserId = otherUserId
            };

            _context.ConversationMembers.AddRange(currentUserMember, otherUserMemeber);
            await _context.SaveChangesAsync();

            return new ConversationDto
            {
                ConversationId = conversation.Id,
                Type = conversation.Type.ToString(),
                Name = conversation.Name,
                IconUrl = conversation.IconUrl
            };
        }

        public async Task<ConversationDto?> CreateGroupConversationAsync(string currentUserId, CreateGroupConversationDto dto)
        {
            if (!dto.MemberIds.Contains(currentUserId))
            {
                dto.MemberIds.Add(currentUserId);
            }

            // at least 2 members are required for a group conversation
            if (dto.MemberIds.Count < 2)
            {
                return null;
            }

            var validUserIds = await _userManager.Users
                .Where(u => dto.MemberIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync();
            if (validUserIds.Count != dto.MemberIds.Count)
            {
                return null;
            }

            var friendIds = await _context.FriendRequests
                .Where(f => 
                    f.Status == FriendRequestStatus.Accepted &&
                    (f.SenderId == currentUserId || f.ReceiverId == currentUserId))
                .Select(f => f.SenderId == currentUserId ? f.ReceiverId : f.SenderId)
                .ToListAsync();

            var nonFriendIds = dto.MemberIds.Where(id => id != currentUserId && !friendIds.Contains(id)).ToList();
            if (nonFriendIds.Any()) return null;    

            var conversation = new Conversation
            {
                Type = ConversationType.Group,
                Name = dto.Name,
                IconUrl = dto.IconUrl
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var members = dto.MemberIds.Select(id => new ConversationMember
            {
                ConversationId = conversation.Id,
                UserId = id
            }).ToList();

            _context.ConversationMembers.AddRange(members);
            await _context.SaveChangesAsync();

            return new ConversationDto
            {
                ConversationId = conversation.Id,
                Type = conversation.Type.ToString(),
                Name = conversation.Name,
                IconUrl = conversation.IconUrl
            };
        }

        public async Task<List<ConversationDto>> GetConversationsAsync(string userId)
        {
            var conversations = await _context.Conversations
                .Include(c => c.Members)
                    .ThenInclude(m => m.User)
                .Where(c => c.Members.Any(m => m.UserId == userId))
                .ToListAsync();

            var result = new List<ConversationDto>();   
            foreach(var conversation in conversations)
            {
                var lastMessage = await _context.Messages
                    .Include(m => m.Sender)
                    .Where(m => m.ConversationId == conversation.Id && !m.IsDeleted)
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefaultAsync();

                string? name = conversation.Name;
                string? iconUrl = conversation.IconUrl;
                string? username = null;

                if (conversation.Type == ConversationType.Direct)
                {
                    var friend = conversation.Members.FirstOrDefault(f => f.UserId != userId);
                    if (friend != null)
                    {
                        name = friend.User.DisplayName;
                        iconUrl = friend.User.ProfilePictureUrl;
                        username = friend.User.UserName;
                    }
                } 

                result.Add(new ConversationDto
                {
                    ConversationId = conversation.Id,
                    Type = conversation.Type.ToString(),
                    Name = name,
                    Username = username,
                    IconUrl = iconUrl,
                    LastMessagePreview = lastMessage == null
                        ? null
                        : lastMessage.Type == MessageType.System
                            ? $"{lastMessage.Sender.DisplayName} {lastMessage.Content}"
                            : lastMessage.Content,
                    LastMessageAt = lastMessage?.SentAt
                });
            }

            return result
                .OrderByDescending(c => c.LastMessageAt ?? DateTime.MinValue)
                .ToList();
        }

        public async Task<ConversationDetailsDto?> GetConversationDetailsAsync(int conversationId, string userId)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null) return null;

            var isMember = conversation.Members
                .Any(m => m.UserId == userId);
            if (!isMember) return null;

            var details = new ConversationDetailsDto
            {
                ConversationId = conversation.Id,
                Type = conversation.Type.ToString(),
                Name = conversation.Name,
                IconUrl = conversation.IconUrl,
            };

            if (conversation.Type == ConversationType.Group)
            {
                details.Members = conversation.Members
                    .Select(m => new ConversationMemberDto
                    {
                        UserId = m.UserId,
                        DisplayName = m.User.DisplayName,
                        Username = m.User.UserName ?? string.Empty,
                        ProfilePictureUrl = m.User.ProfilePictureUrl,
                        IsOnline = m.User.IsOnline
                    }).ToList();
            }

            else if (conversation.Type == ConversationType.Direct)
            {
                var otherMember = conversation.Members.FirstOrDefault(m => m.UserId != userId);
                if (otherMember != null)
                {
                    details.Name = otherMember.User.DisplayName;
                    details.IconUrl = otherMember.User.ProfilePictureUrl;
                    details.OtherUserId = otherMember.UserId;
                    details.Username = otherMember.User.UserName;
                    details.OtherUserBio = otherMember.User.Bio;
                    details.OtherUserJoinedAt = otherMember.User.CreatedAt;

                    var friendship = await _context.FriendRequests
                        .Where(f =>
                            f.Status == FriendRequestStatus.Accepted &&
                            ((f.SenderId == userId && f.ReceiverId == otherMember.UserId) ||
                            (f.SenderId == otherMember.UserId && f.ReceiverId == userId)))
                        .FirstOrDefaultAsync();

                    details.FriendSince = friendship?.RespondedAt;
                }
            }

            return details;
        }

        public async Task<ConversationMemberDto?> AddMemberAsync(int conversationId, string actingUserId, string targetUserId)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.Type ==  ConversationType.Group);

            if (conversation == null) return null;

            var isMember = conversation.Members.Any(m => m.UserId == actingUserId);
            if (!isMember) return null;

            if (targetUserId == actingUserId) 
                throw new InvalidOperationException("You're already in this group.");

            if (conversation.Members.Any(m => m.UserId == targetUserId))
                throw new InvalidOperationException("That person is already in this group.");

            var targetUser = await _userManager.FindByIdAsync(targetUserId);
            if (targetUser == null) 
                throw new InvalidOperationException("User not found.");

            var areFriends = await _context.FriendRequests.AnyAsync(f =>
                f.Status == FriendRequestStatus.Accepted &&
                ((f.SenderId == actingUserId && f.ReceiverId == targetUserId) ||
                (f.SenderId == targetUserId && f.ReceiverId == actingUserId)));

            if (!areFriends)
                throw new InvalidOperationException("You can only add people you're friends with.");

            _context.ConversationMembers.Add(new ConversationMember
            {
                ConversationId = conversationId,
                UserId = targetUserId
            });
            SystemMessage(conversationId, actingUserId, $"added {targetUser.DisplayName} to the group");
            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                userId: targetUserId,
                type: NotificationType.AddedToGroup,
                message: $"added you to {conversation.Name ?? "a group"}",
                conversationId: conversationId,
                relatedUserId: actingUserId
            );

            return new ConversationMemberDto
            {
                UserId = targetUser.Id,
                DisplayName = targetUser.DisplayName,
                Username = targetUser.UserName ?? string.Empty,
                ProfilePictureUrl = targetUser.ProfilePictureUrl,
                IsOnline = targetUser.IsOnline
            };
        }

        public async Task<bool> RemoveMemberAsync(int conversationId, string actingUserId, string targetUserId)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.Type == ConversationType.Group);

            if (conversation == null) return false;

            var actingMember = conversation.Members.FirstOrDefault(m => m.UserId == actingUserId);
            if (actingMember == null) return false; 

            if (targetUserId == actingUserId)
                return false; 

            var targetMember = conversation.Members.FirstOrDefault(m => m.UserId == targetUserId);
            if (targetMember == null) return false; 

            var targetUser = await _userManager.FindByIdAsync(targetUserId);
            if (targetUser == null) 
                throw new InvalidOperationException("User not found.");

            _context.ConversationMembers.Remove(targetMember);
            SystemMessage(conversationId,actingUserId, $" removed {targetUser.DisplayName ?? "a member"} from the group");
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> LeaveConversationAsync(int conversationId, string userId)
        {
            var member = await _context.ConversationMembers
                .FirstOrDefaultAsync(m => m.ConversationId == conversationId && m.UserId == userId);

            if (member == null) return false;

            _context.ConversationMembers.Remove(member);

            var othersRemaining = await _context.ConversationMembers
                .CountAsync(m => m.ConversationId == conversationId && m.UserId != userId);

            if (othersRemaining == 0)
            {
                var conversation = await _context.Conversations.FindAsync(conversationId);
                if (conversation != null) _context.Conversations.Remove(conversation);
            }
            else
            {
                SystemMessage(conversationId, userId, "left the group");
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MessageDto?> SendMessageAsync(int conversationId, string userId, string content)
        {
            var isMember = await _context.ConversationMembers
                .AnyAsync(m => m.ConversationId == conversationId && m.UserId == userId);

            if (!isMember) return null;

            if (!await CanSendAsync(conversationId, userId))
                throw new InvalidOperationException("You can only message people you're friends with.");

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = userId,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var otherMembers = await _context.ConversationMembers
                .Where(m => m.ConversationId == conversationId && m.UserId != userId)
                .Select(m => m.UserId)
                .ToListAsync();
            
            foreach (var member in otherMembers)
            {
                await _notificationService.CreateNotificationAsync(
                    userId: member,
                    type: NotificationType.Message,
                    message: content.Length > 50 ? content.Substring(0, 50) + "..." : content,
                    conversationId: conversationId,
                    relatedUserId: userId
                );
            }

            var sender = await _userManager.FindByIdAsync(userId);

            return new MessageDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = userId,
                SenderDisplayName = sender?.DisplayName ?? string.Empty,
                SenderProfilePictureUrl = sender?.ProfilePictureUrl,
                Content = message.Content,
                SentAt = message.SentAt 
            };
        }

        public async Task<ConversationDto?> UpdateGroupDetailsAsync (int conversationId, string userId, string? name, IFormFile? iconFile, IWebHostEnvironment env)
        {
            var conversation = await _context.Conversations 
                .Include(c => c.Members)
                .FirstOrDefaultAsync (c => c.Id == conversationId && c.Type == ConversationType.Group);

            if (conversation == null) return null;

            var isMember = conversation.Members.Any(m => m.UserId == userId);
            if (!isMember) return null;

            if (!string.IsNullOrEmpty(name))
            {
                conversation.Name = name.Trim();
            }

            if (iconFile != null)
            {
                conversation.IconUrl = await FileStorage.SaveAsync(iconFile, "group-icons", FileStorage.ImageTypes, env);
            }
            
            await _context.SaveChangesAsync();

            return new ConversationDto
            {
                ConversationId = conversation.Id,
                Type = conversation.Type.ToString(),
                Name = conversation.Name,
                IconUrl = conversation.IconUrl
            };
        }

        public async Task<List<MessageDto>?> GetMessagesAsync(int conversationId, string userId)
        {
            var isMember = await _context.ConversationMembers
                .AnyAsync(m => m.ConversationId == conversationId && m.UserId == userId);

            if (!isMember) return null;
            
            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    ConversationId = m.ConversationId,
                    SenderId = m.SenderId,
                    SenderDisplayName = m.Sender.DisplayName,
                    SenderProfilePictureUrl = m.Sender.ProfilePictureUrl,
                    Content = m.Content,
                    Type = m.Type.ToString(),
                    SentAt = m.SentAt,
                    EditedAt = m.EditedAt
                }).ToListAsync();

            var messageIds = messages.Select(m => m.Id).ToList();
            var allReactions = await _context.MessageReactions
                .Where(r => messageIds.Contains(r.MessageId))
                .ToListAsync();

            var allAttachments = await _context.MessageAttachments
                .Where(a => messageIds.Contains(a.MessageId))
                .ToListAsync();

            foreach (var message in messages)
            {
                message.Reactions = allReactions
                    .Where(r => r.MessageId == message.Id)
                    .GroupBy(r => r.Emoji)
                    .Select(g => new ReactionSummaryDto
                    {
                        Emoji = g.Key,
                        Count = g.Count(),
                        ReactedByMe = g.Any(r => r.UserId == userId)
                    }).ToList();

                message.Attachments = allAttachments
                    .Where(a => a.MessageId == message.Id)
                    .Select(a => new AttachmentDto
                    {
                        Id = a.Id,
                        FileUrl = a.FileUrl,
                        FileName = a.FileName,
                        ContentType = a.ContentType,
                        FileSize = a.Filesize
                    })
                .ToList();
            }
            
            return messages;
        }

        public async Task<ConversationDto?> GetConversationByIdAsync(int conversationId, string userId)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null) return null;

            var isMember = conversation.Members
                .Any(m => m.UserId == userId);
            if (!isMember) return null;

            string? name = conversation.Name;
            string? iconUrl = conversation.IconUrl;
            string? messageTo = conversation.Name;
            bool otherUserOnline = false;

            if (conversation.Type == ConversationType.Direct)
            {
                var otherMemeber = conversation.Members.FirstOrDefault(m => m.UserId != userId);
                if (otherMemeber != null)
                {
                    name = otherMemeber.User.DisplayName;
                    iconUrl = otherMemeber.User.ProfilePictureUrl;
                    messageTo = otherMemeber.User.UserName;
                    otherUserOnline = otherMemeber.User.IsOnline;
                }
            }

            return new ConversationDto
            {
                ConversationId = conversationId,
                Type = conversation.Type.ToString(),
                Name = name,
                IconUrl = iconUrl,
                MessageTo = messageTo ?? string.Empty,
                IsOnline = otherUserOnline,
                CanMessage = await CanSendAsync(conversationId, userId)
            };
        }
        
        public async Task<MessageDto?> EditMessageAsync(int conversationId, int messageId, string userId, string content)
        {
            var message = await _context.Messages
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m =>
                    m.Id == messageId &&
                    m.ConversationId == conversationId &&
                    !m.IsDeleted &&
                    m.Type == MessageType.Text);
            
            if (message == null) return null;
            if (message.SenderId != userId) return null;

            message.Content = content;
            message.EditedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new MessageDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                SenderDisplayName = message.Sender.DisplayName,
                SenderProfilePictureUrl = message.Sender.ProfilePictureUrl,
                Content = message.Content,
                SentAt = message.SentAt,
                EditedAt = message.EditedAt
            };
        }

        public async Task<bool> DeleteMessageAsync(int conversationId, int messageId, string userId)
        {
            var message = await _context.Messages  
                .FirstOrDefaultAsync(m => 
                    m.Id == messageId &&
                    m.ConversationId == conversationId &&
                    !m.IsDeleted);
            
            if (message == null) return false;
            if (message.SenderId != userId) return false;

            message.IsDeleted = true;
            message.DeletedAt = DateTime.UtcNow;
            message.Content = string.Empty;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ReactionSummaryDto>?> ToggleReactionAsync(int conversationId, int messageId, string userId, string emoji)
        {
            var isMember = await _context.ConversationMembers
                .AnyAsync(m => m.ConversationId == conversationId && m.UserId == userId);

            if (!isMember) return null;

            var messageExists = await _context.Messages
                .AnyAsync (m => m.Id == messageId && m.ConversationId == conversationId && !m.IsDeleted
                && m.Type == MessageType.Text);
            
            if (!messageExists) return null;

            var existingReaction = await _context.MessageReactions
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId);

            if (existingReaction != null)
            {
                if (existingReaction.Emoji == emoji)
                {
                    _context.MessageReactions.Remove(existingReaction);   
                }

                else
                {
                    existingReaction.Emoji = emoji;
                    existingReaction.CreatedAt = DateTime.UtcNow;
                }
            } 
            
            else
            {
                _context.MessageReactions.Add(new MessageReaction
                {
                    MessageId = messageId,
                    UserId = userId,
                    Emoji = emoji,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return await GetReactionSummaryAsync(messageId, userId);
        }

        private async Task<List<ReactionSummaryDto>> GetReactionSummaryAsync(int messageId, string currentUserId)
        {
            return await _context.MessageReactions
                .Where(r => r.MessageId == messageId)
                .GroupBy(r => r.Emoji)
                .Select(g => new ReactionSummaryDto
                {
                    Emoji = g.Key,
                    Count = g.Count(),
                    ReactedByMe = g.Any(r => r.UserId == currentUserId)
                }).ToListAsync();
        }

        // Attachments
        public async Task<MessageDto?> SendMessageWithAttachment(int conversationId, string userId, string? content, IFormFile file, IWebHostEnvironment env)
        {
            var isMember = await _context.ConversationMembers
                .AnyAsync(m => m.ConversationId == conversationId && m.UserId == userId);

            if (!isMember) return null;

            if (!await CanSendAsync(conversationId, userId))
                throw new InvalidOperationException("You can only message people you're friends with.");

            var fileUrl = await FileStorage.SaveAsync(file, "uploads", FileStorage.AttachmentTypes, env);

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = userId,
                Content = content ?? string.Empty,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync(); 

            var attachment = new MessageAttachment
            {
                MessageId = message.Id,
                FileUrl = fileUrl,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Filesize = file.Length,
                UploadedAt = DateTime.UtcNow
            };

            _context.MessageAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            var sender = await _userManager.FindByIdAsync(userId);

            return new MessageDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = userId,
                SenderDisplayName = sender?.DisplayName ?? string.Empty,
                SenderProfilePictureUrl = sender?.ProfilePictureUrl,
                Content = message.Content,
                SentAt = message.SentAt,
                Attachments = new List<AttachmentDto>
                {
                    new AttachmentDto
                    {
                        Id = attachment.Id,
                        FileUrl = attachment.FileUrl,
                        FileName = attachment.FileName,
                        ContentType = attachment.ContentType,
                        FileSize = attachment.Filesize
                    }
                }
            };
        }

        // Checking whether members can send message
        private async Task<bool> CanSendAsync(int conversationId, string userId)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null) return false;
            if (conversation.Type != ConversationType.Direct) return true;

            var otherId = conversation.Members.FirstOrDefault(m => m.UserId != userId)?.UserId;
            if (otherId == null) return false;

            return await _context.FriendRequests.AnyAsync(f =>
                f.Status == FriendRequestStatus.Accepted &&
                ((f.SenderId == userId && f.ReceiverId == otherId) ||
                (f.SenderId == otherId && f.ReceiverId == userId)));
        }
    }
}