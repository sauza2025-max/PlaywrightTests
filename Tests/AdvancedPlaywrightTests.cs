using Allure.NUnit;
using Allure.NUnit.Attributes;
using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;
using PlaywrightTests.Utils;
using System;
using System.Threading.Tasks;

namespace PlaywrightTests.Tests;

/// <summary>
/// 📚 PLAYWRIGHT ADVANCED CONCEPTS
///
/// Covers:
/// - Network Interception (mock API responses)
/// - Screenshots & Videos
/// - Keyboard & Mouse
/// - Multiple tabs
/// - File Upload/Download
/// - Tracing (record and replay)
/// - Mobile emulation
///
/// PLAYWRIGHT SUPERPOWERS vs Selenium:
/// - Network mocking built-in
/// - Trace viewer (record tests visually)
/// - Auto-wait everywhere
/// - Much faster test execution
/// </summary>
[TestFixture]
[AllureSuite("PlaywrightTest")]
[Category("Login")]
[AllureNUnit]// ← Must be on the concrete fixture class for Allure to capture results
public class AdvancedPlaywrightTests : BasePlaywrightTest
{
    // ✅ TEST 1: Network interception - mock API response
    [Test]
    [AllureTag("playwright")]
    public async Task Network_ShouldInterceptAndMockApiCall()
    {
        // Intercept the API call BEFORE navigating and return fake JSON
        await Page.RouteAsync("**/api/users**", async route =>
        {
            await route.FulfillAsync(new RouteFulfillOptions
            {
                Status      = 200,
                ContentType = "application/json",
                Body        = """[{"id":1,"name":"Mocked User","email":"mock@test.com"}]"""
            });
        });

        // Use a reliable JSON echo site so the intercepted body is rendered as-is
        await Page.GotoAsync("https://reqres.in/api/users",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60_000 });

        var content = await Page.ContentAsync();
        content.Should().Contain("Mocked User");
        System.Console.WriteLine("✅ Network interception works");
    }

    // ✅ TEST 2: Screenshot - full page and element
    [Test]
    [AllureTag("playwright")]
    public async Task Screenshot_ShouldCaptureFullPage()
    {
        await Page.GotoAsync("https://playwright.dev",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60_000 });

        // Full page screenshot
        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path     = "fullpage.png",
            FullPage = true
        });

        // Screenshot of specific element — use nav which is always present on playwright.dev
        var nav = Page.Locator("nav").First;
        await Expect(nav).ToBeVisibleAsync();
        await nav.ScreenshotAsync(new LocatorScreenshotOptions
        {
            Path = "header.png"
        });

        System.Console.WriteLine("✅ Screenshots captured");
    }

    // ✅ TEST 3: Keyboard shortcuts
    [Test]
    [AllureTag("playwright")]
    public async Task Keyboard_ShouldUseShortcuts()
    {
        await Page.GotoAsync("https://the-internet.herokuapp.com/key_presses",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60_000 });

        var input = Page.Locator("#target");
        await Expect(input).ToBeVisibleAsync();

        // Click the input to focus it, then press Enter
        await input.ClickAsync();
        await input.PressAsync("Enter");

        // Wait for #result to show "ENTER" — the page updates it after keyup
        await Expect(Page.Locator("#result"))
            .ToContainTextAsync("ENTER", new LocatorAssertionsToContainTextOptions { Timeout = 15_000 });

        // Press Tab and confirm the result updates
        await input.ClickAsync();
        await input.PressAsync("Tab");
        await Expect(Page.Locator("#result"))
            .ToContainTextAsync("TAB", new LocatorAssertionsToContainTextOptions { Timeout = 15_000 });

        System.Console.WriteLine("✅ Keyboard shortcuts work");
    }

    // ✅ TEST 4: Mouse actions
    [Test]
    [AllureTag("playwright")]
    public async Task Mouse_ShouldHoverAndClick()
    {
        await Page.GotoAsync("https://the-internet.herokuapp.com/hovers",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60_000 });

        var images = Page.Locator(".figure");
        await Expect(images.First).ToBeVisibleAsync();

        await images.Nth(0).HoverAsync();

        var caption = images.Nth(0).Locator(".figcaption");
        await Expect(caption).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 10_000 });

        System.Console.WriteLine("✅ Mouse hover works");
    }

    // ✅ TEST 5: Multiple tabs
    [Test]
    [AllureTag("playwright")]
    public async Task Tabs_ShouldHandleMultipleTabs()
    {
        await Page.GotoAsync("https://the-internet.herokuapp.com/windows",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60_000 });

        // Capture the new page when the link is clicked
        var newPageTask = Context.WaitForPageAsync(new BrowserContextWaitForPageOptions { Timeout = 30_000 });
        await Page.ClickAsync("text=Click Here");
        var newPage = await newPageTask;

        await newPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        newPage.Url.Should().Contain("window");
        await newPage.CloseAsync();

        System.Console.WriteLine("✅ Multi-tab handling works");
    }

    // ✅ TEST 6: File download
    [Test]
    [AllureTag("playwright")]
    public async Task Download_ShouldDownloadFile()
    {
        await Page.GotoAsync("https://the-internet.herokuapp.com/download",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60_000 });

        // Get the first actual file link (not a nav/header link)
        // The download page lists files inside a div.example > a
        var fileLinks = Page.Locator("div.example a");
        await Expect(fileLinks.First).ToBeVisibleAsync();

        // Use a longer timeout — the site can be slow
        var download = await Page.RunAndWaitForDownloadAsync(
            async () => await fileLinks.First.ClickAsync(),
            new PageRunAndWaitForDownloadOptions { Timeout = 60_000 }
        );

        var path = await download.PathAsync();
        path.Should().NotBeNullOrEmpty();
        System.Console.WriteLine($"✅ File downloaded to: {path}");
    }

    // ✅ TEST 7: Tracing (record and replay in Playwright UI)
    [Test]
    [AllureTag("playwright")]
    public async Task Tracing_ShouldRecordTrace()
    {
        await Context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots   = true,
            Sources     = true
        });

        await Page.GotoAsync("https://practicetestautomation.com/practice-test-login/",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60_000 });

        await Expect(Page.Locator("#username")).ToBeVisibleAsync();
        await Page.FillAsync("#username", "student");
        await Page.FillAsync("#password", "Password123");
        await Page.ClickAsync("#submit");

        await Expect(Page.Locator(".post-title")).ToContainTextAsync("Logged In Successfully");

        await Context.Tracing.StopAsync(new TracingStopOptions { Path = "trace.zip" });

        System.Console.WriteLine("✅ Trace saved. View with: playwright show-trace trace.zip");
    }

    // ✅ TEST 8: Mobile emulation
    [Test]
    [AllureTag("playwright")]
    public async Task Mobile_ShouldEmulateIPhone()
    {
        var iphoneContext = await Browser.NewContextAsync(Playwright.Devices["iPhone 13"]);
        var mobilePage = await iphoneContext.NewPageAsync();

        await mobilePage.GotoAsync("https://playwright.dev",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60_000 });

        var viewport = mobilePage.ViewportSize;
        viewport!.Width.Should().BeLessThan(500, "mobile viewport should be narrow");

        await mobilePage.ScreenshotAsync(new PageScreenshotOptions { Path = "mobile.png" });
        await iphoneContext.CloseAsync();

        System.Console.WriteLine("✅ Mobile emulation works");
    }

    // ✅ TEST 9: API testing with Playwright
    [Test]
    [AllureTag("playwright")]
    public async Task Api_ShouldMakeDirectApiCall()
    {
        // Playwright can test APIs directly without a browser!
        var requestContext = await Playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = "https://jsonplaceholder.typicode.com"
        });

        var response = await requestContext.GetAsync("/posts/1");

        response.Status.Should().Be(200);
        var body = await response.JsonAsync();
        body?.GetProperty("id").GetInt32().Should().Be(1);

        System.Console.WriteLine("✅ API testing with Playwright works");
        await requestContext.DisposeAsync();
    }
}
