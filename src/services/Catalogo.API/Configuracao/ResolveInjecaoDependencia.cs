using Catalogo.API.AtualizacaoBanco;
using Catalogo.API.Data.Repositorios.Implementacoes;
using Catalogo.API.Data.Repositorios.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalogo.API.Configuracao
{
    public static class ResolveInjecaoDependencia
    {
        public static void RegistraServicos(this IServiceCollection servicos, IConfiguration configuracao)
        {
            servicos.AddSingleton(configuracao);
            servicos.AddTransient<IAtualizadorBancoDados, AtualizadorBancoDados>();
            servicos.AddScoped<IRepositorioAtualizacao, RepositorioAtualizacao>();
        }
    }
}
