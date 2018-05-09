using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BudgetTracker
{
    public class Chrome : IDisposable
    {
        private readonly string _downloadDir;
        private ChromeDriver _driver;

        public Chrome()
        {
            _downloadDir = Path.Combine(Path.GetTempPath(), "ChromeWebDriverDownloads-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            Directory.CreateDirectory(_downloadDir);
        }

        public ChromeDriver Driver
        {
            get
            {
                if (_driver == null)
                {
                    lock (this)
                    {
                        if (_driver == null)
                        {
                            _driver = CreateDriver(_downloadDir);
                        }
                    }
                }

                return _driver;
            }
        }

        public IList<FileInfo> GetDownloads() => Directory.GetFiles(_downloadDir).Select(v => new FileInfo(v)).ToList();

        public void CleanupDownloads()
        {
            Directory.Delete(_downloadDir,true);
            Directory.CreateDirectory(_downloadDir);
        }

        public void Dispose()
        {
            Directory.Delete(_downloadDir, true);
            _driver?.Close();
            _driver?.Dispose();
        }

        public void Reset()
        {
            CleanupDownloads();
            
            var d = _driver;
            _driver = null;
            d?.Close();
            d?.Dispose();
        }

        private static ChromeDriver CreateDriver(string downloadDir)
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var chromeOptions = new ChromeOptions
            {
                AcceptInsecureCertificates = true
            };

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SHOW_CHROME")))
            {
                chromeOptions.AddArgument("--headless");
            }

            chromeOptions.AddArgument("--no-sandbox");
            chromeOptions.AddArgument("--window-size=1920,1080");
            chromeOptions.AddArgument("--ignore-certificate-errors");
            chromeOptions.AddArgument("--useragent=\"User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:58.0) Gecko/20100101 Firefox/58.0\"");
    
            chromeOptions.SetLoggingPreference(LogType.Browser, LogLevel.Warning);
            chromeOptions.SetLoggingPreference(LogType.Driver, LogLevel.Warning);
            chromeOptions.SetLoggingPreference(LogType.Server, LogLevel.Warning);
            chromeOptions.SetLoggingPreference(LogType.Client, LogLevel.Warning);

            var driverService = ChromeDriverService.CreateDefaultService(path);
            var driver = new ChromeDriver(driverService, chromeOptions);

            var url = driverService.ServiceUrl + "session/" + driver.SessionId + "/chromium/send_command";
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, object>
                    {
                        {"cmd", "Page.setDownloadBehavior"},
                        {
                            "params", new Dictionary<string, string>
                            {
                                {"behavior", "allow"},
                                {"downloadPath", downloadDir}
                            }
                        }
                    }), Encoding.UTF8,
                    "application/json");
                httpClient.PostAsync(url, content).GetAwaiter().GetResult();
            }

            return driver;
        }
    }
}