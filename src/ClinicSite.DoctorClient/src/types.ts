export interface DoctorLoginResult {
  token: string;
  doctorName: string;
  expiresAtUtc: string;
}

export interface DoctorInviteInfo {
  doctorName: string;
  email: string;
}

export interface DoctorBooking {
  patientName: string;
  patientEmail: string;
  comment: string | null;
  startTimeUtc: string;
  endTimeUtc: string;
  status: string;
}
