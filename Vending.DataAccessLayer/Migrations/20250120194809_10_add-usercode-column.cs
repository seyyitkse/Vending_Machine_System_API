using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vending.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class _10_addusercodecolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserCode",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserCode",
                table: "AspNetUsers");
        }
    }
}
