using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_QlBi_a.Migrations
{
    /// <inheritdoc />
    public partial class update_banBia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "vi_tri_y",
                table: "ban_bia",
                type: "decimal(5,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "vi_tri_x",
                table: "ban_bia",
                type: "decimal(5,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "vi_tri_y",
                table: "ban_bia",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "vi_tri_x",
                table: "ban_bia",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);
        }
    }
}
