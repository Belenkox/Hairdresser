using Hairdresser.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hairdresser.Data
{
    public class HairdresserContext : DbContext
    {
        public HairdresserContext(DbContextOptions<HairdresserContext> options) : base(options)
        {

        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>().ToTable("Client");
            modelBuilder.Entity<Employee>().ToTable("Employee");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Roles>().ToTable("Role");
        }
    }
}
