using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityMessageBoard.Migrations
{
    /// <inheritdoc />
    public partial class AddedMessagesToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_messages_users_author_id",
                table: "messages");

            migrationBuilder.AddForeignKey(
                name: "fk_messages_application_user_author_id",
                table: "messages",
                column: "author_id",
                principalTable: "AspNetUsers",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_messages_application_user_author_id",
                table: "messages");

            migrationBuilder.AddForeignKey(
                name: "fk_messages_users_author_id",
                table: "messages",
                column: "author_id",
                principalTable: "AspNetUsers",
                principalColumn: "id");
        }
    }
}
