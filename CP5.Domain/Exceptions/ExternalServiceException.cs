namespace CP5.Domain.Exceptions
{
    public class ExternalServiceException : Exception
    {
        public string ServiceName { get; }
        public int? HttpStatusCode { get; }

        public ExternalServiceException(string serviceName, string message, int? httpStatusCode = null)
            : base(message)
        {
            ServiceName = serviceName;
            HttpStatusCode = httpStatusCode;
        }

        public ExternalServiceException(string serviceName, string message, Exception innerException)
            : base(message, innerException)
        {
            ServiceName = serviceName;
        }
    }
}
