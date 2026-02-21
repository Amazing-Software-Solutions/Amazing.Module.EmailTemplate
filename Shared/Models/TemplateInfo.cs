using System.Collections.Generic;

namespace Amazing.Module.EmailTemplate.Models
{
    public class TemplateInfo
    {
        public int TemplateId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public List<string> RequiredVariables { get; set; }
    }
}
