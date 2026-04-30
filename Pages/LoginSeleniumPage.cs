using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace SeleniumTests.Pages;

/// <summary>
/// 📚 PAGE OBJECT MODEL (POM) - Login Page
///
/// LEARNING NOTES:
/// - POM separates page structure from test logic
/// - Each page = one class with locators + actions
/// - Tests call page methods, not raw Selenium commands
/// - Makes tests readable: loginPage.Login("user", "pass")
///
/// TARGET: https://practicetestautomation.com/practice-test-login/
/// </summary>
public class LoginSeleniumPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    // ✅ LOCATORS - define once, use everywhere
    private readonly By _usernameInput = By.Id("username");
    private readonly By _passwordInput = By.Id("password");
    private readonly By _loginButton   = By.Id("submit");
    private readonly By _errorMessage  = By.Id("error");
    private readonly By _successMsg    = By.CssSelector(".post-title");

    public LoginSeleniumPage(IWebDriver driver)
    {
        Environment.SetEnvironmentVariable("PLAYWRIGHT_HTML_REPORT", "playwright-report");
        _driver = driver;
        _wait   = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    // ✅ ACTIONS - methods that interact with the page
    public void NavigateTo()
    {
        _driver.Navigate().GoToUrl("https://practicetestautomation.com/practice-test-login/");
    }

    public void EnterUsername(string username)
    {
        var field = _wait.Until(d => d.FindElement(_usernameInput));
        field.Clear();
        field.SendKeys(username);
    }

    public void EnterPassword(string password)
    {
        _driver.FindElement(_passwordInput).Clear();
        _driver.FindElement(_passwordInput).SendKeys(password);
    }

    public void ClickLogin()
    {
        _driver.FindElement(_loginButton).Click();
    }

    // ✅ COMPOUND ACTION - combines multiple steps
    public void Login(string username, string password)
    {
        EnterUsername(username);
        EnterPassword(password);
        ClickLogin();
    }

    // ✅ GETTERS - return page state for assertions
    public string GetErrorMessage()
    {
        // Wait until the error element is visible AND has non-empty text
        // This avoids StaleElementReferenceException after a page transition
        return _wait.Until(d =>
        {
            try
            {
                var el = d.FindElement(_errorMessage);
                var text = el.Text;
                return el.Displayed && !string.IsNullOrWhiteSpace(text) ? text : null;
            }
            catch (StaleElementReferenceException)
            {
                return null; // retry
            }
        }) ?? string.Empty;
    }

    public string GetPageTitle()
    {
        return _wait.Until(d =>
        {
            try
            {
                var el = d.FindElement(_successMsg);
                var text = el.Text;
                return el.Displayed && !string.IsNullOrWhiteSpace(text) ? text : null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        }) ?? string.Empty;
    }

    public bool IsErrorDisplayed()
    {
        try
        {
            return _wait.Until(d =>
            {
                try { return d.FindElement(_errorMessage).Displayed; }
                catch (NoSuchElementException) { return false; }
                catch (StaleElementReferenceException) { return false; }
            });
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }
}
