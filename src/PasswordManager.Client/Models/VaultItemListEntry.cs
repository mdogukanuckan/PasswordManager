using System.Globalization;

namespace PasswordManager.Client.Models;

public record VaultItemListEntry(
    Guid Id,
    VaultItemPayload Payload,
    DateTime CreatedAt,
    DateTime? ModifiedAt)
{
    public string Initial =>
        string.IsNullOrWhiteSpace(Payload.Title)
            ? "?"
            : Payload.Title.Trim()[0].ToString(CultureInfo.InvariantCulture).ToUpperInvariant();

    public string UpdatedText => FormatRelative(ModifiedAt ?? CreatedAt);

    public string MetaText => $"{Payload.Category} · {UpdatedText}";

    private static string FormatRelative(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Local
            ? value.ToUniversalTime()
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);

        var span = DateTime.UtcNow - utc;

        if (span.TotalMinutes < 1)
        {
            return "az önce";
        }
        if (span.TotalMinutes < 60)
        {
            return $"{(int)span.TotalMinutes} dk önce";
        }
        if (span.TotalHours < 24)
        {
            return $"{(int)span.TotalHours} sa önce";
        }
        if (span.TotalDays < 30)
        {
            return $"{(int)span.TotalDays} gün önce";
        }

        return utc.ToLocalTime().ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
    }
}
