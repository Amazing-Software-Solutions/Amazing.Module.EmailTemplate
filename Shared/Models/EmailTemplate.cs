using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace Amazing.Module.EmailTemplate.Models       
{   
    [Table("AmazingEmailTemplate")]
    public class EmailTemplate : ModelBase
    {
        [Key]
        public int EmailTemplateId { get; set; }
        
        public int SiteId { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; }
        
        [Required(ErrorMessage = "Body is required")]
        public string Body { get; set; }
        
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; }
        
        public bool IsActive { get; set; }
        
        [StringLength(2000, ErrorMessage = "Template Variables cannot exceed 2000 characters")]
        public string TemplateVariables { get; set; }
    }
}
