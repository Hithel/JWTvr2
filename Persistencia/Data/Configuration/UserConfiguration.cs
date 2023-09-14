

using Dominio.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistencia.Data.Configuration;
    public class UserConfiguration : IEntityTypeConfiguration<User>
        {
            public void Configure(EntityTypeBuilder<User> builder)
            {
                builder.ToTable("User");
    
                builder.HasKey(e => e.Id);
                builder.Property(e => e.Id)
                .HasMaxLength(3);
    
                builder.Property(e => e.UserName)
                .HasColumnName("UserName")
                .HasColumnType("varchar")
                .IsRequired()
                .HasMaxLength(50);

                builder.Property(e => e.Email)
                .HasColumnName("Email")
                .HasColumnType("varchar")
                .IsRequired()
                .HasMaxLength(150);

                builder.Property(e => e.Password)
                .HasColumnName("Password")
                .HasColumnType("varchar")
                .IsRequired()
                .HasMaxLength(255);

                builder
                .HasMany(p => p.Roles)
                .WithMany(p => p.Users)
                .UsingEntity<UserRol>(
                j => j
                .HasOne(pt => pt.Rol)
                .WithMany(t => t.UserRols)
                .HasForeignKey(pt => pt.RolId),
                j => j
                .HasOne(pt => pt.User)
                .WithMany(t => t.UserRols)
                .HasForeignKey(pt => pt.UserId),
                j => 
                {
                    j.HasKey(t => new {t.UserId, t.RolId});
                    });

                builder.HasMany(p => p.RefreshTokens)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId);
            }
        }
