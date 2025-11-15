using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_QlBi_a.Migrations
{
    /// <inheritdoc />
    public partial class AddSoGioToDatBan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "so_gio",
                table: "dat_ban",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "so_gio",
                table: "dat_ban");
        }
    }
}
