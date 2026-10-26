using CP5.Domain.Entities;
using CP5.Domain.Interfaces;
using CP5.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CP5.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> ObterPorUsernameAsync(string username)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.Trim().ToLower());
        }

        public async Task<User?> ValidarCredenciaisAsync(string username, string password)
        {
            var user = await ObterPorUsernameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("Validação de usuário falhou: usuário '{Username}' não existe.", username);
                return null;
            }

            var passwordValid = DbInitializer.VerifyPassword(password, user.PasswordHash);
            if (!passwordValid)
            {
                _logger.LogWarning("Validação de usuário falhou: senha incorreta para '{Username}'.", username);
                return null;
            }

            return user;
        }
    }
}
