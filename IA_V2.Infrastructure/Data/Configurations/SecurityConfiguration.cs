using IA_V2.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V3.Infrastructure.Data.Configurations
{
    public class SecurityConfiguration : IEntityTypeConfiguration<Security>
    {
        public void Configure(EntityTypeBuilder<Security> builder)
        {
            builder.ToTable("Securities");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Login)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Password)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.Role)
                .HasConversion<string>() 
                .HasMaxLength(50)
                .IsRequired(false);

           //relación 1:1 con User
            builder.HasOne(s => s.User)
                .WithOne(u => u.Security) 
                .HasForeignKey<Security>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false); 

            builder.HasIndex(s => s.Login)
                .IsUnique();

            builder.HasIndex(s => s.UserId);

        }
    }
}
