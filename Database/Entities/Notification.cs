using System.ComponentModel.DataAnnotations;

namespace Database.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        
        [Required]
        public int SupplierId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;
        
        public bool IsRead { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ReadAt { get; set; }
        
        // Campos adicionales para notificaciones de ventas
        public int? SaleId { get; set; }
        public int? ProductId { get; set; }
        
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;
        
        public int? Quantity { get; set; }
        
        [MaxLength(200)]
        public string CustomerName { get; set; } = string.Empty;
        
        // Relación con Supplier
        public virtual Supplier Supplier { get; set; } = null!;
    }
} 