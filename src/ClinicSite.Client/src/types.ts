export type Page = 'home' | 'about' | 'services' | 'contacts' | 'booking';

export interface SpecialtyDto {
  id: string;
  name: string;
}

export interface DoctorDto {
  id: string;
  fullName: string;
  specialtyName: string;
}

export interface AppointmentSlotDto {
  slotId: string;
  startTimeUtc: string;
  endTimeUtc: string;
  status: SlotStatus;
}

export enum SlotStatus {
  Free = 1,
  Reserved = 2,
  Booked = 3,
}

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
}
