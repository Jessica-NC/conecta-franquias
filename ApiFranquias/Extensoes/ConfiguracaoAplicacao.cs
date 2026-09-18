using System.Reflection;
using System.Text;
using ConectaFranquias.Api.Data;
using ConectaFranquias.Api.Entities;
using ConectaFranquias.Api.Repositories;
using ConectaFranquias.Api.Services;
using ConectaFranquias.Api.Services.Seguranca;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace ConectaFranquias.Api.Extensoes;

public static class ConfiguracaoAplicacao
{
    private const string EsquemaSeguranca = "Bearer";

    public static IServiceCollection AdicionarCamadasDaAplicacao(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        var conexao = configuracao.GetConnectionString("FranquiasDb")
            ?? throw new InvalidOperationException(
                "A cadeia de conexão 'FranquiasDb' não foi encontrada no appsettings.");

        servicos.AddDbContext<ContextoFranquias>(opcoes => opcoes.UseSqlite(conexao));

        servicos.AddScoped<IUnidadeDeTrabalho, UnidadeDeTrabalho>();
        servicos.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
        servicos.AddScoped<IRepositorioFranqueadora, RepositorioFranqueadora>();
        servicos.AddScoped<IRepositorioFranqueado, RepositorioFranqueado>();
        servicos.AddScoped<IRepositorioUnidade, RepositorioUnidade>();
        servicos.AddScoped<IRepositorioCatalogo, RepositorioCatalogo>();
        servicos.AddScoped<IRepositorioFornecedor, RepositorioFornecedor>();
        servicos.AddScoped<IRepositorioEstoque, RepositorioEstoque>();
        servicos.AddScoped<IRepositorioVenda, RepositorioVenda>();
        servicos.AddScoped<IRepositorioRoyalty, RepositorioRoyalty>();
        servicos.AddScoped<IRepositorioChamado, RepositorioChamado>();
        servicos.AddScoped<IRepositorioRelatorio, RepositorioRelatorio>();

        servicos.AddHttpContextAccessor();
        servicos.AddSingleton<IServicoHashSenha, ServicoHashSenha>();
        servicos.AddScoped<IGeradorTokenJwt, GeradorTokenJwt>();
        servicos.AddScoped<IContextoUsuario, ContextoUsuario>();

        servicos.AddScoped<IServicoUsuario, ServicoUsuario>();
        servicos.AddScoped<IServicoRede, ServicoRede>();
        servicos.AddScoped<IServicoCatalogo, ServicoCatalogo>();
        servicos.AddScoped<IServicoFornecedor, ServicoFornecedor>();
        servicos.AddScoped<IServicoEstoque, ServicoEstoque>();
        servicos.AddScoped<IServicoVenda, ServicoVenda>();
        servicos.AddScoped<IServicoRoyalty, ServicoRoyalty>();
        servicos.AddScoped<IServicoChamado, ServicoChamado>();
        servicos.AddScoped<IServicoRelatorio, ServicoRelatorio>();

        return servicos;
    }

    public static IServiceCollection AdicionarSeguranca(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        servicos.AddOptions<OpcoesJwt>()
            .Bind(configuracao.GetSection(OpcoesJwt.SecaoConfiguracao))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var opcoes = configuracao.GetSection(OpcoesJwt.SecaoConfiguracao).Get<OpcoesJwt>()
            ?? throw new InvalidOperationException("A seção 'Jwt' não foi encontrada no appsettings.");

        servicos
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(parametros => parametros.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = opcoes.Emissor,
                ValidAudience = opcoes.Audiencia,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opcoes.ChaveSecreta)),
                // Sem tolerância de relógio o "expiraEm" devolvido no login é o prazo real.
                ClockSkew = TimeSpan.Zero
            });

        servicos.AddAuthorizationBuilder()
            .AddPolicy(Politicas.SomenteAdministrador, politica =>
                politica.RequireRole(nameof(TipoPerfil.Administrador)))
            .AddPolicy(Politicas.AdministradorOuGestor, politica =>
                politica.RequireRole(nameof(TipoPerfil.Administrador), nameof(TipoPerfil.GestorUnidade)));

        return servicos;
    }

    public static IServiceCollection AdicionarSwagger(this IServiceCollection servicos)
    {
        servicos.AddEndpointsApiExplorer();

        servicos.AddSwaggerGen(opcoes =>
        {
            opcoes.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Conecta Franquias",
                Version = "v1",
                Description =
                    "API REST para gestão de uma rede de franquias: unidades, catálogo, estoque, vendas, " +
                    "royalties, fornecedores, chamados e indicadores gerenciais.\n\n" +
                    "**Como testar:** autentique-se em `POST /api/auth/login`, copie o token retornado e " +
                    "informe-o no botão *Authorize* acima."
            });

            opcoes.AddSecurityDefinition(EsquemaSeguranca, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe apenas o token JWT; o prefixo \"Bearer\" é adicionado automaticamente."
            });

            // O requisito é montado por documento: a referência precisa apontar
            // para o esquema declarado no próprio documento OpenAPI.
            opcoes.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference(EsquemaSeguranca, documento), [] }
            });

            var caminhoXml = Path.Combine(
                AppContext.BaseDirectory,
                $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

            if (File.Exists(caminhoXml))
            {
                opcoes.IncludeXmlComments(caminhoXml);
            }
        });

        return servicos;
    }
}
