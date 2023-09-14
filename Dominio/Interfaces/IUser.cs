
using Dominio.Entities;

namespace Dominio.Interfaces;
    public interface IUser : IGenericRepo<User>
    {
        Task<User> GetUserNameAsync(string UserName);
        Task<User> GetByRefreshTokenAsync(string username);
    }
