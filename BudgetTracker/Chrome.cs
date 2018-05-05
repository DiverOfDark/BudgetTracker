using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BudgetTracker
{
    public class Chrome : IDisposable
    {
        private ChromeDriver _driver;

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
                            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                            var chromeOptions = new ChromeOptions
                            {
                                AcceptInsecureCertificates = true
                            };

                            if (!Debugger.IsAttached)
                            {
                                chromeOptions.AddArgument("--headless");
                                chromeOptions.AddArgument("--no-sandbox");
                                chromeOptions.AddArgument("--ignore-certificate-errors");
                                chromeOptions.AddArgument(
                                    "--useragent=\"User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:58.0) Gecko/20100101 Firefox/58.0\"");
                            }

                            
                            chromeOptions.SetLoggingPreference(LogType.Browser, LogLevel.Warning);
                            chromeOptions.SetLoggingPreference(LogType.Driver, LogLevel.Warning);
                            chromeOptions.SetLoggingPreference(LogType.Server, LogLevel.Warning);
                            chromeOptions.SetLoggingPreference(LogType.Client, LogLevel.Warning);
                            _driver = new ChromeDriver(path, chromeOptions);
                        }
                    }
                }

                return _driver;
            }
        }

        public void Dispose()
        {
            _driver?.Close();
            _driver?.Dispose();
        }

        public void Reset()
        {
            var d = _driver;
            _driver = null;
            d?.Close();
            d?.Dispose();
        }
    }
}