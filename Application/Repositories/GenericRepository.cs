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

        public virtual async Task AddAsync(T entity)
        {
            Console.WriteLine($"GenericRepository.AddAsync: Agregando entidad de tipo {typeof(T).Name}");
            await _db.Set<T>().AddAsync(entity);
            Console.WriteLine($"GenericRepository.AddAsync: Entidad agregada al contexto");
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
            Console.WriteLine($"GenericRepository.Update: Actualizando entidad de tipo {typeof(T).Name}");
            _db.Set<T>().Update(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            Console.WriteLine($"GenericRepository.SaveChangesAsync: Guardando cambios en la base de datos");
            var result = await _db.SaveChangesAsync();
            Console.WriteLine($"GenericRepository.SaveChangesAsync: Cambios guardados. Filas afectadas: {result}");
            return result;
        }
    }
}
