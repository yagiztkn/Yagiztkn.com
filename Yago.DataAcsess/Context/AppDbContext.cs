using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Yago.Core.Entities;

namespace Yago.DataAcsess.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Veritabanı ilk kurulduğunda içine varsayılan bir Admin ekliyoruz.
            // Şifre: 123456 (Kıyma makinesinden geçmiş hali)
            modelBuilder.Entity<Yago.Core.Entities.Admin>().HasData(new Yago.Core.Entities.Admin
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92"
            });
        }


    }
}
