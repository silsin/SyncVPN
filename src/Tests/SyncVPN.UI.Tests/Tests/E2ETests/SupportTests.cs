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

using NUnit.Framework;
using SyncVPN.UI.Tests.TestBase;
using SyncVPN.UI.Tests.TestsHelper;

namespace SyncVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
[Category("ARM")]
public class SupportTests : FreshSessionSetUp
{
    private const string REPORT_ONE = "Connecting to VPN";
    private const string REPORT_TWO = "Browsing speed";
    private const string REPORT_THREE = "Weak or unstable connection";

    [Test]
    [Retry(3)]
    public void SendBugReportViaLoginScreen()
    {
        LoginRobot
            .NavigateToBugReport();
        SupportRobot
            .SelectBugType(REPORT_ONE)
            .ClickContactUs()
            .FillBugReportForm()
            .TickIncludeLogsCheckbox()
            .Verify.IsNoLogsAttachedWarningDisplayed()
            .SendBugReport()
            .Verify.IsSendingSuccessful();
    }

    [Test]
    [Retry(3)]
    public void SendBugReportViaKebabMenuFreeUser()
    {
        CommonUiFlows.FullLogin(TestUserData.FreeUser);

        HomeRobot
            .ExpandKebabMenuButton()
            .ClickOnHelpButton();
        SupportRobot
            .SelectBugType(REPORT_TWO)
            .ClickContactUs()
            .FillBugReportForm()
            .SendBugReport()
            .Verify.IsSendingSuccessful();
    }

    [Test]
    [Retry(3)]
    public void SendBugReportViaSettings()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);

        SettingRobot
            .OpenSettings()
            .OpenBugReportSetting();
        SupportRobot
            .SelectBugType(REPORT_THREE)
            .ClickContactUs()
            .FillBugReportForm()
            .SendBugReport()
            .Verify.IsSendingSuccessful();
    }

    [TearDown]
    public void TearDown()
    {
        BrowserUtils.KillAllBrowsers();
    }
}