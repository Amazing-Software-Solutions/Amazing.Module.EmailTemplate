using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Repository;
using Amazing.Module.EmailTemplate.Repository;

namespace Amazing.Module.EmailTemplate.Services
{
    public class ServerEmailSendingService : IEmailSendingService
    {
        private readonly IEmailTemplateRepository _templateRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public ServerEmailSendingService(
            IEmailTemplateRepository templateRepository,
            INotificationRepository notificationRepository,
            ITenantManager tenantManager,
            ILogManager logger)
        {
            _templateRepository = templateRepository;
            _notificationRepository = notificationRepository;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        public Task<Models.EmailSendResult> SendEmailByTemplateNameAsync(
            int siteId, 
            string templateName, 
            string toEmail, 
            Dictionary<string, string> variables, 
            string toDisplayName = null)
        {
            try
            {
                if (_alias.SiteId != siteId)
                {
                    return Task.FromResult(new Models.EmailSendResult 
                    { 
                        Success = false, 
                        Message = "Site ID mismatch - cannot send cross-site emails" 
                    });
                }

                var template = _templateRepository.GetEmailTemplateByName(templateName);

                if (template == null)
                {
                    _logger.Log(LogLevel.Warning, this, LogFunction.Other, 
                        "Email Template Not Found {TemplateName}", templateName);
                    return Task.FromResult(new Models.EmailSendResult 
                    { 
                        Success = false, 
                        Message = $"Template '{templateName}' not found or inactive" 
                    });
                }

                return SendEmailByTemplateIdAsync(siteId, template.EmailTemplateId, toEmail, variables, toDisplayName);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, 
                    "Error Sending Email By Template Name {TemplateName} {Error}", templateName, ex.Message);
                return Task.FromResult(new Models.EmailSendResult 
                { 
                    Success = false, 
                    Message = ex.Message 
                });
            }
        }

        public Task<Models.EmailSendResult> SendEmailByTemplateIdAsync(
            int siteId, 
            int templateId, 
            string toEmail, 
            Dictionary<string, string> variables, 
            string toDisplayName = null)
        {
            try
            {
                if (_alias.SiteId != siteId)
                {
                    return Task.FromResult(new Models.EmailSendResult 
                    { 
                        Success = false, 
                        Message = "Site ID mismatch - cannot send cross-site emails" 
                    });
                }

                var template = _templateRepository.GetEmailTemplate(templateId, false);

                if (template == null)
                {
                    _logger.Log(LogLevel.Warning, this, LogFunction.Other, 
                        "Email Template Not Found {TemplateId}", templateId);
                    return Task.FromResult(new Models.EmailSendResult 
                    { 
                        Success = false, 
                        Message = $"Template ID {templateId} not found" 
                    });
                }

                if (!template.IsActive)
                {
                    _logger.Log(LogLevel.Warning, this, LogFunction.Other, 
                        "Inactive Template Used {TemplateName}", template.Name);
                    return Task.FromResult(new Models.EmailSendResult 
                    { 
                        Success = false, 
                        Message = $"Template '{template.Name}' is inactive" 
                    });
                }

                string subject = ReplaceVariables(template.Subject, variables);
                string body = ReplaceVariables(template.Body, variables);

                var notification = new Notification(
                    siteId,
                    _alias.Name,
                    string.Empty,
                    toDisplayName ?? string.Empty,
                    toEmail,
                    subject,
                    body
                );

                notification = _notificationRepository.AddNotification(notification);

                _logger.Log(LogLevel.Information, this, LogFunction.Create, 
                    "Email Queued Via Template {TemplateName} To {Email}", template.Name, toEmail);

                return Task.FromResult(new Models.EmailSendResult 
                { 
                    Success = true, 
                    Message = "Email queued for delivery",
                    NotificationId = notification.NotificationId
                });
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, 
                    "Error Sending Email By Template ID {TemplateId} {Error}", templateId, ex.Message);
                return Task.FromResult(new Models.EmailSendResult 
                { 
                    Success = false, 
                    Message = ex.Message 
                });
            }
        }

        public Task<List<Models.TemplateInfo>> GetAvailableTemplatesAsync(int siteId)
        {
            try
            {
                if (_alias.SiteId != siteId)
                {
                    return Task.FromResult(new List<Models.TemplateInfo>());
                }

                var templates = _templateRepository.GetActiveEmailTemplates()
                    .Select(t => new Models.TemplateInfo
                    {
                        TemplateId = t.EmailTemplateId,
                        Name = t.Name,
                        Category = t.Category,
                        Description = t.Description,
                        RequiredVariables = ParseVariables(t.TemplateVariables)
                    })
                    .ToList();

                return Task.FromResult(templates);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, 
                    "Error Getting Available Templates {Error}", ex.Message);
                return Task.FromResult(new List<Models.TemplateInfo>());
            }
        }

        public Task<List<Models.TemplateInfo>> GetTemplatesByCategoryAsync(int siteId, string category)
        {
            try
            {
                if (_alias.SiteId != siteId)
                {
                    return Task.FromResult(new List<Models.TemplateInfo>());
                }

                var templates = _templateRepository.GetEmailTemplatesByCategory(category)
                    .Select(t => new Models.TemplateInfo
                    {
                        TemplateId = t.EmailTemplateId,
                        Name = t.Name,
                        Category = t.Category,
                        Description = t.Description,
                        RequiredVariables = ParseVariables(t.TemplateVariables)
                    })
                    .ToList();

                return Task.FromResult(templates);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, 
                    "Error Getting Templates By Category {Category} {Error}", category, ex.Message);
                return Task.FromResult(new List<Models.TemplateInfo>());
            }
        }

        private string ReplaceVariables(string content, Dictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(content) || variables == null || variables.Count == 0)
            {
                return content;
            }

            foreach (var kvp in variables)
            {
                content = content.Replace($"{{{{{kvp.Key}}}}}", kvp.Value ?? string.Empty);
            }

            return content;
        }

        private List<string> ParseVariables(string templateVariables)
        {
            if (string.IsNullOrEmpty(templateVariables))
            {
                return new List<string>();
            }

            return templateVariables
                .Split(',')
                .Select(v => v.Trim().Replace("{{", "").Replace("}}", ""))
                .Where(v => !string.IsNullOrEmpty(v))
                .Distinct()
                .ToList();
        }
    }
}
