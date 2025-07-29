namespace Database.Enum
{
    /// Roles del sistema de inventario
    public enum Roles
    {
        /// Administrador del sistema - Acceso completo
        Admin,

        /// Auditor - Solo consultas y reportes
        Auditor,

        /// Proveedor - Solo puede ver y gestionar sus productos
        Supplier,

        /// Cliente - Solo puede ver productos y realizar compras
        Customer
    }
} 