using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Database.Entities;
using Database.Repositories;

namespace Application.Services
{
    /// <summary>
    /// Servicio para movimientos de inventario
    /// </summary>
    public class InventoryMovementService : GenericService<InventoryMovement>, IInventoryMovementService
    {
        private readonly IInventoryMovementRepository _inventoryMovementRepository;
        private readonly IProductService _productService;

        public InventoryMovementService(
            IInventoryMovementRepository inventoryMovementRepository,
            IProductService productService) : base(inventoryMovementRepository)
        {
            _inventoryMovementRepository = inventoryMovementRepository;
            _productService = productService;
        }

        public async Task<IReadOnlyList<InventoryMovement>> GetByProductAsync(int productId)
        {
            return await _inventoryMovementRepository.GetByProductAsync(productId);
        }

        public async Task<IReadOnlyList<InventoryMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _inventoryMovementRepository.GetByDateRangeAsync(startDate, endDate);
        }

        public async Task<IReadOnlyList<InventoryMovement>> GetByUserAsync(string userName)
        {
            return await _inventoryMovementRepository.GetByUserAsync(userName);
        }

        public async Task<IReadOnlyList<InventoryMovement>> GetByTypeAsync(string movementType)
        {
            return await _inventoryMovementRepository.GetByTypeAsync(movementType);
        }

        public async Task<InventoryMovement> RegisterEntryAsync(int productId, int quantity, string reason, string? referenceNumber = null, string? referenceType = null, string? notes = null, string? createdBy = null)
        {
            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException("Producto no encontrado");

            var stockBefore = product.Stock;
            product.Stock += quantity;
            await _productService.UpdateAsync(product);

            var movement = new InventoryMovement
            {
                ProductId = productId,
                MovementType = "Entrada",
                Quantity = quantity,
                StockBefore = stockBefore,
                StockAfter = product.Stock,
                Reason = reason,
                ReferenceNumber = referenceNumber,
                ReferenceType = referenceType,
                Notes = notes,
                CreatedBy = createdBy ?? "Sistema",
                MovementDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            return await CreateAsync(movement);
        }

        public async Task<InventoryMovement> RegisterExitAsync(int productId, int quantity, string reason, string? referenceNumber = null, string? referenceType = null, string? notes = null, string? createdBy = null)
        {
            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException("Producto no encontrado");

            if (product.Stock < quantity)
                throw new InvalidOperationException($"Stock insuficiente. Stock actual: {product.Stock}, Cantidad solicitada: {quantity}");

            var stockBefore = product.Stock;
            product.Stock -= quantity;
            await _productService.UpdateAsync(product);

            var movement = new InventoryMovement
            {
                ProductId = productId,
                MovementType = "Salida",
                Quantity = -quantity, // Negativo para salidas
                StockBefore = stockBefore,
                StockAfter = product.Stock,
                Reason = reason,
                ReferenceNumber = referenceNumber,
                ReferenceType = referenceType,
                Notes = notes,
                CreatedBy = createdBy ?? "Sistema",
                MovementDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            return await CreateAsync(movement);
        }

        public async Task<InventoryMovement> RegisterAdjustmentAsync(int productId, int quantity, string reason, string? notes = null, string? createdBy = null)
        {
            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException("Producto no encontrado");

            var stockBefore = product.Stock;
            product.Stock += quantity;
            
            if (product.Stock < 0)
                throw new InvalidOperationException("El ajuste resultaría en stock negativo");

            await _productService.UpdateAsync(product);

            var movementType = quantity > 0 ? "Ajuste Positivo" : "Ajuste Negativo";
            
            var movement = new InventoryMovement
            {
                ProductId = productId,
                MovementType = movementType,
                Quantity = quantity,
                StockBefore = stockBefore,
                StockAfter = product.Stock,
                Reason = reason,
                Notes = notes,
                CreatedBy = createdBy ?? "Sistema",
                MovementDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            return await CreateAsync(movement);
        }

        public async Task<object> GetMovementSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var movements = await GetByDateRangeAsync(startDate, endDate);
            
            return new
            {
                TotalMovements = movements.Count,
                TotalEntries = movements.Count(m => m.IsEntry),
                TotalExits = movements.Count(m => m.IsExit),
                TotalQuantityEntered = movements.Where(m => m.IsEntry).Sum(m => m.Quantity),
                TotalQuantityExited = Math.Abs(movements.Where(m => m.IsExit).Sum(m => m.Quantity)),
                MovementsByType = movements.GroupBy(m => m.MovementType)
                                         .Select(g => new { Type = g.Key, Count = g.Count() })
                                         .ToList()
            };
        }
    }
} 