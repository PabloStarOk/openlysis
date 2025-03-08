using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Openlysis.API.Authentication.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IncludeApiKeyHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKeyHash",
                table: "AspNetUsers",
                type: "VARCHAR(64)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKeyHash",
                table: "AspNetUsers");
        }
    }
}
