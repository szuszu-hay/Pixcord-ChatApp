namespace ChatAPI.Models
{
    public class MessageAttachment
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public Message Message { get; set; } = null!;
        public string FileUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; }  = string.Empty;
        public long Filesize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}