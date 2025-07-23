using System;

namespace Application.ViewModels.SupplierPrice
{
    public class SupplierPriceViewModel
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }
} 