namespace CP5.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // "Admin" ou "User"
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public User() { }

        public User(string username, string passwordHash, string nome, string role)
        {
            Username = username;
            PasswordHash = passwordHash;
            Nome = nome;
            Role = role;
            DataCriacao = DateTime.UtcNow;
        }
    }
}
