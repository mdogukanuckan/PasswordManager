using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.AuthHash).IsRequired();
        builder.Property(u => u.AuthSalt).IsRequired();
        builder.Property(u => u.WrappedVaultKey).IsRequired();
        builder.Property(u => u.WrappedVaultKeyNonce).IsRequired();
        builder.Property(u => u.EncryptionSalt).IsRequired();
    }
}