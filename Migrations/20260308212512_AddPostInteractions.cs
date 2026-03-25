using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication4.Migrations
{
    /// <inheritdoc />
    public partial class AddPostInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostInteractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalPostId = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalAuthorId = table.Column<string>(type: "TEXT", nullable: false),
                    ActorId = table.Column<string>(type: "TEXT", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", nullable: true),
                    ReplyPostId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostInteractions_AspNetUsers_ActorId",
                        column: x => x.ActorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostInteractions_AspNetUsers_OriginalAuthorId",
                        column: x => x.OriginalAuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostInteractions_ImagePosts_OriginalPostId",
                        column: x => x.OriginalPostId,
                        principalTable: "ImagePosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostInteractions_ImagePosts_ReplyPostId",
                        column: x => x.ReplyPostId,
                        principalTable: "ImagePosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostInteractions_ActorId",
                table: "PostInteractions",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_PostInteractions_OriginalAuthorId",
                table: "PostInteractions",
                column: "OriginalAuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_PostInteractions_OriginalPostId",
                table: "PostInteractions",
                column: "OriginalPostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostInteractions_ReplyPostId",
                table: "PostInteractions",
                column: "ReplyPostId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostInteractions");
        }
    }
}
