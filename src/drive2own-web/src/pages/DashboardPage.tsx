import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { dashboard as dashboardApi } from '../api';
import { DashboardData } from '../types';
import { useAuth } from '../contexts/AuthContext';

function formatDistance(meters: number) {
  if (meters >= 1000) return `${(meters / 1000).toFixed(2)} km`;
  return `${Math.round(meters)} m`;
}

function formatDuration(seconds: number) {
  const h = Math.floor(seconds / 3600);
  const m = Math.floor((seconds % 3600) / 60);
  if (h > 0) return `${h}h ${m}m`;
  return `${m}m`;
}

const modeEmoji: Record<string, string> = { Car: '🚗', Walk: '🚶', Cycle: '🚴', Run: '🏃' };

export default function DashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const { user } = useAuth();

  useEffect(() => {
    dashboardApi.get()
      .then(res => setData(res.data))
      .catch(() => {})
      .then(() => setLoading(false));
  }, []);

  if (loading) return <div className="loading">Loading dashboard...</div>;

  return (
    <div className="page-content">
      <h1 className="page-title">Welcome back, {user?.displayName}! 👋</h1>

      {data && (
        <>
          <div className="stats-grid">
            <div className="stat-card">
              <div className="stat-icon">🛣️</div>
              <div className="stat-value">{data.totalRoutes}</div>
              <div className="stat-label">Total Routes</div>
            </div>
            <div className="stat-card">
              <div className="stat-icon">📍</div>
              <div className="stat-value">{data.totalPins}</div>
              <div className="stat-label">Location Pins</div>
            </div>
            <div className="stat-card">
              <div className="stat-icon">📏</div>
              <div className="stat-value">{formatDistance(data.totalDistanceMeters)}</div>
              <div className="stat-label">Total Distance</div>
            </div>
            <div className="stat-card">
              <div className="stat-icon">⏱️</div>
              <div className="stat-value">{formatDuration(data.totalDurationSeconds)}</div>
              <div className="stat-label">Total Time</div>
            </div>
          </div>

          {Object.keys(data.distanceByMode).length > 0 && (
            <div className="card">
              <h2>Distance by Mode</h2>
              <div className="mode-bars">
                {Object.entries(data.distanceByMode).map(([mode, dist]) => (
                  <div key={mode} className="mode-bar-item">
                    <span>{modeEmoji[mode] || '🚶'} {mode}</span>
                    <div className="bar-track">
                      <div className="bar-fill" style={{ width: `${Math.min(100, (dist / data.totalDistanceMeters) * 100)}%` }} />
                    </div>
                    <span className="bar-label">{formatDistance(dist)}</span>
                  </div>
                ))}
              </div>
            </div>
          )}

          <div className="card">
            <div className="card-header">
              <h2>Recent Routes</h2>
              <Link to="/routes" className="btn btn-sm btn-secondary">View All</Link>
            </div>
            {data.recentRoutes.length === 0 ? (
              <div className="empty-state">
                <p>No routes yet. <Link to="/map">Start tracking!</Link></p>
              </div>
            ) : (
              <div className="route-list">
                {data.recentRoutes.map(route => (
                  <div key={route.id} className="route-item">
                    <div className="route-icon">{modeEmoji[route.travelMode] || '🚶'}</div>
                    <div className="route-info">
                      <div className="route-name">{route.name}</div>
                      <div className="route-meta">
                        {formatDistance(route.distanceMeters)} · {formatDuration(route.durationSeconds)} · {new Date(route.startedAt).toLocaleDateString()}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
}
