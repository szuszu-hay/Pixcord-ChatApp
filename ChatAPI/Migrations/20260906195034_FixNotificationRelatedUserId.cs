using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixNotificationRelatedUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedUserId",
                table: "Notifications",
                column: "RelatedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_RelatedUserId",
                table: "Notifications",
                column: "RelatedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_RelatedUserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RelatedUserId",
                table: "Notifications");
        }
    }
}
