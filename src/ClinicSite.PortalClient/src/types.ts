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

export interface CreateSpecialtyDto {
  name: string;
}

export interface UpdateSpecialtyDto {
  name: string;
}

export interface CreateDoctorDto {
  fullName: string;
  specialtyId: string;
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