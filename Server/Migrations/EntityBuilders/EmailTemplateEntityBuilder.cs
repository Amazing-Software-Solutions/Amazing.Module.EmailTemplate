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
        private readonly ForeignKey<EmailTemplateEntityBuilder> _moduleForeignKey = new("FK_AmazingEmailTemplate_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.Cascade);

        public EmailTemplateEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_moduleForeignKey);
        }

        protected override EmailTemplateEntityBuilder BuildTable(ColumnsBuilder table)
        {
            EmailTemplateId = AddAutoIncrementColumn(table,"EmailTemplateId");
            ModuleId = AddIntegerColumn(table,"ModuleId");
            Name = AddMaxStringColumn(table,"Name");
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> EmailTemplateId { get; set; }
        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }
        public OperationBuilder<AddColumnOperation> Name { get; set; }

    }
}
