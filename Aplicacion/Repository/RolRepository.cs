

using Dominio.Entities;
using Dominio.Interfaces;
using Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Aplicacion.Repository;

public class RolRepository : GenericRepo<Rol>, IRol
{
    private readonly ApiContext _context;

    public RolRepository(ApiContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<IEnumerable<Rol>> GetAllAsync()
        {
            return await _context.Roles
            .Include(p => p.Users)
            .ToListAsync();
        }

        public override async Task<Rol> GetByIdAsync(int id)
        {
            return await _context.Roles
            .Include(p => p.Users)
            .FirstOrDefaultAsync(p => p.Id == id);
        }
}