import { apiFetch } from './client';
import type { BookingResultDto, CreateBookingDto } from '../types';

export function createBooking(dto: CreateBookingDto): Promise<BookingResultDto> {
  return apiFetch('/api/bookings', {
    method: 'POST',
    body: JSON.stringify(dto),
  });
}
