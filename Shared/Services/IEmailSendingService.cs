using System.Collections.Generic;
using System.Threading.Tasks;

namespace Amazing.Module.EmailTemplate.Services
{
    /// <summary>
    /// Public service interface for sending emails using templates.
    /// Other Oqtane modules can inject this service to send templated emails.
    /// </summary>
    public interface IEmailSendingService
    {
        /// <summary>
        /// Send email using a template by name
        /// </summary>
        /// <param name="siteId">Site ID for template lookup</param>
        /// <param name="templateName">Name of the template</param>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="variables">Dictionary of variables to replace (key = variable name without braces)</param>
        /// <param name="toDisplayName">Optional recipient display name</param>
        /// <returns>Result with success status and notification ID</returns>
        Task<Models.EmailSendResult> SendEmailByTemplateNameAsync(
            int siteId, 
            string templateName, 
            string toEmail, 
            Dictionary<string, string> variables, 
            string toDisplayName = null);
        
        /// <summary>
        /// Send email using a template by ID
        /// </summary>
        /// <param name="siteId">Site ID for security validation</param>
        /// <param name="templateId">Template ID</param>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="variables">Dictionary of variables to replace</param>
        /// <param name="toDisplayName">Optional recipient display name</param>
        /// <returns>Result with success status and notification ID</returns>
        Task<Models.EmailSendResult> SendEmailByTemplateIdAsync(
            int siteId, 
            int templateId, 
            string toEmail, 
            Dictionary<string, string> variables, 
            string toDisplayName = null);
        
        /// <summary>
        /// Get list of available active templates for a site
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <returns>List of template information including required variables</returns>
        Task<List<Models.TemplateInfo>> GetAvailableTemplatesAsync(int siteId);

        /// <summary>
        /// Get list of templates by category
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="category">Template category</param>
        /// <returns>List of templates in the specified category</returns>
        Task<List<Models.TemplateInfo>> GetTemplatesByCategoryAsync(int siteId, string category);
    }
}
