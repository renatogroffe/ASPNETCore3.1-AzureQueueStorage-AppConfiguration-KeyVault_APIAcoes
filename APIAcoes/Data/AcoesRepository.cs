using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;
using StackExchange.Redis;
using APIAcoes.Models;

namespace APIAcoes.Data
{
    public class AcoesRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ConnectionMultiplexer _conexaoRedis;

        public AcoesRepository(
            IConfiguration configuration,
            ConnectionMultiplexer conexaoRedis)
        {
            _configuration = configuration;
            _conexaoRedis = conexaoRedis;
        }

        public CotacaoAcao Get(string codigo)
        {
            string strDadosAcao =
                _conexaoRedis.GetDatabase().StringGet(
                    $"{_configuration["Redis:PrefixoChave"]}-{codigo}");
            if (!String.IsNullOrWhiteSpace(strDadosAcao))
                return JsonSerializer.Deserialize<CotacaoAcao>(
                    strDadosAcao,
                    new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });
            else
                return null;
        }        
        public List<CotacaoAcao> GetAll()
        {
            using (var conexao = new SqlConnection(_configuration["BaseAcoes"]))
            {
                return conexao.Query<CotacaoAcao>(
                    "SELECT Codigo, DataReferencia AS Data, " +
                    "CodCorretora, NomeCorretora, Valor "  +
                    "FROM dbo.HistoricoAcoes " +
                    "ORDER BY DataReferencia DESC"
                ).AsList();
            }
        }        
    }
}