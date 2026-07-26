import { apiGet, apiPatch, apiPost, apiPut } from './http';
import type {
  AppointmentStatus,
  BookingFilter,
  Dashboard,
  DoctorBooking,
  Paged,
  PatientHistoryItem,
  ScheduleItem,
} from '../types';

export function getDashboard() {
  return apiGet<Dashboard>('/doctor/dashboard');
}

export function getSchedule(fromUtc: string, toUtc: string) {
  const q = `from=${encodeURIComponent(fromUtc)}&to=${encodeURIComponent(toUtc)}`;
  return apiGet<ScheduleItem[]>(`/doctor/schedule?${q}`);
}

export function getBookings(filter: BookingFilter) {
  const params = new URLSearchParams();
  if (filter.from) params.set('from', filter.from);
  if (filter.to) params.set('to', filter.to);
  if (filter.status) params.set('status', filter.status);
  if (filter.search) params.set('search', filter.search);
  params.set('page', String(filter.page));
  params.set('pageSize', String(filter.pageSize));
  return apiGet<Paged<DoctorBooking>>(`/doctor/bookings?${params.toString()}`);
}

export function updateStatus(bookingId: string, status: AppointmentStatus) {
  return apiPatch<void, { status: AppointmentStatus }>(`/doctor/bookings/${bookingId}/status`, { status });
}

export function updateNote(bookingId: string, note: string) {
  return apiPut<{ note: string | null }, { note: string }>(`/doctor/bookings/${bookingId}/note`, { note });
}

export function getPatientHistory(bookingId: string) {
  return apiGet<PatientHistoryItem[]>(`/doctor/bookings/${bookingId}/patient-history`);
}

export function sendPatientMessage(bookingId: string, subject: string, message: string) {
  return apiPost<void, { subject: string; message: string }>(
    `/doctor/bookings/${bookingId}/send-message`,
    { subject, message },
  );
}
