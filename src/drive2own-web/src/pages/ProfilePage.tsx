import React, { useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { users } from '../api';

export default function ProfilePage() {
  const { user, setUser, logout } = useAuth();
  const [form, setForm] = useState({ displayName: user?.displayName || '', avatarUrl: user?.avatarUrl || '' });
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setSuccess('');
    setError('');
    try {
      const res = await users.updateProfile({ displayName: form.displayName, avatarUrl: form.avatarUrl || undefined });
      setUser(res.data);
      setSuccess('Profile updated successfully!');
    } catch {
      setError('Failed to update profile');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="page-content">
      <h1 className="page-title">Profile</h1>
      <div className="card profile-card">
        <div className="profile-avatar">
          {form.avatarUrl ? (
            <img src={form.avatarUrl} alt="avatar" className="avatar-img" />
          ) : (
            <div className="avatar-placeholder">{user?.displayName?.[0]?.toUpperCase() || '?'}</div>
          )}
        </div>

        <form onSubmit={handleSubmit}>
          {success && <div className="alert alert-success">{success}</div>}
          {error && <div className="alert alert-error">{error}</div>}

          <div className="form-group">
            <label>Username</label>
            <input value={user?.username || ''} disabled className="input-disabled" />
          </div>
          <div className="form-group">
            <label>Email</label>
            <input value={user?.email || ''} disabled className="input-disabled" />
          </div>
          <div className="form-group">
            <label>Display Name</label>
            <input value={form.displayName} onChange={e => setForm(f => ({ ...f, displayName: e.target.value }))} required />
          </div>
          <div className="form-group">
            <label>Avatar URL</label>
            <input value={form.avatarUrl} onChange={e => setForm(f => ({ ...f, avatarUrl: e.target.value }))} placeholder="https://..." />
          </div>

          <div className="form-actions">
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving ? 'Saving...' : 'Save Changes'}
            </button>
            <button type="button" className="btn btn-danger" onClick={logout}>Sign Out</button>
          </div>
        </form>

        <div className="profile-meta">
          <p>Member since {new Date(user?.createdAt || '').toLocaleDateString()}</p>
        </div>
      </div>
    </div>
  );
}
