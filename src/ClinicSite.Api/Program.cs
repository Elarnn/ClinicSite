using ClinicSite.Api.Middleware;
using ClinicSite.Application.Interfaces;
using ClinicSite.Application.Interfaces;
using ClinicSite.Application.Services;
using ClinicSite.Application.Services;
using ClinicSite.Infrastructure.Data;
using ClinicSite.Infrastructure.Seed;
using CliniqueSite.Application.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

builder.Services.AddScoped<ISpecialtyService, SpecialtyService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAppointmentSlotService, AppointmentSlotService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAdminBookingService, AdminBookingService>();
builder.Services.AddScoped<IAdminSpecialtyService, AdminSpecialtyService>();

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

app.UseCors("ReactClients");

app.UseAuthorization();

app.MapControllers();

app.Run();