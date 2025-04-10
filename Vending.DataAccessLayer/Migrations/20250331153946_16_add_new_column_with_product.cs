using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vending.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class _16_add_new_column_with_product : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCriticalStock",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCriticalStock",
                table: "Products");
        }
    }
}
