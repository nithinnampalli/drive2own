import apiClient from './client';
import { AuthResponse, DashboardData, LocationPin, Route, RouteShare, SharedRoute, User } from '../types';

export const auth = {
  register: (data: { username: string; email: string; password: string; displayName: string }) =>
    apiClient.post<AuthResponse>('/auth/register', data),
  login: (data: { email: string; password: string }) =>
    apiClient.post<AuthResponse>('/auth/login', data),
};

export const users = {
  getProfile: () => apiClient.get<User>('/users/me'),
  updateProfile: (data: { displayName: string; avatarUrl?: string }) =>
    apiClient.put<User>('/users/me', data),
};

export const locationPins = {
  getAll: () => apiClient.get<LocationPin[]>('/locationpins'),
  create: (data: { title: string; description?: string; latitude: number; longitude: number; iconColor?: string }) =>
    apiClient.post<LocationPin>('/locationpins', data),
  update: (id: number, data: { title: string; description?: string; iconColor?: string }) =>
    apiClient.put<LocationPin>(`/locationpins/${id}`, data),
  delete: (id: number) => apiClient.delete(`/locationpins/${id}`),
};

export const routes = {
  getAll: () => apiClient.get<Route[]>('/routes'),
  getById: (id: number) => apiClient.get<Route>(`/routes/${id}`),
  create: (data: { name: string; description?: string; travelMode: string; startedAt: string }) =>
    apiClient.post<Route>('/routes', data),
  complete: (id: number, data: { completedAt: string; totalDistanceMeters: number; totalDurationSeconds: number }) =>
    apiClient.post<Route>(`/routes/${id}/complete`, data),
  addPoints: (id: number, points: { latitude: number; longitude: number; altitude?: number; speedMps?: number; sequence: number; recordedAt: string }[]) =>
    apiClient.post(`/routes/${id}/points/batch`, points),
  delete: (id: number) => apiClient.delete(`/routes/${id}`),
};

export const dashboard = {
  get: () => apiClient.get<DashboardData>('/dashboard'),
};

export const shares = {
  shareRoute: (routeId: number, data: { isPublic: boolean; expiresInHours?: number }) =>
    apiClient.post<RouteShare>(`/shares/routes/${routeId}`, data),
  getSharedRoute: (token: string) =>
    apiClient.get<SharedRoute>(`/shares/${token}`),
  getMyShares: () => apiClient.get<RouteShare[]>('/shares/my-shares'),
  deleteShare: (id: number) => apiClient.delete(`/shares/${id}`),
};
