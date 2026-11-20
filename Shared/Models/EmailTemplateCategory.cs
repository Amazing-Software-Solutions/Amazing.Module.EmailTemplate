using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace Amazing.Module.EmailTemplate.Models
{
    [Table("AmazingEmailTemplateCategory")]
    public class EmailTemplateCategory : ModelBase
    {
        [Key]
        public int EmailTemplateCategoryId { get; set; }

        public int SiteId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }
    }
}
