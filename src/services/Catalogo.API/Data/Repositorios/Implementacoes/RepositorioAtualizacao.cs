using Catalogo.API.Data.Repositorios.Interfaces;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Catalogo.API.Data.Repositorios.Implementacoes
{
    public class RepositorioAtualizacao : ARepositorio, IRepositorioAtualizacao
    {
        private const string TABELA_SCRIPT = "[Sistema].[ScriptGerenciado]";

        public RepositorioAtualizacao(IConfiguration configuracao) : base(configuracao)
        {
        }

        public void ArmazenaGuidExecutado(string guid)
        {
            try
            {
                var comandoSql = $"INSERT INTO {TABELA_SCRIPT}([Guid], [TimesTamp], [VersaoAssembly]) VALUES (@guid, @timesTamp, @versaoAssembly)";

                var parametros = new DynamicParameters();

                parametros.Add(name: "@guid", value: guid, dbType: DbType.String, direction: ParameterDirection.Input);
                parametros.Add(name: "@timesTamp", value: DateTime.Now, dbType: DbType.DateTime2, direction: ParameterDirection.Input);
                parametros.Add(name: "@versaoAssembly", value: typeof(RepositorioAtualizacao).Assembly.GetName().Version.ToString(), dbType: DbType.String, direction: ParameterDirection.Input);

                Conexao.Execute(sql: comandoSql, param: parametros, transaction: Transacao, commandTimeout: int.MaxValue, commandType: CommandType.Text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CriaBaseDados()
        {
            ConstroiStringConexaoBuilder();

            var nomeBaseDadosReal = StringConexaoBuilder.InitialCatalog;

            StringConexaoBuilder.InitialCatalog = "master";

            using var conexao = new SqlConnection(StringConexaoBuilder.ConnectionString);

            try
            {
                conexao.Open();

                var comando = conexao.CreateCommand();

                comando.CommandTimeout = int.MaxValue;

                comando.CommandText = $"CREATE DATABASE {nomeBaseDadosReal}";
                comando.ExecuteNonQuery();

                comando.CommandText = $"USE {nomeBaseDadosReal}";
                comando.ExecuteNonQuery();

                comando.CommandText = $"CREATE SCHEMA[Cadastro]";
                comando.ExecuteNonQuery();

                comando.CommandText = $"CREATE SCHEMA[Sistema]";
                comando.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conexao.Close();
            }
        }

        public void ExecutaScript(string sql)
        {
            try
            {
                Conexao.Execute(sql: sql, transaction: Transacao, commandTimeout: int.MaxValue, commandType: CommandType.Text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool VerificaBaseDados()
        {
            ServerConnection conexaoServidor;

            try
            {
                ConstroiStringConexaoBuilder();

                if (StringConexaoBuilder.IntegratedSecurity)
                    conexaoServidor = new ServerConnection(StringConexaoBuilder.DataSource);
                else
                    conexaoServidor = new ServerConnection(StringConexaoBuilder.DataSource, StringConexaoBuilder.UserID, StringConexaoBuilder.Password);

                var servidor = new Server(conexaoServidor);

                if (servidor == null)
                    return false;

                if (servidor.Databases[StringConexaoBuilder.InitialCatalog] == null)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public object VerificaGuid(string guid)
        {
            try
            {
                var comandoSql = $"SELECT 1 FROM {TABELA_SCRIPT} WHERE Guid = @guid";

                var parametros = new DynamicParameters();

                parametros.Add(name: "@guid", value: guid, dbType: DbType.String, direction: ParameterDirection.Input);

                return Conexao.QueryFirstOrDefault<object>(sql: comandoSql, param: parametros, transaction: Transacao, commandTimeout: int.MaxValue, commandType: CommandType.Text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
