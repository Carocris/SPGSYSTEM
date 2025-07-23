using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Database.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
  
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _db;

        public GenericRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(T entity)
        {
            await _db.Set<T>().AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _db.Set<T>().Remove(entity);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _db.Set<T>()
                             .AsNoTracking()
                             .ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _db.Set<T>().FindAsync(id);
        }

        public void Update(T entity)
        {
            _db.Set<T>().Update(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync();
        }
    }
}
