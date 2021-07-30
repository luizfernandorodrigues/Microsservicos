using Catalogo.API.Data.Repositorios.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Catalogo.API.AtualizacaoBanco
{
    public class AtualizadorBancoDados : IAtualizadorBancoDados
    {
        private readonly IRepositorioAtualizacao _repositorioAtualizacao;
        private readonly ILogger<AtualizadorBancoDados> _logger;

        public AtualizadorBancoDados(IRepositorioAtualizacao repositorioAtualizacao, ILogger<AtualizadorBancoDados> logger)
        {
            _repositorioAtualizacao = repositorioAtualizacao;
            _logger = logger;
        }

        public void VerificaAtualizacao()
        {
            _logger.LogInformation("Verificando Atualizações.");

            try
            {
                if (!ExisteBaseDados())
                    CriarBaseDados();

                _logger.LogInformation("Base de dados verificada com sucesso.");

                _logger.LogInformation("Carregando scripts gerenciados.");

                CarregarScriptsGerenciado(out IEnumerable<ModeloAtualizacao> modeloAtualizacoes);

                ValidaGuid(modeloAtualizacoes);

                _repositorioAtualizacao.AbrirConexaoComTransacao();

                ExecutaScriptGerenciado(modeloAtualizacoes);

                _repositorioAtualizacao.Commit();

                _logger.LogInformation("Atualizaçao da base de dados realizada com sucesso.");
            }
            catch (Exception ex)
            {
                _repositorioAtualizacao.Rollback();

                _logger.LogError($"Ocorreu um erro na atualização da base de dados: {ex.Message}");
            }
            finally
            {
                _repositorioAtualizacao.FecharConexao();
            }
        }

        private bool ExisteBaseDados()
        {
            _logger.LogInformation("Verificando base de dados.");

            return _repositorioAtualizacao.VerificaBaseDados();
        }

        private void CriarBaseDados()
        {
            _repositorioAtualizacao.CriaBaseDados();
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
                        _logger.LogInformation($"Executando script guid: {item.Versao.Guid}");

                        _repositorioAtualizacao.ExecutaScript(script.Sql);

                        guids.Add(item.Versao.Guid);
                    }
                }
            }

            _logger.LogInformation("Persistindo scripts.");

            if (guids.Count > 0)
                ExecutaPersistenciaGuid(guids);
        }

        private void ExecutaPersistenciaGuid(List<string> guids)
        {
            foreach (string guid in guids)
                _repositorioAtualizacao.ArmazenaGuidExecutado(guid);
        }

        private void CarregarScriptsGerenciado(out IEnumerable<ModeloAtualizacao> modeloAtualizacoes)
        {
            try
            {
                var modeloAtualizacao = new ModeloAtualizacao();

                var assembly = modeloAtualizacao.GetType().Assembly;

                var resources = assembly.GetManifestResourceNames();

                List<ModeloAtualizacao> listaModeloAtualizacoes;

                var quantidadeScriptGerenciado = 10000;

                listaModeloAtualizacoes = new List<ModeloAtualizacao>() { Capacity = quantidadeScriptGerenciado };

                var caminhoScripts = "Catalogo.API.AtualizacaoBanco.ScriptsGerenciados";

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

                _logger.LogError($"Ocorreu um erro para carregar scripts gerenciados: {ex.Message}");
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
    }
}
