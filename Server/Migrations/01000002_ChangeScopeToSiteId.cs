using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Amazing.Module.EmailTemplate.Repository;

namespace Amazing.Module.EmailTemplate.Migrations
{
    [DbContext(typeof(EmailTemplateContext))]
    [Migration("Amazing.Module.EmailTemplate.01.00.00.02")]
    public class ChangeScopeToSiteId : MultiDatabaseMigration
    {
        public ChangeScopeToSiteId(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AmazingEmailTemplate_Module",
                table: "AmazingEmailTemplate");

            migrationBuilder.RenameColumn(
                name: "ModuleId",
                table: "AmazingEmailTemplate",
                newName: "SiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_AmazingEmailTemplate_Site",
                table: "AmazingEmailTemplate",
                column: "SiteId",
                principalTable: "Site",
                principalColumn: "SiteId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AmazingEmailTemplate_Site",
                table: "AmazingEmailTemplate");

            migrationBuilder.RenameColumn(
                name: "SiteId",
                table: "AmazingEmailTemplate",
                newName: "ModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_AmazingEmailTemplate_Module",
                table: "AmazingEmailTemplate",
                column: "ModuleId",
                principalTable: "Module",
                principalColumn: "ModuleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
