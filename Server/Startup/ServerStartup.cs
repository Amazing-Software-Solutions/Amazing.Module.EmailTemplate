using Microsoft.AspNetCore.Builder; 
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using Amazing.Module.EmailTemplate.Repository;
using Amazing.Module.EmailTemplate.Services;

namespace Amazing.Module.EmailTemplate.Startup
{
    public class ServerStartup : IServerStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // not implemented
        }

        public void ConfigureMvc(IMvcBuilder mvcBuilder)
        {
            // not implemented
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IEmailTemplateService, ServerEmailTemplateService>();
            services.AddTransient<IEmailSendingService, ServerEmailSendingService>();
            services.AddDbContextFactory<EmailTemplateContext>(opt => { }, ServiceLifetime.Transient);
        }
    }
}
