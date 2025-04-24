using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployerTimeManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkLogIsSentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSent",
                table: "WorkLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                table: "WorkLogs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSent",
                table: "WorkLogs");

            migrationBuilder.DropColumn(
                name: "SentAt",
                table: "WorkLogs");
        }
    }
}
