using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Oqtane.Services;
using Amazing.Module.EmailTemplate.Services;

namespace Amazing.Module.EmailTemplate.Startup
{
    public class ClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            if (!services.Any(s => s.ServiceType == typeof(IEmailTemplateService)))
            {
                services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            }
            if (!services.Any(s => s.ServiceType == typeof(IEmailTemplateCategoryService)))
            {
                services.AddScoped<IEmailTemplateCategoryService, EmailTemplateCategoryService>();
            }
        }
    }
}
