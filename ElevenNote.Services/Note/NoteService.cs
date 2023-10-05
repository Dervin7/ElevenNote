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
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _dbContext;
        public NoteService(IHttpContextAccessor httpContextAccessor, IMapper mapper, ApplicationDbContext dbContext)
        {
            var userClaims = httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            var value = userClaims.FindFirst("Id")?.Value;
            var validId = int.TryParse(value, out _userId);
            if (!validId)
                throw new Exception("Attempted to build NoteService without User Id claim.");

            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<bool> CreateNoteAsync(NoteCreate request)
        {
            var noteEntity = _mapper.Map<NoteCreate, NoteEntity>(request, opt =>
            opt.AfterMap((src, dest) => dest.OwnerId = _userId)
            );
            {

                _dbContext.Notes.Add(noteEntity);

                var numberOfChanges = await _dbContext.SaveChangesAsync();
                return numberOfChanges == 1;
            }

            public async Task<IEnumerable<NoteListItem>> GetAllNotesAsync()
            {
                var notes = await _dbContext.notes
                .Where(entity => entity.OwnerId == _userId)
                .Select(entity => _mapper.Map<NoteListItem>(entity))
                .ToListAsync();

                return notes;
            }

            public async Task<NoteDetail> GetNoteByIdAsync(int noteId)
            {
                var noteEntity = await _dbContext.Notes
                .FirstOrDefaultAsync(e =>
                e.Id == noteId && e.OwnerId == _userId
                );

                return noteEntity is null ? null : _mapper.Map<NoteDetail>(noteEntity);
            }

            public async Task<bool> UpdateNoteAsync(NoteUpdate request)
            {
                var noteIsUserOwned = await _dbContext.Notes.AnyAsync(note =>
                note.Id == request.Id && note.OwnerId == _userId
                );

                if (!noteIsUserOwned)
                    return false;

                var newEntity = _mapper.Map<NoteUpdate, NoteEntity>(request, opt =>
                opt.AfterMap((src, dest) => dest.OwnerId = _userId)
                );

                _dbContext.Entry(newEntity).State = EntityState.Modified;

                _dbContext.Entry(newEntity).Property(e => e.CreatedUtc).IsModified = false;

                var numberOfChanges = await _dbContext.SaveChangesAsync();

                return numberOfChanges == 1;
            }

            public async Task<bool> DeleteNoteAsync(int noteId)
            {
                var noteEntity = await _dbContext.Notes.FindAsync(noteId);

                if (noteEntity?.OwnerId != _userId)
                    return false;

                _dbContext.Notes.Remove(noteEntity);
                return await _dbContext.SaveChangesAsync() == 1;
            }
        }
    }