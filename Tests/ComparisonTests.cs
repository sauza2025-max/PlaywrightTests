using Allure.NUnit;
using Allure.NUnit.Attributes;
using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using PlaywrightTests.Utils;
using System.Threading.Tasks;

namespace PlaywrightTests.Tests;

/// <summary>
/// 📚 SELENIUM vs PLAYWRIGHT - SIDE BY SIDE COMPARISON
///
/// This file shows the same actions done in both frameworks
/// so you can see the differences clearly.
/// </summary>
[TestFixture]
[AllureSuite("PlaywrightTest")]
[Category("Login")]
[AllureNUnit] // ← Must be on the concrete fixture class for Allure to capture results
public class ComparisonTests : BasePlaywrightTest
{
    /*
    ╔══════════════════════════════════════════════════════════════╗
    ║              SIDE-BY-SIDE COMPARISON                        ║
    ╠══════════════════════════════════════════════════════════════╣
    ║                                                              ║
    ║  ACTION          │ SELENIUM                │ PLAYWRIGHT      ║
    ║  ────────────────┼─────────────────────────┼──────────────── ║
    ║  Navigate        │ driver.Navigate()       │ page.GotoAsync  ║
    ║                  │   .GoToUrl("url")       │   ("url")       ║
    ║                  │                         │                 ║
    ║  Find element    │ driver.FindElement      │ page.Locator    ║
    ║                  │   (By.Id("x"))          │   ("#x")        ║
    ║                  │                         │                 ║
    ║  Click           │ element.Click()         │ await loc       ║
    ║                  │                         │   .ClickAsync() ║
    ║                  │                         │                 ║
    ║  Type text       │ element.SendKeys("hi")  │ await loc       ║
    ║                  │                         │   .FillAsync    ║
    ║                  │                         │   ("hi")        ║
    ║                  │                         │                 ║
    ║  Get text        │ element.Text            │ await loc       ║
    ║                  │                         │   .InnerText    ║
    ║                  │                         │   Async()       ║
    ║                  │                         │                 ║
    ║  Wait            │ new WebDriverWait(...)  │ Built-in!       ║
    ║                  │   .Until(condition)     │ auto-waits      ║
    ║                  │                         │                 ║
    ║  Assert          │ element.Text            │ await Expect    ║
    ║                  │   .Should().Be("x")     │   (loc)         ║
    ║                  │                         │   .ToHaveText   ║
    ║                  │                         │   Async("x")    ║
    ╚══════════════════════════════════════════════════════════════╝
    */

    // ✅ PLAYWRIGHT VERSION - Login Test
    [Test]
    public async Task Playwright_LoginTest()
    {
        // 1. Navigate
        await Page.GotoAsync("https://practicetestautomation.com/practice-test-login/");

        // 2. Find and fill fields (no explicit wait needed!)
        await Page.FillAsync("#username", "student");
        await Page.FillAsync("#password", "Password123");

        // 3. Click
        await Page.ClickAsync("#submit");

        // 4. Assert with auto-retry built in
        await Expect(Page.Locator(".post-title"))
            .ToContainTextAsync("Logged In Successfully");

        System.Console.WriteLine("✅ Playwright test passed");
    }


    // ✅ LOCATOR STRATEGIES COMPARISON
    [Test]
    public async Task Playwright_AllLocatorStrategies()
    {
        await Page.GotoAsync("https://practicetestautomation.com/practice-test-login/");

        /*
        SELENIUM LOCATORS:          │  PLAYWRIGHT LOCATORS:
        ────────────────────────────┼──────────────────────────────
        By.Id("username")           │  page.Locator("#username")
        By.Name("username")         │  page.Locator("[name=username]")
        By.ClassName("form-control")│  page.Locator(".form-control")
        By.CssSelector("#submit")   │  page.Locator("#submit")
        By.XPath("//button")        │  page.Locator("//button")
        By.LinkText("Click here")   │  page.GetByText("Click here")
        By.TagName("h1")            │  page.Locator("h1")
                                    │
                                    │  PLAYWRIGHT ONLY:
                                    │  page.GetByRole(AriaRole.Button)
                                    │  page.GetByLabel("Username")
                                    │  page.GetByPlaceholder("Username")
                                    │  page.GetByTestId("submit-btn")
        */

        // Playwright preferred: semantic selectors
        var usernameByLabel = Page.GetByLabel("Username");
        var submitByRole    = Page.GetByRole(AriaRole.Button, new() { Name = "Submit" });
        var submitByCss     = Page.Locator("#submit");
        var submitByXpath   = Page.Locator("//button[@id='submit']");

        // All of these point to the same elements
        await Expect(usernameByLabel).ToBeVisibleAsync();
        await Expect(submitByRole).ToBeVisibleAsync();
        await Expect(submitByCss).ToBeVisibleAsync();
        await Expect(submitByXpath).ToBeVisibleAsync();

        System.Console.WriteLine("✅ All locator strategies work");
    }
}
