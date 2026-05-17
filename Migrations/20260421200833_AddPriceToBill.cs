using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomManagement.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToBill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ❌ Xóa đoạn này vì PaidAt đã có
            // migrationBuilder.AddColumn<DateTime>(
            //     name: "PaidAt",
            //     table: "Bills",
            //     type: "datetime2",
            //     nullable: true);

            // ✅ Giữ chỉ Price
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Bills",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ❌ Xóa đoạn này
            // migrationBuilder.DropColumn(
            //     name: "PaidAt",
            //     table: "Bills");

            // ✅ Giữ chỉ Price
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Bills");
        }
    }
}