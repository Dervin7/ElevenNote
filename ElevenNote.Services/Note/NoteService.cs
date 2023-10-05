using System.ComponentModel;
using System.Reflection.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ElevenNote.Models.Note;
using ElevenNote.Data;

namespace ElevenNote.Services.Note
{
    public class NoteService : INoteService
    {
        private readonly int _userId;
        private readonly ApplicationDbContext _dbContext;
        public NoteService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            var userClaims = httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            var value = userClaims.FindFirst("Id")?.Value;
            var validId = int.TryParse(value, out _userId);
            if (!validId)
                throw new Exception("Attempted to build NoteService without User Id claim.");

            _dbContext = dbContext;
        }

        public async Task<bool> CreateNoteAsync(NoteCreate request)
        {
            var noteEntity = new NoteEntity
            {
                Title = request.Title,
                Content = request.Content,
                CreatedUtc = DateTimeOffsetConverter.Now,
                OwnerId = userId
            };

            _dbContext.Notes.Add(noteEntity);

            var numberOfChanges = await _dbContext.SaveChangesAsync();
            return numberOfChanges == 1;
        }

        public async Task<IEnumerable<NoteListItem>> GetAllNotesAsync()
        {
            var notes = await _dbContext.notes
            .Where(entity => entity.OwnerId == _userId)
            .Select(entity => new NoteListItem
            {
                Id = entity.Id,
                Title = entity.Title,
                CreatedUtc = entity.CreatedUtc
            })
            .ToListAsync();

            return notes;
        }

        public async Task<NoteDetail> GetNoteByIdAsync(int noteId)
        {
            var noteEntity = await _dbContext.Notes
            .FirstOrDefaultAsync(e =>
            e.Id == noteId && e.OwnerId == _userId
            );

            return noteEntity is null ? null : new NoteDetail
            {
                Id = noteEntity.Id,
                Title = noteEntity.Title,
                Content = noteEntity.Content,
                CreatedUtc = noteEntity.CreatedUtc,
                ModifiedUtc = noteEntity.ModifiedUtc
            };
        }

        public async Task<bool> UpdateNoteAsync(NoteUpdate request)
        {
            var noteEntity = await _dbContext.Notes.FindAsync(request.Id);

            if (noteEntity?.OwnerId != _userId)
                return false;

            noteEntity.Title = request.Title;
            noteEntity.Content = request.Content;
            noteEntity.ModifiedUtc = DateTimeOffset.Now;

            var numberOfChanges = await _dbContext.SaveChangesAsync();

            return numberOfChanges == 1;
        }
    }
}