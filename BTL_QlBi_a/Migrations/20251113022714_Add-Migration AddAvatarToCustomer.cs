using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_QlBi_a.Migrations
{
    /// <inheritdoc />
    public partial class AddMigrationAddAvatarToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avatar",
                table: "khach_hang",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar",
                table: "khach_hang");
        }
    }
}
