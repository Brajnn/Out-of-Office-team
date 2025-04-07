using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Out_of_Office.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDecisionCommentToApprovalRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DecisionComment",
                table: "ApprovalRequest",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecisionComment",
                table: "ApprovalRequest");
        }
    }
}
