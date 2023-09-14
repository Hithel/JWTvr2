using APIProyecto.Dtos;

namespace APIProyecto.Services;
public interface IUserService
{
    Task<string> RegisterAsync(RegisterDto model);
    Task<DatosUsuarioDto> GetTokenAsync(LoginDto model);
    Task<string> AddRoleAsync(AddRolDto model);
    Task<DatosUsuarioDto> RefreshTokenAsync(string refreshToken);
}