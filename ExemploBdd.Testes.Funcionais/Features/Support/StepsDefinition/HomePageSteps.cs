using ExemploBdd.Testes.Funcionais.Features.Support.Pages;
using TechTalk.SpecFlow;
using FluentAssertions;

namespace ExemploBdd.Testes.Funcionais.Features.Support.StepsDefinition
{
    [Binding]
    public class HomePageSteps : WebBindingBase
    {
        private readonly HomePage homepage = new HomePage();
        [StepDefinition]
        public void QueEuAcessoAHomePage()
        {
            homepage.Navegar();
        }

        [Then]
        public void EuDevoVerAMensagem_P0(string mensagem)
        {
            homepage.Titulo.Should().Be(mensagem);
        }
    }
}
