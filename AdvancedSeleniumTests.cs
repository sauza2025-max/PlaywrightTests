using Allure.NUnit;
using Allure.NUnit.Attributes;
using FluentAssertions;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;

namespace SeleniumTests.Tests;

/// <summary>
/// 📚 SELENIUM ADVANCED CONCEPTS
///
/// Covers:
/// - Explicit vs Implicit Waits
/// - Dropdowns (Select element)
/// - Alerts / Popups
/// - iFrames
/// - Keyboard & Mouse Actions
/// - JavaScript Executor
///
/// TEST SITE: https://the-internet.herokuapp.com/
/// </summary>
[TestFixture]
[Category("Advanced")]
[AllureNUnit]  // ← Must be on the concrete fixture class for Allure to capture results
public class AdvancedSeleniumTests : BaseTest
{
    private const string BaseUrl = "https://the-internet.herokuapp.com";

    // ✅ TEST 1: Dropdowns
    [Test]
    [AllureTag("playwright")]
    public void Dropdown_ShouldSelectOptionByText()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/dropdown");

        // Find the <select> element and wrap it
        var dropdownElement = Driver.FindElement(By.Id("dropdown"));
        var dropdown = new SelectElement(dropdownElement);

        // Select by visible text
        dropdown.SelectByText("Option 1");
        dropdown.SelectedOption.Text.Should().Be("Option 1");

        // Select by value attribute
        dropdown.SelectByValue("2");
        dropdown.SelectedOption.Text.Should().Be("Option 2");

        System.Console.WriteLine("✅ Dropdown selection works");
    }

    // ✅ TEST 2: JavaScript Alert
    [Test]
    [AllureTag("playwright")]
    public void Alert_ShouldAcceptJsAlert()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/javascript_alerts");

        // Click button that triggers JS alert
        Driver.FindElement(By.XPath("//button[text()='Click for JS Alert']")).Click();

        // Switch to alert and accept it
        var alert = Driver.SwitchTo().Alert();
        var alertText = alert.Text;
        alert.Accept();

        alertText.Should().Contain("I am a JS Alert");
        System.Console.WriteLine($"✅ Alert text was: {alertText}");
    }

    // ✅ TEST 3: Confirm dialog - dismiss
    [Test]
    [AllureTag("playwright")]
    public void Alert_ShouldDismissConfirmDialog()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/javascript_alerts");

        Driver.FindElement(By.XPath("//button[text()='Click for JS Confirm']")).Click();

        var alert = Driver.SwitchTo().Alert();
        alert.Dismiss(); // Click Cancel

        var result = Driver.FindElement(By.Id("result")).Text;
        result.Should().Contain("Cancel");
    }

    // ✅ TEST 4: iFrames
    [Test]
    [AllureTag("selenium")]
    public void IFrame_ShouldSwitchAndInteract()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/iframe");

        // TinyMCE's frame ID is dynamic (mce_0_ifr, mce_1_ifr, etc.)
        // Use the iframe tag itself to locate it reliably
        var frameElement = Wait.Until(
            SeleniumExtras.WaitHelpers.ExpectedConditions
                .ElementExists(By.CssSelector("iframe[id$='_ifr']")));

        Driver.SwitchTo().Frame(frameElement);

        var body = Driver.FindElement(By.Id("tinymce"));
        body.Clear();
        body.SendKeys("Hello from inside the iframe!");

        body.Text.Should().Contain("Hello from inside the iframe!");

        Driver.SwitchTo().DefaultContent();
        System.Console.WriteLine("✅ iFrame interaction works");
    }

    // ✅ TEST 5: Keyboard actions
    [Test]
    [AllureTag("playwright")]
    public void Keyboard_ShouldUseKeyboardShortcuts()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/key_presses");

        var input = Driver.FindElement(By.Id("target"));

        // Press individual keys
        input.SendKeys(Keys.Enter);
        Driver.FindElement(By.Id("result")).Text.Should().Contain("ENTER");

        input.SendKeys(Keys.Tab);
        Driver.FindElement(By.Id("result")).Text.Should().Contain("TAB");
    }

    // ✅ TEST 6: Mouse hover (Actions class)
    [Test]
    [AllureTag("playwright")]
    public void Mouse_ShouldHoverOverElement()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/hovers");

        var images = Driver.FindElements(By.CssSelector(".figure"));
        var actions = new Actions(Driver);

        // Hover over first image
        actions.MoveToElement(images[0]).Perform();

        var caption = images[0].FindElement(By.CssSelector(".figcaption"));
        caption.Displayed.Should().BeTrue("caption should show on hover");
    }

    // ✅ TEST 7: JavaScript Executor
    [Test]
    [AllureTag("playwright")]
    public void JavaScript_ShouldExecuteScript()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/large");

        var js = (IJavaScriptExecutor)Driver;

        // Scroll to bottom of page
        js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");

        // Get page title via JS
        var title = js.ExecuteScript("return document.title;") as string;
        title.Should().NotBeNullOrEmpty();

        // Highlight an element via JS
        var element = Driver.FindElement(By.TagName("h1"));
        js.ExecuteScript("arguments[0].style.border='3px solid red'", element);

        System.Console.WriteLine($"✅ JS executed, page title: {title}");
    }

    // ✅ TEST 8: Explicit Wait - dynamic content
    [Test]
    [AllureTag("playwright")]
    public void Wait_ShouldWaitForDynamicElement()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/dynamic_loading/1");

        Driver.FindElement(By.CssSelector("#start button")).Click();

        // Wait until the finish element is visible
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
        var finish = wait.Until(d =>
        {
            var el = d.FindElement(By.Id("finish"));
            return el.Displayed ? el : null;
        });

        finish!.Text.Should().Contain("Hello World!");
        System.Console.WriteLine("✅ Dynamic element loaded successfully");
    }

    // ✅ TEST 9: Multiple windows/tabs
    [Test]
    [AllureTag("playwright")]
    public void Windows_ShouldHandleMultipleTabs()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/windows");

        var originalWindow = Driver.CurrentWindowHandle;

        // Click link that opens new window
        Driver.FindElement(By.LinkText("Click Here")).Click();

        // Wait for new window
        Wait.Until(d => d.WindowHandles.Count == 2);

        // Switch to new window
        foreach (var handle in Driver.WindowHandles)
        {
            if (handle != originalWindow)
                Driver.SwitchTo().Window(handle);
        }

        Driver.Title.Should().Contain("New Window");

        // Close new window and switch back
        Driver.Close();
        Driver.SwitchTo().Window(originalWindow);
        Driver.Title.Should().Contain("The Internet");
    }
}
