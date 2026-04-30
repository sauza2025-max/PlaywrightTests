using Allure.NUnit.Attributes;
using FluentAssertions;
using NUnit.Framework;
using PlaywrightTests.Pages;
using PlaywrightTests.Utils;
using System.Threading.Tasks;

namespace PlaywrightTests.Tests;

/// <summary>
/// 📚 PLAYWRIGHT LOGIN TESTS
///
/// LEARNING NOTES:
/// - All Playwright methods are async (use await)
/// - Expect() → Playwright's built-in assertions with auto-retry
/// - No need for explicit waits - Playwright auto-waits
///
/// COMPARISON WITH SELENIUM:
/// Selenium:   driver.FindElement(By.Id("x")).Click();
/// Playwright: await page.Locator("#x").ClickAsync();
/// </summary>
[TestFixture]
[Category("Login")]
public class LoginPlayTests : BasePlaywrightTest
{
    private LoginPlayPage _loginPage = null!;

    [SetUp]
    public async Task SetupLogin()
    {
        _loginPage = new LoginPlayPage(Page);
        await _loginPage.NavigateToAsync();
    }

    // ✅ TEST 1: Valid login
    [Test]
    [AllureTag("playwright")]
    [Category("Smoke")]
    public async Task Login_WithValidCredentials_ShouldSucceed()
    {
        // Act
        await _loginPage.LoginAsync("student", "Password123");

        // Assert - Playwright auto-waits for this condition
        var title = await _loginPage.GetSuccessTitleAsync();
        title.Should().Contain("Logged In Successfully");
    }

    // ✅ TEST 2: Invalid credentials
    [Test]
    [AllureTag("playwright")]
    public async Task Login_WithInvalidCredentials_ShouldShowError()
    {
        await _loginPage.LoginAsync("wronguser", "wrongpass");

        var isErrorVisible = await _loginPage.IsErrorVisibleAsync();
        isErrorVisible.Should().BeTrue();

        var error = await _loginPage.GetErrorMessageAsync();
        error.Should().Contain("Your username is invalid");
    }

    // ✅ TEST 3: Using Playwright's built-in Expect assertions
    [Test]
    [AllureTag("playwright")]
    public async Task Login_WithInvalidPassword_ExpectBuiltIn()
    {
        await _loginPage.LoginAsync("student", "wrongpassword");

        // Playwright Expect: auto-retries until timeout
        await Expect(Page.Locator("#error"))
            .ToBeVisibleAsync();

        await Expect(Page.Locator("#error"))
            .ToContainTextAsync("Your password is invalid");
    }

    // ✅ TEST 4: Data-driven
    [TestCase("student",  "Password123", true)]
    [TestCase("wronguser","Password123", false)]
    [TestCase("student",  "wrongpass",   false)]
    public async Task Login_DataDriven(string user, string pass, bool expectSuccess)
    {
        await _loginPage.LoginAsync(user, pass);

        if (expectSuccess)
        {
            var title = await _loginPage.GetSuccessTitleAsync();
            title.Should().Contain("Logged In Successfully");
        }
        else
        {
            var isError = await _loginPage.IsErrorVisibleAsync();
            isError.Should().BeTrue();
        }
    }
}
