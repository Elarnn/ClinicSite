import { apiFetch } from './client';
import type {
  BookingResultDto,
  BookingSummaryDto,
  CancelBookingResultDto,
  ConfirmBookingResultDto,
  CreateBookingDto,
} from '../types';

export function createBooking(dto: CreateBookingDto): Promise<BookingResultDto> {
  return apiFetch('/api/bookings', {
    method: 'POST',
    body: JSON.stringify(dto),
  });
}

export function confirmBooking(token: string): Promise<ConfirmBookingResultDto> {
  return apiFetch('/api/bookings/confirm', {
    method: 'POST',
    body: JSON.stringify({ token }),
  });
}

export function getCancelInfo(token: string): Promise<BookingSummaryDto> {
  return apiFetch('/api/bookings/cancel-info', {
    method: 'POST',
    body: JSON.stringify({ token }),
  });
}

export function cancelBooking(token: string): Promise<CancelBookingResultDto> {
  return apiFetch('/api/bookings/cancel', {
    method: 'POST',
    body: JSON.stringify({ token }),
  });
}
