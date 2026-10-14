using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConversationRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationMembers_AspNetUsers_UserId",
                table: "ConversationMembers");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationMembers_AspNetUsers_UserId",
                table: "ConversationMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationMembers_AspNetUsers_UserId",
                table: "ConversationMembers");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationMembers_AspNetUsers_UserId",
                table: "ConversationMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
