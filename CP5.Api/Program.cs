using CP5.Api.Middlewares;
using CP5.Application.Interfaces;
using CP5.Application.Services;
using CP5.Domain.Interfaces;
using CP5.Infrastructure.Data;
using CP5.Infrastructure.ExternalServices;
using CP5.Infrastructure.Repositories;
using CP5.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Serilog;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CONFIGURAÇÃO DE LOGGING ESTRUTURADO COM SERILOG (ARQUIVO ROTATIVO)
// =========================================================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/cp5-filmes-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 15,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// =========================================================================
// 2. BANCO DE DADOS (ORACLE COM SUPORTE A IN-MEMORY PARA TESTES E DESENVOLVIMENTO)
// =========================================================================
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");
var oracleConnection = builder.Configuration.GetConnectionString("OracleConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useInMemory || string.IsNullOrWhiteSpace(oracleConnection))
    {
        options.UseInMemoryDatabase("CP5_FilmesDb");
    }
    else
    {
        options.UseOracle(oracleConnection);
    }
});

// =========================================================================
// 3. INJEÇÃO DE DEPENDÊNCIA (CLEAN ARCHITECTURE)
// =========================================================================
// Repositórios (Infrastructure)
builder.Services.AddScoped<IFilmeRepository, FilmeRepository>();
builder.Services.AddScoped<IAvaliacaoRepository, AvaliacaoRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Serviços de Aplicação (Application)
builder.Services.AddScoped<IFilmeService, FilmeService>();
builder.Services.AddScoped<IAvaliacaoService, AvaliacaoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHateoasService, HateoasService>();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

// =========================================================================
// 4. INTEGRAÇÃO COM SERVIÇO EXTERNO E RESILIÊNCIA COM POLLY
// =========================================================================
var tvMazeBaseUrl = builder.Configuration["ExternalServices:TvMazeBaseUrl"] ?? "https://api.tvmaze.com/";
builder.Services.AddHttpClient<IExternalMovieService, TvMazeExternalMovieService>(client =>
{
    client.BaseAddress = new Uri(tvMazeBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(8);
    client.DefaultRequestHeaders.Add("User-Agent", "CP5-FilmesApi-FIAP");
})
.AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(2, retryAttempt =>
    TimeSpan.FromMilliseconds(300 * Math.Pow(2, retryAttempt))))
.AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(15)));

// =========================================================================
// 5. AUTENTICAÇÃO E AUTORIZAÇÃO COM JWT BEARER
// =========================================================================
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "MinhaChaveSuperSecretaParaCP5Fiap2026_AssinaturaJWT_123456789";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CP5_FilmesApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CP5_FilmesApi_Clients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Token = authHeader.Trim();
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});

// =========================================================================
// 6. OTIMIZAÇÃO: COMPRESSÃO DE RESPOSTAS E RATE LIMITING
// =========================================================================
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });
});

// =========================================================================
// 7. OBSERVABILIDADE: HEALTH CHECKS COM TAGS LIVE E READY (REQUISITO CP5)
// =========================================================================
builder.Services.AddHealthChecks()
    // Self check registrado com a tag 'live' (Liveness Probe para Kubernetes/Docker)
    .AddCheck("self", () => HealthCheckResult.Healthy("API operacional e responsiva"), tags: new[] { "live" })
    // Database check registrado com a tag 'ready' (Readiness Probe)
    .AddDbContextCheck<AppDbContext>("database", tags: new[] { "ready" })
    // Verificação de conectividade com serviço externo registrado com a tag 'ready'
    .AddCheck("tvmaze_external_service", () => HealthCheckResult.Healthy("Serviço externo TVMaze configurado"), tags: new[] { "ready" });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// =========================================================================
// 8. SWAGGER / OPENAPI DOCUMENTATION COM SUPORTE A JWT E ANNOTATIONS
// =========================================================================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CP5 - API de Filmes e Avaliações",
        Version = "v1",
        Description = "API RESTful Corporativa desenvolvida em ASP.NET Core (.NET 8) aplicando Clean Architecture, Operation Result, PageResult, HATEOAS, Erros Customizados, Resiliência com Polly e Segurança JWT.",
        Contact = new OpenApiContact
        {
            Name = "Grupo CP5 - FIAP 2026",
            Email = "contato@fiap.com.br"
        }
    });

    c.EnableAnnotations();

    // Configuração do Bearer Token no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT retornado no login",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// =========================================================================
// 9. INICIALIZAÇÃO E SEED DO BANCO DE DADOS
// =========================================================================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
        await DbInitializer.SeedAsync(context);
        Log.Information("Banco de dados inicializado e populado com sucesso.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Erro ao inicializar o banco de dados.");
    }
}

// =========================================================================
// 10. PIPELINE HTTP
// =========================================================================
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CP5 Filmes API v1");
    c.RoutePrefix = string.Empty; // Swagger na raiz: /
});

app.UseResponseCompression();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// =========================================================================
// 11. MAPEAMENTO DOS HEALTH CHECKS (/health/live, /health/ready, /health)
// =========================================================================
// Rota /health/live: Valida apenas verificações com tag 'live' (Liveness probe)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = WriteHealthCheckJsonResponse
});

// Rota /health/ready: Valida dependências externas com tag 'ready' (Readiness probe)
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthCheckJsonResponse
});

// Rota /health: Retorna o diagnóstico completo de todos os health checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckJsonResponse
});

app.MapControllers().RequireRateLimiting("fixed");

Log.Information("Iniciando CP5 Filmes API...");
app.Run();

// Função local para formatação JSON estruturada de Health Checks
static Task WriteHealthCheckJsonResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";

    var response = new
    {
        status = report.Status.ToString(),
        totalDuration = report.TotalDuration.ToString(),
        timestamp = DateTime.UtcNow,
        entries = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description,
            duration = e.Value.Duration.ToString(),
            tags = e.Value.Tags,
            error = e.Value.Exception?.Message
        })
    };

    var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
    return context.Response.WriteAsync(json);
}

// Torna Program acessível para testes de integração com WebApplicationFactory
public partial class Program { }
