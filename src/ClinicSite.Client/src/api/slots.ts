import { apiFetch } from './client';
import type { AppointmentSlotDto, ReserveSlotResultDto } from '../types';

export function getFreeSlots(doctorId: string): Promise<AppointmentSlotDto[]> {
  return apiFetch(`/api/appointmentslots/free?doctorId=${doctorId}`);
}

export function getAllSlots(doctorId: string): Promise<AppointmentSlotDto[]> {
  return apiFetch(`/api/appointmentslots/all?doctorId=${doctorId}`);
}

export function reserveSlot(slotId: string): Promise<ReserveSlotResultDto> {
  return apiFetch(`/api/appointmentslots/${slotId}/reserve`, { method: 'POST' });
}
