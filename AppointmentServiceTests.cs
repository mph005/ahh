using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MassageBookingSystem.Services;

namespace MassageBookingSystem.Tests
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _mockAppointmentRepository;
        private readonly Mock<IServiceRepository> _mockServiceRepository;
        private readonly Mock<ITherapistRepository> _mockTherapistRepository;
        private readonly Mock<ILogger<AppointmentService>> _mockLogger;
        private readonly AppointmentService _appointmentService;
        
        public AppointmentServiceTests()
        {
            _mockAppointmentRepository = new Mock<IAppointmentRepository>();
            _mockServiceRepository = new Mock<IServiceRepository>();
            _mockTherapistRepository = new Mock<ITherapistRepository>();
            _mockLogger = new Mock<ILogger<AppointmentService>>();
            
            _appointmentService = new AppointmentService(
                _mockAppointmentRepository.Object,
                _mockServiceRepository.Object,
                _mockTherapistRepository.Object,
                _mockLogger.Object);
        }
        
        [Fact]
        public async Task CreateAppointmentAsync_WithValidData_ReturnsAppointment()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var therapistId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddDays(1);
            
            var createDto = new CreateAppointmentDto
            {
                ClientId = clientId,
                TherapistId = therapistId,
                ServiceId = serviceId,
                StartTime = startTime,
                Notes = "Test appointment"
            };
            
            var service = new Service
            {
                ServiceId = serviceId,
                Name = "Test Service",
                Duration = 60,
                Price = 100
            };
            
            _mockServiceRepository
                .Setup(repo => repo.GetByIdAsync(serviceId))
                .ReturnsAsync(service);
                
            _mockTherapistRepository
                .Setup(repo => repo.IsAvailableAsync(
                    therapistId, 
                    startTime, 
                    startTime.AddMinutes(service.Duration)))
                .ReturnsAsync(true);
                
            _mockAppointmentRepository
                .Setup(repo => repo.AddAsync(It.IsAny<Appointment>()))
                .Returns(Task.CompletedTask);
                
            // Act
            var result = await _appointmentService.CreateAppointmentAsync(createDto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(clientId, result.ClientId);
            Assert.Equal(therapistId, result.TherapistId);
            Assert.Equal(serviceId, result.ServiceId);
            Assert.Equal(startTime, result.StartTime);
            Assert.Equal(startTime.AddMinutes(service.Duration), result.EndTime);
            Assert.Equal(AppointmentStatus.Scheduled, result.Status);
            Assert.Equal("Test appointment", result.Notes);
            
            _mockAppointmentRepository.Verify(
                repo => repo.AddAsync(It.IsAny<Appointment>()), 
                Times.Once);
        }
        
        [Fact]
        public async Task CreateAppointmentAsync_WithPastStartTime_ThrowsArgumentException()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var therapistId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddMinutes(-30); // Past time
            
            var createDto = new CreateAppointmentDto
            {
                ClientId = clientId,
                TherapistId = therapistId,
                ServiceId = serviceId,
                StartTime = startTime,
                Notes = "Test appointment"
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _appointmentService.CreateAppointmentAsync(createDto));
                
            _mockAppointmentRepository.Verify(
                repo => repo.AddAsync(It.IsAny<Appointment>()), 
                Times.Never);
        }
        
        [Fact]
        public async Task CreateAppointmentAsync_WithNonExistentService_ThrowsKeyNotFoundException()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var therapistId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddDays(1);
            
            var createDto = new CreateAppointmentDto
            {
                ClientId = clientId,
                TherapistId = therapistId,
                ServiceId = serviceId,
                StartTime = startTime,
                Notes = "Test appointment"
            };
            
            _mockServiceRepository
                .Setup(repo => repo.GetByIdAsync(serviceId))
                .ReturnsAsync((Service)null);
                
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _appointmentService.CreateAppointmentAsync(createDto));
                
            _mockAppointmentRepository.Verify(
                repo => repo.AddAsync(It.IsAny<Appointment>()), 
                Times.Never);
        }
        
        [Fact]
        public async Task CreateAppointmentAsync_WithUnavailableTherapist_ThrowsInvalidOperationException()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var therapistId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddDays(1);
            
            var createDto = new CreateAppointmentDto
            {
                ClientId = clientId,
                TherapistId = therapistId,
                ServiceId = serviceId,
                StartTime = startTime,
                Notes = "Test appointment"
            };
            
            var service = new Service
            {
                ServiceId = serviceId,
                Name = "Test Service",
                Duration = 60,
                Price = 100
            };
            
            _mockServiceRepository
                .Setup(repo => repo.GetByIdAsync(serviceId))
                .ReturnsAsync(service);
                
            _mockTherapistRepository
                .Setup(repo => repo.IsAvailableAsync(
                    therapistId, 
                    startTime, 
                    startTime.AddMinutes(service.Duration)))
                .ReturnsAsync(false); // Therapist not available
                
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _appointmentService.CreateAppointmentAsync(createDto));
                
            _mockAppointmentRepository.Verify(
                repo => repo.AddAsync(It.IsAny<Appointment>()), 
                Times.Never);
        }
        
        [Fact]
        public async Task CancelAppointmentAsync_WithValidAppointment_CancelsAppointment()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var appointment = new Appointment
            {
                AppointmentId = appointmentId,
                ClientId = Guid.NewGuid(),
                TherapistId = Guid.NewGuid(),
                ServiceId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddDays(2),
                EndTime = DateTime.UtcNow.AddDays(2).AddMinutes(60),
                Status = AppointmentStatus.Scheduled,
                Notes = "Test appointment",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };
            
            _mockAppointmentRepository
                .Setup(repo => repo.GetByIdAsync(appointmentId))
                .ReturnsAsync(appointment);
                
            _mockAppointmentRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<Appointment>()))
                .Returns(Task.CompletedTask);
                
            // Act
            await _appointmentService.CancelAppointmentAsync(appointmentId);
            
            // Assert
            Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
            Assert.NotNull(appointment.UpdatedAt);
            
            _mockAppointmentRepository.Verify(
                repo => repo.UpdateAsync(It.IsAny<Appointment>()), 
                Times.Once);
        }
        
        [Fact]
        public async Task CancelAppointmentAsync_WithNonExistentAppointment_ThrowsKeyNotFoundException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            
            _mockAppointmentRepository
                .Setup(repo => repo.GetByIdAsync(appointmentId))
                .ReturnsAsync((Appointment)null);
                
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _appointmentService.CancelAppointmentAsync(appointmentId));
                
            _mockAppointmentRepository.Verify(
                repo => repo.UpdateAsync(It.IsAny<Appointment>()), 
                Times.Never);
        }
        
        [Fact]
        public async Task CancelAppointmentAsync_WithAlreadyCancelledAppointment_ThrowsInvalidOperationException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var appointment = new Appointment
            {
                AppointmentId = appointmentId,
                ClientId = Guid.NewGuid(),
                TherapistId = Guid.NewGuid(),
                ServiceId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddDays(2),
                EndTime = DateTime.UtcNow.AddDays(2).AddMinutes(60),
                Status = AppointmentStatus.Cancelled, // Already cancelled
                Notes = "Test appointment",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };
            
            _mockAppointmentRepository
                .Setup(repo => repo.GetByIdAsync(appointmentId))
                .ReturnsAsync(appointment);
                
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _appointmentService.CancelAppointmentAsync(appointmentId));
                
            _mockAppointmentRepository.Verify(
                repo => repo.UpdateAsync(It.IsAny<Appointment>()), 
                Times.Never);
        }
        
        [Fact]
        public async Task CancelAppointmentAsync_WithCompletedAppointment_ThrowsInvalidOperationException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var appointment = new Appointment
            {
                AppointmentId = appointmentId,
                ClientId = Guid.NewGuid(),
                TherapistId = Guid.NewGuid(),
                ServiceId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddDays(-2),
                EndTime = DateTime.UtcNow.AddDays(-2).AddMinutes(60),
                Status = AppointmentStatus.Completed, // Already completed
                Notes = "Test appointment",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            };
            
            _mockAppointmentRepository
                .Setup(repo => repo.GetByIdAsync(appointmentId))
                .ReturnsAsync(appointment);
                
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _appointmentService.CancelAppointmentAsync(appointmentId));
                
            _mockAppointmentRepository.Verify(
                repo => repo.UpdateAsync(It.IsAny<Appointment>()), 
                Times.Never);
        }
    }
    
    // Simple mock models for testing
    public class Service
    {
        public Guid ServiceId { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
    }
} 