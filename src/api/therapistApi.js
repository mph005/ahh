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
 * Fetch all therapists
 * @param {Object} filters - Optional filter parameters
 * @param {string} filters.serviceId - Filter by service ID
 * @returns {Promise<Array>} - Promise resolving to array of therapists
 */
export const fetchTherapists = async (filters = {}) => {
  try {
    const response = await axios.get(
      `${API_URL}/therapists`,
      { 
        params: filters,
        ...configureAxios()
      }
    );
    return response.data.data || [];
  } catch (error) {
    console.error('Error fetching therapists:', error);
    throw error;
  }
};

/**
 * Fetch a specific therapist by ID
 * @param {string} therapistId - The therapist ID
 * @returns {Promise<Object>} - Promise resolving to therapist details
 */
export const getTherapistById = async (therapistId) => {
  try {
    const response = await axios.get(
      `${API_URL}/therapists/${therapistId}`,
      configureAxios()
    );
    return response.data.data;
  } catch (error) {
    console.error(`Error fetching therapist with ID ${therapistId}:`, error);
    throw error;
  }
};

/**
 * Fetch therapist availability
 * @param {string} therapistId - The therapist ID
 * @param {string} startDate - Start date in ISO format (YYYY-MM-DD)
 * @param {string} endDate - End date in ISO format (YYYY-MM-DD)
 * @returns {Promise<Array>} - Promise resolving to array of availability slots
 */
export const getTherapistAvailability = async (therapistId, startDate, endDate) => {
  try {
    const response = await axios.get(
      `${API_URL}/therapists/${therapistId}/availability`,
      { 
        params: { startDate, endDate },
        ...configureAxios()
      }
    );
    return response.data.data || [];
  } catch (error) {
    console.error(`Error fetching availability for therapist ${therapistId}:`, error);
    throw error;
  }
}; 