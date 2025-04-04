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
 * Fetch all available services
 * @returns {Promise<Array>} - Promise resolving to array of services
 */
export const fetchServices = async () => {
  try {
    const response = await axios.get(
      `${API_URL}/services`,
      configureAxios()
    );
    return response.data.data || [];
  } catch (error) {
    console.error('Error fetching services:', error);
    throw error;
  }
};

/**
 * Fetch a specific service by ID
 * @param {string} serviceId - The service ID
 * @returns {Promise<Object>} - Promise resolving to service details
 */
export const getServiceById = async (serviceId) => {
  try {
    const response = await axios.get(
      `${API_URL}/services/${serviceId}`,
      configureAxios()
    );
    return response.data.data;
  } catch (error) {
    console.error(`Error fetching service with ID ${serviceId}:`, error);
    throw error;
  }
}; 