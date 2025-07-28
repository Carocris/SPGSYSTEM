using Application.Interfaces.Repositories;
using Database.Entities;

namespace Application.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de movimientos de inventario
    /// </summary>
    public interface IInventoryMovementRepository : IGenericRepository<InventoryMovement>
    {
        /// <summary>
        /// Obtiene los movimientos de un producto específico
        /// </summary>
        Task<IReadOnlyList<InventoryMovement>> GetByProductAsync(int productId);

        /// <summary>
        /// Obtiene los movimientos por fecha
        /// </summary>
        Task<IReadOnlyList<InventoryMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Obtiene los movimientos por usuario
        /// </summary>
        Task<IReadOnlyList<InventoryMovement>> GetByUserAsync(string userName);

        /// <summary>
        /// Obtiene los movimientos por tipo
        /// </summary>
        Task<IReadOnlyList<InventoryMovement>> GetByTypeAsync(string movementType);

        /// <summary>
        /// Obtiene todos los movimientos con detalles del producto
        /// </summary>
        Task<IReadOnlyList<InventoryMovement>> GetAllWithDetailsAsync();
    }
} 