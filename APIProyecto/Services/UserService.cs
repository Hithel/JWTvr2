using APIProyecto.Helpers;
using Dominio.Entities;
using Dominio.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using APIProyecto.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace APIProyecto.Services;
    public class UserService : IUserService
{
    private readonly JWT _jwt;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(IUnitOfWork unitOfWork, IOptions<JWT> jwt, IPasswordHasher<User> passwordHasher)
        {
            _jwt = jwt.Value;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }
        public async Task<string> RegisterAsync(RegisterDto registerDto)
        {
            var user = new User
            {
                Email = registerDto.Email,
                UserName = registerDto.UserName
            };

            user.Password = _passwordHasher.HashPassword(user, registerDto.Password); //Encrypt password

            var existingUser = _unitOfWork.Usuarios
                                        .Find(u => u.UserName.ToLower() == registerDto.UserName.ToLower())
                                        .FirstOrDefault();

            if (existingUser == null)
            {
                // var rolDefault = _unitOfWork.Roles
                //                         .Find(u => u.Nombre == Authorization.rol_default.ToString())
                //                         .First();
                try
                {
                    // user.Roles.Add(rolDefault);
                    _unitOfWork.Usuarios.Add(user);
                    await _unitOfWork.SaveAsync();

                    return $"User  {registerDto.UserName} has been registered successfully";
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                    return $"Error: {message}";
                }
            }
            else
            {
                return $"User {registerDto.UserName} already registered.";
            }
        }

        public async Task<string> AddRoleAsync(AddRolDto model)
        {
            var usuario = await _unitOfWork.Usuarios 
                                .GetUserNameAsync(model.UserName);

            if (usuario == null)
            {
                return $"No existe algun usuario registrado con la cuenta olvido algun caracter?{model.UserName}.";
            }

            var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.Password, model.Password);

            if (resultado == PasswordVerificationResult.Success)
            {
                var rolExiste = _unitOfWork.Roles
                                                .Find(u => u.Nombre.ToLower() == model.Rol.ToLower())
                                                .FirstOrDefault();

                if (rolExiste != null)
                {
                    var usuarioTieneRol = usuario.Roles
                                                    .Any(u => u.Id == rolExiste.Id);

                    if (usuarioTieneRol == false)
                    {
                        usuario.Roles.Add(rolExiste);
                        _unitOfWork.Usuarios .Update(usuario);
                        await _unitOfWork.SaveAsync();
                    }

                    return $"Rol {model.Rol} agregado a la cuenta {model.UserName} de forma exitosa.";
                }

                return $"Rol {model.Rol} no encontrado.";
            }

            return $"Credenciales incorrectas para el ususario {usuario.UserName}.";
        }
        public async Task<DatosUsuarioDto> GetTokenAsync(LoginDto model)
        {
            DatosUsuarioDto dataUserDto = new DatosUsuarioDto();
            var user = await _unitOfWork.Usuarios
                        .GetUserNameAsync(model.UserName);

            if (user == null)
            {
                dataUserDto.EstaAutenticado = false;
                dataUserDto.Mensaje = $"User does not exist with username {model.UserName}.";
                return dataUserDto;
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

            if (result == PasswordVerificationResult.Success)
            {
                dataUserDto.EstaAutenticado = true;
                JwtSecurityToken jwtSecurityToken = CreateJwtToken(user);
                dataUserDto.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                dataUserDto.Email = user.Email;
                dataUserDto.UserName = user.UserName;
                dataUserDto.Roles = user.Roles
                                                .Select(u => u.Nombre)
                                                .ToList();

                if (user.RefreshTokens.Any(a => a.IsActive))
                {
                    var activeRefreshToken = user.RefreshTokens.Where(a => a.IsActive == true).FirstOrDefault();
                    dataUserDto.RefreshToken = activeRefreshToken.Token;
                    dataUserDto.RefreshTokenExpiration = activeRefreshToken.Expires;
                }
                else
                {
                    var refreshToken = CreateRefreshToken();
                    dataUserDto.RefreshToken = refreshToken.Token;
                    dataUserDto.RefreshTokenExpiration = refreshToken.Expires;
                    user.RefreshTokens.Add(refreshToken);
                    _unitOfWork.Usuarios.Update(user);
                    await _unitOfWork.SaveAsync();
                }

                return dataUserDto;
            }
            dataUserDto.EstaAutenticado = false;
            dataUserDto.Mensaje = $"Credenciales incorrectas para el usuario {user.UserName}.";
            return dataUserDto;
        }

        public async Task<DatosUsuarioDto> RefreshTokenAsync(string refreshToken)
        {
            var DatosUsuarioDto = new DatosUsuarioDto();

            var usuario = await _unitOfWork.Usuarios
                            .GetByRefreshTokenAsync(refreshToken);

            if (usuario == null)
            {
                DatosUsuarioDto.EstaAutenticado = false;
                DatosUsuarioDto.Mensaje = $"Token is not assigned to any user.";
                return DatosUsuarioDto;
            }

            var refreshTokenBd = usuario.RefreshTokens.Single(x => x.Token == refreshToken);

            if (!refreshTokenBd.IsActive)
            {
                DatosUsuarioDto.EstaAutenticado = false;
                DatosUsuarioDto.Mensaje = $"Token is not active.";
                return DatosUsuarioDto;
            }
            //Revoque the current refresh token and
            refreshTokenBd.Revoked = DateTime.UtcNow;
            //generate a new refresh token and save it in the database
            var newRefreshToken = CreateRefreshToken();
            usuario.RefreshTokens.Add(newRefreshToken);
            _unitOfWork.Usuarios.Update(usuario);
            await _unitOfWork.SaveAsync();
            //Generate a new Json Web Token
            DatosUsuarioDto.EstaAutenticado = true;
            JwtSecurityToken jwtSecurityToken = CreateJwtToken(usuario);
            DatosUsuarioDto.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            DatosUsuarioDto.Email = usuario.Email;
            DatosUsuarioDto.UserName = usuario.UserName;
            DatosUsuarioDto.Roles = usuario.Roles
                                            .Select(u => u.Nombre)
                                            .ToList();
            DatosUsuarioDto.RefreshToken = newRefreshToken.Token;
            DatosUsuarioDto.RefreshTokenExpiration = newRefreshToken.Expires;
            return DatosUsuarioDto;
        }
        private RefreshToken CreateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomNumber);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomNumber),
                    Expires = DateTime.UtcNow.AddDays(10),
                    Created = DateTime.UtcNow
                };
            }
        }

        private JwtSecurityToken CreateJwtToken(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "El usuario no puede ser nulo.");
            }

            var roles = user.Roles;
            var roleClaims = new List<Claim>();
            foreach (var role in roles)
            {
                roleClaims.Add(new Claim("roles", role.Nombre));
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("uid", user.Id.ToString())
            }
            .Union(roleClaims);

            if (string.IsNullOrEmpty(_jwt.Key) || string.IsNullOrEmpty(_jwt.Issuer) || string.IsNullOrEmpty(_jwt.Audience))
            {
                throw new ArgumentNullException("La configuración del JWT es nula o vacía.");
            }

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha512Signature);

            var JwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: signingCredentials);

            return JwtSecurityToken;
        }

    }

