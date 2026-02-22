using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace Amazing.Module.EmailTemplate.Migrations.EntityBuilders
{
    public class EmailTemplateEntityBuilder : AuditableBaseEntityBuilder<EmailTemplateEntityBuilder>
    {
        private const string _entityTableName = "AmazingEmailTemplate";
        private readonly PrimaryKey<EmailTemplateEntityBuilder> _primaryKey = new("PK_AmazingEmailTemplate", x => x.EmailTemplateId);
        private readonly ForeignKey<EmailTemplateEntityBuilder> _siteForeignKey = new("FK_AmazingEmailTemplate_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public EmailTemplateEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override EmailTemplateEntityBuilder BuildTable(ColumnsBuilder table)
        {
            EmailTemplateId = AddAutoIncrementColumn(table,"EmailTemplateId");
            SiteId = AddIntegerColumn(table,"SiteId");
            Name = AddMaxStringColumn(table,"Name");
            Description = AddStringColumn(table, "Description", 500, true);
            Subject = AddStringColumn(table, "Subject", 200);
            Body = AddMaxStringColumn(table, "Body");
            Category = AddStringColumn(table, "Category", 50, true);
            IsActive = AddBooleanColumn(table, "IsActive", false, true);
            TemplateVariables = AddStringColumn(table, "TemplateVariables", 2000, true);
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> EmailTemplateId { get; set; }
        public OperationBuilder<AddColumnOperation> SiteId { get; set; }
        public OperationBuilder<AddColumnOperation> Name { get; set; }
        public OperationBuilder<AddColumnOperation> Description { get; set; }
        public OperationBuilder<AddColumnOperation> Subject { get; set; }
        public OperationBuilder<AddColumnOperation> Body { get; set; }
        public OperationBuilder<AddColumnOperation> Category { get; set; }
        public OperationBuilder<AddColumnOperation> IsActive { get; set; }
        public OperationBuilder<AddColumnOperation> TemplateVariables { get; set; }
    }
}
