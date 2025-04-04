using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassageBooking.API.Models;

namespace MassageBooking.API.Data.Repositories
{
    /// <summary>
    /// Repository interface for managing SOAP notes
    /// </summary>
    public interface ISoapNoteRepository
    {
        /// <summary>
        /// Gets a SOAP note by its ID
        /// </summary>
        /// <param name="soapNoteId">The SOAP note ID</param>
        /// <returns>The SOAP note or null if not found</returns>
        Task<SoapNote> GetByIdAsync(Guid soapNoteId);
        
        /// <summary>
        /// Gets a SOAP note for a specific appointment
        /// </summary>
        /// <param name="appointmentId">The appointment ID</param>
        /// <returns>The SOAP note or null if not found</returns>
        Task<SoapNote> GetByAppointmentIdAsync(Guid appointmentId);
        
        /// <summary>
        /// Gets all SOAP notes for a client
        /// </summary>
        /// <param name="clientId">The client ID</param>
        /// <returns>List of SOAP notes for the client</returns>
        Task<List<SoapNote>> GetByClientIdAsync(Guid clientId);
        
        /// <summary>
        /// Gets all SOAP notes created by a therapist
        /// </summary>
        /// <param name="therapistId">The therapist ID</param>
        /// <returns>List of SOAP notes created by the therapist</returns>
        Task<List<SoapNote>> GetByTherapistIdAsync(Guid therapistId);
        
        /// <summary>
        /// Adds a new SOAP note
        /// </summary>
        /// <param name="soapNote">The SOAP note to add</param>
        /// <returns>The added SOAP note</returns>
        Task<SoapNote> AddAsync(SoapNote soapNote);
        
        /// <summary>
        /// Updates an existing SOAP note
        /// </summary>
        /// <param name="soapNote">The SOAP note to update</param>
        /// <returns>The updated SOAP note</returns>
        Task<SoapNote> UpdateAsync(SoapNote soapNote);
        
        /// <summary>
        /// Finalizes a SOAP note, making it read-only
        /// </summary>
        /// <param name="soapNoteId">The SOAP note ID</param>
        /// <returns>True if successful, false if not found</returns>
        Task<bool> FinalizeAsync(Guid soapNoteId);
        
        /// <summary>
        /// Deletes a SOAP note (soft delete)
        /// </summary>
        /// <param name="soapNoteId">The SOAP note ID</param>
        /// <returns>True if successful, false if not found</returns>
        Task<bool> DeleteAsync(Guid soapNoteId);
    }
} 