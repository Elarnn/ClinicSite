import { apiGet } from './http';
import type { DoctorDto } from '../types';

export function getDoctors() {
  return apiGet<DoctorDto[]>('/doctors');
}