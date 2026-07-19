import { apiGet } from './http';
import type { SpecialtyDto } from '../types';

export function getSpecialties() {
  return apiGet<SpecialtyDto[]>('/specialties');
}