import axios from 'axios';

// Create an Axios instance
const instance = axios.create({
  // The "proxy" setting in package.json should handle the base URL forwarding
  // during development. For production, you might set a baseURL explicitly.
  // baseURL: 'https://your-production-api.com/api'
  withCredentials: true // IMPORTANT: Send cookies with requests
});

// REMOVED: Request interceptor for JWT - no longer needed for cookie auth
// instance.interceptors.request.use(...);

// Helper function for handling API responses and errors
const handleResponse = (response) => response.data;
const handleError = (error) => {
  console.error('API call failed:', error.response || error.message);
  // Optionally handle specific status codes (like 401/403) globally if needed
  // if (error.response && (error.response.status === 401 || error.response.status === 403)) {
  //   // Potentially trigger a logout or redirect here
  //   console.log('Unauthorized or Forbidden response received.');
  // }
  throw error; // Rethrow the error for component-level handling
};

// Define API methods using the configured instance
const apiClient = {
  // --- Auth Endpoint ---
  async login(email, password) {
    return instance.post('/api/auth/login', { email, password }).then(handleResponse).catch(handleError);
  },

  async logout() {
    // Fires request to clear the HTTP-only cookie on the server
    return instance.post('/api/auth/logout').then(handleResponse).catch(handleError);
  },

  async getUserInfo() {
    // Gets current user info if authenticated via cookie
    return instance.get('/api/auth/userinfo').then(handleResponse).catch(handleError);
  },

  // --- Therapist Endpoints ---
  async getTherapists() {
    return instance.get('/api/therapists').then(handleResponse).catch(handleError);
  },

  async getActiveTherapists() {
    return instance.get('/api/therapists/active').then(handleResponse).catch(handleError);
  },

  async getTherapist(id) {
    if (!id) throw new Error('Therapist ID is required');
    return instance.get(`/api/therapists/${id}`).then(handleResponse).catch(handleError);
  },

  async getTherapistServices(therapistId) {
    if (!therapistId) throw new Error('Therapist ID is required');
    return instance.get(`/api/therapists/${therapistId}/services`).then(handleResponse).catch(handleError);
  },

  // --- Client Endpoints ---
  async getClients() {
    return instance.get('/api/clients').then(handleResponse).catch(handleError);
  },

  async getClient(id) {
    if (!id) throw new Error('Client ID is required');
    return instance.get(`/api/clients/${id}`).then(handleResponse).catch(handleError);
  },

  // --- Service Endpoints ---
  async getServices() {
    return instance.get('/api/services').then(handleResponse).catch(handleError);
  },

  async getService(id) {
    if (!id) throw new Error('Service ID is required');
    return instance.get(`/api/services/${id}`).then(handleResponse).catch(handleError);
  },

  // --- Appointment Endpoints ---
  async getAppointment(id) {
    if (!id) throw new Error('Appointment ID is required');
    return instance.get(`/api/appointments/${id}`).then(handleResponse).catch(handleError);
  },

  async getAppointmentsInRange(startDate, endDate) {
    if (!startDate || !endDate) throw new Error('Start and end date are required');
    const start = startDate instanceof Date ? startDate.toISOString() : startDate;
    const end = endDate instanceof Date ? endDate.toISOString() : endDate;
    return instance.get(`/api/appointments/range?startDate=${start}&endDate=${end}`).then(handleResponse).catch(handleError);
  },

  async getClientAppointments(clientId) {
    if (!clientId) throw new Error('Client ID is required');
    return instance.get(`/api/appointments/client/${clientId}`).then(handleResponse).catch(handleError);
  },

  async getTherapistAppointments(therapistId, startDate, endDate) {
    if (!therapistId) throw new Error('Therapist ID is required');
    const start = startDate instanceof Date ? startDate.toISOString() : startDate;
    const end = endDate instanceof Date ? endDate.toISOString() : endDate;
    return instance.get(`/api/appointments/therapist/${therapistId}/schedule?startDate=${start}&endDate=${end}`).then(handleResponse).catch(handleError);
  },

  async findAvailableSlots(serviceId, therapistId, startDate, endDate) {
    if (!serviceId || !startDate || !endDate) {
      throw new Error('ServiceId, StartDate, and EndDate are required to find slots.');
    }
    const params = new URLSearchParams({
      serviceId,
      startDate: startDate instanceof Date ? startDate.toISOString() : startDate,
      endDate: endDate instanceof Date ? endDate.toISOString() : endDate,
    });
    if (therapistId) {
      params.append('therapistId', therapistId);
    }
    return instance.get(`/api/appointments/available-slots?${params.toString()}`).then(handleResponse).catch(handleError);
  },

  async bookAppointment(bookingData) {
    if (!bookingData || !bookingData.clientId || !bookingData.therapistId || !bookingData.serviceId || !bookingData.startTime) {
      throw new Error('Client, Therapist, Service, and StartTime are required for booking.');
    }
    return instance.post('/api/appointments', bookingData).then(handleResponse).catch(handleError);
  },

  async rescheduleAppointment(rescheduleData) {
     if (!rescheduleData || !rescheduleData.appointmentId || !rescheduleData.newStartTime) {
      throw new Error('Appointment ID and New Start Time are required for rescheduling.');
    }
    return instance.put(`/api/appointments/${rescheduleData.appointmentId}/reschedule`, { newStartTime: rescheduleData.newStartTime })
      .then(handleResponse).catch(handleError);
  },

  async cancelAppointment(appointmentId, reason = '') {
     if (!appointmentId) throw new Error('Appointment ID is required for cancellation.');
    const params = new URLSearchParams();
    if (reason) {
        params.append('reason', reason);
    }
    const url = `/api/appointments/${appointmentId}/cancel${reason ? '?' + params.toString() : ''}`;
    return instance.put(url).then(response => response.status === 200 || response.status === 204).catch(handleError);
  },

  // --- Admin Endpoints ---
  async getAdminDashboardStats() {
    // Requires Admin role, interceptor handles token
    return instance.get('/api/admin/dashboard-stats').then(handleResponse).catch(handleError);
  }

};

export default apiClient; 