using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.Notification
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Proveedor")]
        public int SupplierId { get; set; }
        
        [Display(Name = "Proveedor")]
        public string SupplierName { get; set; } = string.Empty;
        
        [Display(Name = "Título")]
        public string Title { get; set; } = string.Empty;
        
        [Display(Name = "Mensaje")]
        public string Message { get; set; } = string.Empty;
        
        [Display(Name = "Tipo")]
        public string Type { get; set; } = string.Empty;
        
        [Display(Name = "Leída")]
        public bool IsRead { get; set; }
        
        [Display(Name = "Fecha de Creación")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Fecha de Lectura")]
        public DateTime? ReadAt { get; set; }
        
        [Display(Name = "ID de Venta")]
        public int? SaleId { get; set; }
        
        [Display(Name = "ID de Producto")]
        public int? ProductId { get; set; }
        
        [Display(Name = "Nombre del Producto")]
        public string ProductName { get; set; } = string.Empty;
        
        [Display(Name = "Cantidad")]
        public int? Quantity { get; set; }
        
        [Display(Name = "Nombre del Cliente")]
        public string CustomerName { get; set; } = string.Empty;
    }
} 