using ClinicSite.Application.DTOs.Bookings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSite.Application.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResultDto> CreateBookingAsync(CreateBookingDto dto);
    }
}
