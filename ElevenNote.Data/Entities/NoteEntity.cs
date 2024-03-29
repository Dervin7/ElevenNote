using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevenNote.Data.Entities
{
    public class NoteEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Owner))]
        public int OwnerId { get; set; }
        public UserEntity Owner { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTimeOffset CreateUtc { get; set; }
        public DateTimeOffset? ModifiedUtc { get; set; }
    }
}