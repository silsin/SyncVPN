using System.Runtime.InteropServices;
using SyncVPN.OperatingSystems.WebAuthn.Interop.Marshalers;
using SyncVPN.OperatingSystems.WebAuthn.Interop.StructVersions;

namespace SyncVPN.OperatingSystems.WebAuthn.Interop.Structs;

/// <summary>
/// Information about credential.
/// </summary>
/// <remarks>Corresponds to WEBAUTHN_CREDENTIAL.</remarks>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class CredentialOut
{
    /// <summary>
    /// Version of this structure, to allow for modifications in the future.
    /// </summary>
    private protected CredentialVersion Version { get; set; } = CredentialVersion.Current;

    private int _idLength;


    private ByteArrayOut _id;

    /// <summary>
    /// Well-known credential type specifying what this particular credential is.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Unique ID for this particular credential.
    /// </summary>
    public byte[] Id => _id?.Read(_idLength);

    private CredentialOut() { }
}
