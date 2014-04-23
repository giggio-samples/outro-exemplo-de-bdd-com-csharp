using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.PhantomJS;
using TechTalk.SpecFlow;

namespace ExemploBdd.Testes.Funcionais.Features.Support
{
    [Binding]
    public class WebBindingBase
    {
        public static readonly List<IWebDriver> Drivers = new List<IWebDriver>();
        private static ChromeDriver chromeDriver;
        private static PhantomJSDriver phantomJSDriver;
        private static FirefoxDriver firefoxDriver;
        private static InternetExplorerDriver ieDriver;

        public static bool IsTFSBuild { get; set; }

        public static IWebDriver WebDriver { get; set; }

        public static ChromeDriver ChromeDriver
        {
            get
            {
                if (chromeDriver != null) return chromeDriver;
                var svc = ChromeDriverService.CreateDefaultService();
                svc.LogPath = Path.Combine(Path.GetTempPath(), "logchrome.txt");
                var opt = new ChromeOptions();
                chromeDriver = new ChromeDriver(svc, opt);
                Drivers.Add(chromeDriver);
                return chromeDriver;
            }
        }

        public static InternetExplorerDriver IEDriver
        {
            get
            {
                if (ieDriver != null) return ieDriver;
                ieDriver = new InternetExplorerDriver();
                Drivers.Add(ieDriver);
                return ieDriver;
            }
        }

        public static PhantomJSDriver PhantomJSDriver
        {
            get
            {
                if (phantomJSDriver != null) return phantomJSDriver;
                var svc = PhantomJSDriverService.CreateDefaultService();
                svc.LogFile = Path.Combine(Path.GetTempPath(), "logphantom.txt");
                var opt = new PhantomJSOptions();
                phantomJSDriver = new PhantomJSDriver(svc, opt);
                Drivers.Add(phantomJSDriver);
                phantomJSDriver.Manage().Window.Size = new Size(1600, 1200);
                return phantomJSDriver;
            }
        }

        public static FirefoxDriver FirefoxDriver
        {
            get
            {
                if (firefoxDriver != null) return firefoxDriver;
                firefoxDriver = new FirefoxDriver();
                Drivers.Add(firefoxDriver);
                return firefoxDriver;
            }
        }

        public static void FechaTodos()
        {
            foreach (var driver in Drivers.Where(d => d != null))
            {
                var driver1 = driver;
                Funcoes.TentaEEscondeAExcecao(() =>
                {
                    driver1.Quit();
                    driver1.Dispose();
                });
            }
        }
    }
}