using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExemploBdd.Testes.Funcionais.Features.Support;

namespace ExemploBdd.Testes.Funcionais
{
    [TestClass]
    public class AssemblyInitialize
    {
        private static IISExpressDriver iisDriver;

        [AssemblyInitialize]
        public static void IniciarTestes(TestContext contexto)
        {
            try
            {
                WebBindingBase.IsTFSBuild = IsTfsBuild();
                IniciarIISExpress();
            }
            catch
            {
                FinalizarTestes();
                throw;
            }
        }

        private static void IniciarIISExpress()
        {
            var appHost = CriarApphost();
            iisDriver = new IISExpressDriver();
            iisDriver.Iniciar(appHost);
        }

        private static string CriarApphost()
        {
            var caminho = IsTfsBuild() ? @"_PublishedWebSites\ExemploBDD.WebApp" : "..\\..\\..\\ExemploBDD.WebApp";
            var caminhoBinarios = Path.GetDirectoryName(Uri.UnescapeDataString(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath));
            var caminhoSite = Path.GetFullPath(Path.Combine(caminhoBinarios, caminho));
            var applicationhost = File.ReadAllText(Path.Combine(caminhoBinarios, "applicationhost.config"));
            var appconfigProcessado = applicationhost.Replace("{path to website}", caminhoSite);
            var caminhoApphost = Path.Combine(caminhoBinarios, "applicationhost.config");
            File.WriteAllText(caminhoApphost, appconfigProcessado);
            return caminhoApphost;
        }

        private static bool IsTfsBuild()
        {
            var isTfsBuild = Directory.Exists("_PublishedWebSites");
            return isTfsBuild;
        }

        [AssemblyCleanup]
        public static void FinalizarTestes()
        {
            Funcoes.TentaEEscondeAExcecao(() => { if (iisDriver != null) iisDriver.Dispose(); });
            Funcoes.TentaEEscondeAExcecao(FecharNavegador);
        }

        private static void FecharNavegador()
        {
            WebBindingBase.FechaTodos();
        }
    }
}
