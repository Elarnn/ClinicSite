import { apiDelete, apiGet, apiPost, apiPut } from './http';
import type { CreateDoctorDto, DoctorDto, InviteDoctorDto, UpdateDoctorDto } from '../types';

export function getDoctors() {
  return apiGet<DoctorDto[]>('/doctors');
}

export function getAdminDoctors() {
  return apiGet<DoctorDto[]>('/admin/doctors');
}

export function createDoctor(dto: CreateDoctorDto) {
  return apiPost<DoctorDto, CreateDoctorDto>('/admin/doctors', dto);
}

export function updateDoctor(id: string, dto: UpdateDoctorDto) {
  return apiPut<DoctorDto, UpdateDoctorDto>(`/admin/doctors/${id}`, dto);
}

export function deactivateDoctor(id: string) {
  return apiPost<DoctorDto, null>(`/admin/doctors/${id}/deactivate`, null);
}

export function activateDoctor(id: string) {
  return apiPost<DoctorDto, null>(`/admin/doctors/${id}/activate`, null);
}

export function deleteDoctor(id: string) {
  return apiDelete(`/admin/doctors/${id}`);
}

export function inviteDoctor(id: string, email: string) {
  return apiPost<DoctorDto, InviteDoctorDto>(`/admin/doctors/${id}/invite`, { email });
}
