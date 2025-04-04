import React, { useState, useEffect } from 'react';
import { 
  Container, 
  Typography, 
  Box, 
  FormControl, 
  InputLabel, 
  Select, 
  MenuItem,
  Grid,
  Paper,
  Button,
  Snackbar,
  Alert
} from '@mui/material';
import AppointmentCalendarView from '../components/AppointmentCalendarView';
import { fetchServices } from '../api/serviceApi';
import { fetchTherapists } from '../api/therapistApi';

/**
 * Appointment Calendar Page component that displays the weekly calendar view
 * with filtering options for services and therapists.
 */
const AppointmentCalendarPage = () => {
  // State variables
  const [services, setServices] = useState([]);
  const [therapists, setTherapists] = useState([]);
  const [selectedServiceId, setSelectedServiceId] = useState('');
  const [selectedTherapistId, setSelectedTherapistId] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [notification, setNotification] = useState({ open: false, message: '', severity: 'info' });
  
  // Fetch services and therapists
  useEffect(() => {
    const loadData = async () => {
      setLoading(true);
      setError(null);
      
      try {
        // Fetch both services and therapists in parallel
        const [servicesData, therapistsData] = await Promise.all([
          fetchServices(),
          fetchTherapists()
        ]);
        
        setServices(servicesData);
        setTherapists(therapistsData);
      } catch (err) {
        console.error('Error loading data:', err);
        setError('Failed to load services and therapists data. Please try again later.');
      } finally {
        setLoading(false);
      }
    };
    
    loadData();
  }, []);
  
  // Handle service selection
  const handleServiceChange = (event) => {
    setSelectedServiceId(event.target.value);
  };
  
  // Handle therapist selection
  const handleTherapistChange = (event) => {
    setSelectedTherapistId(event.target.value);
  };
  
  // Reset filters
  const handleResetFilters = () => {
    setSelectedServiceId('');
    setSelectedTherapistId('');
  };
  
  // Handle slot selection
  const handleSlotSelect = (slot) => {
    // Show notification when a slot is selected
    setNotification({
      open: true,
      message: `Selected slot on ${new Date(slot.startTime).toLocaleDateString()} at ${new Date(slot.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`,
      severity: 'success'
    });
  };
  
  // Close notification
  const handleCloseNotification = () => {
    setNotification(prev => ({ ...prev, open: false }));
  };
  
  return (
    <Container maxWidth="lg">
      <Box sx={{ my: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Appointment Calendar
        </Typography>
        
        {/* Filters */}
        <Paper sx={{ p: 2, mb: 3 }}>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} md={4}>
              <FormControl fullWidth variant="outlined" size="small">
                <InputLabel id="service-select-label">Service</InputLabel>
                <Select
                  labelId="service-select-label"
                  id="service-select"
                  value={selectedServiceId}
                  onChange={handleServiceChange}
                  label="Service"
                >
                  <MenuItem value="">
                    <em>All Services</em>
                  </MenuItem>
                  {services.map(service => (
                    <MenuItem key={service.id} value={service.id}>
                      {service.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            
            <Grid item xs={12} md={4}>
              <FormControl fullWidth variant="outlined" size="small">
                <InputLabel id="therapist-select-label">Therapist</InputLabel>
                <Select
                  labelId="therapist-select-label"
                  id="therapist-select"
                  value={selectedTherapistId}
                  onChange={handleTherapistChange}
                  label="Therapist"
                >
                  <MenuItem value="">
                    <em>All Therapists</em>
                  </MenuItem>
                  {therapists.map(therapist => (
                    <MenuItem key={therapist.id} value={therapist.id}>
                      {therapist.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            
            <Grid item xs={12} md={4}>
              <Button 
                variant="outlined" 
                onClick={handleResetFilters}
                fullWidth
              >
                Reset Filters
              </Button>
            </Grid>
          </Grid>
        </Paper>
        
        {/* Calendar View */}
        <AppointmentCalendarView 
          serviceId={selectedServiceId || null}
          therapistId={selectedTherapistId || null}
          onSlotSelect={handleSlotSelect}
          initialDate={new Date()}
        />
      </Box>
      
      {/* Notifications */}
      <Snackbar 
        open={notification.open} 
        autoHideDuration={6000} 
        onClose={handleCloseNotification}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert 
          onClose={handleCloseNotification} 
          severity={notification.severity}
          sx={{ width: '100%' }}
        >
          {notification.message}
        </Alert>
      </Snackbar>
    </Container>
  );
};

export default AppointmentCalendarPage; 