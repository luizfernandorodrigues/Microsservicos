using Catalogo.API.Data.Repositorios.Interfaces;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Catalogo.API.AtualizacaoBanco
{
    public class AtualizadorBancoDados
    {
        private string _stringConexao;
        private SqlConnectionStringBuilder _sqlConnectionStringBuilder;
        private Action<string> _log;
        private readonly IRepositorioAtualizacao _repositorioAtualizacao;

        public AtualizadorBancoDados(IRepositorioAtualizacao repositorioAtualizacao)
        {
            _repositorioAtualizacao = repositorioAtualizacao;

            _sqlConnectionStringBuilder = new SqlConnectionStringBuilder(_repositorioAtualizacao.ObterStringConexao());

            MontaStringConexao();
        }

        public void VerificaAtualizacoes(Action<string> log)
        {
            _log = log;

            var existeBase = ExisteBaseDados();

            if (!existeBase)
                CriarBaseDados();

            VerificaAtualizacao();
        }

        private void VerificaAtualizacao()
        {
            IEnumerable<ModeloAtualizacao> modeloAtualizacoes = null;
            try
            {
                _log("Verificando Atualizações");

                _repositorioAtualizacao.AbrirConexaoComTransacao();

                CarregarObjetos(out modeloAtualizacoes);

                ValidaGuid(modeloAtualizacoes);

                ExecutaScriptGerenciado(modeloAtualizacoes);

                _repositorioAtualizacao.Transacao.Commit();
            }
            catch (Exception ex)
            {
                _repositorioAtualizacao.Rollback();

                _log($"Ocorreu um erro ao verificar atualizações. {ex.Message}");
            }
            finally
            {
                _repositorioAtualizacao.FecharConexao();
            }
        }

        private void ExecutaScriptGerenciado(IEnumerable<ModeloAtualizacao> listaScriptGerenciados)
        {
            var guids = new List<string>();

            foreach (var item in listaScriptGerenciados.OrderBy(script => script.Versao.Numero))
            {
                foreach (var script in item.ListaScripts)
                {
                    if (_repositorioAtualizacao.VerificaGuid(item.Versao.Guid) == null)
                    {
                        _log($"Executando script Guid: {item.Versao.Guid}");

                        _repositorioAtualizacao.ExecutaScript(script.Sql);

                        guids.Add(item.Versao.Guid);
                    }
                }
            }

            if (guids.Count > 0)
                ExecutaPersistenciaGuid(guids);
        }

        private void ExecutaPersistenciaGuid(List<string> guids)
        {
            foreach (string guid in guids)
                _repositorioAtualizacao.ArmazenaGuidExecutado(guid);
        }

        private void CarregarObjetos(out IEnumerable<ModeloAtualizacao> modeloAtualizacoes)
        {
            try
            {
                var modeloAtualizacao = new ModeloAtualizacao();

                var assembly = modeloAtualizacao.GetType().Assembly;

                var resources = assembly.GetManifestResourceNames();

                List<ModeloAtualizacao> listaModeloAtualizacoes;

                var quantidadeScriptGerenciado = 10000;

                listaModeloAtualizacoes = new List<ModeloAtualizacao>() { Capacity = quantidadeScriptGerenciado };

                var caminhoScripts = "SERMUSA.AtualizacaoBancoDados.Fontes.Scripts";

                foreach (var resource in resources)
                {
                    if (resource.Contains(caminhoScripts))
                        listaModeloAtualizacoes.Add(GerarObjetoAtravesXML<ModeloAtualizacao>(resource, assembly));
                }

                modeloAtualizacoes = listaModeloAtualizacoes;
            }
            catch (Exception ex)
            {
                modeloAtualizacoes = null;

                _log($"Ocorreu um erro para carregar os objetos. {ex.Message}");
            }
        }

        private static void ValidaGuid(IEnumerable<ModeloAtualizacao> listaScripts)
        {
            foreach (var item in listaScripts)
            {
                if (string.IsNullOrEmpty(item.Versao.Guid))
                    item.Versao.Guid = Guid.NewGuid().ToString();
            }
        }

        private T GerarObjetoAtravesXML<T>(string resource, Assembly assembly)
        {
            Stream streamAtualizacao = null;

            try
            {
                streamAtualizacao = assembly.GetManifestResourceStream(resource);

                var xmlSerializer = new XmlSerializer(typeof(T));

                return (T)xmlSerializer.Deserialize(streamAtualizacao);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (streamAtualizacao != null)
                    streamAtualizacao.Close();
            }
        }

        private void MontaStringConexao()
        {
            _stringConexao = $"Data Source = {_sqlConnectionStringBuilder.DataSource}; Initial Catalog = {_sqlConnectionStringBuilder.InitialCatalog}; Persist Security Info = True; User ID = {_sqlConnectionStringBuilder.UserID}; Password = {_sqlConnectionStringBuilder.Password};";
        }

        private void CriarBaseDados()
        {
            var nomeBanco = _sqlConnectionStringBuilder.InitialCatalog;

            _sqlConnectionStringBuilder.InitialCatalog = "master";

            MontaStringConexao();

            using var conexao = new SqlConnection(_stringConexao);

            try
            {
                conexao.Open();

                var comando = conexao.CreateCommand();

                comando.CommandTimeout = int.MaxValue;

                //cria a base

                _sqlConnectionStringBuilder.InitialCatalog = nomeBanco;

                _log("Criando banco de dados");

                comando.CommandText = $"CREATE DATABASE {_sqlConnectionStringBuilder.InitialCatalog}";
                comando.ExecuteNonQuery();

                comando.CommandText = $"USE {_sqlConnectionStringBuilder.InitialCatalog}";
                comando.ExecuteNonQuery();

                comando.CommandText = $"CREATE SCHEMA[Cadastro]";
                comando.ExecuteNonQuery();

                comando.CommandText = $"CREATE SCHEMA[Configuracao]";
                comando.ExecuteNonQuery();

                _log("Banco de dados configurado com sucesso!");
            }
            catch (Exception ex)
            {
                _log($"Ocorreu um erro ao criar banco de dados.\n{ex.Message}");
            }
            finally
            {
                conexao.Close();
            }
        }

        private bool ExisteBaseDados()
        {
            _log("Verificando banco de dados");

            var bancoDados = string.Empty;

            if (!string.IsNullOrEmpty(_stringConexao))
            {
                var stringConexaoBuilder = new SqlConnectionStringBuilder(_stringConexao);

                if (!string.IsNullOrEmpty(stringConexaoBuilder.InitialCatalog))
                    bancoDados = stringConexaoBuilder.InitialCatalog;

                try
                {
                    ServerConnection conexaoServidor;

                    if (stringConexaoBuilder.IntegratedSecurity)
                        conexaoServidor = new ServerConnection(stringConexaoBuilder.DataSource);
                    else
                        conexaoServidor = new ServerConnection(stringConexaoBuilder.DataSource, stringConexaoBuilder.UserID, stringConexaoBuilder.Password);

                    var servidor = new Server(conexaoServidor);

                    if (servidor == null)
                        return false;

                    if (servidor.Databases[bancoDados] == null)
                        return false;

                    _log("Bando de dados verificado com sucesso!");

                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return false;
        }
    }
}
