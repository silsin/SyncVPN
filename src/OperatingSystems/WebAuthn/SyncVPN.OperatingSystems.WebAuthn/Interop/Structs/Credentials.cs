using System.Runtime.InteropServices;
using SyncVPN.OperatingSystems.WebAuthn.Interop.Marshalers;

namespace SyncVPN.OperatingSystems.WebAuthn.Interop.Structs;

/// <summary>
/// Information about credential list with extra information.
/// </summary>
/// <remarks>Corresponds to WEBAUTHN_CREDENTIALS.</remarks>
[StructLayout(LayoutKind.Sequential)]
public sealed class Credentials : SafeStructArrayIn<CredentialIn>
{
    public Credentials(CredentialIn[] credentials) : base(credentials) { }
}
