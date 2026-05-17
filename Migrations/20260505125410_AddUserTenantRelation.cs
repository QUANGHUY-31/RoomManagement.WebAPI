using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomManagement.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTenantRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bills_Rooms_RoomId",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "FK_ContractTenants_Rooms_RoomId",
                table: "ContractTenants");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Tenants_PrimaryTenantId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_PrimaryTenantId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_ContractTenants_RoomId",
                table: "ContractTenants");

            migrationBuilder.DropIndex(
                name: "IX_Bills_RoomId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "PrimaryTenantId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "ContractTenants");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Bills");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Tenants",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_UserId",
                table: "Tenants",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Users_UserId",
                table: "Tenants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Users_UserId",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_UserId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Tenants");

            migrationBuilder.AddColumn<int>(
                name: "PrimaryTenantId",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "ContractTenants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "Bills",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_PrimaryTenantId",
                table: "Rooms",
                column: "PrimaryTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTenants_RoomId",
                table: "ContractTenants",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_RoomId",
                table: "Bills",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_Rooms_RoomId",
                table: "Bills",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ContractTenants_Rooms_RoomId",
                table: "ContractTenants",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Tenants_PrimaryTenantId",
                table: "Rooms",
                column: "PrimaryTenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
