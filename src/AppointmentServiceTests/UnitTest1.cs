using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppointmentServiceTests
{
    // Define necessary interfaces and models for testing
    public interface IAppointmentRepository
    {
        Task AddAsync(Appointment appointment);
        Task<Appointment> GetByIdAsync(Guid id);
        Task UpdateAsync(Appointment appointment);
        Task<bool> HasSchedulingConflictAsync(Guid therapistId, DateTime startTime, DateTime endTime, Guid? excludeAppointmentId = null);
    }

    public interface IServiceRepository
    {
        Task<Service> GetByIdAsync(Guid id);
    }

    public interface ITherapistRepository
    {
        Task<Therapist> GetByIdAsync(Guid id);
        Task<IEnumerable<Service>> GetTherapistServicesAsync(Guid therapistId);
    }

    public interface IClientRepository
    {
        Task<Client> GetByIdAsync(Guid id);
    }

    public interface IEmailService
    {
        Task SendAppointmentConfirmationAsync(Appointment appointment, Client client, Therapist therapist, Service service);
    }

    public interface IMapper
    {
        TDestination Map<TSource, TDestination>(TSource source);
    }

    public enum AppointmentStatus
    {
        Scheduled,
        Completed,
        Cancelled,
        NoShow,
        Rescheduled
    }

    public class Appointment
    {
        public Guid AppointmentId { get; set; }
        public Guid ClientId { get; set; }
        public Guid TherapistId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancellationReason { get; set; }
    }

    public class Service
    {
        public Guid ServiceId { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
    }

    public class Therapist
    {
        public Guid TherapistId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class Client
    {
        public Guid ClientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class AppointmentBookingDTO
    {
        public Guid ClientId { get; set; }
        public Guid TherapistId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTime StartTime { get; set; }
        public string Notes { get; set; }
    }

    public class BookingResultDTO
    {
        public bool Success { get; set; }
        public Guid AppointmentId { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class AppointmentDetailsDTO
    {
        public Guid AppointmentId { get; set; }
        public Guid ClientId { get; set; }
        public string ClientName { get; set; }
        public Guid TherapistId { get; set; }
        public string TherapistName { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AppointmentStatus Status { get; set; }
    }

    // Implement the AppointmentService for testing
    public class AppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly ITherapistRepository _therapistRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<AppointmentService> _logger;
        private readonly IMapper _mapper;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IServiceRepository serviceRepository,
            ITherapistRepository therapistRepository,
            IClientRepository clientRepository,
            IEmailService emailService,
            ILogger<AppointmentService> logger,
            IMapper mapper)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _therapistRepository = therapistRepository ?? throw new ArgumentNullException(nameof(therapistRepository));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<BookingResultDTO> BookAppointmentAsync(AppointmentBookingDTO bookingRequest)
        {
            try
            {
                // Validate that the service exists
                var service = await _serviceRepository.GetByIdAsync(bookingRequest.ServiceId);
                if (service == null)
                {
                    return new BookingResultDTO
                    {
                        Success = false,
                        ErrorMessage = "The selected service does not exist."
                    };
                }

                // Validate that the therapist exists
                var therapist = await _therapistRepository.GetByIdAsync(bookingRequest.TherapistId);
                if (therapist == null)
                {
                    return new BookingResultDTO
                    {
                        Success = false,
                        ErrorMessage = "The selected therapist does not exist."
                    };
                }

                // Validate that the therapist offers this service
                var therapistServices = await _therapistRepository.GetTherapistServicesAsync(bookingRequest.TherapistId);
                bool offersService = false;
                foreach (var therapistService in therapistServices)
                {
                    if (therapistService.ServiceId == bookingRequest.ServiceId)
                    {
                        offersService = true;
                        break;
                    }
                }

                if (!offersService)
                {
                    return new BookingResultDTO
                    {
                        Success = false,
                        ErrorMessage = "The selected therapist does not offer this service."
                    };
                }

                // Validate that the client exists
                var client = await _clientRepository.GetByIdAsync(bookingRequest.ClientId);
                if (client == null)
                {
                    return new BookingResultDTO
                    {
                        Success = false,
                        ErrorMessage = "The client does not exist."
                    };
                }

                // Calculate end time based on service duration
                var appointmentEndTime = bookingRequest.StartTime.AddMinutes(service.Duration);

                // Check for scheduling conflicts
                var hasConflict = await _appointmentRepository.HasSchedulingConflictAsync(
                    bookingRequest.TherapistId,
                    bookingRequest.StartTime,
                    appointmentEndTime);

                if (hasConflict)
                {
                    return new BookingResultDTO
                    {
                        Success = false,
                        ErrorMessage = "The selected time slot is no longer available."
                    };
                }

                // Create new appointment
                var appointment = new Appointment
                {
                    AppointmentId = Guid.NewGuid(),
                    ClientId = bookingRequest.ClientId,
                    TherapistId = bookingRequest.TherapistId,
                    ServiceId = bookingRequest.ServiceId,
                    StartTime = bookingRequest.StartTime,
                    EndTime = appointmentEndTime,
                    Status = AppointmentStatus.Scheduled,
                    Notes = bookingRequest.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _appointmentRepository.AddAsync(appointment);

                // Send confirmation email
                try
                {
                    await _emailService.SendAppointmentConfirmationAsync(appointment, client, therapist, service);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send confirmation email for appointment {AppointmentId}", appointment.AppointmentId);
                    // Continue even if email fails
                }

                return new BookingResultDTO
                {
                    Success = true,
                    AppointmentId = appointment.AppointmentId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking appointment: {BookingRequest}", bookingRequest);
                return new BookingResultDTO
                {
                    Success = false,
                    ErrorMessage = "An error occurred while booking the appointment. Please try again later."
                };
            }
        }

        public async Task<bool> CancelAppointmentAsync(Guid appointmentId, string reason)
        {
            try
            {
                var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
                if (appointment == null)
                {
                    return false;
                }

                appointment.Status = AppointmentStatus.Cancelled;
                appointment.CancelledAt = DateTime.UtcNow;
                appointment.CancellationReason = reason;
                appointment.UpdatedAt = DateTime.UtcNow;

                await _appointmentRepository.UpdateAsync(appointment);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment {AppointmentId}", appointmentId);
                return false;
            }
        }
    }

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
                .Returns(Task.CompletedTask);
                
            // Act
            var result = await _appointmentService.BookAppointmentAsync(bookingRequest);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(Guid.Empty, result.AppointmentId);
            Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
            
            _mockAppointmentRepository.Verify(
                repo => repo.AddAsync(It.IsAny<Appointment>()), 
                Times.Once());
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
                Times.Never());
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
            var result = await _appointmentService.CancelAppointmentAsync(appointmentId, "Testing cancellation");
            
            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(AppointmentStatus.Cancelled, appointment.Status);
            Assert.IsNotNull(appointment.CancelledAt);
            Assert.AreEqual("Testing cancellation", appointment.CancellationReason);
            
            _mockAppointmentRepository.Verify(
                repo => repo.UpdateAsync(It.IsAny<Appointment>()), 
                Times.Once());
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
            var result = await _appointmentService.CancelAppointmentAsync(appointmentId, null);
            
            // Assert
            Assert.IsFalse(result);
            
            _mockAppointmentRepository.Verify(
                repo => repo.UpdateAsync(It.IsAny<Appointment>()), 
                Times.Never());
        }
    }
} 