/*
 * Copyright (c) 2024 Proton AG
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

using System.Threading;
using NUnit.Framework;
using SyncVPN.UI.Tests.Robots;
using SyncVPN.UI.Tests.TestBase;
using SyncVPN.UI.Tests.TestsHelper;

namespace SyncVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
[Category("ARM")]
public class LoginSsoTests : FreshSessionSetUp
{
    private const string SSO_LOGIN_ERROR = "Email domain associated to an existing organization. Please sign in with SSO";
    private const string REGULAR_LOGIN_ERROR = "Email domain not found, please sign in with a password";

    [Test]
    [Retry(3)]
    public void LoginWithSso()
    {
        VerifyIsOnLoginPage();

        LoginRobot
            .ClickSignInWithSso()
            .EnterEmail(TestUserData.SsoUser);

        CompleteSsoLogin();
    }

    [Test]
    public void LoginRegularWithSso()
    {
        VerifyIsOnLoginPage();

        LoginRobot
            .Login(TestUserData.SsoUser)
            .Verify.IsErrorMessageDisplayed(SSO_LOGIN_ERROR);
    }

    [Test]
    public void LoginToSsoWithRegular()
    {
        VerifyIsOnLoginPage();

        LoginRobot
            .ClickSignInWithSso()
            .EnterEmail(TestUserData.FakePlusUserWithDomain)
            .ClickSignInButton()
            .Verify.IsErrorMessageDisplayed(REGULAR_LOGIN_ERROR);
    }

    private void CompleteSsoLogin()
    {
        LoginRobot
            .ClickSignInButton()
            .DoLoginSsoWebview(TestUserData.SsoUser.Password);

        NavigationRobot
            .Verify.IsOnMainPage();
    }

    private void VerifyIsOnLoginPage()
    {
        //Delay to allow app to setup unauth session
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        NavigationRobot
            .Verify.IsOnLoginPage();
    }
}