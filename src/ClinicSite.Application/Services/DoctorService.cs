using ClinicSite.Application.DTOs.Doctors;
using ClinicSite.Application.Exceptions;
using ClinicSite.Application.Interfaces;
using ClinicSite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSite.Application.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IApplicationDbContext _context;

        public DoctorService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DoctorDto>> GetDoctorsAsync(Guid? specialtyId = null)
        {
            var query = _context.Doctors.Where(d => d.IsActive);

            if (specialtyId.HasValue)
                query = query.Where(d => d.SpecialtyId == specialtyId.Value);

            return await query
                .Select(d => new DoctorDto
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    SpecialtyId = d.SpecialtyId,
                    SpecialtyName = d.Specialty.Name,
                    IsActive = d.IsActive,
                    HasPhoto = d.Photo != null
                })
                .ToListAsync();
        }

        public async Task<List<DoctorDto>> GetAllDoctorsAsync()
        {
            // Admin view: includes account email + status. (The public GetDoctorsAsync omits these.)
            return await _context.Doctors
                .OrderBy(d => d.FullName)
                .Select(d => new DoctorDto
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    SpecialtyId = d.SpecialtyId,
                    SpecialtyName = d.Specialty.Name,
                    IsActive = d.IsActive,
                    HasPhoto = d.Photo != null,
                    Email = d.Email,
                    AccountStatus = d.AccountStatus.ToString()
                })
                .ToListAsync();
        }

        public async Task<DoctorDto> CreateDoctorAsync(CreateDoctorDto request)
        {
            var fullName = request.FullName.Trim();

            if (string.IsNullOrEmpty(fullName))
            {
                throw new ValidationException("The doctor's name cannot be empty.");
            }

            var specialty = await _context.Specialties
                .FirstOrDefaultAsync(s => s.Id == request.SpecialtyId);

            if (specialty is null)
            {
                throw new ValidationException("The specified specialty was not found.");
            }

            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                SpecialtyId = specialty.Id,
                IsActive = true
            };

            _context.Doctors.Add(doctor);

            await _context.SaveChangesAsync();

            return new DoctorDto
            {
                Id = doctor.Id,
                FullName = doctor.FullName,
                SpecialtyId = specialty.Id,
                SpecialtyName = specialty.Name,
                IsActive = doctor.IsActive,
                HasPhoto = doctor.Photo != null,
                Email = doctor.Email,
                AccountStatus = doctor.AccountStatus.ToString()
            };
        }

        public async Task<DoctorDto?> UpdateDoctorAsync(Guid doctorId, UpdateDoctorDto request)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor is null)
            {
                return null;
            }

            var fullName = request.FullName.Trim();

            if (string.IsNullOrEmpty(fullName))
            {
                throw new ValidationException("The doctor's name cannot be empty.");
            }

            var specialty = await _context.Specialties
                .FirstOrDefaultAsync(s => s.Id == request.SpecialtyId);

            if (specialty is null)
            {
                throw new ValidationException("The specified specialty was not found.");
            }

            doctor.FullName = fullName;
            doctor.SpecialtyId = specialty.Id;

            await _context.SaveChangesAsync();

            return new DoctorDto
            {
                Id = doctor.Id,
                FullName = doctor.FullName,
                SpecialtyId = specialty.Id,
                SpecialtyName = specialty.Name,
                IsActive = doctor.IsActive,
                HasPhoto = doctor.Photo != null,
                Email = doctor.Email,
                AccountStatus = doctor.AccountStatus.ToString()
            };
        }

        public async Task<DoctorDto?> DeactivateDoctorAsync(Guid doctorId)
        {
            return await SetActiveAsync(doctorId, isActive: false);
        }

        public async Task<DoctorDto?> ActivateDoctorAsync(Guid doctorId)
        {
            return await SetActiveAsync(doctorId, isActive: true);
        }

        public async Task<bool> DeleteDoctorAsync(Guid doctorId)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor is null)
            {
                return false;
            }

            var hasSlots = await _context.AppointmentSlots
                .AnyAsync(s => s.DoctorId == doctorId);

            if (hasSlots)
            {
                throw new ConflictException(
                    "Cannot delete a doctor who has appointment slots. Deactivate the doctor first.");
            }

            _context.Doctors.Remove(doctor);

            await _context.SaveChangesAsync();

            return true;
        }

        private const int MaxPhotoBytes = 3 * 1024 * 1024; // 3 MB

        private static readonly HashSet<string> AllowedPhotoTypes =
            new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp", "image/gif" };

        public async Task SetPhotoAsync(Guid doctorId, byte[] data, string contentType)
        {
            if (data is null || data.Length == 0)
            {
                throw new ValidationException("No image was provided.");
            }
            if (data.Length > MaxPhotoBytes)
            {
                throw new ValidationException("Image is too large (maximum 3 MB).");
            }
            if (string.IsNullOrWhiteSpace(contentType) || !AllowedPhotoTypes.Contains(contentType))
            {
                throw new ValidationException("Unsupported image type. Use JPEG, PNG, WebP or GIF.");
            }

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
            if (doctor is null)
            {
                throw new NotFoundException("Doctor not found.");
            }

            doctor.Photo = data;
            doctor.PhotoContentType = contentType;
            await _context.SaveChangesAsync();
        }

        public async Task<DoctorPhoto?> GetPhotoAsync(Guid doctorId)
        {
            // Load the blob only for this one doctor (never in the list/detail projections).
            var row = await _context.Doctors
                .AsNoTracking()
                .Where(d => d.Id == doctorId && d.Photo != null)
                .Select(d => new { d.Photo, d.PhotoContentType })
                .FirstOrDefaultAsync();

            if (row?.Photo is null)
            {
                return null;
            }

            return new DoctorPhoto(row.Photo, row.PhotoContentType ?? "application/octet-stream");
        }

        public async Task RemovePhotoAsync(Guid doctorId)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
            if (doctor is null)
            {
                throw new NotFoundException("Doctor not found.");
            }

            doctor.Photo = null;
            doctor.PhotoContentType = null;
            await _context.SaveChangesAsync();
        }

        private async Task<DoctorDto?> SetActiveAsync(Guid doctorId, bool isActive)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor is null)
            {
                return null;
            }

            doctor.IsActive = isActive;

            await _context.SaveChangesAsync();

            return new DoctorDto
            {
                Id = doctor.Id,
                FullName = doctor.FullName,
                SpecialtyId = doctor.SpecialtyId,
                SpecialtyName = doctor.Specialty.Name,
                IsActive = doctor.IsActive,
                HasPhoto = doctor.Photo != null,
                Email = doctor.Email,
                AccountStatus = doctor.AccountStatus.ToString()
            };
        }
    }
}
