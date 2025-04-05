import React, { useState, useEffect } from 'react';
import apiClient from '../api/apiClient';

function TherapistList() {
  const [therapists, setTherapists] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchTherapists = async () => {
      try {
        const data = await apiClient.getTherapists();
        setTherapists(data);
        setLoading(false);
      } catch (error) {
        setError(error.message);
        setLoading(false);
      }
    };

    fetchTherapists();
  }, []);

  if (loading) {
    return <div className="loading">Loading therapists...</div>;
  }

  if (error) {
    return <div className="error">Error loading therapists: {error}</div>;
  }

  return (
    <div className="card">
      <h2>Our Therapists</h2>
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Specialty</th>
            <th>Email</th>
            <th>Phone</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {therapists.length > 0 ? (
            therapists.map(therapist => (
              <tr key={therapist.id}>
                <td>{`${therapist.firstName} ${therapist.lastName}`}</td>
                <td>{therapist.specialty}</td>
                <td>{therapist.email}</td>
                <td>{therapist.phoneNumber}</td>
                <td>{therapist.isActive ? 'Active' : 'Inactive'}</td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan="5">No therapists found</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

export default TherapistList; 