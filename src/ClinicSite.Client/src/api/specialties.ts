import { apiFetch } from './client';
import type { SpecialtyDto } from '../types';

export function getSpecialties(): Promise<SpecialtyDto[]> {
  return apiFetch('/api/specialties');
}
