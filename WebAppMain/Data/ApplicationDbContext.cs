using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using WebAppMain.Models;

namespace WebAppMain.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<Journal> Journal { get; set; }
        public DbSet<Predmet> Predmet { get; set; }
        public DbSet<Estimation> Estimation { get; set; }
        public DbSet<Class> Class { get; set; }
        public DbSet<JournalPredmet> JournalPredmet { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Assignment> Assignment { get; set; }

        public DbSet<Notification> Notification { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);// Задает схему для базы данных
            builder.HasDefaultSchema("Identity");   /* Переименовывает таблицу пользователей из AspNetUsers в Identity.User. */
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable(name: "User");
            });
            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "Role");
            });
            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UserRoles");
            });
            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UserClaims");
            });
            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UserLogins");
            });
            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RoleClaims");
            });
            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UserTokens");
            });

            builder.Entity<Assignment>()
                .HasOne(h => h.Class)
                .WithMany(v => v.Assignment)
                .HasForeignKey(v => v.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasOne(h => h.Class)
                .WithMany(v => v.ApplicationUser)
                .HasForeignKey(v => v.Id_Class)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
               .HasOne(u => u.Journal)
               .WithOne(j => j.User)
               .HasForeignKey<Journal>(j => j.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<JournalPredmet>()
                .HasKey(jp => jp.Id);

            builder.Entity<JournalPredmet>()
                .HasOne(jp => jp.Journal)
                .WithMany(j => j.JournalPredmets)
                .HasForeignKey(jp => jp.JournalId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<JournalPredmet>()
                .HasOne(jp => jp.Predmet)
                .WithMany(p => p.JournalPredmets)
                .HasForeignKey(jp => jp.PredmetId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Estimation>()
                .HasOne(e => e.JournalPredmet)
                .WithMany(jp => jp.Estimations)
                .HasForeignKey(e => e.JournalPredmetId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Note>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notes)
                .HasForeignKey(n => n.UserId);


            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId);

        }


    }
}

