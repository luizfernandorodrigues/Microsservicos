using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Catalogo.API.Data.Repositorios
{
    public abstract class ARepositorio
    {
        private readonly IConfiguration _configuracao;
        private const string STRING_CONEXAO = "StringConexao";
        private readonly string _stringConexao;

        protected IDbTransaction Transacao { get; private set; }
        protected IDbConnection Conexao { get; private set; }
        protected SqlConnectionStringBuilder StringConexaoBuilder { get; private set; }

        protected ARepositorio(IConfiguration configuracao)
        {
            _configuracao = configuracao;
            _stringConexao = _configuracao.GetConnectionString(STRING_CONEXAO);
        }

        public void ConstroiStringConexaoBuilder()
        {
            StringConexaoBuilder = new SqlConnectionStringBuilder(_stringConexao);
        }

        public void AbrirConexaoComTransacao()
        {
            try
            {
                Conexao = new SqlConnection(_stringConexao);

                Conexao.Open();

                Transacao = Conexao.BeginTransaction();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void AbrirConexaoSemTransacao()
        {
            try
            {
                Conexao = new SqlConnection(_stringConexao);

                Conexao.Open();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void Commit()
        {
            try
            {
                if (Transacao != null)
                    Transacao.Commit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void FecharConexao()
        {
            try
            {
                if (Conexao != null && Conexao.State.Equals(ConnectionState.Open))
                    Conexao.Close();
            }
            catch (Exception ex) { throw ex; }

        }

        public void Rollback()
        {
            try
            {
                if (Transacao != null)
                    Transacao.Rollback();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
