using Allure.Net.Commons;
using Allure.NUnit;
using AngleSharp.Dom;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PlaywrightTests.Utils;

/// <summary>
/// 📚 PLAYWRIGHT BASE TEST
///
/// LEARNING NOTES:
/// - Playwright uses async/await (modern C# pattern)
/// - IPage     → represents a browser tab
/// - IBrowser  → the browser instance
/// - IContext  → isolated browser session (like incognito)
///
/// KEY DIFFERENCES FROM SELENIUM:
/// - Auto-waits: no need for explicit Thread.Sleep or WebDriverWait
/// - Built-in retry logic for assertions
/// - Faster and more reliable than Selenium
/// - Can intercept network requests
///
/// WHY [AllureNUnit] is on each concrete test class (NOT here):
/// - Allure.NUnit 2.x crashes with "No container context is active" if [AllureNUnit]
///   appears on BOTH a base class and a subclass (double-registration bug).
/// - Keeping it only on the concrete [TestFixture] classes avoids this.
/// </summary>
/// 
[AllureNUnit]
public abstract class BasePlaywrightTest
{
    protected IPlaywright Playwright;
    protected IBrowser Browser;
    protected IPage Page;
    protected IBrowserContext Context;    // ✅ add this

    protected string url = "https://automationexercise.com/login";

    [OneTimeSetUp]                                    // ✅ runs once, sync — safe for Allure
    public void GlobalSetup()
    {
        TestEnvironment.WriteEnvironmentProperties("Chrome/Playwright");
    }

    [SetUp]
    public async Task BeforeTest()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
        });

        Context = await Browser.NewContextAsync();   // ✅ must be before Page
        Page = await Context.NewPageAsync();       // ✅ Page from Context not Browser
        await Page.GotoAsync(url);
    }

    [TearDown]                                        // ✅ runs per test, async
    public async Task AfterTest()
    {
        var status = TestContext.CurrentContext.Result.Outcome.Status;

        if (status == TestStatus.Failed && Page != null)
        {
            try
            {
                var screenshotDir = Path.Combine(
                    TestContext.CurrentContext.WorkDirectory, "screenshots"
                );
                Directory.CreateDirectory(screenshotDir);

                var screenshotPath = Path.Combine(
                    screenshotDir,
                    $"{TestContext.CurrentContext.Test.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.png"
                );

                await Page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = screenshotPath,
                    FullPage = false
                });

                AllureApi.AddAttachment(
                    name: "Screenshot on failure",
                    type: "image/png",
                    path: screenshotPath
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Screenshot failed: {ex.Message}");
            }
        }

        await Browser.CloseAsync();
        Playwright.Dispose();
    }

    // ✅ Helper: Take screenshot
    protected async Task TakeScreenshotAsync(string name)
    {
        var path = $"screenshot_{name}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = path });
        Console.WriteLine($"📸 Screenshot: {path}");
    }
}
