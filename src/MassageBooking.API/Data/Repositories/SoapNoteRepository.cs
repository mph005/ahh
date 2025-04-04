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
        public async Task<SoapNote> GetByIdAsync(Guid soapNoteId)
        {
            try
            {
                return await _context.SoapNotes
                    .Include(s => s.Appointment)
                    .Include(s => s.Client)
                    .Include(s => s.Therapist)
                    .FirstOrDefaultAsync(s => s.SoapNoteId == soapNoteId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SOAP note with ID {SoapNoteId}", soapNoteId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<SoapNote> GetByAppointmentIdAsync(Guid appointmentId)
        {
            try
            {
                return await _context.SoapNotes
                    .Include(s => s.Appointment)
                    .Include(s => s.Client)
                    .Include(s => s.Therapist)
                    .FirstOrDefaultAsync(s => s.AppointmentId == appointmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SOAP note for appointment {AppointmentId}", appointmentId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<List<SoapNote>> GetByClientIdAsync(Guid clientId)
        {
            try
            {
                return await _context.SoapNotes
                    .Include(s => s.Appointment)
                    .Include(s => s.Therapist)
                    .Where(s => s.ClientId == clientId)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SOAP notes for client {ClientId}", clientId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<List<SoapNote>> GetByTherapistIdAsync(Guid therapistId)
        {
            try
            {
                return await _context.SoapNotes
                    .Include(s => s.Appointment)
                    .Include(s => s.Client)
                    .Where(s => s.TherapistId == therapistId)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SOAP notes for therapist {TherapistId}", therapistId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<SoapNote> AddAsync(SoapNote soapNote)
        {
            try
            {
                // Set creation and update timestamps
                soapNote.CreatedAt = DateTime.UtcNow;
                soapNote.UpdatedAt = DateTime.UtcNow;
                
                await _context.SoapNotes.AddAsync(soapNote);
                await _context.SaveChangesAsync();
                
                return soapNote;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding SOAP note for appointment {AppointmentId}", soapNote.AppointmentId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<SoapNote> UpdateAsync(SoapNote soapNote)
        {
            try
            {
                // Check if the note is already finalized
                var existingNote = await _context.SoapNotes.FindAsync(soapNote.SoapNoteId);
                if (existingNote == null)
                {
                    return null;
                }
                
                if (existingNote.IsFinalized)
                {
                    throw new InvalidOperationException("Cannot update a finalized SOAP note.");
                }
                
                // Update timestamp
                soapNote.UpdatedAt = DateTime.UtcNow;
                
                _context.Entry(existingNote).CurrentValues.SetValues(soapNote);
                await _context.SaveChangesAsync();
                
                return existingNote;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating SOAP note {SoapNoteId}", soapNote.SoapNoteId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> FinalizeAsync(Guid soapNoteId)
        {
            try
            {
                var soapNote = await _context.SoapNotes.FindAsync(soapNoteId);
                if (soapNote == null)
                {
                    return false;
                }
                
                // Mark as finalized
                soapNote.IsFinalized = true;
                soapNote.FinalizedAt = DateTime.UtcNow;
                soapNote.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finalizing SOAP note {SoapNoteId}", soapNoteId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Guid soapNoteId)
        {
            try
            {
                var soapNote = await _context.SoapNotes.FindAsync(soapNoteId);
                if (soapNote == null)
                {
                    return false;
                }
                
                _context.SoapNotes.Remove(soapNote);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting SOAP note {SoapNoteId}", soapNoteId);
                throw;
            }
        }
    }
} 