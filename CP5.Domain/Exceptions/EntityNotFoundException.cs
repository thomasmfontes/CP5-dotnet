namespace CP5.Domain.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public string EntityName { get; }
        public object Key { get; }

        public EntityNotFoundException(string entityName, object key)
            : base($"A entidade '{entityName}' com identificador '{key}' não foi encontrada.")
        {
            EntityName = entityName;
            Key = key;
        }

        public EntityNotFoundException(string message) : base(message)
        {
            EntityName = "Entidade";
            Key = 0;
        }
    }
}
