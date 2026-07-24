using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleBookingsPerSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_AppointmentSlotId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_AppointmentSlotId",
                table: "Bookings",
                column: "AppointmentSlotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_AppointmentSlotId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_AppointmentSlotId",
                table: "Bookings",
                column: "AppointmentSlotId",
                unique: true);
        }
    }
}
