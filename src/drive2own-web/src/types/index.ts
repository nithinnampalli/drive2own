export interface User {
  id: number;
  username: string;
  email: string;
  displayName: string;
  avatarUrl?: string;
  createdAt: string;
}

export interface AuthResponse {
  token: string;
  user: User;
}

export interface LocationPin {
  id: number;
  userId: number;
  title: string;
  description?: string;
  latitude: number;
  longitude: number;
  iconColor?: string;
  createdAt: string;
}

export interface RoutePoint {
  id: number;
  latitude: number;
  longitude: number;
  altitude?: number;
  speedMps?: number;
  sequence: number;
  recordedAt: string;
}

export interface Route {
  id: number;
  userId: number;
  name: string;
  description?: string;
  travelMode: string;
  totalDistanceMeters: number;
  totalDurationSeconds: number;
  startedAt: string;
  completedAt?: string;
  isCompleted: boolean;
  createdAt: string;
  points: RoutePoint[];
}

export interface DashboardData {
  totalRoutes: number;
  totalDistanceMeters: number;
  totalDurationSeconds: number;
  totalPins: number;
  recentRoutes: RouteMetric[];
  distanceByMode: Record<string, number>;
}

export interface RouteMetric {
  id: number;
  name: string;
  travelMode: string;
  distanceMeters: number;
  durationSeconds: number;
  startedAt: string;
}

export interface RouteShare {
  id: number;
  routeId: number;
  shareToken: string;
  isPublic: boolean;
  createdAt: string;
  expiresAt?: string;
  routeUrl: string;
}

export interface SharedRoute {
  route: Route;
  sharedBy: string;
  sharedAt: string;
}
