import { apiGet, apiPost } from './http';
import type { AdminBookingDto } from '../types';

export function getBookings() {
  return apiGet<AdminBookingDto[]>('/admin/bookings');
}

export function cancelBooking(bookingId: string) {
  return apiPost<void, null>(`/admin/bookings/${bookingId}/cancel`, null);
}