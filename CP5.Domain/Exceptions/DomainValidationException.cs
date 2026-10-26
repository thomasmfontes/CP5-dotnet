namespace CP5.Domain.Exceptions
{
    public class DomainValidationException : Exception
    {
        public IReadOnlyList<string> Erros { get; }

        public DomainValidationException(string message) : base(message)
        {
            Erros = new List<string> { message };
        }

        public DomainValidationException(string message, IEnumerable<string> erros) : base(message)
        {
            Erros = erros?.ToList() ?? new List<string> { message };
        }
    }
}
