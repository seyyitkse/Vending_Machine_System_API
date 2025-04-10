using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vending.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class _14_add_log_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_UserCode",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "UserCode",
                table: "Orders",
                newName: "AppUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_UserCode",
                table: "Orders",
                newName: "IX_Orders_AppUserId");

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_AppUserId",
                table: "Orders",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_AppUserId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "Orders",
                newName: "UserCode");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_AppUserId",
                table: "Orders",
                newName: "IX_Orders_UserCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_UserCode",
                table: "Orders",
                column: "UserCode",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
