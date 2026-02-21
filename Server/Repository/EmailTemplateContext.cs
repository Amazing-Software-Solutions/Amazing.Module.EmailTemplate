using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Infrastructure;
using Oqtane.Repository.Databases.Interfaces;

namespace Amazing.Module.EmailTemplate.Repository
{
    public class EmailTemplateContext : DBContextBase, ITransientService, IMultiDatabase
    {
        public virtual DbSet<Models.EmailTemplate> EmailTemplate { get; set; }

        public EmailTemplateContext(IDBContextDependencies DBContextDependencies) : base(DBContextDependencies)
        {
            // ContextBase handles multi-tenant database connections
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Models.EmailTemplate>().ToTable(ActiveDatabase.RewriteName("AmazingEmailTemplate"));
        }
    }
}
