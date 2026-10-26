using CP5.Domain.Entities;

namespace CP5.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> ObterPorUsernameAsync(string username);
        Task<User?> ValidarCredenciaisAsync(string username, string password);
    }
}
