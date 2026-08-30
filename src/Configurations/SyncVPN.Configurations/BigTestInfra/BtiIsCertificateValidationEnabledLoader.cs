/*
 * Copyright (c) 2023 Proton AG
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

using SyncVPN.Builds.Variables;
using SyncVPN.Common.Core.Extensions;
using SyncVPN.Common.Core.OperatingSystems.EnvironmentVariables;

namespace SyncVPN.Configurations.BigTestInfra;

public class BtiIsCertificateValidationEnabledLoader
{
    public static bool Get(object? defaultValue)
    {
        return GetCertificateValidation() 
            ?? (defaultValue is null || defaultValue is not bool cv || cv);
    }

    private static bool? GetCertificateValidation()
    {
        bool? btiCertificateValidation = EnvironmentVariableLoader.GetOrNull("BTI_CERT_VALIDATION")?.ToBoolOrNull();
        if (!btiCertificateValidation.HasValue)
        {
            btiCertificateValidation = GlobalConfig.BtiCertificateValidation.ToBoolOrNull();
            if (btiCertificateValidation.HasValue)
            {
                return btiCertificateValidation;
            }
        }
        else
        {
            return btiCertificateValidation;
        }
        return null;
    }
}