using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Photo",
                table: "Doctors",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoContentType",
                table: "Doctors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Photo",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "PhotoContentType",
                table: "Doctors");
        }
    }
}
