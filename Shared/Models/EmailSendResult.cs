namespace Amazing.Module.EmailTemplate.Models
{
    public class EmailSendResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? NotificationId { get; set; }
    }
}
