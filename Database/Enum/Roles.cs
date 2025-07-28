namespace Database.Enum
{
    /// Roles del sistema de inventario
    public enum Roles
    {
        /// Administrador del sistema - Acceso completo
        Admin,

        /// Gestor de inventario - Gestión de productos, categorías, proveedores
        InventoryManager,

        /// Vendedor - Gestión de ventas y clientes
        SalesUser,

        /// Auditor - Solo consultas y reportes
        Auditor
    }
} 