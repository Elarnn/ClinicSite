using ClinicSite.Application.Exceptions;
using ClinicSite.Domain.Enums;
using ClinicSite.Tests.TestSupport;

namespace ClinicSite.Tests;

public class DoctorAccountServiceTests
{
    [Fact]
    public async Task Invite_set_password_and_login_full_flow()
    {
        using var db = new TestDatabase();
        var email = new FakeEmailService();
        var doctorId = db.SeedDoctor("Dr. House");

        using (var ctx = db.CreateContext())
        {
            var dto = await db.CreateDoctorAccountService(ctx, email).InviteAsync(doctorId, "house@example.com");
            Assert.Equal("Invited", dto.AccountStatus);
            Assert.Equal("house@example.com", dto.Email);
        }

        Assert.Equal(1, email.DoctorInviteCount);
        var token = email.LastInviteToken!;

        using (var ctx = db.CreateContext())
        {
            var info = await db.CreateDoctorAccountService(ctx, email).GetInviteInfoAsync(token);
            Assert.Equal("Dr. House", info.DoctorName);
            Assert.Equal("house@example.com", info.Email);
        }

        using (var ctx = db.CreateContext())
        {
            await db.CreateDoctorAccountService(ctx, email).SetPasswordAsync(token, "s3cret-password");
        }

        var stored = db.GetDoctor(doctorId);
        Assert.Equal(DoctorAccountStatus.Active, stored.AccountStatus);
        Assert.Null(stored.InviteTokenHash); // single-use: token consumed
        Assert.NotNull(stored.PasswordHash);

        using (var ctx = db.CreateContext())
        {
            var svc = db.CreateDoctorAccountService(ctx, email);
            Assert.NotNull(await svc.AuthenticateAsync("house@example.com", "s3cret-password"));
            Assert.Null(await svc.AuthenticateAsync("house@example.com", "wrong-password"));
        }
    }

    [Fact]
    public async Task Invite_email_failure_rolls_back_the_invitation()
    {
        using var db = new TestDatabase();
        var email = new FakeEmailService { ThrowOnDoctorInvite = true };
        var doctorId = db.SeedDoctor();

        using (var ctx = db.CreateContext())
        {
            var svc = db.CreateDoctorAccountService(ctx, email);
            await Assert.ThrowsAsync<EmailDeliveryException>(() => svc.InviteAsync(doctorId, "x@example.com"));
        }

        var doctor = db.GetDoctor(doctorId);
        Assert.Equal(DoctorAccountStatus.None, doctor.AccountStatus);
        Assert.Null(doctor.Email);
        Assert.Null(doctor.InviteTokenHash);
    }

    [Fact]
    public async Task Expired_invite_is_rejected()
    {
        using var db = new TestDatabase();
        var email = new FakeEmailService();
        var doctorId = db.SeedDoctor();

        using (var ctx = db.CreateContext())
        {
            await db.CreateDoctorAccountService(ctx, email).InviteAsync(doctorId, "x@example.com");
        }

        var token = email.LastInviteToken!;
        db.ExpireDoctorInvite(doctorId);

        using (var ctx = db.CreateContext())
        {
            var svc = db.CreateDoctorAccountService(ctx, email);
            await Assert.ThrowsAsync<GoneException>(() => svc.GetInviteInfoAsync(token));
            await Assert.ThrowsAsync<GoneException>(() => svc.SetPasswordAsync(token, "s3cret-password"));
        }
    }

    [Fact]
    public async Task Invalid_token_is_rejected()
    {
        using var db = new TestDatabase();
        using var ctx = db.CreateContext();
        var svc = db.CreateDoctorAccountService(ctx, new FakeEmailService());

        await Assert.ThrowsAsync<NotFoundException>(() => svc.SetPasswordAsync("not-a-real-token", "s3cret-password"));
    }

    [Fact]
    public async Task Short_password_is_rejected()
    {
        using var db = new TestDatabase();
        var email = new FakeEmailService();
        var doctorId = db.SeedDoctor();

        using (var ctx = db.CreateContext())
        {
            await db.CreateDoctorAccountService(ctx, email).InviteAsync(doctorId, "x@example.com");
        }

        using (var ctx = db.CreateContext())
        {
            var svc = db.CreateDoctorAccountService(ctx, email);
            await Assert.ThrowsAsync<ValidationException>(() => svc.SetPasswordAsync(email.LastInviteToken!, "short"));
        }
    }

    [Fact]
    public async Task Inviting_an_already_active_doctor_conflicts()
    {
        using var db = new TestDatabase();
        var email = new FakeEmailService();
        var doctorId = db.SeedDoctor();

        using (var ctx = db.CreateContext())
        {
            await db.CreateDoctorAccountService(ctx, email).InviteAsync(doctorId, "x@example.com");
        }
        using (var ctx = db.CreateContext())
        {
            await db.CreateDoctorAccountService(ctx, email).SetPasswordAsync(email.LastInviteToken!, "s3cret-password");
        }

        using (var ctx = db.CreateContext())
        {
            var svc = db.CreateDoctorAccountService(ctx, email);
            await Assert.ThrowsAsync<ConflictException>(() => svc.InviteAsync(doctorId, "other@example.com"));
        }
    }
}
