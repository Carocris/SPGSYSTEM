using System;

namespace Application.ViewModels.SupplierPriceHistory
{
    public class SupplierPriceHistoryViewModel
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string Currency { get; set; }
        public DateTime ChangeDate { get; set; }
        public string ChangedBy { get; set; }
        public string Notes { get; set; }
        public string ChangeReason { get; set; }

        // Propiedades calculadas
        public decimal PriceDifference => NewPrice - OldPrice;
        public decimal PriceChangePercentage => OldPrice > 0 ? ((NewPrice - OldPrice) / OldPrice) * 100 : 0;
        public string PriceChangeDirection => PriceDifference > 0 ? "Aumento" : PriceDifference < 0 ? "Disminución" : "Sin cambio";
        public string PriceChangeBadgeClass => PriceDifference > 0 ? "bg-danger" : PriceDifference < 0 ? "bg-success" : "bg-secondary";
    }
} 