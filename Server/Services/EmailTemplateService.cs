using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;
using Oqtane.Repository;
using Amazing.Module.EmailTemplate.Repository;

namespace Amazing.Module.EmailTemplate.Services
{
    public class ServerEmailTemplateService : IEmailTemplateService
    {
        private readonly IEmailTemplateRepository _EmailTemplateRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerEmailTemplateService(IEmailTemplateRepository EmailTemplateRepository, INotificationRepository notificationRepository, IUserPermissions userPermissions, ITenantManager tenantManager, ILogManager logger, IHttpContextAccessor accessor)
        {
            _EmailTemplateRepository = EmailTemplateRepository;
            _notificationRepository = notificationRepository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
        }

        public Task<List<Models.EmailTemplate>> GetEmailTemplatesAsync(int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_EmailTemplateRepository.GetEmailTemplates(_alias.SiteId).ToList());
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplate Get Attempt {SiteId}", _alias.SiteId);
                return null;
            }
        }

        public Task<Models.EmailTemplate> GetEmailTemplateAsync(int EmailTemplateId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                var template = _EmailTemplateRepository.GetEmailTemplate(EmailTemplateId);
                if (template != null && template.SiteId == _alias.SiteId)
                {
                    return Task.FromResult(template);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "EmailTemplate Not Found Or Site Mismatch {EmailTemplateId}", EmailTemplateId);
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplate Get Attempt {EmailTemplateId} {ModuleId}", EmailTemplateId, ModuleId);
                return null;
            }
        }

        public Task<Models.EmailTemplate> AddEmailTemplateAsync(Models.EmailTemplate EmailTemplate, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.Edit))
            {
                EmailTemplate.SiteId = _alias.SiteId;
                EmailTemplate = _EmailTemplateRepository.AddEmailTemplate(EmailTemplate);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "EmailTemplate Added {EmailTemplate}", EmailTemplate);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplate Add Attempt", EmailTemplate);
                EmailTemplate = null;
            }
            return Task.FromResult(EmailTemplate);
        }

        public Task<Models.EmailTemplate> UpdateEmailTemplateAsync(Models.EmailTemplate EmailTemplate, int ModuleId)
        {
            var existingTemplate = _EmailTemplateRepository.GetEmailTemplate(EmailTemplate.EmailTemplateId);
            if (existingTemplate != null && existingTemplate.SiteId == _alias.SiteId && 
                _userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.Edit))
            {
                EmailTemplate.SiteId = _alias.SiteId;
                EmailTemplate = _EmailTemplateRepository.UpdateEmailTemplate(EmailTemplate);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "EmailTemplate Updated {EmailTemplate}", EmailTemplate);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplate Update Attempt {EmailTemplate}", EmailTemplate);
                EmailTemplate = null;
            }
            return Task.FromResult(EmailTemplate);
        }

        public Task DeleteEmailTemplateAsync(int EmailTemplateId, int ModuleId)
        {
            var template = _EmailTemplateRepository.GetEmailTemplate(EmailTemplateId);
            if (template != null && template.SiteId == _alias.SiteId &&
                _userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.Edit))
            {
                _EmailTemplateRepository.DeleteEmailTemplate(EmailTemplateId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "EmailTemplate Deleted {EmailTemplateId}", EmailTemplateId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplate Delete Attempt {EmailTemplateId} {ModuleId}", EmailTemplateId, ModuleId);
            }
            return Task.CompletedTask;
        }

        public Task<Models.TestEmailResult> SendTestEmailAsync(int moduleId, string toEmail, string subject, string body, string variableJson)
        {
            if (!_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Test Email Attempt {ModuleId}", moduleId);
                return Task.FromResult(new Models.TestEmailResult { Success = false, Message = "Unauthorized" });
            }

            try
            {
                string processedSubject = subject;
                string processedBody = body;

                if (!string.IsNullOrEmpty(variableJson))
                {
                    processedSubject = ReplaceTemplateVariables(subject, variableJson);
                    processedBody = ReplaceTemplateVariables(body, variableJson);
                }

                var notification = new Notification(
                    _alias.SiteId,
                    _alias.Name,
                    string.Empty,
                    string.Empty,
                    toEmail,
                    $"[TEST] {processedSubject}",
                    processedBody
                );

                _notificationRepository.AddNotification(notification);

                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Test Email Queued For Delivery {ToEmail}", toEmail);

                return Task.FromResult(new Models.TestEmailResult 
                { 
                    Success = true, 
                    Message = "Test email queued for delivery. Check your inbox in a few moments." 
                });
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, "Error Sending Test Email {Error}", ex.Message);
                return Task.FromResult(new Models.TestEmailResult { Success = false, Message = ex.Message });
            }
        }

        private string ReplaceTemplateVariables(string content, string variableJson)
        {
            try
            {
                var variables = JsonSerializer.Deserialize<Dictionary<string, string>>(variableJson);
                if (variables != null)
                {
                    foreach (var kvp in variables)
                    {
                        content = content.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
                    }
                }
            }
            catch
            {
            }

            return content;
        }
    }
}
