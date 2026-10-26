namespace CP5.Domain.Common
{
    public class ExternalMovieDetails
    {
        public string Title { get; set; } = string.Empty;
        public string Language { get; set; } = "Inglês";
        public string? Premiered { get; set; }
        public decimal? Rating { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string? OfficialSite { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> Genres { get; set; } = new();
        public string SourceApi { get; set; } = "TVMaze Public REST API";
        public bool IsFromCacheOrFallback { get; set; }
    }
}
