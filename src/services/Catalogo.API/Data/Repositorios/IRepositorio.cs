using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace Catalogo.API.Data.Repositorios
{
    public interface IRepositorio
    {
        DbConnection Conexao { get; set; }
        DbTransaction Transacao { get; set; }
        IConfiguration Configuracao { get; set; }
        string ObterStringConexao();
        void AbrirConexaoComTransacao();
        void AbrirConexaoSemTransacao();
        void Commit();
        void FecharConexao();
        void Rollback();
    }
}
