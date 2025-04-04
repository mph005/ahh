import React, { useState, useEffect, useCallback } from 'react';
import { 
  Paper, 
  Typography, 
  Grid, 
  Box, 
  Button, 
  IconButton, 
  Tooltip, 
  useTheme, 
  useMediaQuery,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Chip,
  CircularProgress
} from '@mui/material';
import {
  ChevronLeft as ChevronLeftIcon,
  ChevronRight as ChevronRightIcon,
  Event as EventIcon,
} from '@mui/icons-material';
import { styled } from '@mui/material/styles';
import { format, addDays, startOfWeek, endOfWeek, isSameDay, isWithinInterval, parseISO } from 'date-fns';
import { useAuth } from '../contexts/AuthContext';

// API service imports
import { 
  fetchAppointments, 
  fetchAvailableSlots, 
  createAppointment 
} from '../api/appointmentApi';

// Constants
const HOURS_IN_DAY = 12; // Show 8AM to 8PM
const START_HOUR = 8; // Starting at 8AM
const TIME_SLOT_HEIGHT = 60; // pixels per hour for styling
const DEFAULT_TIMESLOT_DURATION = 60; // minutes

// Color coding by appointment status
const STATUS_COLORS = {
  Scheduled: '#4caf50', // Green
  Completed: '#2196f3', // Blue
  Cancelled: '#f44336', // Red
  NoShow: '#ff9800',    // Orange
  Available: '#e0e0e0'  // Light gray for available slots
};

// Styled components for calendar elements
const CalendarContainer = styled(Paper)(({ theme }) => ({
  padding: theme.spacing(2),
  height: 'calc(100vh - 140px)',
  overflowY: 'auto',
  [theme.breakpoints.down('sm')]: {
    height: 'calc(100vh - 120px)',
    padding: theme.spacing(1)
  }
}));

const TimeSlot = styled(Box)(({ theme, status, selected }) => ({
  height: `${TIME_SLOT_HEIGHT}px`,
  padding: theme.spacing(1),
  marginBottom: '1px',
  borderRadius: '4px',
  backgroundColor: STATUS_COLORS[status] || STATUS_COLORS.Available,
  cursor: status === 'Available' ? 'pointer' : 'default',
  border: selected ? `2px solid ${theme.palette.primary.dark}` : 'none',
  '&:hover': {
    opacity: status === 'Available' ? 0.8 : 1,
  },
  display: 'flex',
  flexDirection: 'column',
  justifyContent: 'space-between',
  transition: 'all 0.2s ease-in-out',
  [theme.breakpoints.down('sm')]: {
    padding: theme.spacing(0.5),
    fontSize: '0.75rem'
  }
}));

const TimeLabel = styled(Box)(({ theme }) => ({
  width: '60px',
  textAlign: 'right',
  paddingRight: theme.spacing(1),
  fontSize: '0.75rem',
  color: theme.palette.text.secondary,
  [theme.breakpoints.down('sm')]: {
    width: '40px',
    fontSize: '0.7rem',
  }
}));

const DayHeader = styled(Box)(({ theme }) => ({
  textAlign: 'center',
  padding: theme.spacing(1),
  backgroundColor: theme.palette.primary.main,
  color: theme.palette.primary.contrastText,
  borderRadius: '4px 4px 0 0',
  fontWeight: 'bold',
  [theme.breakpoints.down('sm')]: {
    padding: theme.spacing(0.5),
    fontSize: '0.8rem'
  }
}));

/**
 * AppointmentCalendarView component that displays a weekly calendar view
 * of appointments and available time slots.
 */
const AppointmentCalendarView = ({ 
  serviceId = null,
  therapistId = null, 
  onSlotSelect = null,
  initialDate = new Date() 
}) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const { user } = useAuth();
  
  // State variables
  const [currentDate, setCurrentDate] = useState(initialDate);
  const [weekStart, setWeekStart] = useState(startOfWeek(currentDate, { weekStartsOn: 1 })); // Start on Monday
  const [weekDays, setWeekDays] = useState([]);
  const [appointments, setAppointments] = useState([]);
  const [availableSlots, setAvailableSlots] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedSlot, setSelectedSlot] = useState(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  
  // Generate an array of days for the current week
  useEffect(() => {
    const start = startOfWeek(currentDate, { weekStartsOn: 1 });
    setWeekStart(start);
    
    const days = [];
    for (let i = 0; i < 7; i++) {
      days.push(addDays(start, i));
    }
    setWeekDays(days);
  }, [currentDate]);
  
  // Fetch appointments and available slots
  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    
    try {
      // Convert dates to strings for API
      const startDateStr = format(weekStart, 'yyyy-MM-dd');
      const endDateStr = format(endOfWeek(weekStart, { weekStartsOn: 1 }), 'yyyy-MM-dd');
      
      // Fetch both appointments and available slots in parallel
      const [appointmentsData, availableSlotsData] = await Promise.all([
        fetchAppointments(user.id, startDateStr, endDateStr),
        fetchAvailableSlots(serviceId, therapistId, startDateStr, endDateStr)
      ]);
      
      setAppointments(appointmentsData);
      setAvailableSlots(availableSlotsData);
    } catch (err) {
      console.error('Error fetching calendar data:', err);
      setError('Failed to load calendar data. Please try again later.');
    } finally {
      setLoading(false);
    }
  }, [weekStart, user.id, serviceId, therapistId]);
  
  // Fetch data when dependencies change
  useEffect(() => {
    fetchData();
  }, [fetchData]);
  
  // Navigate to previous week
  const handlePreviousWeek = () => {
    setCurrentDate(addDays(weekStart, -7));
  };
  
  // Navigate to next week
  const handleNextWeek = () => {
    setCurrentDate(addDays(weekStart, 7));
  };
  
  // Handle slot selection
  const handleSlotSelect = (slot) => {
    setSelectedSlot(slot);
    if (onSlotSelect) {
      onSlotSelect(slot);
    } else {
      setDialogOpen(true); // Open the dialog if no external handler
    }
  };
  
  // Handle booking confirmation
  const handleBookAppointment = async () => {
    if (!selectedSlot) return;
    
    try {
      // Create appointment DTO
      const appointmentData = {
        clientId: user.id,
        therapistId: selectedSlot.therapistId,
        serviceId: serviceId || selectedSlot.serviceId,
        startTime: selectedSlot.startTime,
        notes: ''
      };
      
      // Call API to create appointment
      await createAppointment(appointmentData);
      
      // Refresh data
      fetchData();
      setDialogOpen(false);
      setSelectedSlot(null);
    } catch (err) {
      console.error('Error booking appointment:', err);
      setError('Failed to book appointment. Please try again later.');
    }
  };
  
  // Check if a time slot is available
  const isSlotAvailable = (day, hour) => {
    const slotDate = new Date(day);
    slotDate.setHours(hour, 0, 0, 0);
    
    return availableSlots.some(slot => {
      const startTime = parseISO(slot.startTime);
      const endTime = parseISO(slot.endTime);
      
      return isWithinInterval(slotDate, {
        start: startTime,
        end: endTime
      });
    });
  };
  
  // Find appointment for a specific time slot
  const findAppointment = (day, hour) => {
    const slotDate = new Date(day);
    slotDate.setHours(hour, 0, 0, 0);
    
    return appointments.find(appointment => {
      const startTime = parseISO(appointment.startTime);
      const endTime = parseISO(appointment.endTime);
      
      // Check if the slot time falls within the appointment
      return isWithinInterval(slotDate, {
        start: startTime,
        end: endTime
      });
    });
  };
  
  // Find available slot for booking
  const findAvailableSlot = (day, hour) => {
    const slotDate = new Date(day);
    slotDate.setHours(hour, 0, 0, 0);
    
    return availableSlots.find(slot => {
      const startTime = parseISO(slot.startTime);
      const endTime = parseISO(slot.endTime);
      
      // Check if the slot time falls within the available slot
      return isWithinInterval(slotDate, {
        start: startTime,
        end: endTime
      });
    });
  };
  
  // Render time slots for a day
  const renderTimeSlots = (day) => {
    const slots = [];
    
    for (let hour = START_HOUR; hour < START_HOUR + HOURS_IN_DAY; hour++) {
      const appointment = findAppointment(day, hour);
      
      if (appointment) {
        // Render booked appointment
        slots.push(
          <TimeSlot 
            key={`${day}-${hour}`} 
            status={appointment.status}
          >
            <Typography variant="caption" sx={{ fontWeight: 'bold' }}>
              {format(parseISO(appointment.startTime), 'h:mm a')}
            </Typography>
            <Typography variant="body2" noWrap>
              {appointment.serviceName}
            </Typography>
            <Typography variant="caption" noWrap>
              {appointment.clientName}
            </Typography>
          </TimeSlot>
        );
      } else if (isSlotAvailable(day, hour)) {
        // Render available slot
        const availableSlot = findAvailableSlot(day, hour);
        slots.push(
          <TimeSlot 
            key={`${day}-${hour}`} 
            status="Available"
            selected={selectedSlot && selectedSlot.startTime === availableSlot.startTime}
            onClick={() => handleSlotSelect(availableSlot)}
          >
            <Typography variant="caption" sx={{ fontWeight: 'bold' }}>
              {format(parseISO(availableSlot.startTime), 'h:mm a')}
            </Typography>
            <Typography variant="body2" align="center">
              Available
            </Typography>
            {availableSlot.therapistId && (
              <Typography variant="caption" noWrap>
                {availableSlot.therapistName}
              </Typography>
            )}
          </TimeSlot>
        );
      } else {
        // Render empty slot
        slots.push(
          <TimeSlot 
            key={`${day}-${hour}`} 
            sx={{ backgroundColor: '#f5f5f5', opacity: 0.5 }}
          >
            <Typography variant="caption" sx={{ color: '#aaa' }}>
              {`${hour > 12 ? hour - 12 : hour}:00 ${hour >= 12 ? 'PM' : 'AM'}`}
            </Typography>
          </TimeSlot>
        );
      }
    }
    
    return slots;
  };
  
  // Render time labels on the left side
  const renderTimeLabels = () => {
    const labels = [];
    
    for (let hour = START_HOUR; hour < START_HOUR + HOURS_IN_DAY; hour++) {
      labels.push(
        <TimeLabel key={hour}>
          {`${hour > 12 ? hour - 12 : hour}:00 ${hour >= 12 ? 'PM' : 'AM'}`}
        </TimeLabel>
      );
    }
    
    return labels;
  };
  
  // Render legend for appointment status colors
  const renderLegend = () => {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', flexWrap: 'wrap', gap: 1, mt: 2 }}>
        {Object.entries(STATUS_COLORS).map(([status, color]) => (
          <Chip
            key={status}
            label={status}
            size="small"
            sx={{ 
              backgroundColor: color, 
              color: status === 'Available' ? 'text.primary' : 'white',
              mb: 1
            }}
          />
        ))}
      </Box>
    );
  };
  
  return (
    <>
      <CalendarContainer elevation={3}>
        {/* Calendar header with navigation */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <IconButton onClick={handlePreviousWeek} aria-label="Previous week">
            <ChevronLeftIcon />
          </IconButton>
          
          <Typography variant={isMobile ? "h6" : "h5"} component="h2">
            {format(weekStart, 'MMMM d')} - {format(endOfWeek(weekStart, { weekStartsOn: 1 }), 'MMMM d, yyyy')}
          </Typography>
          
          <IconButton onClick={handleNextWeek} aria-label="Next week">
            <ChevronRightIcon />
          </IconButton>
        </Box>
        
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '400px' }}>
            <CircularProgress />
          </Box>
        ) : error ? (
          <Box sx={{ textAlign: 'center', color: 'error.main', py: 4 }}>
            <Typography variant="body1">{error}</Typography>
            <Button variant="outlined" onClick={fetchData} sx={{ mt: 2 }}>
              Retry
            </Button>
          </Box>
        ) : (
          <Grid container spacing={1}>
            {/* Time labels column */}
            <Grid item xs={1}>
              <Box sx={{ mt: 5 }}>
                {renderTimeLabels()}
              </Box>
            </Grid>
            
            {/* Day columns */}
            {weekDays.map((day, index) => (
              <Grid item xs={isMobile ? (index < 3 ? 3.6 : 0) : 1.57} key={day.toISOString()} 
                    sx={{ display: isMobile && index >= 3 ? 'none' : 'block' }}>
                <DayHeader>
                  <Typography variant="caption" display="block">
                    {format(day, 'EEE')}
                  </Typography>
                  <Typography variant="body2">
                    {format(day, 'd')}
                  </Typography>
                </DayHeader>
                {renderTimeSlots(day)}
              </Grid>
            ))}
          </Grid>
        )}
        
        {/* Mobile indicators if days are hidden */}
        {isMobile && (
          <Box sx={{ textAlign: 'center', mt: 2 }}>
            <Button 
              startIcon={<ChevronLeftIcon />} 
              endIcon={<ChevronRightIcon />}
              onClick={() => {
                // Implementation for showing remaining days on mobile
                // This would involve state to track which days are visible
              }}
            >
              Swipe for More Days
            </Button>
          </Box>
        )}
        
        {/* Legend for color coding */}
        {renderLegend()}
      </CalendarContainer>
      
      {/* Appointment booking dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Confirm Appointment</DialogTitle>
        <DialogContent>
          {selectedSlot && (
            <Box>
              <Typography variant="body1" gutterBottom>
                <strong>Date:</strong> {format(parseISO(selectedSlot.startTime), 'EEEE, MMMM d, yyyy')}
              </Typography>
              <Typography variant="body1" gutterBottom>
                <strong>Time:</strong> {format(parseISO(selectedSlot.startTime), 'h:mm a')} - {format(parseISO(selectedSlot.endTime), 'h:mm a')}
              </Typography>
              {selectedSlot.therapistName && (
                <Typography variant="body1" gutterBottom>
                  <strong>Therapist:</strong> {selectedSlot.therapistName}
                </Typography>
              )}
              {serviceId && (
                <Typography variant="body1" gutterBottom>
                  <strong>Service:</strong> {/* Would need to fetch service name from context or props */}
                </Typography>
              )}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleBookAppointment} variant="contained" color="primary">
            Book Appointment
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default AppointmentCalendarView; 