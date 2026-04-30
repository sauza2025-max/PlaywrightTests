using Allure.NUnit;
using AngleSharp.Dom;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System;
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
public class BasePlaywrightTest : PageTest
{
    // PageTest (from Microsoft.Playwright.NUnit) gives us:
    // - this.Page       → current browser page
    // - this.Browser    → browser instance
    // - this.Context    → browser context

    protected IPlaywright Playwright;
    protected IBrowser Browser;
    protected IPage Page;
    protected string url = "https://automationexercise.com/login";

    // ✅ Override browser launch options — raise default timeout for slow public test sites
    //public override BrowserNewContextOptions ContextOptions()
    //{
    //    return new BrowserNewContextOptions
    //    {
    //        ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
    //        Locale       = "en-US",
    //        // RecordVideoDir = "videos/" // Uncomment to record videos
    //    };
    //}

    [SetUp]
    public void SetDefaultTimeout()
    {
        // Raise global assertion + action timeout to 60s
        // Public test sites (herokuapp, reqres.in) can be slow
        Page.SetDefaultTimeout(60_000);
        Page.SetDefaultNavigationTimeout(60_000);
    }

    // ✅ Helper: Take screenshot
    protected async Task TakeScreenshotAsync(string name)
    {
        var path = $"screenshot_{name}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = path });
        Console.WriteLine($"📸 Screenshot: {path}");
    }

    [OneTimeSetUp]
    public void GlobalSetup()
    {
        TestEnvironment.WriteEnvironmentProperties("Chrome/Playwright");
    }

    [SetUp]
    public async Task BeforeTest()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new()
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
        });
        Page = await Browser.NewPageAsync();
        await Page.SetViewportSizeAsync(1920, 1080);
        await Page.GotoAsync(url);
    }

    [TearDown]
    public async Task AfterTest()
    {
        await Browser.CloseAsync();
        Playwright.Dispose();
    }
}
