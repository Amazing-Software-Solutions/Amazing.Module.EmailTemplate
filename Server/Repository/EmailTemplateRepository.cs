using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;

namespace Amazing.Module.EmailTemplate.Repository
{
    public interface IEmailTemplateRepository
    {
        IEnumerable<Models.EmailTemplate> GetEmailTemplates(int ModuleId);
        Models.EmailTemplate GetEmailTemplate(int EmailTemplateId);
        Models.EmailTemplate GetEmailTemplate(int EmailTemplateId, bool tracking);
        Models.EmailTemplate AddEmailTemplate(Models.EmailTemplate EmailTemplate);
        Models.EmailTemplate UpdateEmailTemplate(Models.EmailTemplate EmailTemplate);
        void DeleteEmailTemplate(int EmailTemplateId);
        Models.EmailTemplate GetEmailTemplateByName(string templateName, int? moduleId = null);
        IEnumerable<Models.EmailTemplate> GetActiveEmailTemplates();
        IEnumerable<Models.EmailTemplate> GetEmailTemplatesByCategory(string category);
    }

    public class EmailTemplateRepository : IEmailTemplateRepository, ITransientService
    {
        private readonly IDbContextFactory<EmailTemplateContext> _factory;

        public EmailTemplateRepository(IDbContextFactory<EmailTemplateContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<Models.EmailTemplate> GetEmailTemplates(int ModuleId)
        {
            using var db = _factory.CreateDbContext();
            return db.EmailTemplate.Where(item => item.ModuleId == ModuleId).ToList();
        }

        public Models.EmailTemplate GetEmailTemplate(int EmailTemplateId)
        {
            return GetEmailTemplate(EmailTemplateId, true);
        }

        public Models.EmailTemplate GetEmailTemplate(int EmailTemplateId, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.EmailTemplate.Find(EmailTemplateId);
            }
            else
            {
                return db.EmailTemplate.AsNoTracking().FirstOrDefault(item => item.EmailTemplateId == EmailTemplateId);
            }
        }

        public Models.EmailTemplate AddEmailTemplate(Models.EmailTemplate EmailTemplate)
        {
            using var db = _factory.CreateDbContext();
            db.EmailTemplate.Add(EmailTemplate);
            db.SaveChanges();
            return EmailTemplate;
        }

        public Models.EmailTemplate UpdateEmailTemplate(Models.EmailTemplate EmailTemplate)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(EmailTemplate).State = EntityState.Modified;
            db.SaveChanges();
            return EmailTemplate;
        }

        public void DeleteEmailTemplate(int EmailTemplateId)
        {
            using var db = _factory.CreateDbContext();
            Models.EmailTemplate EmailTemplate = db.EmailTemplate.Find(EmailTemplateId);
            db.EmailTemplate.Remove(EmailTemplate);
            db.SaveChanges();
        }

        public Models.EmailTemplate GetEmailTemplateByName(string templateName, int? moduleId = null)
        {
            using var db = _factory.CreateDbContext();
            var query = db.EmailTemplate.Where(item => item.Name == templateName && item.IsActive);
            
            if (moduleId.HasValue)
            {
                query = query.Where(item => item.ModuleId == moduleId.Value);
            }
            
            return query.FirstOrDefault();
        }

        public IEnumerable<Models.EmailTemplate> GetActiveEmailTemplates()
        {
            using var db = _factory.CreateDbContext();
            return db.EmailTemplate.Where(item => item.IsActive).ToList();
        }

        public IEnumerable<Models.EmailTemplate> GetEmailTemplatesByCategory(string category)
        {
            using var db = _factory.CreateDbContext();
            return db.EmailTemplate.Where(item => item.Category == category && item.IsActive).ToList();
        }
    }
}
