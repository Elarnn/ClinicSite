import { apiPost } from './http';
import type { DoctorInviteInfo, DoctorLoginResult } from '../types';

export function getInviteInfo(token: string) {
  return apiPost<DoctorInviteInfo, { token: string }>('/doctor/auth/invite-info', { token });
}

export function setPassword(token: string, password: string) {
  return apiPost<DoctorInviteInfo, { token: string; password: string }>(
    '/doctor/auth/set-password',
    { token, password },
  );
}

export function login(email: string, password: string) {
  return apiPost<DoctorLoginResult, { email: string; password: string }>(
    '/doctor/auth/login',
    { email, password },
  );
}
