using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolEvents.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGalleryAlbums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlbumId",
                table: "GalleryPhotos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GalleryAlbums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoverPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryAlbums", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GalleryPhotos_AlbumId",
                table: "GalleryPhotos",
                column: "AlbumId");

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryPhotos_GalleryAlbums_AlbumId",
                table: "GalleryPhotos",
                column: "AlbumId",
                principalTable: "GalleryAlbums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GalleryPhotos_GalleryAlbums_AlbumId",
                table: "GalleryPhotos");

            migrationBuilder.DropTable(
                name: "GalleryAlbums");

            migrationBuilder.DropIndex(
                name: "IX_GalleryPhotos_AlbumId",
                table: "GalleryPhotos");

            migrationBuilder.DropColumn(
                name: "AlbumId",
                table: "GalleryPhotos");
        }
    }
}
