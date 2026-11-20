using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;

namespace Amazing.Module.EmailTemplate.Repository
{
    public interface IEmailTemplateCategoryRepository
    {
        IEnumerable<Models.EmailTemplateCategory> GetEmailTemplateCategories(int SiteId);
        Models.EmailTemplateCategory GetEmailTemplateCategory(int EmailTemplateCategoryId);
        Models.EmailTemplateCategory GetEmailTemplateCategory(int EmailTemplateCategoryId, bool tracking);
        Models.EmailTemplateCategory AddEmailTemplateCategory(Models.EmailTemplateCategory EmailTemplateCategory);
        Models.EmailTemplateCategory UpdateEmailTemplateCategory(Models.EmailTemplateCategory EmailTemplateCategory);
        void DeleteEmailTemplateCategory(int EmailTemplateCategoryId);
    }

    public class EmailTemplateCategoryRepository : IEmailTemplateCategoryRepository, ITransientService
    {
        private readonly IDbContextFactory<EmailTemplateContext> _factory;

        public EmailTemplateCategoryRepository(IDbContextFactory<EmailTemplateContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<Models.EmailTemplateCategory> GetEmailTemplateCategories(int SiteId)
        {
            using var db = _factory.CreateDbContext();
            return db.EmailTemplateCategory.Where(item => item.SiteId == SiteId).ToList();
        }

        public Models.EmailTemplateCategory GetEmailTemplateCategory(int EmailTemplateCategoryId)
        {
            return GetEmailTemplateCategory(EmailTemplateCategoryId, true);
        }

        public Models.EmailTemplateCategory GetEmailTemplateCategory(int EmailTemplateCategoryId, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.EmailTemplateCategory.Find(EmailTemplateCategoryId);
            }
            else
            {
                return db.EmailTemplateCategory.AsNoTracking().FirstOrDefault(item => item.EmailTemplateCategoryId == EmailTemplateCategoryId);
            }
        }

        public Models.EmailTemplateCategory AddEmailTemplateCategory(Models.EmailTemplateCategory EmailTemplateCategory)
        {
            using var db = _factory.CreateDbContext();
            db.EmailTemplateCategory.Add(EmailTemplateCategory);
            db.SaveChanges();
            return EmailTemplateCategory;
        }

        public Models.EmailTemplateCategory UpdateEmailTemplateCategory(Models.EmailTemplateCategory EmailTemplateCategory)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(EmailTemplateCategory).State = EntityState.Modified;
            db.SaveChanges();
            return EmailTemplateCategory;
        }

        public void DeleteEmailTemplateCategory(int EmailTemplateCategoryId)
        {
            using var db = _factory.CreateDbContext();
            Models.EmailTemplateCategory EmailTemplateCategory = db.EmailTemplateCategory.Find(EmailTemplateCategoryId);
            db.EmailTemplateCategory.Remove(EmailTemplateCategory);
            db.SaveChanges();
        }
    }
}
