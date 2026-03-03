// Tests for pure utility functions used in the app
export {};

function formatDistance(meters: number): string {
  if (meters >= 1000) return `${(meters / 1000).toFixed(2)} km`;
  return `${Math.round(meters)} m`;
}

function formatDuration(seconds: number): string {
  const h = Math.floor(seconds / 3600);
  const m = Math.floor((seconds % 3600) / 60);
  if (h > 0) return `${h}h ${m}m`;
  return `${m}m`;
}

function haversine(lat1: number, lon1: number, lat2: number, lon2: number): number {
  const R = 6371000;
  const dLat = ((lat2 - lat1) * Math.PI) / 180;
  const dLon = ((lon2 - lon1) * Math.PI) / 180;
  const a = Math.sin(dLat / 2) ** 2 + Math.cos((lat1 * Math.PI) / 180) * Math.cos((lat2 * Math.PI) / 180) * Math.sin(dLon / 2) ** 2;
  return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}

describe('formatDistance', () => {
  test('formats meters when under 1000', () => {
    expect(formatDistance(500)).toBe('500 m');
  });

  test('formats kilometers when 1000 or more', () => {
    expect(formatDistance(1500)).toBe('1.50 km');
  });

  test('rounds meters to nearest integer', () => {
    expect(formatDistance(99.7)).toBe('100 m');
  });
});

describe('formatDuration', () => {
  test('formats minutes only', () => {
    expect(formatDuration(300)).toBe('5m');
  });

  test('formats hours and minutes', () => {
    expect(formatDuration(3900)).toBe('1h 5m');
  });

  test('formats 0 seconds', () => {
    expect(formatDuration(0)).toBe('0m');
  });
});

describe('haversine', () => {
  test('returns 0 for same point', () => {
    expect(haversine(51.5, -0.1, 51.5, -0.1)).toBeCloseTo(0, 1);
  });

  test('returns approximate distance between London and Paris', () => {
    // London (51.5, -0.12) to Paris (48.85, 2.35) ≈ 340 km
    const dist = haversine(51.5, -0.12, 48.85, 2.35);
    expect(dist).toBeGreaterThan(330000);
    expect(dist).toBeLessThan(350000);
  });
});
