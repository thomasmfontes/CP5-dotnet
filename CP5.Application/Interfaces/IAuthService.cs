using CP5.Application.DTOs.Auth;
using CP5.Domain.Common;

namespace CP5.Application.Interfaces
{
    public interface IAuthService
    {
        Task<OperationResult<LoginResponseDto>> LoginAsync(LoginRequestDto request);
    }
}
