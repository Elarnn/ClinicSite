import { apiGet } from './http';
import type { DoctorBooking } from '../types';

export function getMyBookings() {
  return apiGet<DoctorBooking[]>('/doctor/bookings');
}
