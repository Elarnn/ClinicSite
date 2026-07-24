using System.Threading.RateLimiting;
using ClinicSite.Api;
using ClinicSite.Api.BackgroundServices;
using ClinicSite.Api.Middleware;
using ClinicSite.Application.Common;
using ClinicSite.Application.Interfaces;
using ClinicSite.Application.Services;
using ClinicSite.Infrastructure.Data;
using ClinicSite.Infrastructure.Email;
using ClinicSite.Infrastructure.Seed;
using CliniqueSite.Application.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
	options.AddPolicy("ReactClients", policy =>
	{
		policy
			.WithOrigins(
				"http://localhost:5173",
				"http://localhost:5174"
			)
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var conString = builder.Configuration.GetConnectionString("ClinicSiteDb") ??
     throw new InvalidOperationException("Connection string 'ClinicSiteDb'" +
    " not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(conString));

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<AppDbContext>());

// --- Booking confirmation e-mail (SMTP / Gmail) ---
builder.Services.Configure<BookingOptions>(
    builder.Configuration.GetSection(BookingOptions.SectionName));
builder.Services.Configure<SmtpEmailOptions>(
    builder.Configuration.GetSection(SmtpEmailOptions.SectionName));

// SMTP delivery via Gmail. The App Password comes from User Secrets (Email:SmtpPassword),
// never from appsettings.json.
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

builder.Services.AddScoped<ISpecialtyService, SpecialtyService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAppointmentSlotService, AppointmentSlotService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAdminBookingService, AdminBookingService>();
builder.Services.AddScoped<IAdminSpecialtyService, AdminSpecialtyService>();

// Background sweep that expires unconfirmed bookings and frees their slots.
builder.Services.AddHostedService<BookingExpirationBackgroundService>();

// Basic abuse protection on the public booking endpoints (per client IP, fixed window).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(RateLimitPolicies.CreateBooking, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.AddPolicy(RateLimitPolicies.ConfirmCancel, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await context.Database.MigrateAsync();

    await DbSeeder.SeedAsync(context);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("ReactClients");

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposed so WebApplicationFactory-based integration tests can reference the entry point.
public partial class Program { }
