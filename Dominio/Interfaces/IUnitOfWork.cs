

namespace Dominio.Interfaces; 
public interface IUnitOfWork
    {
        IUser Usuarios  { get; }
        IRol Roles { get; }

        Task<int> SaveAsync();
    }