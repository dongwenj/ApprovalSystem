using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWebApi.Migrations
{
    /// <inheritdoc />
    public partial class TableNameUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_購買申請單細項",
                table: "購買申請單細項");

            migrationBuilder.DropPrimaryKey(
                name: "PK_購買申請單",
                table: "購買申請單");

            migrationBuilder.DropPrimaryKey(
                name: "PK_人事資料",
                table: "人事資料");

            migrationBuilder.RenameTable(
                name: "購買申請單細項",
                newName: "ApplicationFormDetail");

            migrationBuilder.RenameTable(
                name: "購買申請單",
                newName: "ApplicationForm");

            migrationBuilder.RenameTable(
                name: "人事資料",
                newName: "SystemUser");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationFormDetail",
                table: "ApplicationFormDetail",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationForm",
                table: "ApplicationForm",
                column: "ApplicationNo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SystemUser",
                table: "SystemUser",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SystemUser",
                table: "SystemUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationFormDetail",
                table: "ApplicationFormDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationForm",
                table: "ApplicationForm");

            migrationBuilder.RenameTable(
                name: "SystemUser",
                newName: "人事資料");

            migrationBuilder.RenameTable(
                name: "ApplicationFormDetail",
                newName: "購買申請單細項");

            migrationBuilder.RenameTable(
                name: "ApplicationForm",
                newName: "購買申請單");

            migrationBuilder.AddPrimaryKey(
                name: "PK_人事資料",
                table: "人事資料",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_購買申請單細項",
                table: "購買申請單細項",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_購買申請單",
                table: "購買申請單",
                column: "ApplicationNo");
        }
    }
}
