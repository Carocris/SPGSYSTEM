using System;
using System.Collections.Generic;

namespace Application.ViewModels.PurchaseOrder
{
    public class PurchaseOrderViewModel
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModified { get; set; }

        // Propiedades calculadas
        public string StatusDisplay => Status switch
        {
            "Pending" => "Pendiente",
            "Confirmed" => "Confirmada",
            "Shipped" => "Enviada",
            "Delivered" => "Entregada",
            "Cancelled" => "Cancelada",
            _ => Status
        };

        public string StatusBadgeClass => Status switch
        {
            "Pending" => "bg-warning",
            "Confirmed" => "bg-info",
            "Shipped" => "bg-primary",
            "Delivered" => "bg-success",
            "Cancelled" => "bg-danger",
            _ => "bg-secondary"
        };

        public bool IsDelivered => Status == "Delivered";
        public bool IsCancelled => Status == "Cancelled";
        public bool CanEdit => Status == "Pending" || Status == "Confirmed";

        // Navigation properties
        public List<PurchaseOrderDetailViewModel> Details { get; set; } = new List<PurchaseOrderDetailViewModel>();
    }

    public class PurchaseOrderDetailViewModel
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Notes { get; set; }
        public int? ReceivedQuantity { get; set; }

        // Propiedades calculadas
        public int PendingQuantity => Quantity - (ReceivedQuantity ?? 0);
        public bool IsFullyReceived => PendingQuantity <= 0;
        public string ReceivedStatus => IsFullyReceived ? "Completado" : $"Pendiente ({PendingQuantity})";
        public string ReceivedBadgeClass => IsFullyReceived ? "bg-success" : "bg-warning";
    }
} 