namespace CP5.Domain.Common
{
    public class LinkDto
    {
        public string Rel { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
        public string Method { get; set; } = "GET";

        public LinkDto() { }

        public LinkDto(string rel, string href, string method = "GET")
        {
            Rel = rel;
            Href = href;
            Method = method;
        }
    }

    public abstract class ResourceWithLinks
    {
        public IList<LinkDto> Links { get; set; } = new List<LinkDto>();
    }
}
