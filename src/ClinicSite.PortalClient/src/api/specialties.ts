import { apiDelete, apiGet, apiPost, apiPut } from './http';
import type { CreateSpecialtyDto, SpecialtyDto, UpdateSpecialtyDto } from '../types';

export function getSpecialties() {
  return apiGet<SpecialtyDto[]>('/specialties');
}

export function createSpecialty(dto: CreateSpecialtyDto) {
  return apiPost<SpecialtyDto, CreateSpecialtyDto>('/admin/specialties', dto);
}

export function updateSpecialty(id: string, dto: UpdateSpecialtyDto) {
  return apiPut<SpecialtyDto, UpdateSpecialtyDto>(`/admin/specialties/${id}`, dto);
}

export function deleteSpecialty(id: string) {
  return apiDelete(`/admin/specialties/${id}`);
}
