
using Aplicacion.Repository;
using Dominio.Entities;
using Dominio.Interfaces;
using Persistencia;

namespace Aplicacion.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApiContext context;
        
        private UserRepository _users;
        private RolRepository _rol;

public UnitOfWork (ApiContext _context)
        {
            context = _context;
        }
public IUser Usuarios 
        {
            get{
                if(_users == null)
                {
                    _users = new UserRepository(context);
                }
                return _users;
                }
        }

public IRol Roles
        {
            get{
                if(_rol == null)
                {
                    _rol = new RolRepository(context);
                }
                return _rol;
                }
        }
public void Dispose()
        {
            context.Dispose();
        }

        public async Task<int> SaveAsync()
        {
            return await context.SaveChangesAsync();
        }

    }