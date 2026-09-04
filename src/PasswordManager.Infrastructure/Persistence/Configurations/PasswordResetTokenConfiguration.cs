using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Infrastructure.Persistence.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.HasOne(r => r.User)
        .WithMany(u => u.PasswordResetTokens)
        .HasForeignKey(r => r.UserId)
        .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(r => r.TokenHash).IsUnique();
        builder.Property(r => r.TokenHash).IsRequired();
    }
}