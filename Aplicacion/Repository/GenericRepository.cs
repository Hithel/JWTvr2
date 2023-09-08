
using System.Linq.Expressions;
using Dominio.Entities;
using Dominio.Interfaces;
using Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Aplicacion.Repository;

public class GenericRepo<T> : IGenericRepo<T> where T : BaseEntity
{
    private readonly ApiContext _context;

    public GenericRepo(ApiContext context)
    {
        _context = context;
    }

    public virtual void Add(T entity)
        {
            _context.Set<T>().Add(entity);
        }

        public virtual void AddRange(IEnumerable<T> entities)
        {
            _context.Set<T>().AddRange(entities);
        }
        public virtual IEnumerable<T> Find(Expression<Func<T,bool>> expression)
        {
            return _context.Set<T>().Where(expression);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public virtual  Task<T> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public void Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }
}
