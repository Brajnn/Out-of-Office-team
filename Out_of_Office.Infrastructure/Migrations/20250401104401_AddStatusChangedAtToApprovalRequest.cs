using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Out_of_Office.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusChangedAtToApprovalRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StatusChangedAt",
                table: "ApprovalRequest",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusChangedAt",
                table: "ApprovalRequest");
        }
    }
}
