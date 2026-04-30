using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightTests.Utils
{
    // TestEnvironment.cs — shared by both base classes
    public static class TestEnvironment
    {
        public static void WriteEnvironmentProperties(string browser)
        {
            var resultsDir = Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                "allure-results"
            );
            Directory.CreateDirectory(resultsDir);

            File.WriteAllLines(
                Path.Combine(resultsDir, "environment.properties"),
                new[]
                {
               $"Environment=QA",
               $"Browser={browser}",
               $"BaseUrl=https://automationexercise.com",
               $"OS={Environment.OSVersion}",
               $"DotNet={Environment.Version}",
               $"MachineName={Environment.MachineName}",
               $"BuildDate={DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                }
            );
        }
    }
}
