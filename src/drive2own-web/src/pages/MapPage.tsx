import React, { useEffect, useRef, useState, useCallback } from 'react';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import { locationPins, routes } from '../api';
import { LocationPin, Route, RoutePoint } from '../types';

// Fix leaflet default icon
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: require('leaflet/dist/images/marker-icon-2x.png'),
  iconUrl: require('leaflet/dist/images/marker-icon.png'),
  shadowUrl: require('leaflet/dist/images/marker-shadow.png'),
});

type MapMode = 'view' | 'pin' | 'record';

export default function MapPage() {
  const mapRef = useRef<L.Map | null>(null);
  const mapDivRef = useRef<HTMLDivElement>(null);
  const [pins, setPins] = useState<LocationPin[]>([]);
  const [userRoutes, setUserRoutes] = useState<Route[]>([]);
  const [mode, setMode] = useState<MapMode>('view');
  const [recording, setRecording] = useState(false);
  const [recordedPoints, setRecordedPoints] = useState<RoutePoint[]>([]);
  const [travelMode, setTravelMode] = useState('Car');
  const [routeName, setRouteName] = useState('');
  const [showPinModal, setShowPinModal] = useState(false);
  const [pendingLatLng, setPendingLatLng] = useState<[number, number] | null>(null);
  const [pinForm, setPinForm] = useState({ title: '', description: '', iconColor: '#FF5733' });
  const [watchId, setWatchId] = useState<number | null>(null);
  const [status, setStatus] = useState('');
  const polylineRef = useRef<L.Polyline | null>(null);
  const markersRef = useRef<L.Marker[]>([]);
  const routeLayersRef = useRef<L.Polyline[]>([]);
  const pinMarkersRef = useRef<L.Marker[]>([]);
  const sequenceRef = useRef(0);
  const currentRouteRef = useRef<Route | null>(null);

  useEffect(() => {
    if (mapDivRef.current && !mapRef.current) {
      const map = L.map(mapDivRef.current).setView([51.505, -0.09], 13);
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
      }).addTo(map);

      mapRef.current = map;

      // Try to center on user's location
      navigator.geolocation?.getCurrentPosition(pos => {
        map.setView([pos.coords.latitude, pos.coords.longitude], 15);
      });
    }
  }, []);

  // Update click handler when mode changes
  useEffect(() => {
    const map = mapRef.current;
    if (!map) return;
    map.off('click');
    map.on('click', (e: L.LeafletMouseEvent) => {
      if (mode === 'pin') {
        setPendingLatLng([e.latlng.lat, e.latlng.lng]);
        setShowPinModal(true);
      }
    });
  }, [mode]);

  const loadPins = useCallback(async () => {
    const res = await locationPins.getAll();
    setPins(res.data);
  }, []);

  const loadRoutes = useCallback(async () => {
    const res = await routes.getAll();
    setUserRoutes(res.data);
  }, []);

  useEffect(() => { loadPins(); loadRoutes(); }, [loadPins, loadRoutes]);

  // Render pins on map
  useEffect(() => {
    const map = mapRef.current;
    if (!map) return;
    pinMarkersRef.current.forEach(m => map.removeLayer(m));
    pinMarkersRef.current = pins.map(pin => {
      const icon = L.divIcon({
        html: `<div style="width:20px;height:20px;border-radius:50%;background:${pin.iconColor};border:2px solid white;box-shadow:0 2px 4px rgba(0,0,0,0.4)"></div>`,
        className: '',
        iconSize: [20, 20],
        iconAnchor: [10, 10],
      });
      const marker = L.marker([pin.latitude, pin.longitude], { icon })
        .addTo(map)
        .bindPopup(`<b>${pin.title}</b>${pin.description ? `<br>${pin.description}` : ''}`);
      return marker;
    });
  }, [pins]);

  // Render routes on map
  useEffect(() => {
    const map = mapRef.current;
    if (!map) return;
    routeLayersRef.current.forEach(l => map.removeLayer(l));
    routeLayersRef.current = userRoutes
      .filter(r => r.isCompleted && r.points.length > 1)
      .map(r => {
        const coords: [number, number][] = r.points.map(p => [p.latitude, p.longitude]);
        return L.polyline(coords, { color: '#4CAF50', weight: 3, opacity: 0.7 }).addTo(map)
          .bindPopup(`<b>${r.name}</b><br>${r.travelMode}`);
      });
  }, [userRoutes]);

  const startRecording = async () => {
    if (!routeName.trim()) { setStatus('Please enter a route name'); return; }
    try {
      const res = await routes.create({ name: routeName, travelMode, startedAt: new Date().toISOString() });
      const route = res.data;
      currentRouteRef.current = route;
      setRecordedPoints([]);
      sequenceRef.current = 0;
      setRecording(true);
      setStatus('Recording...');

      const map = mapRef.current;
      if (map) {
        polylineRef.current = L.polyline([], { color: '#FF5733', weight: 4 }).addTo(map);
      }

      const id = navigator.geolocation.watchPosition(async (pos) => {
        const { latitude, longitude, altitude, speed } = pos.coords;
        const point = {
          latitude, longitude,
          altitude: altitude ?? undefined,
          speedMps: speed ?? undefined,
          sequence: sequenceRef.current++,
          recordedAt: new Date().toISOString()
        };

        const r = currentRouteRef.current;
        if (r) {
          await routes.addPoints(r.id, [point]);
        }

        setRecordedPoints(prev => [...prev, point as RoutePoint]);
        polylineRef.current?.addLatLng([latitude, longitude]);
        map?.setView([latitude, longitude]);

        const dotIcon = L.divIcon({
          html: '<div style="width:10px;height:10px;border-radius:50%;background:#FF5733;border:2px solid white"></div>',
          className: '', iconSize: [10, 10], iconAnchor: [5, 5],
        });
        if (sequenceRef.current === 1) {
          markersRef.current.push(L.marker([latitude, longitude], { icon: dotIcon }).addTo(map!));
        }
      }, (err) => setStatus(`GPS error: ${err.message}`), { enableHighAccuracy: true, maximumAge: 0 });

      setWatchId(id);
    } catch {
      setStatus('Failed to start recording');
    }
  };

  const stopRecording = async () => {
    if (watchId !== null) navigator.geolocation.clearWatch(watchId);
    setWatchId(null);
    setRecording(false);

    const r = currentRouteRef.current;
    if (r && recordedPoints.length > 0) {
      try {
        const totalDist = calculateTotalDistance(recordedPoints);
        const firstPoint = recordedPoints[0];
        const lastPoint = recordedPoints[recordedPoints.length - 1];
        const durationSecs = (new Date(lastPoint.recordedAt).getTime() - new Date(firstPoint.recordedAt).getTime()) / 1000;
        await routes.complete(r.id, {
          completedAt: new Date().toISOString(),
          totalDistanceMeters: totalDist,
          totalDurationSeconds: durationSecs
        });
        setStatus(`Route saved! Distance: ${(totalDist / 1000).toFixed(2)} km`);
        loadRoutes();
      } catch {
        setStatus('Route recorded but failed to finalize');
      }
    }
    currentRouteRef.current = null;
    setRouteName('');
  };

  const handleAddPin = async () => {
    if (!pendingLatLng || !pinForm.title.trim()) return;
    try {
      await locationPins.create({
        title: pinForm.title,
        description: pinForm.description || undefined,
        latitude: pendingLatLng[0],
        longitude: pendingLatLng[1],
        iconColor: pinForm.iconColor
      });
      setPinForm({ title: '', description: '', iconColor: '#FF5733' });
      setShowPinModal(false);
      setPendingLatLng(null);
      loadPins();
    } catch {
      setStatus('Failed to add pin');
    }
  };

  return (
    <div className="map-page">
      <div className="map-toolbar">
        <div className="toolbar-modes">
          <button className={`btn btn-sm ${mode === 'view' ? 'btn-primary' : 'btn-secondary'}`} onClick={() => setMode('view')}>👁 View</button>
          <button className={`btn btn-sm ${mode === 'pin' ? 'btn-primary' : 'btn-secondary'}`} onClick={() => setMode('pin')}>📍 Pin</button>
          <button className={`btn btn-sm ${mode === 'record' ? 'btn-primary' : 'btn-secondary'}`} onClick={() => setMode('record')}>🔴 Record</button>
        </div>
        {mode === 'record' && !recording && (
          <div className="record-controls">
            <input placeholder="Route name" value={routeName} onChange={e => setRouteName(e.target.value)} className="input-sm" />
            <select value={travelMode} onChange={e => setTravelMode(e.target.value)} className="input-sm">
              <option>Car</option><option>Walk</option><option>Cycle</option><option>Run</option>
            </select>
            <button className="btn btn-sm btn-danger" onClick={startRecording}>▶ Start</button>
          </div>
        )}
        {mode === 'record' && recording && (
          <div className="record-controls">
            <span className="recording-badge">● Recording: {routeName}</span>
            <span className="points-count">{recordedPoints.length} points</span>
            <button className="btn btn-sm btn-secondary" onClick={stopRecording}>⏹ Stop</button>
          </div>
        )}
        {status && <div className="status-bar">{status}</div>}
      </div>

      <div ref={mapDivRef} className="map-container" />

      {showPinModal && (
        <div className="modal-overlay" onClick={() => setShowPinModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h3>Add Location Pin</h3>
            <div className="form-group">
              <label>Title *</label>
              <input value={pinForm.title} onChange={e => setPinForm(f => ({ ...f, title: e.target.value }))} placeholder="Pin title" />
            </div>
            <div className="form-group">
              <label>Description</label>
              <input value={pinForm.description} onChange={e => setPinForm(f => ({ ...f, description: e.target.value }))} placeholder="Optional description" />
            </div>
            <div className="form-group">
              <label>Color</label>
              <input type="color" value={pinForm.iconColor} onChange={e => setPinForm(f => ({ ...f, iconColor: e.target.value }))} />
            </div>
            <div className="modal-actions">
              <button className="btn btn-secondary" onClick={() => setShowPinModal(false)}>Cancel</button>
              <button className="btn btn-primary" onClick={handleAddPin}>Add Pin</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function calculateTotalDistance(points: { latitude: number; longitude: number }[]): number {
  let total = 0;
  for (let i = 1; i < points.length; i++) {
    total += haversine(points[i - 1].latitude, points[i - 1].longitude, points[i].latitude, points[i].longitude);
  }
  return total;
}

function haversine(lat1: number, lon1: number, lat2: number, lon2: number): number {
  const R = 6371000;
  const dLat = ((lat2 - lat1) * Math.PI) / 180;
  const dLon = ((lon2 - lon1) * Math.PI) / 180;
  const a = Math.sin(dLat / 2) ** 2 + Math.cos((lat1 * Math.PI) / 180) * Math.cos((lat2 * Math.PI) / 180) * Math.sin(dLon / 2) ** 2;
  return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}
