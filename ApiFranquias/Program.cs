using System.Text.Json.Serialization;
using ConectaFranquias.Api.Data;
using ConectaFranquias.Api.DTOs;
using ConectaFranquias.Api.Extensoes;
using ConectaFranquias.Api.Middlewares;
using Microsoft.AspNetCore.Mvc;

var construtor = WebApplication.CreateBuilder(args);

construtor.Services
    .AdicionarCamadasDaAplicacao(construtor.Configuration)
    .AdicionarSeguranca(construtor.Configuration)
    .AdicionarSwagger();

construtor.Services
    .AddControllers()
    .AddJsonOptions(opcoes =>
    {
        opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opcoes.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Falhas de validação de modelo saem no mesmo formato dos demais erros da API.
construtor.Services.Configure<ApiBehaviorOptions>(opcoes =>
{
    opcoes.InvalidModelStateResponseFactory = contexto =>
    {
        var erros = contexto.ModelState
            .Where(entrada => entrada.Value?.Errors.Count > 0)
            .ToDictionary(
                entrada => entrada.Key,
                entrada => entrada.Value!.Errors.Select(erro => erro.ErrorMessage).ToArray());

        return new BadRequestObjectResult(new RespostaErro(
            StatusCodes.Status400BadRequest,
            "Dados inválidos",
            "Um ou mais campos não passaram na validação. Confira o detalhamento em 'erros'.",
            erros));
    };
});

var app = construtor.Build();

// O tratamento de exceções vem primeiro para capturar tudo que vier depois.
app.UseMiddleware<MiddlewareTratamentoExcecoes>();

app.UseSwagger();
app.UseSwaggerUI(opcoes => opcoes.SwaggerEndpoint("/swagger/v1/swagger.json", "Conecta Franquias v1"));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await InicializadorBanco.InicializarAsync(app.Services);

app.Run();
