export type Page = 'home' | 'about' | 'services' | 'contacts' | 'booking';

export interface SpecialtyDto {
  id: string;
  name: string;
}

export interface DoctorDto {
  id: string;
  fullName: string;
  specialtyName: string;
  hasPhoto: boolean;
}

export interface AppointmentSlotDto {
  slotId: string;
  startTimeUtc: string;
  endTimeUtc: string;
  status: SlotStatus;
}

// Const object instead of a TS enum so the code stays erasable (erasableSyntaxOnly is on).
export const SlotStatus = {
  Free: 1,
  Reserved: 2,
  Booked: 3,
} as const;

export type SlotStatus = (typeof SlotStatus)[keyof typeof SlotStatus];

export interface ReserveSlotResultDto {
  slotId: string;
  reservedUntilUtc: string | null;
  reservationToken: string | null;
}

export interface CreateBookingDto {
  appointmentSlotId: string;
  reservationToken: string;
  patientName: string;
  patientEmail: string;
  comment?: string;
}

export interface BookingResultDto {
  bookingId: string;
  appointmentSlotId: string;
  patientName: string;
  patientEmail: string;
  comment?: string;
  doctorName: string;
  startTimeUtc: string;
  endTimeUtc: string;
  createdAtUtc: string;
  status: string;
  confirmationExpiresAtUtc?: string | null;
}

export interface ConfirmBookingResultDto {
  patientName: string;
  doctorName: string;
  specialtyName: string;
  startTimeUtc: string;
  endTimeUtc: string;
  comment?: string | null;
  status: string;
}

export interface BookingSummaryDto {
  doctorName: string;
  specialtyName: string;
  startTimeUtc: string;
  endTimeUtc: string;
  status: string;
  cancellable: boolean;
}

export interface CancelBookingResultDto {
  doctorName: string;
  specialtyName: string;
  startTimeUtc: string;
  endTimeUtc: string;
  status: string;
}
