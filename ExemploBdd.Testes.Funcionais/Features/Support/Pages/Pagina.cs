using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace ExemploBdd.Testes.Funcionais.Features.Support.Pages
{
    public interface IPagina
    {
        string EnderecoAtual { get; }
        string Path { get; }
        IWebElement Body { get; }
        void EhAtual();
        bool ControleComErro(IWebElement elemento);
        void Navegar(params string[] partes);
        void EstaNaoAutorizado();
        Task EsperarAjaxAsync();
        Task<bool> EsperarJqueryCarregadoAsync(int milliTimeout, bool joga = false);
        Task<bool> EsperarElementoTerListenersAsync(string selector, bool joga = false);
        Task<bool> EsperarElementoClicavelAsync(string selector, bool joga = false, int segundos = 5);
        Task<bool> EsperarAsync(Func<IWebDriver, bool> fn, int segundos = 5);
        void TireScreenshoot(string nomeArquivo = null);
        IWebDriver WebDriver { get; set; }
    }

    public abstract class Pagina : IPagina
    {
        public IWebDriver WebDriver { get; set; }
        protected Pagina() : this(WebBindingBase.WebDriver) {}

        protected Pagina(IWebDriver driver)
        {
            WebDriver = driver;
        }

        protected string UrlBase
        {
            get { return "http://localhost:8008"; }
        }

        public T ProtegerDeStaleReference<T>(Func<T> func, int vezes = 5)
        {
            var i = 0;
            StaleElementReferenceException exception = null;
            while (i++ < vezes)
            {
                try
                {
                    return func();
                }
                catch (StaleElementReferenceException ex)
                {
                    exception = ex;
                    Thread.Sleep(200);
                }
            }
            if (exception == null) throw new ApplicationException(string.Format("Tentou-se obter o valor {0} vezes, sem sucesso.", vezes));
            throw new ApplicationException("Não conseguiu obter o valor, houve StaleElementReferenceException.", exception);
        }

        protected string Url
        {
            get { return UrlBase + Path; }
        }

        public string EnderecoAtual
        {
            get { return WebDriver.Url.Replace(UrlBase, "").ToLower(); }
        }

        public abstract string Path { get; }

        public void Reload()
        {
            WebDriver.Navigate().Refresh();
        }

        public IWebElement Body
        {
            get { return ObterElementoPeloSeletorCSS("body"); }
        }

        public void EhAtual()
        {
            EnderecoAtual.ToLower().Should().Be(Path.ToLower());
        }

        public bool ControleComErro(IWebElement elemento)
        {
            Thread.Sleep(1000);

            if (elemento.GetAttribute("class").Contains("has-error")) return true;

            try
            {
                var pai = elemento.FindElement(By.XPath(".."));
                return ControleComErro(pai);
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public virtual void Navegar(params string[] partes)
        {
            if (Url.Contains("{") && Url.Substring(Url.IndexOf("{") + 2, 1) == "}")
            {
                var urlStringFormat = string.Format(Url, partes);
                WebDriver.Navigate().GoToUrl(urlStringFormat);
                return;
            }
            var url = partes.Aggregate(Url, (atual, parte) => atual + ("/" + parte));
            WebDriver.Navigate().GoToUrl(url);
        }

        public void EstaNaoAutorizado()
        {
            var el = WebDriver.FindElement(By.ClassName("NaoAutorizado"));
            el.Displayed.Should().BeTrue();
            el.Enabled.Should().BeTrue();
        }

        public async Task EsperarAjaxAsync()
        {
            await EsperarJqueryCarregadoAsync(20000, true);
            await EsperarAsync(driver =>
            {
                var ativo = Convert.ToBoolean(((IJavaScriptExecutor) WebBindingBase.WebDriver).ExecuteScript("return typeof(window.$) === 'undefined' ? false : window.$.active === 0"));
                return ativo;
            });
        }

        public Task<bool> EsperarJqueryCarregadoAsync(int milliTimeout, bool joga = false)
        {
            Func<Task<bool>> espera = () => EsperarAsync(driver =>
            {
                var carregado = Convert.ToBoolean(((IJavaScriptExecutor) WebBindingBase.WebDriver).ExecuteScript("return typeof(window.$) === 'function'"));
                return carregado;
            });
            return Espera(espera, milliTimeout, joga, "jQuery não encontrado.");
        }

        public async Task<bool> EsperarElementoClicavelAsync(string selector, bool joga = false, int segundos = 5)
        {
            var achou = await EsperarAsync(driver =>
            {
                var elementos = WebDriver.FindElements(By.CssSelector(selector));
                if (elementos.Count == 0) return false;
                var elemento = elementos[0];
                return elemento.Displayed && elemento.Enabled;
            }, segundos);
            if (achou) return true;
            if (joga) throw new ApplicationException(string.Format("Elemento {0} não encontrado ou não clicável.", selector));
            return false;
        }

        public async Task<bool> EsperarElementoTerListenersAsync(string selector, bool joga = false)
        {
            await EsperarJqueryCarregadoAsync(20000, true);
            var carregou = await EsperarAsync(driver =>
            {
                try
                {
                    var possuiSelectors = Convert.ToBoolean(((IJavaScriptExecutor) WebBindingBase.WebDriver).ExecuteScript(string.Format(@"return typeof(window.$) === 'undefined' ? false : Object.keys(window.$._data($('{0}')[0], 'events')).length > 0;", selector)));
                    return possuiSelectors;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Erro ao esperar elemento ter listener: " + ex.ToString());
                    return false;
                }
            });
            if (carregou) return true;
            if (joga) throw new ApplicationException(string.Format("Elemento {0} não possui listener carregado.", selector));
            return false;
        }

        public async Task<bool> EsperarAsync(Func<IWebDriver, bool> fn, int segundos = 5)
        {
            await Task.Yield();
            var espera = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(segundos));
            try
            {
                return espera.Until(fn);
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public void TireScreenshoot(string nomeArquivo = null)
        {
            WebDriver.TireScreenshoot(nomeArquivo);
        }

        protected IWebElement ObterElementoPeloSeletorCSS(string seletor)
        {
            return ObterElemento(By.CssSelector(seletor));
        }

        protected IWebElement ObterElemento(By por)
        {
            var espera = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(5));
            IWebElement elemento = null;
            try
            {
                if (espera.Until(x => (elemento = WebDriver.FindElement(por)).Displayed && elemento.Enabled)) return elemento;
            }
            catch
            {
                return null;
            }
            return null;
        }

        protected List<IWebElement> ObterElementosPeloSeletorCSS(string seletor)
        {
            Func<List<IWebElement>> obterElementos = () =>
            {
                try
                {
                    return WebDriver.FindElements(By.CssSelector(seletor)).ToList();
                }
                catch (NoSuchElementException)
                {
                    if (!WebDriver.PageSource.Contains("Cannot open database")) throw;

                    WebDriver.Navigate().Refresh();

                    return WebDriver.FindElements(By.CssSelector(seletor)).ToList();
                }
            };

            var espera = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(20));
            espera.Until(x => obterElementos().Any());

            return obterElementos();
        }

        protected bool EhVisivel(string selector)
        {
            var els = WebDriver.FindElements(By.CssSelector(selector));
            if (els.Count == 0) return false;
            var el = els[0];
            return el.Displayed;
        }

        protected void Clique(string selector)
        {
            WebDriver.FindElement(By.CssSelector(selector)).Click();
        }

        private async Task<bool> Espera(Func<Task<bool>> tarefa, int milli, bool joga = false, string mensagemErroEspera = "Condição de espera não atendida.")
        {
            var cSource = new CancellationTokenSource();
            var t = Espera(tarefa, cSource.Token);
            await Task.WhenAny(t, Task.Delay(milli));
            var completouATarefa = t.IsCompleted;
            if (!completouATarefa) cSource.Cancel();
            var concluiu = await t;
            if (concluiu) return true;
            if (joga) throw new ApplicationException(mensagemErroEspera);
            return false;
        }

        private async Task<bool> Espera(Func<Task<bool>> tarefa, CancellationToken token)
        {
            do
            {
                var concluiu = await tarefa();
                if (concluiu) return true;
                if (token.IsCancellationRequested) return false;
            } while (true);
        }
    }
}