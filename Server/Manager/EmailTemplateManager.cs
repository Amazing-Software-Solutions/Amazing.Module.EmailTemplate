using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Oqtane.Modules;
using Oqtane.Models;
using Oqtane.Infrastructure;
using Oqtane.Interfaces;
using Oqtane.Enums;
using Oqtane.Repository;
using Amazing.Module.EmailTemplate.Repository;
using System.Threading.Tasks;

namespace Amazing.Module.EmailTemplate.Manager
{
    public class EmailTemplateManager : MigratableModuleBase, IInstallable, IPortable, ISearchable
    {
        private readonly IEmailTemplateRepository _EmailTemplateRepository;
        private readonly IDBContextDependencies _DBContextDependencies;

        public EmailTemplateManager(IEmailTemplateRepository EmailTemplateRepository, IDBContextDependencies DBContextDependencies)
        {
            _EmailTemplateRepository = EmailTemplateRepository;
            _DBContextDependencies = DBContextDependencies;
        }

        public bool Install(Tenant tenant, string version)
        {
            return Migrate(new EmailTemplateContext(_DBContextDependencies), tenant, MigrationType.Up);
        }

        public bool Uninstall(Tenant tenant)
        {
            return Migrate(new EmailTemplateContext(_DBContextDependencies), tenant, MigrationType.Down);
        }

        public string ExportModule(Oqtane.Models.Module module)
        {
            string content = "";
            List<Models.EmailTemplate> EmailTemplates = _EmailTemplateRepository.GetEmailTemplates(module.ModuleId).ToList();
            if (EmailTemplates != null)
            {
                content = JsonSerializer.Serialize(EmailTemplates);
            }
            return content;
        }

        public void ImportModule(Oqtane.Models.Module module, string content, string version)
        {
            List<Models.EmailTemplate> EmailTemplates = null;
            if (!string.IsNullOrEmpty(content))
            {
                EmailTemplates = JsonSerializer.Deserialize<List<Models.EmailTemplate>>(content);
            }
            if (EmailTemplates != null)
            {
                foreach(var EmailTemplate in EmailTemplates)
                {
                    _EmailTemplateRepository.AddEmailTemplate(new Models.EmailTemplate 
                    { 
                        ModuleId = module.ModuleId, 
                        Name = EmailTemplate.Name,
                        Description = EmailTemplate.Description,
                        Subject = EmailTemplate.Subject,
                        Body = EmailTemplate.Body,
                        Category = EmailTemplate.Category,
                        IsActive = EmailTemplate.IsActive,
                        TemplateVariables = EmailTemplate.TemplateVariables
                    });
                }
            }
        }

        public Task<List<SearchContent>> GetSearchContentsAsync(PageModule pageModule, DateTime lastIndexedOn)
        {
           var searchContentList = new List<SearchContent>();

           foreach (var EmailTemplate in _EmailTemplateRepository.GetEmailTemplates(pageModule.ModuleId))
           {
               if (EmailTemplate.ModifiedOn >= lastIndexedOn)
               {
                   searchContentList.Add(new SearchContent
                   {
                       EntityName = "AmazingEmailTemplate",
                       EntityId = EmailTemplate.EmailTemplateId.ToString(),
                       Title = EmailTemplate.Name,
                       Body = $"{EmailTemplate.Description} {EmailTemplate.Subject} {EmailTemplate.Body}",
                       ContentModifiedBy = EmailTemplate.ModifiedBy,
                       ContentModifiedOn = EmailTemplate.ModifiedOn
                   });
               }
           }

           return Task.FromResult(searchContentList);
        }
    }
}
