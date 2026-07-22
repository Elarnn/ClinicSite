import { apiGet, apiPost } from './http';
import type { CreateDoctorDto, DoctorDto } from '../types';

export function getDoctors() {
  return apiGet<DoctorDto[]>('/doctors');
}

export function createDoctor(dto: CreateDoctorDto) {
  return apiPost<DoctorDto, CreateDoctorDto>('/admin/doctors', dto);
}
