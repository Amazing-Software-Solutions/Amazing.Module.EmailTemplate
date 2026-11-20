using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Amazing.Module.EmailTemplate.Migrations.EntityBuilders;
using Amazing.Module.EmailTemplate.Repository;

namespace Amazing.Module.EmailTemplate.Migrations
{
    [DbContext(typeof(EmailTemplateContext))]
    [Migration("Amazing.Module.EmailTemplate.01.00.00.03")]
    public class AddEmailTemplateCategory : MultiDatabaseMigration
    {
        public AddEmailTemplateCategory(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var entityBuilder = new EmailTemplateCategoryEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var entityBuilder = new EmailTemplateCategoryEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilder.Drop();
        }
    }
}
