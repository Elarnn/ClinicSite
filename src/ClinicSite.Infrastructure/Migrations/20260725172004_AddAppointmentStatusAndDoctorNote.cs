using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentStatusAndDoctorNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppointmentStatus",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DoctorNote",
                table: "Bookings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppointmentStatus",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DoctorNote",
                table: "Bookings");
        }
    }
}
