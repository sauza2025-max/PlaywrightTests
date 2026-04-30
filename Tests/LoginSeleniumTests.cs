using Allure.NUnit;
using Allure.NUnit.Attributes;
using FluentAssertions;
using NUnit.Framework;
using PlaywrightTests.Pages;
using PlaywrightTests.Utils;

namespace SeleniumTests.Tests;

/// <summary>
/// 📚 SELENIUM LOGIN TESTS
///
/// LEARNING NOTES:
/// - [Test]           → Marks a method as a test case
/// - [TestCase]       → Data-driven: run same test with different data
/// - [Category]       → Group tests (run by category in CI/CD)
/// - FluentAssertions → Readable assertions: result.Should().Be("expected")
///
/// TEST SITE: https://practicetestautomation.com/practice-test-login/
/// Valid credentials: student / Password123
/// </summary>
[TestFixture]
[AllureSuite("Login")]
[Category("Login")]
[AllureNUnit]  // ← Must be on the concrete fixture class for Allure to capture results
public class LoginSeleniumTests : BaseTest
{
    private LoginSeleniumPage _loginPage = null!;

    [SetUp]
    public new void Setup()
    {
        base.Setup();
        _loginPage = new LoginSeleniumPage(Driver);
        _loginPage.NavigateTo();
    }

    // ✅ TEST 1: Happy path - valid login
    [Test]
    [Category("Smoke")]
    public void Login_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var username = "student";
        var password = "Password123";

        // Act
        _loginPage.Login(username, password);

        // Assert
        var title = _loginPage.GetPageTitle();
        title.Should().Contain("Logged In Successfully",
            because: "valid credentials should grant access");
    }

    // ✅ TEST 2: Invalid username
    [Test]
    [Category("Smoke")]
    public void Login_WithInvalidUsername_ShouldShowError()
    {
        _loginPage.Login("wronguser", "Password123");

        _loginPage.IsErrorDisplayed().Should().BeTrue();
        // The site returns: "Your username is invalid!"
        _loginPage.GetErrorMessage().Should().ContainEquivalentOf("username is invalid");
    }

    // ✅ TEST 3: Invalid password
    [Test]
    [Category("Smoke")]
    public void Login_WithInvalidPassword_ShouldShowError()
    {
        _loginPage.Login("student", "wrongpassword");

        _loginPage.IsErrorDisplayed().Should().BeTrue();
        // The site returns: "Your password is invalid!"
        _loginPage.GetErrorMessage().Should().ContainEquivalentOf("password is invalid");
    }

    // ✅ TEST 4: Data-driven test - multiple invalid combos
    [TestCase("",        "Password123", "username")]
    [TestCase("student", "",            "password")]
    [TestCase("",        "",            "username")]
    [Category("Smoke")]
    public void Login_WithMissingFields_ShouldShowError(
        string username, string password, string expectedError)
    {
        // Act
        _loginPage.Login(username, password);

        // Assert
        _loginPage.IsErrorDisplayed().Should().BeTrue();
    }
}
