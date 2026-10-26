namespace CP5.Domain.Exceptions
{
    public class RepositoryOperationException : Exception
    {
        public string Operation { get; }

        public RepositoryOperationException(string operation, string message)
            : base(message)
        {
            Operation = operation;
        }

        public RepositoryOperationException(string operation, string message, Exception innerException)
            : base(message, innerException)
        {
            Operation = operation;
        }
    }
}
