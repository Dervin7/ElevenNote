using Microsoft.EntityFrameworkCore;
using ElevenNote.Data.Entities;

namespace ElevenNote.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<UserEntity> Users { get; set; }
    }
}