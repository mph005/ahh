using AutoMapper;
using MassageBooking.API.Models;
using MassageBooking.API.DTOs;

namespace MassageBooking.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Add mappings here
            CreateMap<Appointment, AppointmentDetailsDTO>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => 
                    src.Client != null ? $"{src.Client.FirstName} {src.Client.LastName}".Trim() : null))
                .ForMember(dest => dest.TherapistName, opt => opt.MapFrom(src => 
                    src.Therapist != null ? $"{src.Therapist.FirstName} {src.Therapist.LastName}".Trim() : null))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => 
                    src.Service != null ? src.Service.Name : null))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => 
                    src.Service != null ? src.Service.DurationMinutes : 0)); // Default duration to 0 if service is null

            // Add mapping for Client -> ClientListItemDTO
            CreateMap<Client, ClientListItemDTO>();

            // Add other mappings as needed, for example:
            // CreateMap<Therapist, TherapistListItemDTO>();
            // CreateMap<Service, ServiceDTO>();
        }
    }
} 