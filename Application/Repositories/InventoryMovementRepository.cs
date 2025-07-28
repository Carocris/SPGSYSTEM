using Application.Interfaces.Repositories;
using Database.Contexts;
using Database.Entities;
using Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    /// <summary>
    /// Repositorio para movimientos de inventario
    /// </summary>
    public class InventoryMovementRepository : GenericRepository<InventoryMovement>, IInventoryMovementRepository
    {
        public InventoryMovementRepository(ApplicationDbContext db) : base(db) { }

        public async Task<IReadOnlyList<InventoryMovement>> GetByProductAsync(int productId)
        {
            return await _db.InventoryMovements
                            .Include(m => m.Product)
                            .Where(m => m.ProductId == productId)
                            .OrderByDescending(m => m.MovementDate)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<IReadOnlyList<InventoryMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _db.InventoryMovements
                            .Include(m => m.Product)
                            .Where(m => m.MovementDate >= startDate && m.MovementDate <= endDate)
                            .OrderByDescending(m => m.MovementDate)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<IReadOnlyList<InventoryMovement>> GetByUserAsync(string userName)
        {
            return await _db.InventoryMovements
                            .Include(m => m.Product)
                            .Where(m => m.CreatedBy == userName)
                            .OrderByDescending(m => m.MovementDate)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<IReadOnlyList<InventoryMovement>> GetByTypeAsync(string movementType)
        {
            return await _db.InventoryMovements
                            .Include(m => m.Product)
                            .Where(m => m.MovementType == movementType)
                            .OrderByDescending(m => m.MovementDate)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<IReadOnlyList<InventoryMovement>> GetAllWithDetailsAsync()
        {
            return await _db.InventoryMovements
                            .Include(m => m.Product)
                            .OrderByDescending(m => m.MovementDate)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public override async Task<IReadOnlyList<InventoryMovement>> GetAllAsync()
        {
            return await GetAllWithDetailsAsync();
        }
    }
} 