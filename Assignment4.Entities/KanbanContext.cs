using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using Assignment4;
using Assignment4.Core;

namespace Assignment4.Entities
{
    public class KanbanContext : DbContext
    {
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }

        public KanbanContext(DbContextOptions<KanbanContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Task>()
                .Property(s => s.State)
                .HasConversion<string>();

            modelBuilder
                .Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique(true);

            modelBuilder
                .Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique(true);

            modelBuilder
                .Entity<Tag>()
                .HasMany(t => t.Tasks)
                .WithMany(p => p.Tags);
            
            modelBuilder
                .Entity<Task>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Tasks);
        }
    }
}
