using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;

namespace Amazing.Module.EmailTemplate.Services
{
    public interface IEmailTemplateCategoryService
    {
        Task<List<Models.EmailTemplateCategory>> GetEmailTemplateCategoriesAsync(int ModuleId);

        Task<Models.EmailTemplateCategory> GetEmailTemplateCategoryAsync(int EmailTemplateCategoryId, int ModuleId);

        Task<Models.EmailTemplateCategory> AddEmailTemplateCategoryAsync(Models.EmailTemplateCategory EmailTemplateCategory, int ModuleId);

        Task<Models.EmailTemplateCategory> UpdateEmailTemplateCategoryAsync(Models.EmailTemplateCategory EmailTemplateCategory, int ModuleId);

        Task DeleteEmailTemplateCategoryAsync(int EmailTemplateCategoryId, int ModuleId);
    }

    public class EmailTemplateCategoryService : ServiceBase, IEmailTemplateCategoryService
    {
        public EmailTemplateCategoryService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("EmailTemplateCategory");

        public async Task<List<Models.EmailTemplateCategory>> GetEmailTemplateCategoriesAsync(int ModuleId)
        {
            List<Models.EmailTemplateCategory> EmailTemplateCategories = await GetJsonAsync<List<Models.EmailTemplateCategory>>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), Enumerable.Empty<Models.EmailTemplateCategory>().ToList());
            return EmailTemplateCategories.OrderBy(item => item.Name).ToList();
        }

        public async Task<Models.EmailTemplateCategory> GetEmailTemplateCategoryAsync(int EmailTemplateCategoryId, int ModuleId)
        {
            return await GetJsonAsync<Models.EmailTemplateCategory>(CreateAuthorizationPolicyUrl($"{Apiurl}/{EmailTemplateCategoryId}/{ModuleId}", EntityNames.Module, ModuleId));
        }

        public async Task<Models.EmailTemplateCategory> AddEmailTemplateCategoryAsync(Models.EmailTemplateCategory EmailTemplateCategory, int ModuleId)
        {
            return await PostJsonAsync<Models.EmailTemplateCategory>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), EmailTemplateCategory);
        }

        public async Task<Models.EmailTemplateCategory> UpdateEmailTemplateCategoryAsync(Models.EmailTemplateCategory EmailTemplateCategory, int ModuleId)
        {
            return await PutJsonAsync<Models.EmailTemplateCategory>(CreateAuthorizationPolicyUrl($"{Apiurl}/{EmailTemplateCategory.EmailTemplateCategoryId}?moduleid={ModuleId}", EntityNames.Module, ModuleId), EmailTemplateCategory);
        }

        public async Task DeleteEmailTemplateCategoryAsync(int EmailTemplateCategoryId, int ModuleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/{EmailTemplateCategoryId}/{ModuleId}", EntityNames.Module, ModuleId));
        }
    }
}
