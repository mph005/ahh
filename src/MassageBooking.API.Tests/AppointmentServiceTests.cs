using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MassageBooking.API.Data.Repositories;
using MassageBooking.API.DTOs;
using MassageBooking.API.Models;
using MassageBooking.API.Services;
using Moq;
using AutoMapper;

namespace MassageBooking.API.Tests
{
    [TestClass]
    public class AppointmentServiceTests
    {
        private Mock<IAppointmentRepository> _mockAppointmentRepository;
        private Mock<IServiceRepository> _mockServiceRepository;
        private Mock<ITherapistRepository> _mockTherapistRepository;
        private Mock<IClientRepository> _mockClientRepository;
        private Mock<IEmailService> _mockEmailService;
        private Mock<ILogger<AppointmentService>> _mockLogger;
        private Mock<IMapper> _mockMapper;
        private AppointmentService _appointmentService;
        
        [TestInitialize]
        public void Initialize()
        {
            _mockAppointmentRepository = new Mock<IAppointmentRepository>();
            _mockServiceRepository = new Mock<IServiceRepository>();
            _mockTherapistRepository = new Mock<ITherapistRepository>();
            _mockClientRepository = new Mock<IClientRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _mockLogger = new Mock<ILogger<AppointmentService>>();
            _mockMapper = new Mock<IMapper>();
            
            _appointmentService = new AppointmentService(
                _mockAppointmentRepository.Object,
                _mockServiceRepository.Object,
                _mockTherapistRepository.Object,
                _mockClientRepository.Object,
                _mockEmailService.Object,
                _mockLogger.Object,
                _mockMapper.Object);
        }
        
        [TestMethod]
        public async Task BookAppointmentAsync_WithValidData_ReturnsSuccessfulBookingResult()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var therapistId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddDays(1);
            
            var bookingRequest = new AppointmentBookingDTO
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
            
            var therapist = new Therapist
            {
                TherapistId = therapistId,
                FirstName = "John",
                LastName = "Doe"
            };
            
            var client = new Client
            {
                ClientId = clientId,
                FirstName = "Jane",
                LastName = "Smith"
            };
            
            // Set up the repository method responses
            _mockServiceRepository
                .Setup(repo => repo.GetByIdAsync(serviceId))
                .ReturnsAsync(service);
                
            _mockTherapistRepository
                .Setup(repo => repo.GetByIdAsync(therapistId))
                .ReturnsAsync(therapist);
                
            _mockClientRepository
                .Setup(repo => repo.GetByIdAsync(clientId))
                .ReturnsAsync(client);
            
            _mockTherapistRepository
                .Setup(repo => repo.GetTherapistServicesAsync(therapistId))
                .ReturnsAsync(new List<Service> { service });
                
            _mockAppointmentRepository
                .Setup(repo => repo.HasSchedulingConflictAsync(
                    therapistId, 
                    startTime, 
                    startTime.AddMinutes(service.Duration),
                    null))
                .ReturnsAsync(false);
                
            _mockAppointmentRepository
                .Setup(repo => repo.AddAsync(It.IsAny<Appointment>()))
                .Returns(Task.CompletedTask);
            
            _mockEmailService
                .Setup(email => email.SendAppointmentConfirmationAsync(
                    It.IsAny<Appointment>(),
                    It.IsAny<Client>(),
                    It.IsAny<Therapist>(),
                    It.IsAny<Service>()))
                .ReturnsAsync(true);
                
            // Act
            var result = await _appointmentService.BookAppointmentAsync(bookingRequest);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(Guid.Empty, result.AppointmentId);
            Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
            
            _mockAppointmentRepository.Verify(
                repo => repo.AddAsync(It.IsAny<Appointment>()), 
                Times.Once);
        }
        
        [TestMethod]
        public async Task BookAppointmentAsync_WithNonExistentService_ReturnsFailureResult()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var therapistId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddDays(1);
            
            var bookingRequest = new AppointmentBookingDTO
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
                
            // Act
            var result = await _appointmentService.BookAppointmentAsync(bookingRequest);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.AreEqual("The selected service does not exist.", result.ErrorMessage);
            
            _mockAppointmentRepository.Verify(
                repo => repo.AddAsync(It.IsAny<Appointment>()), 
                Times.Never);
        }
        
        [TestMethod]
        public async Task BookAppointmentAsync_WithNonExistentTherapist_ReturnsFailureResult()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var therapistId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddDays(1);
            
            var bookingRequest = new AppointmentBookingDTO
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
                .Setup(repo => repo.GetByIdAsync(therapistId))
                .ReturnsAsync((Therapist)null);
                
            // Act
            var result = await _appointmentService.BookAppointmentAsync(bookingRequest);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.AreEqual("The selected therapist does not exist.", result.ErrorMessage);
            
            _mockAppointmentRepository.Verify(
                repo => repo.AddAsync(It.IsAny<Appointment>()), 
                Times.Never);
        }
        
        [TestMethod]
        public async Task BookAppointmentAsync_WithSchedulingConflict_ReturnsFailureResult()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var therapistId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddDays(1);
            
            var bookingRequest = new AppointmentBookingDTO
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
            
            var therapist = new Therapist
            {
                TherapistId = therapistId,
                FirstName = "John",
                LastName = "Doe"
            };
            
            var client = new Client
            {
                ClientId = clientId,
                FirstName = "Jane",
                LastName = "Smith"
            };
            
            _mockServiceRepository
                .Setup(repo => repo.GetByIdAsync(serviceId))
                .ReturnsAsync(service);
                
            _mockTherapistRepository
                .Setup(repo => repo.GetByIdAsync(therapistId))
                .ReturnsAsync(therapist);
                
            _mockClientRepository
                .Setup(repo => repo.GetByIdAsync(clientId))
                .ReturnsAsync(client);
            
            _mockTherapistRepository
                .Setup(repo => repo.GetTherapistServicesAsync(therapistId))
                .ReturnsAsync(new List<Service> { service });
                
            _mockAppointmentRepository
                .Setup(repo => repo.HasSchedulingConflictAsync(
                    therapistId, 
                    startTime, 
                    startTime.AddMinutes(service.Duration),
                    null))
                .ReturnsAsync(true); // Conflict exists
                
            // Act
            var result = await _appointmentService.BookAppointmentAsync(bookingRequest);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.AreEqual("The selected time slot is no longer available.", result.ErrorMessage);
            
            _mockAppointmentRepository.Verify(
                repo => repo.AddAsync(It.IsAny<Appointment>()), 
                Times.Never);
        }
        
        [TestMethod]
        public async Task CancelAppointmentAsync_WithValidAppointment_ReturnsTrue()
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
            var result = await _appointmentService.CancelAppointmentAsync(appointmentId, "Testing cancellation") == true;
            
            // Assert
            Assert.IsTrue(result);
            
            _mockAppointmentRepository.Verify(
                repo => repo.UpdateAsync(It.IsAny<Appointment>()), 
                Times.Once);
        }
        
        [TestMethod]
        public async Task CancelAppointmentAsync_WithNonExistentAppointment_ReturnsFalse()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            
            _mockAppointmentRepository
                .Setup(repo => repo.GetByIdAsync(appointmentId))
                .ReturnsAsync((Appointment)null);
                
            // Act
            var result = await _appointmentService.CancelAppointmentAsync(appointmentId, null) == true;
            
            // Assert
            Assert.IsFalse(result);
            
            _mockAppointmentRepository.Verify(
                repo => repo.UpdateAsync(It.IsAny<Appointment>()), 
                Times.Never);
        }
    }
} 