using ClinicSite.Application.DTOs.Bookings;
using ClinicSite.Application.Exceptions;
using ClinicSite.Application.Security;
using ClinicSite.Domain.Enums;
using ClinicSite.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace ClinicSite.Tests;

public class BookingServiceTests
{
    // ---- 1. Booking is created in PendingConfirmation state ------------------------------------
    [Fact]
    public async Task CreateBooking_creates_pending_confirmation()
    {
        using var db = new TestDatabase();
        var (slotId, resvToken) = db.SeedReservedSlot();
        var email = new FakeEmailService();

        await CreateBooking(db, email, slotId, resvToken);

        var booking = db.GetBooking();
        Assert.Equal(BookingStatus.PendingConfirmation, booking.Status);
        Assert.Equal(SlotStatus.Booked, db.GetSlot(slotId).Status);
        Assert.Equal(1, email.ConfirmationRequestCount);
    }

    // ---- 2. ConfirmationExpiresAtUtc is set ~30 minutes out -----------------------------------
    [Fact]
    public async Task CreateBooking_sets_confirmation_expiry()
    {
        using var db = new TestDatabase();
        var (slotId, resvToken) = db.SeedReservedSlot();
        var email = new FakeEmailService();

        await CreateBooking(db, email, slotId, resvToken);

        var booking = db.GetBooking();
        Assert.NotNull(booking.ConfirmationExpiresAtUtc);
        var minutes = (booking.ConfirmationExpiresAtUtc!.Value - DateTime.UtcNow).TotalMinutes;
        Assert.InRange(minutes, 25, 31);
    }

    // ---- 3. The raw token is never stored (only its hash) -------------------------------------
    [Fact]
    public async Task CreateBooking_stores_hash_not_raw_token()
    {
        using var db = new TestDatabase();
        var (slotId, resvToken) = db.SeedReservedSlot();
        var email = new FakeEmailService();

        await CreateBooking(db, email, slotId, resvToken);

        var rawToken = email.LastConfirmationToken!;
        var booking = db.GetBooking();

        Assert.NotNull(booking.ConfirmationTokenHash);
        Assert.NotEqual(rawToken, booking.ConfirmationTokenHash);
        Assert.Equal(64, booking.ConfirmationTokenHash!.Length); // SHA-256 as lowercase hex
    }

    // ---- 4. The correct token confirms the booking --------------------------------------------
    [Fact]
    public async Task Confirm_with_correct_token_confirms_booking()
    {
        using var db = new TestDatabase();
        var (slotId, resvToken) = db.SeedReservedSlot();
        var email = new FakeEmailService();
        await CreateBooking(db, email, slotId, resvToken);

        var result = await Confirm(db, email, email.LastConfirmationToken!);

        Assert.Equal("Confirmed", result.Status);
        var booking = db.GetBooking();
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.NotNull(booking.ConfirmedAtUtc);
    }

    // ---- 5. A wrong token does not confirm ----------------------------------------------------
    [Fact]
    public async Task Confirm_with_wrong_token_does_not_confirm()
    {
        using var db = new TestDatabase();
        var (slotId, resvToken) = db.SeedReservedSlot();
        var email = new FakeEmailService();
        await CreateBooking(db, email, slotId, resvToken);

        var wrongToken = BookingTokens.Create().Token;

        await Assert.ThrowsAsync<NotFoundException>(() => Confirm(db, email, wrongToken));
        Assert.Equal(BookingStatus.PendingConfirmation, db.GetBooking().Status);
    }

    // ---- 6. An expired token returns 410 (Gone) and frees the slot ----------------------------
    [Fact]
    public async Task Confirm_with_expired_token_returns_gone_and_frees_slot()
    {
        using var db = new TestDatabase();
        var (slotId, resvToken) = db.SeedReservedSlot();
        var email = new FakeEmailService();
        await CreateBooking(db, email, slotId, resvToken);
        db.ExpireConfirmationWindow();

        await Assert.ThrowsAsync<GoneException>(() => Confirm(db, email, email.LastConfirmationToken!));

        Assert.Equal(BookingStatus.Expired, db.GetBooking().Status);
        Assert.Equal(SlotStatus.Free, db.GetSlot(slotId).Status);
    }

    // ---- 7 & 17. Confirming twice is idempotent (no double state change / e-mail) --------------
    [Fact]
    public async Task Confirm_is_idempotent()
    {
        using var db = new TestDatabase();
        var (slotId, resvToken) = db.SeedReservedSlot();
        var email = new FakeEmailService();
        await CreateBooking(db, email, slotId, resvToken);
        var token = email.LastConfirmationToken!;

        var first = await Confirm(db, email, token);
        var confirmedAt = db.GetBooking().ConfirmedAtUtc;

        var second = await Confirm(db, email, token);

        Assert.Equal("Confirmed", first.Status);
        Assert.Equal("Confirmed", second.Status);
        Assert.Equal(1, email.ConfirmedCount); // second confirm sends no additional e-mail
        Assert.Equal(confirmedAt, db.GetBooking().ConfirmedAtUtc);
    }

    // ---- 8. The confirmed e-mail is sent after a successful confirmation -----------------------
    [Fact]
    public async Task Confirm_sends_confirmed_email()
    {
        using var db = new TestDatabase();
        var (slotId, resvToken) = db.SeedReservedSlot();
        var email = new FakeEmailService();
        await CreateBooking(db, email, slotId, resvToken);

        await Confirm(db, email, email.LastConfirmationToken!);

        Assert.Equal(1, email.ConfirmedCount);
        Assert.NotNull(email.LastCancellationToken);
    }

    // ---- 9. A confirmed-e-mail failure does not roll back the confirmation ---------------------
    [Fact]
    public async Task Confirm_still_succeeds_when_confirmed_email_fails()
    {
        using var db = new TestDatabase();
        var (slotId, resvToken) = db.SeedReservedSlot();
        var email = new FakeEmailService { ThrowOnConfirmed = true };
        await CreateBooking(db, email, slotId, resvToken);

        var result = await Confirm(db, email, email.LastConfirmationToken!);

        Assert.Equal("Confirmed", result.Status);
        Assert.Equal(BookingStatus.Confirmed, db.GetBooking().Status);
    }

    // ---- 10. Confirmation and cancellation tokens differ --------------------------------------
    [Fact]
    public async Task Confirmation_and_cancellation_tokens_differ()
    {
        using var db = new TestDatabase();
        var (slotId, resvToken) = db.SeedReservedSlot();
        var email = new FakeEmailService();
        await CreateBooking(db, email, slotId, resvToken);
        var confirmationToken = email.LastConfirmationToken!;

        await Confirm(db, email, confirmationToken);
        var cancellationToken = email.LastCancellationToken!;

        Assert.NotEqual(confirmationToken, cancellationToken);
        var booking = db.GetBooking();
        Assert.NotEqual(booking.ConfirmationTokenHash, booking.CancellationTokenHash);
    }

    // ---- 11 & 12. The correct cancellation token cancels and frees the slot --------------------
    [Fact]
    public async Task Cancel_with_correct_token_cancels_and_frees_slot()
    {
        using var db = new TestDatabase();
        var (slotId, resvToken) = db.SeedReservedSlot();
        var email = new FakeEmailService();
        await CreateBooking(db, email, slotId, resvToken);
        await Confirm(db, email, email.LastConfirmationToken!);

        var result = await Cancel(db, email, email.LastCancellationToken!);

        Assert.Equal("Cancelled", result.Status);
        Assert.Equal(BookingStatus.Cancelled, db.GetBooking().Status);
        Assert.Equal(SlotStatus.Free, db.GetSlot(slotId).Status);
    }

    // ---- 13. Cancelling twice is safe (idempotent) --------------------------------------------
    [Fact]
    public async Task Cancel_is_idempotent()
    {
        using var db = new TestDatabase();
        var (slotId, resvToken) = db.SeedReservedSlot();
        var email = new FakeEmailService();
        await CreateBooking(db, email, slotId, resvToken);
        await Confirm(db, email, email.LastConfirmationToken!);
        var cancellationToken = email.LastCancellationToken!;

        await Cancel(db, email, cancellationToken);
        var cancelledAt = db.GetBooking().CancelledAtUtc;

        var second = await Cancel(db, email, cancellationToken);

        Assert.Equal("Cancelled", second.Status);
        Assert.Equal(cancelledAt, db.GetBooking().CancelledAtUtc);
        Assert.Equal(SlotStatus.Free, db.GetSlot(slotId).Status);
    }

    // ---- 14. The background sweep expires stale bookings and frees their slots -----------------
    [Fact]
    public async Task ExpirePendingBookings_frees_expired_slot()
    {
        using var db = new TestDatabase();
        var (slotId, resvToken) = db.SeedReservedSlot();
        var email = new FakeEmailService();
        await CreateBooking(db, email, slotId, resvToken);
        db.ExpireConfirmationWindow();

        int expired;
        using (var ctx = db.CreateContext())
        {
            var svc = db.CreateBookingService(ctx, email);
            expired = await svc.ExpirePendingBookingsAsync();
        }

        Assert.Equal(1, expired);
        Assert.Equal(BookingStatus.Expired, db.GetBooking().Status);
        Assert.Equal(SlotStatus.Free, db.GetSlot(slotId).Status);
    }

    // ---- 16. A first-e-mail failure must not leave the slot blocked ----------------------------
    [Fact]
    public async Task CreateBooking_first_email_failure_frees_slot()
    {
        using var db = new TestDatabase();
        var (slotId, resvToken) = db.SeedReservedSlot();
        var email = new FakeEmailService { ThrowOnConfirmationRequest = true };

        await Assert.ThrowsAsync<EmailDeliveryException>(
            () => CreateBooking(db, email, slotId, resvToken));

        Assert.Equal(SlotStatus.Free, db.GetSlot(slotId).Status);
        Assert.Equal(BookingStatus.Expired, db.GetBooking().Status);
    }

    // ---- A slot can be booked again after its previous booking expired -------------------------
    [Fact]
    public async Task Slot_can_be_rebooked_after_previous_booking_expires()
    {
        using var db = new TestDatabase();
        var (slotId, resvToken) = db.SeedReservedSlot();
        var email = new FakeEmailService();

        await CreateBooking(db, email, slotId, resvToken);
        db.ExpireConfirmationWindow();
        using (var ctx = db.CreateContext())
        {
            await db.CreateBookingService(ctx, email).ExpirePendingBookingsAsync();
        }

        // Slot is free again -> a brand new reservation + booking must succeed (no unique-key clash).
        var newResvToken = db.ReReserveSlot(slotId);
        await CreateBooking(db, email, slotId, newResvToken, name: "Second Patient", patientEmail: "second@example.com");

        using var verify = db.CreateContext();
        var bookings = verify.Bookings.Where(b => b.AppointmentSlotId == slotId).ToList();
        Assert.Equal(2, bookings.Count);
        Assert.Single(bookings, b => b.Status == BookingStatus.Expired);
        Assert.Single(bookings, b => b.Status == BookingStatus.PendingConfirmation);
    }

    // ---- Bonus: cancel-info never leaks e-mail / ids ------------------------------------------
    [Fact]
    public async Task CancelInfo_returns_safe_summary()
    {
        using var db = new TestDatabase();
        var (slotId, resvToken) = db.SeedReservedSlot();
        var email = new FakeEmailService();
        await CreateBooking(db, email, slotId, resvToken);
        await Confirm(db, email, email.LastConfirmationToken!);

        BookingSummaryDto summary;
        using (var ctx = db.CreateContext())
        {
            var svc = db.CreateBookingService(ctx, email);
            summary = await svc.GetCancellableBookingAsync(email.LastCancellationToken!);
        }

        Assert.True(summary.Cancellable);
        Assert.Equal("Dr. Smith", summary.DoctorName);
        Assert.Equal("Cardiology", summary.SpecialtyName);
    }

    // ---- helpers ------------------------------------------------------------------------------

    private static async Task CreateBooking(
        TestDatabase db,
        FakeEmailService email,
        Guid slotId,
        string reservationToken,
        string name = "John Doe",
        string patientEmail = "john@example.com",
        string? comment = null)
    {
        using var ctx = db.CreateContext();
        var svc = db.CreateBookingService(ctx, email);
        await svc.CreateBookingAsync(new CreateBookingDto
        {
            AppointmentSlotId = slotId,
            ReservationToken = reservationToken,
            PatientName = name,
            PatientEmail = patientEmail,
            Comment = comment
        });
    }

    private static async Task<ConfirmBookingResultDto> Confirm(TestDatabase db, FakeEmailService email, string token)
    {
        using var ctx = db.CreateContext();
        var svc = db.CreateBookingService(ctx, email);
        return await svc.ConfirmBookingAsync(token);
    }

    private static async Task<CancelBookingResultDto> Cancel(TestDatabase db, FakeEmailService email, string token)
    {
        using var ctx = db.CreateContext();
        var svc = db.CreateBookingService(ctx, email);
        return await svc.CancelBookingAsync(token);
    }
}
