using Application.Interfaces.Services;
using Database.Entities;

namespace Application.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de movimientos de inventario
    /// </summary>
    public interface IInventoryMovementService : IGenericService<InventoryMovement>
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
        /// Registra una entrada de inventario
        /// </summary>
        Task<InventoryMovement> RegisterEntryAsync(int productId, int quantity, string reason, string? referenceNumber = null, string? referenceType = null, string? notes = null, string? createdBy = null);

        /// <summary>
        /// Registra una salida de inventario
        /// </summary>
        Task<InventoryMovement> RegisterExitAsync(int productId, int quantity, string reason, string? referenceNumber = null, string? referenceType = null, string? notes = null, string? createdBy = null);

        /// <summary>
        /// Registra un ajuste de inventario
        /// </summary>
        Task<InventoryMovement> RegisterAdjustmentAsync(int productId, int quantity, string reason, string? notes = null, string? createdBy = null);

        /// <summary>
        /// Obtiene el resumen de movimientos para el dashboard
        /// </summary>
        Task<object> GetMovementSummaryAsync(DateTime startDate, DateTime endDate);
    }
} 