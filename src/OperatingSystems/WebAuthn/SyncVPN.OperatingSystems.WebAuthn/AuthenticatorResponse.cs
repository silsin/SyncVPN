using System.Text.Json.Serialization;
using SyncVPN.OperatingSystems.WebAuthn.Serialization;

namespace SyncVPN.OperatingSystems.WebAuthn;

/// <summary>
/// Authenticators respond to Relying Party requests by returning an object derived from the AuthenticatorResponse class.
/// </summary>
public abstract class AuthenticatorResponse
{
    /// <summary>
    /// This attribute contains a JSON-compatible serialization of the client data.
    /// </summary>
    [JsonPropertyName("clientDataJSON")]
    [JsonConverter(typeof(Base64UrlConverter))]
    public byte[] ClientDataJson { get; set; }
}
