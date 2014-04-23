using TechTalk.SpecFlow;

namespace ExemploBdd.Testes.Funcionais.Features.Support
{
    [Binding]
    public class WebFeatureHooks
    {
        [BeforeScenario]
        public void BeforeScenario() {}

        [Before("js")]
        public void ReconfigurarSessaoWeb()
        {
            IniciarNavegador("chrome");
        }

        [Before("web")]
        public void ReconfigurarSessaoJavaScript()
        {
            IniciarNavegador("phantom");
        }

        [Before("ie")]
        public void ReconfigurarSessaoIE()
        {
            IniciarNavegador("ie");
        }

        [Before("ff")]
        public void ReconfigurarSessaoFirefox()
        {
            IniciarNavegador("firefox");
        }

        [AfterScenario]
        public void AfterScenario()
        {
            if (ScenarioContext.Current.TestError != null)
            {
                WebBindingBase.WebDriver.TireScreenshoot(ScenarioContext.Current.ScenarioInfo.Title);
            }
        }

        [After("web")]
        [After("js")]
        [After("ie")]
        [After("ff")]
        public void FinalizarSessaoWeb() {}

        private void IniciarNavegador(string navegador)
        {
            switch (navegador)
            {
                case "chrome":
                    WebBindingBase.WebDriver = WebBindingBase.ChromeDriver;
                    break;
                case "firefox":
                    WebBindingBase.WebDriver = WebBindingBase.FirefoxDriver;
                    break;
                case "ie":
                    WebBindingBase.WebDriver = WebBindingBase.IEDriver;
                    break;
                case "phantom":
                    WebBindingBase.WebDriver = WebBindingBase.PhantomJSDriver;
                    break;
            }
        }
    }
}