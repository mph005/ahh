import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import apiClient from '../api/apiClient';

function ClientAppointments() {
  const { id } = useParams();
  const clientId = parseInt(id, 10);
  
  const [client, setClient] = useState(null);
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        // Fetch client details
        const clientData = await apiClient.getClient(clientId);
        setClient(clientData);

        // Fetch client's appointments
        const appointmentsData = await apiClient.getClientAppointments(clientId);
        setAppointments(appointmentsData);
        
        setLoading(false);
      } catch (error) {
        setError(error.message);
        setLoading(false);
      }
    };

    fetchData();
  }, [clientId]);

  if (loading) {
    return <div className="loading">Loading client appointments...</div>;
  }

  if (error) {
    return <div className="error">Error loading client appointments: {error}</div>;
  }

  // Group appointments into upcoming and past
  const now = new Date();
  const upcomingAppointments = appointments.filter(appt => new Date(appt.startTime) > now);
  const pastAppointments = appointments.filter(appt => new Date(appt.startTime) <= now);

  return (
    <div>
      {client && (
        <h2>{`${client.firstName} ${client.lastName}'s Appointments`}</h2>
      )}

      <div className="card">
        <h3>Upcoming Appointments</h3>
        {upcomingAppointments.length > 0 ? (
          <table>
            <thead>
              <tr>
                <th>Date</th>
                <th>Time</th>
                <th>Therapist</th>
                <th>Service</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {upcomingAppointments.map(appointment => (
                <tr key={appointment.id}>
                  <td>{appointment.startTime.toLocaleDateString()}</td>
                  <td>{appointment.startTime.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</td>
                  <td>{`${appointment.therapist.firstName} ${appointment.therapist.lastName}`}</td>
                  <td>{appointment.service.name}</td>
                  <td>{appointment.status}</td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <p>No upcoming appointments</p>
        )}
      </div>

      <div className="card" style={{ marginTop: '20px' }}>
        <h3>Past Appointments</h3>
        {pastAppointments.length > 0 ? (
          <table>
            <thead>
              <tr>
                <th>Date</th>
                <th>Time</th>
                <th>Therapist</th>
                <th>Service</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {pastAppointments.map(appointment => (
                <tr key={appointment.id}>
                  <td>{appointment.startTime.toLocaleDateString()}</td>
                  <td>{appointment.startTime.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</td>
                  <td>{`${appointment.therapist.firstName} ${appointment.therapist.lastName}`}</td>
                  <td>{appointment.service.name}</td>
                  <td>{appointment.status}</td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <p>No past appointments</p>
        )}
      </div>
    </div>
  );
}

export default ClientAppointments; 