using OpenQA.Selenium;
using TechTalk.SpecFlow;
using FluentAssertions;

namespace ExemploBdd.Testes.Funcionais.Features.Support.StepsDefinition
{
    [Binding]
    public class SomaSteps : WebBindingBase
    {
        [Given(@"que estou na página de soma")]
        public void DadoQueEstouNaPaginaDeSoma()
        {
            WebDriver.Navigate().GoToUrl("http://localhost:8008/Home/Soma");
        }

        [When(@"somo (.*) e (.*)")]
        public void QuandoSomoE(int a, int b)
        {
            WebDriver.FindElement(By.CssSelector("#a")).Clear();
            WebDriver.FindElement(By.CssSelector("#a")).SendKeys(a.ToString());
            WebDriver.FindElement(By.CssSelector("#b")).Clear();
            WebDriver.FindElement(By.CssSelector("#b")).SendKeys(b.ToString());
            WebDriver.FindElement(By.CssSelector("#somar")).Click();
        }

        [Then(@"o resultado tem que ser (.*)")]
        public void EntaoOResultadoTemQueSer(int resultadoEsperado)
        {
            WebDriver.FindElement(By.CssSelector("#resultado")).Text.Should().Be(resultadoEsperado.ToString());
        }
    }
}
