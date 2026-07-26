export interface SpecialtyDto {
  id: string;
  name: string;
}

export interface DoctorDto {
  id: string;
  fullName: string;
  specialtyId: string;
  specialtyName: string;
  isActive: boolean;
  hasPhoto: boolean;
  email?: string | null;
  accountStatus: string; // "None" | "Invited" | "Active"
}

export interface InviteDoctorDto {
  email: string;
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

export interface UpdateDoctorDto {
  fullName: string;
  specialtyId: string;
}

export interface AdminBookingDto {
  bookingId: string;
  appointmentSlotId: string;

  patientName: string;
  patientEmail: string;
  comment?: string | null;

  doctorId: string;
  doctorName: string;

  specialtyId: string;
  specialtyName: string;

  startTimeUtc: string;
  endTimeUtc: string;

  status: string;
  isCancelled: boolean;
  createdAtUtc: string;
}