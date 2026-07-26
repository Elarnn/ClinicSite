import { API_BASE_URL, apiDelete, apiGet, apiPost, apiPut } from './http';
import type { CreateDoctorDto, DoctorDto, InviteDoctorDto, UpdateDoctorDto } from '../types';

export function doctorPhotoUrl(id: string): string {
  return `${API_BASE_URL}/doctors/${id}/photo`;
}

export async function uploadDoctorPhoto(id: string, file: File): Promise<void> {
  const form = new FormData();
  form.append('file', file);

  const res = await fetch(`${API_BASE_URL}/admin/doctors/${id}/photo`, { method: 'POST', body: form });
  if (!res.ok) {
    const text = await res.text().catch(() => '');
    let message = `Upload failed: ${res.status}`;
    try {
      const parsed = JSON.parse(text) as { message?: string };
      if (parsed?.message) message = parsed.message;
    } catch {
      // not JSON — keep the generic message
    }
    throw new Error(message);
  }
}

export function removeDoctorPhoto(id: string) {
  return apiDelete(`/admin/doctors/${id}/photo`);
}

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
