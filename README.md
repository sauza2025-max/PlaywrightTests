# PlaywrightTests + Allure Setup

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (for Playwright browser install)
- [Allure CLI](https://allurereport.org/docs/install/) (to view reports)

---
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <RootNamespace>PlaywrightTests</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <!-- Allure - NUnit only (removed Allure.Xunit which caused conflicts) -->
    <PackageReference Include="Allure.NUnit" Version="2.14.1" />

    <!-- Playwright - versions must match exactly -->
    <PackageReference Include="Microsoft.Playwright" Version="1.44.0" />
    <PackageReference Include="Microsoft.Playwright.NUnit" Version="1.44.0" />

    <!-- NUnit Testing Framework -->
    <PackageReference Include="NUnit" Version="4.1.0" />
    <PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />

    <!-- Selenium -->
    <PackageReference Include="DotNetSeleniumExtras.WaitHelpers" Version="3.11.0" />
    <PackageReference Include="Selenium.Support" Version="4.18.1" />
    <PackageReference Include="Selenium.WebDriver" Version="4.18.1" />
    <PackageReference Include="WebDriverManager" Version="2.17.7" />

    <!-- Extras -->
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Bogus" Version="35.3.0" />
  </ItemGroup>

</Project>


## 1. Restore packages

```bash
dotnet restore
```

## 2. Install Playwright browsers

```bash
# Build first so the install script is available
dotnet build

# Then install browsers (run from project root)
pwsh bin/Debug/net8.0/playwright.ps1 install
# OR on Linux/macOS:
# ./bin/Debug/net8.0/playwright.ps1 install
```

> If `pwsh` is not available, install PowerShell: https://github.com/PowerShell/PowerShell

---

## 3. Run tests — guaranteed allure-results location

The most reliable way is to set `ALLURE_RESULTS_DIR` before running:

```powershell
# PowerShell — run from the project root folder
$env:ALLURE_RESULTS_DIR = "$PWD\allure-results"
dotnet test --settings .runsettings
```

Or in a single line:
```powershell
$env:ALLURE_RESULTS_DIR="$PWD\allure-results"; dotnet test --settings .runsettings
```

After running, `allure-results\` will appear in your **project root** (same folder as `.runsettings`).

---

## 4. View the Allure report

```powershell
allure serve allure-results
```

If the above says "allure-results does not exist", the folder was written to the build output instead. Check there:
```powershell
# If results ended up in build output, serve from there
allure serve bin\Debug\net8.0\allure-results
```

---

## Running specific test categories

```bash
# Playwright tests only
dotnet test --settings .runsettings --filter Category=Advanced

# Selenium login tests only
dotnet test --settings .runsettings --filter Category=Login

# Smoke tests only
dotnet test --settings .runsettings --filter Category=Smoke
```

---

## Project Structure

```
PlaywrightTests/
├── .runsettings              ← Registers Allure NUnit listener (REQUIRED for allure-results/)
├── allureConfig.json         ← Allure output directory config
├── PlaywrightTests.csproj    ← Fixed: removed Allure.Xunit, aligned Playwright versions
│
├── BasePlaywrightTest.cs     ← Base class for all Playwright tests
├── BaseTest.cs               ← Base class for all Selenium tests
├── LoginPage.cs              ← Page Object Model for login page
│
├── LoginTests.cs             ← Selenium login tests (data-driven)
├── AdvancedSeleniumTests.cs  ← Selenium advanced: alerts, iframes, JS, hover
├── AdvancedPlaywrightTests.cs← Playwright advanced: network mock, tracing, mobile
└── ComparisonTests.cs        ← Side-by-side Selenium vs Playwright comparison
```

---

## Why allure-results wasn't being created

Three issues were present in the original project:

| Problem | Fix |
|---|---|
| Missing `.runsettings` | Created — registers `AllureNUnitExtension` with the test runner |
| Missing `allureConfig.json` | Created — tells Allure where to write results |
| `Allure.Xunit` package conflict | Removed — this project uses NUnit, not xUnit |
| `Microsoft.Playwright` (1.59) vs `Microsoft.Playwright.NUnit` (1.43) version mismatch | Both set to `1.49.0` |

The `.runsettings` file is the most critical piece — without it, the Allure listener is never loaded and no results are written regardless of `[AllureTag]` attributes in your tests.
