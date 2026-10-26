namespace CP5.Domain.Common
{
    public class OperationResult
    {
        public bool IsSuccess { get; }
        public int StatusCode { get; }
        public string Message { get; }
        public IReadOnlyList<string> Errors { get; }

        protected OperationResult(bool isSuccess, int statusCode, string message, IEnumerable<string>? errors = null)
        {
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            Message = message;
            Errors = errors?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
        }

        public static OperationResult Success(string message = "Operação realizada com sucesso.", int statusCode = 200)
            => new(true, statusCode, message);

        public static OperationResult Failure(string message, int statusCode = 400, params string[] errors)
            => new(false, statusCode, message, errors);

        public static OperationResult Failure(string message, IEnumerable<string> errors, int statusCode = 400)
            => new(false, statusCode, message, errors);

        public static OperationResult NotFound(string message = "Recurso não encontrado.")
            => new(false, 404, message);

        public static OperationResult Conflict(string message)
            => new(false, 409, message);

        public static OperationResult Unauthorized(string message = "Acesso não autorizado.")
            => new(false, 401, message);

        public static OperationResult Forbidden(string message = "Você não possui permissão para esta operação.")
            => new(false, 403, message);

        public static OperationResult InternalError(string message = "Ocorreu um erro interno no servidor.")
            => new(false, 500, message);
    }

    public class OperationResult<T> : OperationResult
    {
        public T? Data { get; }

        private OperationResult(bool isSuccess, int statusCode, string message, T? data = default, IEnumerable<string>? errors = null)
            : base(isSuccess, statusCode, message, errors)
        {
            Data = data;
        }

        public static OperationResult<T> Success(T data, string message = "Operação realizada com sucesso.", int statusCode = 200)
            => new(true, statusCode, message, data);

        public static OperationResult<T> Created(T data, string message = "Recurso criado com sucesso.")
            => new(true, 201, message, data);

        public static new OperationResult<T> Failure(string message, int statusCode = 400, params string[] errors)
            => new(false, statusCode, message, default, errors);

        public static new OperationResult<T> Failure(string message, IEnumerable<string> errors, int statusCode = 400)
            => new(false, statusCode, message, default, errors);

        public static new OperationResult<T> NotFound(string message = "Recurso não encontrado.")
            => new(false, 404, message, default);

        public static new OperationResult<T> Conflict(string message)
            => new(false, 409, message, default);

        public static new OperationResult<T> Unauthorized(string message = "Acesso não autorizado.")
            => new(false, 401, message, default);

        public static new OperationResult<T> Forbidden(string message = "Você não possui permissão para esta operação.")
            => new(false, 403, message, default);

        public static new OperationResult<T> InternalError(string message = "Ocorreu um erro interno no processamento.")
            => new(false, 500, message, default);
    }
}
