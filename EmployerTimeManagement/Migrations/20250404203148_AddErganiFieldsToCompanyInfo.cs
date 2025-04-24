using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployerTimeManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddErganiFieldsToCompanyInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErganiApiKey",
                table: "CompanyInfos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErganiApiKey",
                table: "CompanyInfos");
        }
    }
}
