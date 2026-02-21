using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Amazing.Module.EmailTemplate.Migrations.EntityBuilders;
using Amazing.Module.EmailTemplate.Repository;

namespace Amazing.Module.EmailTemplate.Migrations
{
    [DbContext(typeof(EmailTemplateContext))]
    [Migration("Amazing.Module.EmailTemplate.01.00.00.01")]
    public class AddEmailTemplateFields : MultiDatabaseMigration
    {
        public AddEmailTemplateFields(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var emailTemplateBuilder = new EmailTemplateEntityBuilder(migrationBuilder, ActiveDatabase);
            
            emailTemplateBuilder.AddStringColumn("Description", 500, true);
            emailTemplateBuilder.AddStringColumn("Subject", 200, false);
            emailTemplateBuilder.AddMaxStringColumn("Body", false);
            emailTemplateBuilder.AddStringColumn("Category", 50, true);
            emailTemplateBuilder.AddBooleanColumn("IsActive", false, true);
            emailTemplateBuilder.AddStringColumn("TemplateVariables", 2000, true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var emailTemplateBuilder = new EmailTemplateEntityBuilder(migrationBuilder, ActiveDatabase);
            
            emailTemplateBuilder.DropColumn("TemplateVariables");
            emailTemplateBuilder.DropColumn("IsActive");
            emailTemplateBuilder.DropColumn("Category");
            emailTemplateBuilder.DropColumn("Body");
            emailTemplateBuilder.DropColumn("Subject");
            emailTemplateBuilder.DropColumn("Description");
        }
    }
}
