using CP5.Domain.Entities;

namespace CP5.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        (string Token, DateTime Expiration) GenerateToken(User user);
    }
}
