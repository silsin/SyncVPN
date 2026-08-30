using SyncVPN.OperatingSystems.WebAuthn.Enums;
using SyncVPN.OperatingSystems.WebAuthn.Interop;
using SyncVPN.OperatingSystems.WebAuthn.Interop.Structs;

namespace SyncVPN.OperatingSystems.WebAuthn;

public partial class WebAuthnApi
{
    /// <summary>
    /// Produces an assertion signature representing an assertion by the authenticator that the user has consented to a specific transaction, such as logging in or completing a purchase.
    /// </summary>
    public async Task<AuthenticatorAssertionResponse> AuthenticatorGetAssertionAsync(
        string rpId,
        byte[] challenge,
        UserVerificationRequirement userVerificationRequirement,
        AuthenticatorAttachment authenticatorAttachment = AuthenticatorAttachment.Any,
        int timeoutMilliseconds = ApiConstants.DefaultTimeoutMilliseconds,
        IReadOnlyList<PublicKeyCredentialDescriptor> allowCredentials = null,
        AuthenticationExtensionsClientInputs extensions = null,
        CredentialLargeBlobOperation largeBlobOperation = CredentialLargeBlobOperation.None,
        byte[] largeBlob = null,
        bool browserInPrivateMode = false,
        HybridStorageLinkedData linkedDevice = null,
        WindowHandle windowHandle = default,
        CancellationToken cancellationToken = default
    )
    {
        await using CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(() => { this.CancelCurrentOperation(); });
        return await Task.Run(() => AuthenticatorGetAssertion(
            rpId,
            challenge,
            userVerificationRequirement,
            authenticatorAttachment,
            timeoutMilliseconds,
            allowCredentials,
            extensions,
            largeBlobOperation,
            largeBlob,
            browserInPrivateMode,
            linkedDevice,
            windowHandle
        )).ConfigureAwait(false);
    }
}
