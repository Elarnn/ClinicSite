import { apiFetch } from './client';
import type { DoctorDto } from '../types';

export function getDoctors(specialtyId?: string): Promise<DoctorDto[]> {
  const query = specialtyId ? `?specialtyId=${specialtyId}` : '';
  return apiFetch(`/api/doctors${query}`);
}
