using Microsoft.Playwright;
using System.Threading.Tasks;

namespace PlaywrightTests.Pages;

/// <summary>
/// 📚 PLAYWRIGHT PAGE OBJECT - Login Page
///
/// LEARNING NOTES:
/// - Playwright uses CSS selectors, text selectors, role selectors
/// - GetByRole()  → semantic selector (ARIA roles) - PREFERRED
/// - GetByText()  → find by visible text
/// - GetByLabel() → find form fields by label text
/// - Locator      → lazy element reference (doesn't throw until acted on)
///
/// TARGET: https://practicetestautomation.com/practice-test-login/
/// </summary>
public class LoginPage
{
    private readonly IPage _page;

    // ✅ LOCATORS using Playwright's powerful selectors
    private ILocator UsernameInput => _page.GetByLabel("Username");
    private ILocator PasswordInput => _page.GetByLabel("Password");
    private ILocator LoginButton   => _page.GetByRole(AriaRole.Button, new() { Name = "Submit" });
    private ILocator ErrorMessage  => _page.Locator("#error");
    private ILocator SuccessTitle  => _page.Locator(".post-title");

    public LoginPage(IPage page) => _page = page;

    public async Task NavigateToAsync()
    {
        await _page.GotoAsync("https://practicetestautomation.com/practice-test-login/");
    }

    public async Task LoginAsync(string username, string password)
    {
        await UsernameInput.WaitForAsync();
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
    }

    public async Task<string> GetErrorMessageAsync()
    {
        await ErrorMessage.WaitForAsync();
        return await ErrorMessage.InnerTextAsync();
    }

    public async Task<string> GetSuccessTitleAsync()
    {
        await SuccessTitle.WaitForAsync();
        return await SuccessTitle.InnerTextAsync();
    }

    public async Task<bool> IsErrorVisibleAsync()
    {
        return await ErrorMessage.IsVisibleAsync();
    }
}
