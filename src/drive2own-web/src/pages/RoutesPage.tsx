import React, { useEffect, useState } from 'react';
import { routes as routesApi, shares } from '../api';
import { Route, RouteShare } from '../types';

function formatDistance(meters: number) {
  return meters >= 1000 ? `${(meters / 1000).toFixed(2)} km` : `${Math.round(meters)} m`;
}

function formatDuration(seconds: number) {
  const h = Math.floor(seconds / 3600);
  const m = Math.floor((seconds % 3600) / 60);
  return h > 0 ? `${h}h ${m}m` : `${m}m`;
}

const modeEmoji: Record<string, string> = { Car: '🚗', Walk: '🚶', Cycle: '🚴', Run: '🏃' };

export default function RoutesPage() {
  const [routeList, setRouteList] = useState<Route[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedRoute, setSelectedRoute] = useState<Route | null>(null);
  const [shareResult, setShareResult] = useState<RouteShare | null>(null);
  const [sharing, setSharing] = useState(false);
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    routesApi.getAll()
      .then(res => setRouteList(res.data))
      .catch(() => {})
      .then(() => setLoading(false));
  }, []);

  const handleDelete = async (id: number) => {
    if (!window.confirm('Delete this route?')) return;
    await routesApi.delete(id);
    setRouteList(r => r.filter(x => x.id !== id));
    if (selectedRoute?.id === id) setSelectedRoute(null);
  };

  const handleShare = async (route: Route) => {
    setSharing(true);
    try {
      const res = await shares.shareRoute(route.id, { isPublic: true });
      setShareResult(res.data);
    } finally {
      setSharing(false);
    }
  };

  const copyShareUrl = () => {
    if (shareResult) {
      navigator.clipboard.writeText(shareResult.routeUrl);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  if (loading) return <div className="loading">Loading routes...</div>;

  return (
    <div className="page-content">
      <h1 className="page-title">My Routes</h1>

      {routeList.length === 0 ? (
        <div className="empty-state card">
          <div className="empty-icon">🗺️</div>
          <h3>No routes yet</h3>
          <p>Head to the map and start recording your first route!</p>
        </div>
      ) : (
        <div className="routes-layout">
          <div className="routes-list">
            {routeList.map(route => (
              <div
                key={route.id}
                className={`route-card ${selectedRoute?.id === route.id ? 'selected' : ''}`}
                onClick={() => setSelectedRoute(route)}
              >
                <div className="route-card-header">
                  <span className="route-mode-icon">{modeEmoji[route.travelMode] || '🚶'}</span>
                  <div>
                    <div className="route-card-name">{route.name}</div>
                    <div className="route-card-date">{new Date(route.startedAt).toLocaleDateString()}</div>
                  </div>
                  <span className={`status-badge ${route.isCompleted ? 'completed' : 'in-progress'}`}>
                    {route.isCompleted ? 'Completed' : 'In Progress'}
                  </span>
                </div>
                {route.isCompleted && (
                  <div className="route-card-stats">
                    <span>📏 {formatDistance(route.totalDistanceMeters)}</span>
                    <span>⏱️ {formatDuration(route.totalDurationSeconds)}</span>
                    <span>📍 {route.points.length} pts</span>
                  </div>
                )}
              </div>
            ))}
          </div>

          {selectedRoute && (
            <div className="route-detail card">
              <div className="route-detail-header">
                <h2>{modeEmoji[selectedRoute.travelMode]} {selectedRoute.name}</h2>
                <div className="route-actions">
                  <button className="btn btn-sm btn-secondary" onClick={() => handleShare(selectedRoute)} disabled={sharing}>
                    {sharing ? '...' : '🔗 Share'}
                  </button>
                  <button className="btn btn-sm btn-danger" onClick={() => handleDelete(selectedRoute.id)}>🗑️ Delete</button>
                </div>
              </div>

              {selectedRoute.description && <p className="route-description">{selectedRoute.description}</p>}

              <div className="detail-stats">
                <div className="detail-stat"><strong>Mode</strong><span>{selectedRoute.travelMode}</span></div>
                <div className="detail-stat"><strong>Status</strong><span>{selectedRoute.isCompleted ? '✅ Completed' : '⏳ In Progress'}</span></div>
                <div className="detail-stat"><strong>Started</strong><span>{new Date(selectedRoute.startedAt).toLocaleString()}</span></div>
                {selectedRoute.completedAt && <div className="detail-stat"><strong>Completed</strong><span>{new Date(selectedRoute.completedAt).toLocaleString()}</span></div>}
                {selectedRoute.isCompleted && <>
                  <div className="detail-stat"><strong>Distance</strong><span>{formatDistance(selectedRoute.totalDistanceMeters)}</span></div>
                  <div className="detail-stat"><strong>Duration</strong><span>{formatDuration(selectedRoute.totalDurationSeconds)}</span></div>
                  <div className="detail-stat"><strong>Points</strong><span>{selectedRoute.points.length}</span></div>
                </>}
              </div>

              {shareResult && shareResult.routeId === selectedRoute.id && (
                <div className="share-result">
                  <p>✅ Share link created!</p>
                  <div className="share-url-container">
                    <input readOnly value={shareResult.routeUrl} className="share-url" />
                    <button className="btn btn-sm btn-primary" onClick={copyShareUrl}>{copied ? '✓ Copied' : 'Copy'}</button>
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
