using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Infrastructure.Persistence.Configurations;

public class VaultItemConfiguration : IEntityTypeConfiguration<VaultItem>
{
    public void Configure(EntityTypeBuilder<VaultItem> builder)
    {
        builder.HasOne(v => v.User)
                .WithMany(u => u.VaultItems)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        builder.Property(v => v.EncryptedData).IsRequired();
        builder.Property(v => v.Nonce).IsRequired();
    }
}