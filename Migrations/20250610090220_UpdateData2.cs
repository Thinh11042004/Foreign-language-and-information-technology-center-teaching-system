using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Migrations
{
    /// <inheritdoc />
    public partial class UpdateData2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebsiteVisits_AspNetUsers_UserId",
                table: "WebsiteVisits");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "WebsiteVisits",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_WebsiteVisits_AspNetUsers_UserId",
                table: "WebsiteVisits",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebsiteVisits_AspNetUsers_UserId",
                table: "WebsiteVisits");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "WebsiteVisits",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WebsiteVisits_AspNetUsers_UserId",
                table: "WebsiteVisits",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
