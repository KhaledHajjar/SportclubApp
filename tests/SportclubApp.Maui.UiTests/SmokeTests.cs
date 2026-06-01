using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;
using SportclubApp.Maui.UiTests.Infrastructure;

namespace SportclubApp.Maui.UiTests;

// Single test that proves the Appium harness can install the APK, launch the
// app on the emulator and find a known element on the Login page. If this
// passes, the rest of the UI flows can build on the same pattern.
public sealed class SmokeTests : IAsyncLifetime
{
    private AndroidDriver _driver = null!;

    public ValueTask InitializeAsync()
    {
        _driver = AppiumDriverFactory.CreateAndroidDriver();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _driver?.Quit();
        _driver?.Dispose();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public void LoginPage_renders_after_app_launch()
    {
        // .NET MAUI 10 maps AutomationId to Android's `resource-id`. XPath on
        // @resource-id is the most reliable selector here — `By.Id` has been
        // unreliable across the various uiautomator2 driver versions.
        var locator = By.XPath(
            $"//*[@resource-id='{AppiumDriverFactory.AppPackage}:id/LoginSubmit']");

        try
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(60));
            var submit = wait.Until(driver => driver.FindElement(locator));

            Assert.NotNull(submit);
            Assert.True(submit.Displayed);
        }
        catch
        {
            DumpForDebug();
            throw;
        }
    }

    private void DumpForDebug()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "AppiumDebug");
        Directory.CreateDirectory(dir);
        var sourcePath = Path.Combine(dir, "page-source.xml");
        var shotPath = Path.Combine(dir, "screen.png");
        try { File.WriteAllText(sourcePath, _driver.PageSource); } catch { }
        try { _driver.GetScreenshot().SaveAsFile(shotPath); } catch { }
        Console.WriteLine($"[debug] Page source: {sourcePath}");
        Console.WriteLine($"[debug] Screenshot: {shotPath}");
    }
}
