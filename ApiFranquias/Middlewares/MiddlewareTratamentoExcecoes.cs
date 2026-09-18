using System.Text.Json;
using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Excecoes;
using Microsoft.EntityFrameworkCore;

namespace ConectaFranquias.Api.Middlewares;

public class MiddlewareTratamentoExcecoes(
    RequestDelegate proximo,
    ILogger<MiddlewareTratamentoExcecoes> log,
    IHostEnvironment ambiente)
{
    private static readonly JsonSerializerOptions OpcoesSerializacao = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await proximo(contexto);
        }
        catch (Exception excecao)
        {
            await TratarAsync(contexto, excecao);
        }
    }

    private async Task TratarAsync(HttpContext contexto, Exception excecao)
    {
        var (status, titulo, mensagem) = MapearExcecao(excecao);

        if (status >= StatusCodes.Status500InternalServerError)
        {
            log.LogError(excecao, "Erro não tratado ao processar {Metodo} {Caminho}.",
                contexto.Request.Method, contexto.Request.Path);
        }

        // Se a resposta já começou a ser enviada, não há como reescrevê-la.
        if (contexto.Response.HasStarted)
        {
            return;
        }

        contexto.Response.Clear();
        contexto.Response.StatusCode = status;
        contexto.Response.ContentType = "application/json; charset=utf-8";

        var corpo = new RespostaErro(status, titulo, mensagem);

        await contexto.Response.WriteAsync(JsonSerializer.Serialize(corpo, OpcoesSerializacao));
    }

    private (int Status, string Titulo, string Mensagem) MapearExcecao(Exception excecao) => excecao switch
    {
        ExcecaoNaoEncontrado => (StatusCodes.Status404NotFound, "Recurso não encontrado", excecao.Message),

        ExcecaoConflito => (StatusCodes.Status409Conflict, "Conflito de dados", excecao.Message),

        ExcecaoAutorizacao => (StatusCodes.Status403Forbidden, "Acesso negado", excecao.Message),

        ExcecaoNegocio => (StatusCodes.Status400BadRequest, "Regra de negócio violada", excecao.Message),

        // Violação de índice único que escapou das validações do serviço.
        DbUpdateException => (
            StatusCodes.Status409Conflict,
            "Conflito de dados",
            "A operação viola uma restrição de integridade do banco de dados. Verifique se o registro já existe."),

        _ => (
            StatusCodes.Status500InternalServerError,
            "Erro interno",
            ambiente.IsDevelopment()
                ? excecao.Message
                : "Ocorreu um erro inesperado ao processar a requisição. Tente novamente mais tarde.")
    };
}
