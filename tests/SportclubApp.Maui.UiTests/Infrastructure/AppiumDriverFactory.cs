using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace SportclubApp.Maui.UiTests.Infrastructure;

// Builds an AndroidDriver against a local Appium server.
//
// Optional environment variables (sensible defaults provided):
//   SPORTCLUBAPP_APK_PATH   — override the default Debug APK location
//   APPIUM_SERVER_URL       — default http://127.0.0.1:4723
//   SPORTCLUBAPP_DEVICE     — udid or AVD name, default "emulator-5554"
internal static class AppiumDriverFactory
{
    public const string AppPackage = "com.avans.sportclubapp";

    public static AndroidDriver CreateAndroidDriver()
    {
        var apkPath = Environment.GetEnvironmentVariable("SPORTCLUBAPP_APK_PATH");
        if (string.IsNullOrWhiteSpace(apkPath))
        {
            apkPath = ResolveDefaultApkPath();
        }

        if (!File.Exists(apkPath))
        {
            throw new FileNotFoundException(
                $"APK not found at {apkPath}. Build the MAUI Android target first: " +
                "`dotnet build src/SportclubApp.Maui -f net10.0-android -c Debug`. " +
                "Or set SPORTCLUBAPP_APK_PATH to a non-default location.",
                apkPath);
        }

        var serverUri = new Uri(
            Environment.GetEnvironmentVariable("APPIUM_SERVER_URL") ?? "http://127.0.0.1:4723");

        var options = new AppiumOptions
        {
            PlatformName = "Android",
            AutomationName = "UiAutomator2",
            App = apkPath,
            DeviceName = Environment.GetEnvironmentVariable("SPORTCLUBAPP_DEVICE") ?? "emulator-5554",
        };
        options.AddAdditionalAppiumOption("appium:appPackage", AppPackage);
        // .NET MAUI Android emits a hashed launcher activity (crc64xxx.MainActivity).
        // uiautomator2's `am start` + foreground-activity poll races with MAUI's
        // splash→main transition on Android 16 and reports "never started" even
        // though the app is actually running. The cleaner path: install the APK
        // here, skip auto-launch, then activate the app via the driver API which
        // doesn't depend on the foreground-activity regex.
        options.AddAdditionalAppiumOption("appium:autoLaunch", false);
        options.AddAdditionalAppiumOption("appium:noReset", false);
        options.AddAdditionalAppiumOption("appium:fullReset", false);
        options.AddAdditionalAppiumOption("appium:newCommandTimeout", 180);
        // The first install + instrumentation startup on a cold AVD can easily
        // exceed the default 30s timeout. 180s is comfortable for slow emulators.
        options.AddAdditionalAppiumOption("appium:uiautomator2ServerInstallTimeout", 180000);
        options.AddAdditionalAppiumOption("appium:uiautomator2ServerLaunchTimeout", 180000);
        options.AddAdditionalAppiumOption("appium:adbExecTimeout", 60000);
        // Auto-accept any runtime permission dialogs (notifications, etc.).
        options.AddAdditionalAppiumOption("appium:autoGrantPermissions", true);

        var driver = new AndroidDriver(serverUri, options, TimeSpan.FromSeconds(180));
        driver.ActivateApp(AppPackage);
        // Let the MAUI splash → first-page transition settle before tests start
        // polling the view tree. uiautomator2's accessibility-snapshot needs a
        // quiet screen, otherwise it bails with "active window is constantly
        // changing".
        Thread.Sleep(TimeSpan.FromSeconds(3));
        return driver;
    }

    // Walk up from the test assembly's bin/Debug/net10.0/ to the repo root and
    // resolve the standard MAUI Android Debug output. This keeps the harness
    // zero-config for the common case of "build + test from the repo".
    private static string ResolveDefaultApkPath()
    {
        var repoRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        return Path.Combine(
            repoRoot,
            "src", "SportclubApp.Maui", "bin", "Debug", "net10.0-android",
            "com.avans.sportclubapp-Signed.apk");
    }
}
