using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Migrations
{
    /// <inheritdoc />
    public partial class FixUserRoleDiscriminator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUserRoles");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 12, 9, 23, 43, 225, DateTimeKind.Utc).AddTicks(6164));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 12, 9, 23, 43, 225, DateTimeKind.Utc).AddTicks(6170));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 12, 9, 23, 43, 225, DateTimeKind.Utc).AddTicks(6173));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 12, 9, 23, 43, 225, DateTimeKind.Utc).AddTicks(6177));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUserRoles",
                type: "nvarchar(34)",
                maxLength: 34,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 11, 11, 28, 0, 301, DateTimeKind.Utc).AddTicks(3120));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 11, 11, 28, 0, 301, DateTimeKind.Utc).AddTicks(3123));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 11, 11, 28, 0, 301, DateTimeKind.Utc).AddTicks(3125));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 11, 11, 28, 0, 301, DateTimeKind.Utc).AddTicks(3128));
        }
    }
}
