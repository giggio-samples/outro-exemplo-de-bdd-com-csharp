using System;

namespace ExemploBdd.Testes.Funcionais.Features.Support
{
    public static class Funcoes
    {
        public static void TentaEEscondeAExcecao(Action acao)
        {
            try
            { acao(); }
            catch
            { }
        }
    }
}
