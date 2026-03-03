import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import { shares } from '../api';
import { SharedRoute } from '../types';

export default function SharedRoutePage() {
  const { token } = useParams<{ token: string }>();
  const [data, setData] = useState<SharedRoute | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (token) {
      shares.getSharedRoute(token)
        .then(res => setData(res.data))
        .catch(err => setError(err.response?.status === 410 ? 'This share link has expired.' : 'Route not found.'))
        .then(() => setLoading(false));
    }
  }, [token]);

  useEffect(() => {
    if (!data) return;
    const map = L.map('shared-map').setView([0, 0], 2);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    const { points } = data.route;
    if (points.length > 0) {
      const coords: [number, number][] = points.map(p => [p.latitude, p.longitude]);
      const polyline = L.polyline(coords, { color: '#4CAF50', weight: 4 }).addTo(map);
      map.fitBounds(polyline.getBounds(), { padding: [50, 50] });

      L.marker(coords[0]).addTo(map).bindPopup('Start').openPopup();
      if (coords.length > 1) {
        L.marker(coords[coords.length - 1]).addTo(map).bindPopup('End');
      }
    }
    return () => { map.remove(); };
  }, [data]);

  if (loading) return <div className="loading">Loading shared route...</div>;
  if (error) return <div className="error-page"><h2>😕 {error}</h2></div>;
  if (!data) return null;

  const { route, sharedBy, sharedAt } = data;
  const modeEmoji: Record<string, string> = { Car: '🚗', Walk: '🚶', Cycle: '🚴', Run: '🏃' };
  const formatDist = (m: number) => m >= 1000 ? `${(m / 1000).toFixed(2)} km` : `${Math.round(m)} m`;
  const formatDur = (s: number) => { const h = Math.floor(s / 3600); const m = Math.floor((s % 3600) / 60); return h > 0 ? `${h}h ${m}m` : `${m}m`; };

  return (
    <div className="shared-route-page">
      <div className="shared-header">
        <div className="shared-logo">🗺️ Drive2Own</div>
        <h1>{modeEmoji[route.travelMode]} {route.name}</h1>
        <p>Shared by <strong>{sharedBy}</strong> on {new Date(sharedAt).toLocaleDateString()}</p>
      </div>

      <div id="shared-map" className="shared-map" />

      <div className="shared-stats">
        <div className="stat-card"><div className="stat-icon">📏</div><div className="stat-value">{formatDist(route.totalDistanceMeters)}</div><div className="stat-label">Distance</div></div>
        <div className="stat-card"><div className="stat-icon">⏱️</div><div className="stat-value">{formatDur(route.totalDurationSeconds)}</div><div className="stat-label">Duration</div></div>
        <div className="stat-card"><div className="stat-icon">📍</div><div className="stat-value">{route.points.length}</div><div className="stat-label">GPS Points</div></div>
        <div className="stat-card"><div className="stat-icon">{modeEmoji[route.travelMode]}</div><div className="stat-value">{route.travelMode}</div><div className="stat-label">Mode</div></div>
      </div>

      {route.description && <div className="shared-description"><p>{route.description}</p></div>}
    </div>
  );
}
