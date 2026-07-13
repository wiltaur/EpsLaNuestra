namespace EpsLaNuestra.Domain.Persistence;

public class AuthenticationSettings
{
    public string AuthKey { get; set; } = string.Empty;
    public string AuthIssuer { get; set; } = string.Empty;
    public string AuthAudience { get; set; } = string.Empty;
    public short MinExpire { get; set; }
}