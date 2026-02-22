using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;

namespace Amazing.Module.EmailTemplate.Services
{
    public interface IEmailTemplateService 
    {
        Task<List<Models.EmailTemplate>> GetEmailTemplatesAsync(int ModuleId);

        Task<Models.EmailTemplate> GetEmailTemplateAsync(int EmailTemplateId, int ModuleId);

        Task<Models.EmailTemplate> AddEmailTemplateAsync(Models.EmailTemplate EmailTemplate, int ModuleId);

        Task<Models.EmailTemplate> UpdateEmailTemplateAsync(Models.EmailTemplate EmailTemplate, int ModuleId);

        Task DeleteEmailTemplateAsync(int EmailTemplateId, int ModuleId);

        Task<Models.TestEmailResult> SendTestEmailAsync(int moduleId, string toEmail, string subject, string body, string variableJson);
    }

    public class EmailTemplateService : ServiceBase, IEmailTemplateService
    {
        public EmailTemplateService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("EmailTemplate");

        public async Task<List<Models.EmailTemplate>> GetEmailTemplatesAsync(int ModuleId)
        {
            List<Models.EmailTemplate> EmailTemplates = await GetJsonAsync<List<Models.EmailTemplate>>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), Enumerable.Empty<Models.EmailTemplate>().ToList());
            return EmailTemplates.OrderBy(item => item.Name).ToList();
        }

        public async Task<Models.EmailTemplate> GetEmailTemplateAsync(int EmailTemplateId, int ModuleId)
        {
            return await GetJsonAsync<Models.EmailTemplate>(CreateAuthorizationPolicyUrl($"{Apiurl}/{EmailTemplateId}/{ModuleId}", EntityNames.Module, ModuleId));
        }

        public async Task<Models.EmailTemplate> AddEmailTemplateAsync(Models.EmailTemplate EmailTemplate, int ModuleId)
        {
            return await PostJsonAsync<Models.EmailTemplate>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), EmailTemplate);
        }

        public async Task<Models.EmailTemplate> UpdateEmailTemplateAsync(Models.EmailTemplate EmailTemplate, int ModuleId)
        {
            return await PutJsonAsync<Models.EmailTemplate>(CreateAuthorizationPolicyUrl($"{Apiurl}/{EmailTemplate.EmailTemplateId}?moduleid={ModuleId}", EntityNames.Module, ModuleId), EmailTemplate);
        }

        public async Task DeleteEmailTemplateAsync(int EmailTemplateId, int ModuleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/{EmailTemplateId}/{ModuleId}", EntityNames.Module, ModuleId));
        }

        public async Task<Models.TestEmailResult> SendTestEmailAsync(int moduleId, string toEmail, string subject, string body, string variableJson)
        {
            var request = new Models.TestEmailRequest
            {
                ToEmail = toEmail,
                Subject = subject,
                Body = body,
                VariableJson = variableJson
            };

            return await PostJsonAsync<Models.TestEmailRequest, Models.TestEmailResult>(
                CreateAuthorizationPolicyUrl($"{Apiurl}/sendtest/{moduleId}", EntityNames.Module, moduleId),
                request);
        }
    }
}
