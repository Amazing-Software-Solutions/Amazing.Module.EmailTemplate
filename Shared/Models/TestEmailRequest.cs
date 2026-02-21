namespace Amazing.Module.EmailTemplate.Models
{
    public class TestEmailRequest
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string VariableJson { get; set; }
    }
}
