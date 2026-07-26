export interface DoctorLoginResult {
  token: string;
  doctorName: string;
  expiresAtUtc: string;
}

export interface DoctorInviteInfo {
  doctorName: string;
  email: string;
}

export type AppointmentStatus =
  | 'Scheduled'
  | 'CheckedIn'
  | 'InProgress'
  | 'Completed'
  | 'NoShow'
  | 'Cancelled';

export interface ScheduleItem {
  slotId: string;
  startTimeUtc: string;
  endTimeUtc: string;
  slotStatus: string; // "Free" | "Reserved" | "Booked" | "Blocked"
  recurringBlocked: boolean; // this time-of-day is blocked on future days (daily block active)
  bookingId: string | null;
  patientName: string | null;
  patientEmail: string | null;
  patientComment: string | null;
  doctorNote: string | null;
  status: AppointmentStatus | null;
}

export interface Dashboard {
  todayCount: number;
  remainingCount: number;
  nextPatientStartUtc: string | null;
  freeWindowsCount: number;
  today: ScheduleItem[];
}

export interface DoctorBooking {
  bookingId: string;
  appointmentSlotId: string;
  patientName: string;
  patientEmail: string;
  patientComment: string | null;
  doctorNote: string | null;
  startTimeUtc: string;
  endTimeUtc: string;
  status: AppointmentStatus;
}

export interface Paged<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface PatientHistoryItem {
  startTimeUtc: string;
  endTimeUtc: string;
  doctorName: string;
  specialtyName: string;
  status: AppointmentStatus;
}

export interface BookingFilter {
  from?: string;
  to?: string;
  status?: AppointmentStatus;
  search?: string;
  page: number;
  pageSize: number;
}
