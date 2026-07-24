using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingEmailConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationTokenHash",
                table: "Bookings",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConfirmationEmailAttempts",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmationEmailSentAtUtc",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmationExpiresAtUtc",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmationTokenHash",
                table: "Bookings",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAtUtc",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastConfirmationEmailSentAtUtc",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Bookings",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Migrate the old boolean IsCancelled onto the new Status enum before dropping it.
            // Existing bookings were final once created: cancelled -> Cancelled(3), otherwise Confirmed(2).
            migrationBuilder.Sql(
                "UPDATE [Bookings] SET [Status] = CASE WHEN [IsCancelled] = 1 THEN 3 ELSE 2 END;");

            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CancellationTokenHash",
                table: "Bookings",
                column: "CancellationTokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ConfirmationTokenHash",
                table: "Bookings",
                column: "ConfirmationTokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_CancellationTokenHash",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ConfirmationTokenHash",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CancellationTokenHash",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ConfirmationEmailAttempts",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ConfirmationEmailSentAtUtc",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ConfirmationExpiresAtUtc",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ConfirmationTokenHash",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ConfirmedAtUtc",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "LastConfirmationEmailSentAtUtc",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Bookings");

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
