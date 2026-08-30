using System.Runtime.InteropServices;
using SyncVPN.OperatingSystems.WebAuthn.Interop.Marshalers;

namespace SyncVPN.OperatingSystems.WebAuthn.Interop.Structs;

/// <summary>
/// Information about Extensions.
/// </summary>
/// <remarks>Corresponds to WEBAUTHN_EXTENSIONS.</remarks>
[StructLayout(LayoutKind.Sequential)]
public class ExtensionsIn : SafeStructArrayIn<ExtensionIn>
{
    public ExtensionsIn(ExtensionIn[] extensions) : base(extensions)
    {
    }
}
