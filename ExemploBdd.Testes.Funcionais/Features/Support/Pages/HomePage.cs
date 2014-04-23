using OpenQA.Selenium;

namespace ExemploBdd.Testes.Funcionais.Features.Support.Pages
{
    public class HomePage : Pagina
    {
        public override string Path { get; } = "/";
        public object Titulo
        {
            get
            {
                return WebDriver.FindElement(By.CssSelector("#titulo")).Text;
            }
        }
    }
}
