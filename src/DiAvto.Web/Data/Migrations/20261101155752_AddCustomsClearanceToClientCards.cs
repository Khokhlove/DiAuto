using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiAvto.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomsClearanceToClientCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CustomsClearanceRub",
                table: "ClientCards",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CustomsClearanceTitle",
                table: "ClientCards",
                type: "TEXT",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomsClearanceRub",
                table: "ClientCards");

            migrationBuilder.DropColumn(
                name: "CustomsClearanceTitle",
                table: "ClientCards");
        }
    }
}
