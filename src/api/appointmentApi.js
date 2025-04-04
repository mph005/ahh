import axios from 'axios';

// Base API URL
const API_URL = process.env.REACT_APP_API_URL || 'https://api.massage-booking.example.com/api';

// Get auth token from storage
const getAuthToken = () => {
  return localStorage.getItem('authToken');
};

// Configure axios with default headers
const configureAxios = () => {
  const token = getAuthToken();
  return {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : ''
    }
  };
};

/**
 * Fetch appointments for a user within a date range
 * @param {string} userId - The user ID (client or therapist)
 * @param {string} startDate - Start date in ISO format (YYYY-MM-DD)
 * @param {string} endDate - End date in ISO format (YYYY-MM-DD)
 * @returns {Promise<Array>} - Promise resolving to array of appointments
 */
export const fetchAppointments = async (userId, startDate, endDate) => {
  try {
    const response = await axios.get(
      `${API_URL}/appointments`, 
      { 
        params: { userId, startDate, endDate },
        ...configureAxios()
      }
    );
    return response.data.data || [];
  } catch (error) {
    console.error('Error fetching appointments:', error);
    throw error;
  }
};

/**
 * Fetch available appointment slots
 * @param {string} serviceId - The service ID (optional)
 * @param {string} therapistId - The therapist ID (optional)
 * @param {string} startDate - Start date in ISO format (YYYY-MM-DD)
 * @param {string} endDate - End date in ISO format (YYYY-MM-DD)
 * @returns {Promise<Array>} - Promise resolving to array of available slots
 */
export const fetchAvailableSlots = async (serviceId, therapistId, startDate, endDate) => {
  try {
    const response = await axios.get(
      `${API_URL}/appointments/available`, 
      { 
        params: { 
          serviceId, 
          therapistId, 
          startDate, 
          endDate 
        },
        ...configureAxios()
      }
    );
    return response.data.data || [];
  } catch (error) {
    console.error('Error fetching available slots:', error);
    throw error;
  }
};

/**
 * Create a new appointment
 * @param {Object} appointmentData - The appointment data
 * @param {string} appointmentData.clientId - Client ID
 * @param {string} appointmentData.therapistId - Therapist ID
 * @param {string} appointmentData.serviceId - Service ID
 * @param {string} appointmentData.startTime - Start time in ISO format
 * @param {string} appointmentData.notes - Optional notes
 * @returns {Promise<Object>} - Promise resolving to created appointment
 */
export const createAppointment = async (appointmentData) => {
  try {
    const response = await axios.post(
      `${API_URL}/appointments`, 
      appointmentData,
      configureAxios()
    );
    return response.data.data;
  } catch (error) {
    console.error('Error creating appointment:', error);
    throw error;
  }
};

/**
 * Update an existing appointment
 * @param {string} appointmentId - The appointment ID
 * @param {Object} appointmentData - The updated appointment data
 * @returns {Promise<Object>} - Promise resolving to updated appointment
 */
export const updateAppointment = async (appointmentId, appointmentData) => {
  try {
    const response = await axios.put(
      `${API_URL}/appointments/${appointmentId}`, 
      appointmentData,
      configureAxios()
    );
    return response.data.data;
  } catch (error) {
    console.error('Error updating appointment:', error);
    throw error;
  }
};

/**
 * Cancel an appointment
 * @param {string} appointmentId - The appointment ID
 * @returns {Promise<Object>} - Promise resolving to cancelled appointment
 */
export const cancelAppointment = async (appointmentId) => {
  try {
    const response = await axios.post(
      `${API_URL}/appointments/${appointmentId}/cancel`,
      {},
      configureAxios()
    );
    return response.data.data;
  } catch (error) {
    console.error('Error cancelling appointment:', error);
    throw error;
  }
};

/**
 * Get appointment details
 * @param {string} appointmentId - The appointment ID
 * @returns {Promise<Object>} - Promise resolving to appointment details
 */
export const getAppointmentDetails = async (appointmentId) => {
  try {
    const response = await axios.get(
      `${API_URL}/appointments/${appointmentId}`,
      configureAxios()
    );
    return response.data.data;
  } catch (error) {
    console.error('Error fetching appointment details:', error);
    throw error;
  }
}; 