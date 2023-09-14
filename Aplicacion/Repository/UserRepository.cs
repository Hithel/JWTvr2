

using Dominio.Entities;
using Dominio.Interfaces;
using Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Aplicacion.Repository;

public class UserRepository : GenericRepo<User>, IUser
{
    private readonly ApiContext _context;

    public UserRepository(ApiContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
            .Include(p => p.Roles)
            .ToListAsync();
        }

        public override async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users
            .Include(p => p.Roles)
            .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<User> GetUserNameAsync(string UserName)
        {
            return await _context.Users
            .Include(p => p.Roles)
            .FirstOrDefaultAsync(p => p.UserName.ToLower() == UserName.ToLower());
        }

            public async Task<User> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken));
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.UserName.ToLower() == username.ToLower());
    }
}