import { apiPatch, apiPost } from './http';

// Single slot (Week view).
export function blockSlot(slotId: string) {
  return apiPatch<void, null>(`/doctor/slots/${slotId}/block`, null);
}

export function unblockSlot(slotId: string) {
  return apiPatch<void, null>(`/doctor/slots/${slotId}/unblock`, null);
}

// Recurring — same time-of-day on every future day (Today view).
export function blockRecurring(slotId: string) {
  return apiPost<{ blocked: number }, null>(`/doctor/slots/${slotId}/block-recurring`, null);
}

export function unblockRecurring(slotId: string) {
  return apiPost<{ unblocked: number }, null>(`/doctor/slots/${slotId}/unblock-recurring`, null);
}
