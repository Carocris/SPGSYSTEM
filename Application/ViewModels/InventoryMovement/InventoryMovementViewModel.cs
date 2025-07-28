using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.InventoryMovement
{
    /// <summary>
    /// ViewModel para mostrar movimientos de inventario
    /// </summary>
    public class InventoryMovementViewModel
    {
        public int Id { get; set; }
        
        // Información del producto
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        
        // Información del movimiento
        public string MovementType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int StockBefore { get; set; }
        public int StockAfter { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        
        // Información de referencia
        public string? ReferenceNumber { get; set; }
        public string? ReferenceType { get; set; }
        
        // Información temporal
        public DateTime MovementDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Propiedades calculadas
        public bool IsEntry { get; set; }
        public bool IsExit { get; set; }
        public string MovementTypeBadge { get; set; } = string.Empty;
        public string QuantityDisplay => IsExit ? $"-{Math.Abs(Quantity)}" : $"+{Quantity}";
        public string StockChangeDisplay => $"{StockBefore} → {StockAfter}";
    }
} 