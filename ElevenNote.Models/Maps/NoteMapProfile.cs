using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ElevenNote.Data.Entities;
using ElevenNote.Models.Note;

namespace ElevenNote.Models.Maps
{
    public class NoteMapProfile : Profile
    {
        public NoteMapProfile()
        {
            CreateMap<NoteEntity, NoteDetail>();
            CreateMap<NoteEntity, NoteListItem>();

            CreateMap<NoteCreate, NoteEntity>()
            .ForMember(Note => note.ModifiedUtc,
            opt => opt.MapFrom(src => DateTimeOffsetConverter.Now)
            );

            CreateMap<NoteUpdate, NoteEntity>()
            .ForMember(note => note.ModifiedUtc,
            opt => opt.MapFrom(src => DateTimeOffset.Now)
            );
        }
    }
}