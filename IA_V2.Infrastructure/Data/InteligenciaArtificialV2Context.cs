using IA_V2.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Infrastructure.Data
{
    public partial class InteligenciaArtificialV2Context : DbContext
    {
        public InteligenciaArtificialV2Context() { }
        public InteligenciaArtificialV2Context(DbContextOptions<InteligenciaArtificialV2Context> options) : base(options) { }

        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Text> Texts { get; set; } = null!;
        public virtual DbSet<Prediction> Predictions { get; set; } = null!;
        public virtual DbSet<Security> Securities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=LAPTOP-UA1C57E6;Database=IAV1_Db;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }
    }
}
