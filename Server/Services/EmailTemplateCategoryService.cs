using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;
using Amazing.Module.EmailTemplate.Repository;

namespace Amazing.Module.EmailTemplate.Services
{
    public class ServerEmailTemplateCategoryService : IEmailTemplateCategoryService
    {
        private readonly IEmailTemplateCategoryRepository _EmailTemplateCategoryRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerEmailTemplateCategoryService(IEmailTemplateCategoryRepository EmailTemplateCategoryRepository, IUserPermissions userPermissions, ITenantManager tenantManager, ILogManager logger, IHttpContextAccessor accessor)
        {
            _EmailTemplateCategoryRepository = EmailTemplateCategoryRepository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
        }

        public Task<List<Models.EmailTemplateCategory>> GetEmailTemplateCategoriesAsync(int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_EmailTemplateCategoryRepository.GetEmailTemplateCategories(_alias.SiteId).ToList());
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplateCategory Get Attempt {SiteId}", _alias.SiteId);
                return null;
            }
        }

        public Task<Models.EmailTemplateCategory> GetEmailTemplateCategoryAsync(int EmailTemplateCategoryId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                var category = _EmailTemplateCategoryRepository.GetEmailTemplateCategory(EmailTemplateCategoryId);
                if (category != null && category.SiteId == _alias.SiteId)
                {
                    return Task.FromResult(category);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "EmailTemplateCategory Not Found Or Site Mismatch {EmailTemplateCategoryId}", EmailTemplateCategoryId);
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplateCategory Get Attempt {EmailTemplateCategoryId} {ModuleId}", EmailTemplateCategoryId, ModuleId);
                return null;
            }
        }

        public Task<Models.EmailTemplateCategory> AddEmailTemplateCategoryAsync(Models.EmailTemplateCategory EmailTemplateCategory, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.Edit))
            {
                EmailTemplateCategory.SiteId = _alias.SiteId;
                EmailTemplateCategory = _EmailTemplateCategoryRepository.AddEmailTemplateCategory(EmailTemplateCategory);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "EmailTemplateCategory Added {EmailTemplateCategory}", EmailTemplateCategory);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplateCategory Add Attempt", EmailTemplateCategory);
                EmailTemplateCategory = null;
            }
            return Task.FromResult(EmailTemplateCategory);
        }

        public Task<Models.EmailTemplateCategory> UpdateEmailTemplateCategoryAsync(Models.EmailTemplateCategory EmailTemplateCategory, int ModuleId)
        {
            var existing = _EmailTemplateCategoryRepository.GetEmailTemplateCategory(EmailTemplateCategory.EmailTemplateCategoryId, false);
            if (existing != null && existing.SiteId == _alias.SiteId &&
                _userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.Edit))
            {
                EmailTemplateCategory.SiteId = _alias.SiteId;
                EmailTemplateCategory = _EmailTemplateCategoryRepository.UpdateEmailTemplateCategory(EmailTemplateCategory);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "EmailTemplateCategory Updated {EmailTemplateCategory}", EmailTemplateCategory);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplateCategory Update Attempt", EmailTemplateCategory);
                EmailTemplateCategory = null;
            }
            return Task.FromResult(EmailTemplateCategory);
        }

        public Task DeleteEmailTemplateCategoryAsync(int EmailTemplateCategoryId, int ModuleId)
        {
            var existing = _EmailTemplateCategoryRepository.GetEmailTemplateCategory(EmailTemplateCategoryId, false);
            if (existing != null && existing.SiteId == _alias.SiteId &&
                _userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.Edit))
            {
                _EmailTemplateCategoryRepository.DeleteEmailTemplateCategory(EmailTemplateCategoryId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "EmailTemplateCategory Deleted {EmailTemplateCategoryId}", EmailTemplateCategoryId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplateCategory Delete Attempt {EmailTemplateCategoryId}", EmailTemplateCategoryId);
            }
            return Task.CompletedTask;
        }
    }
}
