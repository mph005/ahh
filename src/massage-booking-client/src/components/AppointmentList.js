import React, { useState, useEffect } from 'react';
import apiClient from '../api/apiClient';

function AppointmentList() {
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchAppointments = async () => {
      setLoading(true);
      setError(null);
      try {
        // Calculate date range: Today to 7 days from now
        const startDate = new Date();
        const endDate = new Date();
        endDate.setDate(startDate.getDate() + 7);

        // Fetch appointments for the calculated range
        const data = await apiClient.getAppointmentsInRange(startDate, endDate);
        
        // Sort appointments by start time
        const sortedData = data.sort((a, b) => new Date(a.startTime) - new Date(b.startTime));
        
        setAppointments(sortedData);
      } catch (error) {
        console.error("Error fetching appointments:", error); // Log the full error
        setError(error.message || 'Failed to load appointments');
      } finally {
        setLoading(false);
      }
    };

    fetchAppointments();
  }, []);

  if (loading) {
    return <div className="loading">Loading appointments...</div>;
  }

  if (error) {
    return <div className="error">Error loading appointments: {error}</div>;
  }

  return (
    <div className="card">
      <h2>Upcoming Appointments (Next 7 Days)</h2>
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Time</th>
            <th>Client</th>
            <th>Therapist</th>
            <th>Treatment</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {appointments.length > 0 ? (
            appointments.map(appointment => (
              <tr key={appointment.appointmentId}>
                <td>{new Date(appointment.startTime).toLocaleDateString()}</td>
                <td>{new Date(appointment.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</td>
                <td>{appointment.clientName}</td>
                <td>{appointment.therapistName}</td>
                <td>{appointment.serviceName}</td>
                <td>{appointment.status}</td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan="6">No appointments found</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

export default AppointmentList; 