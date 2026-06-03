using CliniqueSite.Application.DTOs.Bookings;
using CliniqueSite.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliniqueSite.Application.Services;

public class BookingService : IBookingService
{
    private readonly IApplicationDbContext _context;
    public BookingService(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<BookingResultDto> CreateBookingAsync(CreateBookingDto dto)
    {
        
    }
}