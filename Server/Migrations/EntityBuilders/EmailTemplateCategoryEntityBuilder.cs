using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace Amazing.Module.EmailTemplate.Migrations.EntityBuilders
{
    public class EmailTemplateCategoryEntityBuilder : AuditableBaseEntityBuilder<EmailTemplateCategoryEntityBuilder>
    {
        private const string _entityTableName = "AmazingEmailTemplateCategory";
        private readonly PrimaryKey<EmailTemplateCategoryEntityBuilder> _primaryKey = new("PK_AmazingEmailTemplateCategory", x => x.EmailTemplateCategoryId);
        private readonly ForeignKey<EmailTemplateCategoryEntityBuilder> _siteForeignKey = new("FK_AmazingEmailTemplateCategory_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public EmailTemplateCategoryEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override EmailTemplateCategoryEntityBuilder BuildTable(ColumnsBuilder table)
        {
            EmailTemplateCategoryId = AddAutoIncrementColumn(table, "EmailTemplateCategoryId");
            SiteId = AddIntegerColumn(table, "SiteId");
            Name = AddStringColumn(table, "Name", 50);
            Description = AddStringColumn(table, "Description", 500, true);
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> EmailTemplateCategoryId { get; set; }
        public OperationBuilder<AddColumnOperation> SiteId { get; set; }
        public OperationBuilder<AddColumnOperation> Name { get; set; }
        public OperationBuilder<AddColumnOperation> Description { get; set; }
    }
}
