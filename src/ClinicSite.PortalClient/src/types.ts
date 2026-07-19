export interface SpecialtyDto {
  id: string;
  name: string;
}

export interface DoctorDto {
  id: string;
  fullName: string;
  specialtyName: string;
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

export interface AdminBookingDto {
  bookingId: string;
  appointmentSlotId: string;

  patientName: string;
  patientEmail: string;
  comment?: string | null;

  doctorName: string;

  startTimeUtc: string;
  endTimeUtc: string;

  isCancelled: boolean;
  createdAtUtc: string;
}