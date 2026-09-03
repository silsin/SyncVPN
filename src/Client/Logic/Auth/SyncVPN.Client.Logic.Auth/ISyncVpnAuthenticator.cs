/*
 * Copyright (c) 2026 Proton AG
 *
 * This file is part of SyncVPN.
 *
 * SyncVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * SyncVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with SyncVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Security;
using SyncVPN.Client.Logic.Auth.Contracts.Models;

namespace SyncVPN.Client.Logic.Auth;

// Mirrors ISrpAuthenticator's shape so UserAuthenticator can delegate to either behind the same
// AuthResult contract. Deliberately narrower: no security-key/WebAuthn support (Proton-specific,
// not part of the new API), and no post-login orchestration (VPN plan/certificate/feature flags) -
// this is credential exchange only, for internal testing behind BackendCapability.Auth. See the
// migration plan's Phase 4 scoping note.
public interface ISyncVpnAuthenticator
{
    bool HasAuthenticatedSessionData();

    Task<AuthResult> LoginUserAsync(string username, SecureString password, CancellationToken cancellationToken);

    // Exchanges a 16-character login_code (e.g. from a POST /purchases guest response) for a device
    // token - the code-login counterpart to LoginUserAsync's email+password flow.
    Task<AuthResult> LoginWithCodeAsync(string code, CancellationToken cancellationToken);

    Task<AuthResult> SendTwoFactorCodeAsync(string code, CancellationToken cancellationToken);

    Task<AuthResult> ValidateSessionAsync(CancellationToken cancellationToken);

    Task LogoutAsync(CancellationToken cancellationToken);
}
