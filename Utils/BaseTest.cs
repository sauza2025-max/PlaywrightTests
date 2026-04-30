using Allure.NUnit;
using AngleSharp.Dom;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using PlaywrightTests.Utils;
using System;
using System.IO;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace Utils;

/// <summary>
/// 📚 BASE TEST CLASS - All Selenium tests inherit from this
///
/// LEARNING NOTES:
/// - [SetUp]             → Runs BEFORE each test
/// - [TearDown]          → Runs AFTER each test — always quits the browser
/// - [NonParallelizable] → Forces tests to run ONE AT A TIME
///                         Without this, NUnit opens one Chrome per test simultaneously
///                         which exhausts system ports and causes hangs
/// - WebDriverWait       → Explicit wait for a specific condition
/// - ImplicitWait        → Global wait on every FindElement call
/// </summary>
[NonParallelizable]  // ← CRITICAL: run Selenium tests sequentially, one browser at a time
[AllureNUnit]
public class BaseTest
{
    protected IWebDriver Driver = null!;
    protected WebDriverWait Wait = null!;
    protected string url = "https://automationexercise.com/login";

    [SetUp]
    public void Setup()
    {
        // ✅ fix — matches ChromeDriver to whatever Chrome is installed
        new DriverManager().SetUpDriver(
            new ChromeConfig(),
            VersionResolveStrategy.MatchingBrowser
        );

        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        options.AddArgument("--disable-notifications");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        // Uncomment to run without a visible browser window (good for CI/CD):
        // options.AddArgument("--headless=new");

        Driver = new ChromeDriver(options);

        // Page load timeout: abort if a page takes longer than 30s to load
        Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        // Implicit wait: retry FindElement for up to 5s before throwing
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));

        Console.WriteLine($"✅ Browser started: {TestContext.CurrentContext.Test.Name}");
    }

    [TearDown]
    public void AfterTest()
    {
        Driver?.Quit();
        Driver?.Dispose();
        Driver = null;
    }

    [TearDown]
    public void Teardown()
    {
        try
        {
            if (TestContext.CurrentContext.Result.Outcome.Status ==
                NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                TakeScreenshot("FAILED");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Screenshot failed: {ex.Message}");
        }
        finally
        {
            // Always quit — even if screenshot throws
            // Without finally, a failed screenshot leaves Chrome open forever
            try { Driver?.Quit(); } catch { /* already closed */ }
            try { Driver?.Dispose(); } catch { /* already disposed */ }
            Console.WriteLine($"🔚 Browser closed: {TestContext.CurrentContext.Test.Name}");
        }
    }

    protected void TakeScreenshot(string name)
    {
        var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
        var path = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"screenshot_{name}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
        screenshot.SaveAsFile(path);
        Console.WriteLine($"📸 Screenshot saved: {path}");
    }

    protected IWebElement WaitForElement(By locator)
    {
        return Wait.Until(
            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(locator));
    }

    protected void ScrollToElement(IWebElement element)
    {
        ((IJavaScriptExecutor)Driver)
            .ExecuteScript("arguments[0].scrollIntoView(true);", element);
    }

    [OneTimeSetUp]
    public void GlobalSetup()
    {
        TestEnvironment.WriteEnvironmentProperties("Chrome/Selenium");    // BaseTest
    }

}
