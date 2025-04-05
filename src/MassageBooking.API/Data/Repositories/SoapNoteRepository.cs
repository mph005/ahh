using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MassageBooking.API.Models;

namespace MassageBooking.API.Data.Repositories
{
    /// <summary>
    /// Repository for managing SOAP notes
    /// </summary>
    public class SoapNoteRepository : ISoapNoteRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SoapNoteRepository> _logger;

        public SoapNoteRepository(
            ApplicationDbContext context,
            ILogger<SoapNoteRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<SoapNote> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Fetching SOAP note by ID: {Id}", id);
            try
            {
                // Include related data needed for mapping/display
                return await _context.SoapNotes
                                     .Include(sn => sn.Appointment) // Include Appointment
                                         .ThenInclude(a => a.Service) // Then Include Service from Appointment
                                     .Include(sn => sn.Therapist) // Include Therapist
                                     // Removed direct Client include - ClientId is on SoapNote now
                                     // .Include(sn => sn.Client) 
                                     .FirstOrDefaultAsync(sn => sn.SoapNoteId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching SOAP note by ID: {Id}", id);
                throw; // Re-throw the exception to be handled by the caller
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SoapNote>> GetByAppointmentIdAsync(Guid appointmentId)
        {
            _logger.LogInformation("Fetching SOAP notes by Appointment ID: {AppointmentId}", appointmentId);
            return await _context.SoapNotes
                                 .Include(sn => sn.Therapist)
                                 .Include(sn => sn.Appointment).ThenInclude(a => a.Service)
                                 // Removed direct Client include
                                 .Where(sn => sn.AppointmentId == appointmentId)
                                 .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SoapNote>> GetByClientIdAsync(Guid clientId)
        {
            _logger.LogInformation("Fetching SOAP notes by Client ID: {ClientId}", clientId);
            // Filter directly on the ClientId property
            return await _context.SoapNotes
                                 .Include(sn => sn.Therapist)
                                 .Include(sn => sn.Appointment).ThenInclude(a => a.Service)
                                 .Where(sn => sn.ClientId == clientId) 
                                 .OrderByDescending(sn => sn.Appointment.StartTime) // Example ordering
                                 .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SoapNote>> GetByTherapistIdAsync(Guid therapistId)
        {
            _logger.LogInformation("Fetching SOAP notes by Therapist ID: {TherapistId}", therapistId);
            return await _context.SoapNotes
                                 .Include(sn => sn.Appointment).ThenInclude(a => a.Service)
                                 // Removed direct Client include
                                 .Where(sn => sn.TherapistId == therapistId)
                                 .OrderByDescending(sn => sn.Appointment.StartTime) // Example ordering
                                 .ToListAsync();
        }

        /// <inheritdoc />
        public async Task AddAsync(SoapNote soapNote)
        {
             _logger.LogInformation("Adding new SOAP note for Appointment ID: {AppointmentId}", soapNote.AppointmentId);
             if (soapNote == null)
             {
                 throw new ArgumentNullException(nameof(soapNote));
             }
             try
             {
                 _context.SoapNotes.Add(soapNote);
                 await _context.SaveChangesAsync();
                 _logger.LogInformation("Successfully added SOAP note ID: {SoapNoteId}", soapNote.SoapNoteId);
             }
             catch (DbUpdateException ex)
             {
                 _logger.LogError(ex, "Error adding SOAP note ID: {SoapNoteId}", soapNote.SoapNoteId);
                 throw; 
             }
        }

        /// <inheritdoc />
        public async Task UpdateAsync(SoapNote soapNote)
        {
            _logger.LogInformation("Updating SOAP note ID: {SoapNoteId}", soapNote.SoapNoteId);
             if (soapNote == null)
             {
                 throw new ArgumentNullException(nameof(soapNote));
             }
             
            // Ensure the entity is tracked by the context
            var existingNote = await _context.SoapNotes.FindAsync(soapNote.SoapNoteId);
            if (existingNote == null)
            {
                 _logger.LogWarning("Attempted to update non-existent SOAP note ID: {SoapNoteId}", soapNote.SoapNoteId);
                 // Or throw an exception, depending on desired behavior
                 return; 
            }

            // Update the tracked entity's values
            _context.Entry(existingNote).CurrentValues.SetValues(soapNote);
            _context.Entry(existingNote).State = EntityState.Modified;

            try
            {
                 await _context.SaveChangesAsync();
                 _logger.LogInformation("Successfully updated SOAP note ID: {SoapNoteId}", soapNote.SoapNoteId);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                 _logger.LogError(ex, "Concurrency error updating SOAP note ID: {SoapNoteId}", soapNote.SoapNoteId);
                 throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating SOAP note ID: {SoapNoteId}", soapNote.SoapNoteId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting SOAP note ID: {Id}", id);
            var soapNote = await _context.SoapNotes.FindAsync(id);
            if (soapNote != null)
            {
                if (soapNote.IsFinalized)
                {
                    _logger.LogWarning("Attempted to delete finalized SOAP note ID: {Id}", id);
                    throw new InvalidOperationException("Cannot delete a finalized SOAP note.");
                }
                _context.SoapNotes.Remove(soapNote);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted SOAP note ID: {Id}", id);
            }
            else
            {
                _logger.LogWarning("Attempted to delete non-existent SOAP note ID: {Id}", id);
            }
        }

        /// <inheritdoc />
        public async Task<bool> FinalizeAsync(Guid id)
        {
            _logger.LogInformation("Finalizing SOAP note ID: {Id}", id);
            var soapNote = await _context.SoapNotes.FindAsync(id);
            if (soapNote == null)
            {
                 _logger.LogWarning("Attempted to finalize non-existent SOAP note ID: {Id}", id);
                return false;
            }

            if (soapNote.IsFinalized)
            {
                _logger.LogInformation("SOAP note ID: {Id} is already finalized.", id);
                return true; // Already done
            }

            soapNote.IsFinalized = true;
            soapNote.FinalizedAt = DateTime.UtcNow; // Use correct property name
            soapNote.UpdatedAt = DateTime.UtcNow;
            _context.Entry(soapNote).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                 _logger.LogInformation("Successfully finalized SOAP note ID: {Id}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                 _logger.LogError(ex, "Concurrency error finalizing SOAP note ID: {Id}", id);
                 throw;
            }
            catch(Exception ex)
            {
                 _logger.LogError(ex, "Error finalizing SOAP note ID: {Id}", id);
                 return false;
            }
        }
    }
} 