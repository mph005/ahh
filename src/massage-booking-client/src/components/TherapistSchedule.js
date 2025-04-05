import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import apiClient from '../api/apiClient';

function TherapistSchedule() {
  const { id } = useParams();
  const therapistId = parseInt(id, 10);
  
  const [therapist, setTherapist] = useState(null);
  const [appointments, setAppointments] = useState([]);
  const [services, setServices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        // Fetch therapist data
        const therapistData = await apiClient.getTherapist(therapistId);
        setTherapist(therapistData);
        
        // Fetch therapist's appointments
        const appointmentsData = await apiClient.getTherapistAppointments(therapistId);
        setAppointments(appointmentsData);
        
        // Fetch services for this therapist
        const servicesData = await apiClient.getServicesByTherapistId(therapistId);
        setServices(servicesData);
        
        setLoading(false);
      } catch (error) {
        setError(error.message);
        setLoading(false);
      }
    };

    fetchData();
  }, [therapistId]);

  if (loading) {
    return <div className="loading">Loading therapist schedule...</div>;
  }

  if (error) {
    return <div className="error">Error loading therapist schedule: {error}</div>;
  }

  // Group appointments by date
  const appointmentsByDate = appointments.reduce((acc, appointment) => {
    const date = new Date(appointment.appointmentDate).toLocaleDateString();
    if (!acc[date]) {
      acc[date] = [];
    }
    acc[date].push(appointment);
    return acc;
  }, {});

  return (
    <div className="card">
      {therapist && (
        <h2>{therapist.firstName} {therapist.lastName}'s Schedule</h2>
      )}

      {Object.keys(appointmentsByDate).length > 0 ? (
        Object.keys(appointmentsByDate).map(date => (
          <div key={date} className="card" style={{ marginBottom: '20px' }}>
            <h3>{date}</h3>
            <table>
              <thead>
                <tr>
                  <th>Time</th>
                  <th>Client</th>
                  <th>Treatment</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {appointmentsByDate[date].map(appointment => (
                  <tr key={appointment.id}>
                    <td>{new Date(appointment.appointmentDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</td>
                    <td>{appointment.client.firstName} {appointment.client.lastName}</td>
                    <td>{appointment.treatmentType}</td>
                    <td>{appointment.status}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ))
      ) : (
        <p>No appointments scheduled</p>
      )}
    </div>
  );
}

export default TherapistSchedule; 