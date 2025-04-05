import React, { useState, useEffect } from 'react';
import apiClient from '../api/apiClient';

function ClientList() {
  const [clients, setClients] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchClients = async () => {
      try {
        const data = await apiClient.getClients();
        setClients(data);
        setLoading(false);
      } catch (error) {
        setError(error.message);
        setLoading(false);
      }
    };

    fetchClients();
  }, []);

  if (loading) {
    return <div className="loading">Loading clients...</div>;
  }

  if (error) {
    return <div className="error">Error loading clients: {error}</div>;
  }

  return (
    <div className="card">
      <h2>Clients</h2>
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Phone</th>
            <th>Date of Birth</th>
          </tr>
        </thead>
        <tbody>
          {clients.length > 0 ? (
            clients.map(client => (
              <tr key={client.id}>
                <td>{`${client.firstName} ${client.lastName}`}</td>
                <td>{client.email}</td>
                <td>{client.phoneNumber}</td>
                <td>{new Date(client.dateOfBirth).toLocaleDateString()}</td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan="4">No clients found</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

export default ClientList; 