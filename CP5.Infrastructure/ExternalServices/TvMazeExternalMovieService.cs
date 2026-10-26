using CP5.Domain.Common;
using CP5.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CP5.Infrastructure.ExternalServices
{
    public class TvMazeExternalMovieService : IExternalMovieService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TvMazeExternalMovieService> _logger;

        public TvMazeExternalMovieService(HttpClient httpClient, ILogger<TvMazeExternalMovieService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<OperationResult<ExternalMovieDetails>> ObterDetalhesExternosAsync(
            string titulo, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("ExternalService: Consultando API pública TVMaze para o título '{Titulo}'", titulo);

            try
            {
                var encodedTitle = Uri.EscapeDataString(titulo.Trim());
                var requestUri = $"singlesearch/shows?q={encodedTitle}";

                var response = await _httpClient.GetAsync(requestUri, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("ExternalService: TVMaze respondeu com status {StatusCode} para '{Titulo}'. Aplicando fallback.",
                        response.StatusCode, titulo);

                    return OperationResult<ExternalMovieDetails>.Success(
                        GerarFallback(titulo, "Informações obtidas via mecanismo de resiliência e fallback."),
                        "Dados externos retornados via política de fallback resiliente.");
                }

                var show = await response.Content.ReadFromJsonAsync<TvMazeShowResponse>(cancellationToken: cancellationToken);

                if (show == null)
                {
                    return OperationResult<ExternalMovieDetails>.Success(
                        GerarFallback(titulo, "Sem dados detalhados disponíveis na API externa."),
                        "Dados externos obtidos via fallback.");
                }

                var cleanSummary = !string.IsNullOrWhiteSpace(show.Summary)
                    ? Regex.Replace(show.Summary, "<.*?>", string.Empty).Trim()
                    : "Sinopse não disponível na base externa.";

                var details = new ExternalMovieDetails
                {
                    Title = show.Name ?? titulo,
                    Language = show.Language ?? "Inglês",
                    Premiered = show.Premiered,
                    Rating = show.Rating?.Average,
                    Summary = cleanSummary,
                    OfficialSite = show.OfficialSite,
                    ImageUrl = show.Image?.Original ?? show.Image?.Medium,
                    Genres = show.Genres ?? new List<string>(),
                    SourceApi = "TVMaze Public REST API (https://api.tvmaze.com)",
                    IsFromCacheOrFallback = false
                };

                _logger.LogInformation("ExternalService: Dados externos obtidos com sucesso da TVMaze para '{Titulo}'", titulo);
                return OperationResult<ExternalMovieDetails>.Success(details, "Dados enriquecidos com sucesso da API externa.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExternalService: Exceção ao consultar API externa para '{Titulo}'. Ativando Fallback.", titulo);

                // Política de Fallback resiliente: nunca quebra a API do cliente se o serviço externo falhar
                var fallbackDetails = GerarFallback(titulo, $"Dados sintetizados via fallback resiliente devido a: {ex.Message}");
                return OperationResult<ExternalMovieDetails>.Success(
                    fallbackDetails,
                    "API externa temporariamente indisponível. Dados retornados através do mecanismo de Fallback resiliente.");
            }
        }

        private static ExternalMovieDetails GerarFallback(string titulo, string nota)
        {
            return new ExternalMovieDetails
            {
                Title = titulo,
                Language = "Português/Inglês",
                Premiered = "N/A",
                Rating = 8.0m,
                Summary = $"Informações do filme '{titulo}' consolidadas via Fallback. {nota}",
                OfficialSite = "https://www.tvmaze.com",
                ImageUrl = "https://images.unsplash.com/photo-1489599849927-2ee91cede3ba",
                Genres = new List<string> { "Cinema", "Entretenimento" },
                SourceApi = "TVMaze Resilient Fallback Provider",
                IsFromCacheOrFallback = true
            };
        }

        private sealed class TvMazeShowResponse
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("language")]
            public string? Language { get; set; }

            [JsonPropertyName("genres")]
            public List<string>? Genres { get; set; }

            [JsonPropertyName("premiered")]
            public string? Premiered { get; set; }

            [JsonPropertyName("officialSite")]
            public string? OfficialSite { get; set; }

            [JsonPropertyName("rating")]
            public TvMazeRating? Rating { get; set; }

            [JsonPropertyName("image")]
            public TvMazeImage? Image { get; set; }

            [JsonPropertyName("summary")]
            public string? Summary { get; set; }
        }

        private sealed class TvMazeRating
        {
            [JsonPropertyName("average")]
            public decimal? Average { get; set; }
        }

        private sealed class TvMazeImage
        {
            [JsonPropertyName("medium")]
            public string? Medium { get; set; }

            [JsonPropertyName("original")]
            public string? Original { get; set; }
        }
    }
}
