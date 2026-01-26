using System;
using System.ComponentModel.DataAnnotations;

namespace PuntoDeVenta.API.DTOs
{
    /// <summary>
    /// DTO para mostrar informacion de cliente
    /// </summary>
    public class ClienteDTO
    {
        public int IdCliente { get; set; }
        public string RazonSocial { get; set; }
        public string Documento { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Domicilio { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
    }

    /// <summary>
    /// DTO para crear un nuevo cliente
    /// </summary>
    public class ClienteCreateDTO
    {
        [Required(ErrorMessage = "La razon social es requerida")]
        [StringLength(200, ErrorMessage = "La razon social no puede exceder 200 caracteres")]
        public string RazonSocial { get; set; }

        [Required(ErrorMessage = "El documento es requerido")]
        [StringLength(20, ErrorMessage = "El documento no puede exceder 20 caracteres")]
        public string Documento { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El email no es valido")]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(200)]
        public string Domicilio { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un cliente
    /// </summary>
    public class ClienteUpdateDTO
    {
        [StringLength(200)]
        public string RazonSocial { get; set; }

        [StringLength(20)]
        public string Documento { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(200)]
        public string Domicilio { get; set; }

        public bool? Activo { get; set; }
    }

    /// <summary>
    /// DTO para cambiar estado (activar/desactivar)
    /// </summary>
    public class ClienteCambiarEstadoDTO
    {
        [Required]
        public bool Activo { get; set; }
    }

    /// <summary>
    /// DTO con estadisticas del cliente
    /// </summary>
    public class ClienteEstadisticasDTO
    {
        public int IdCliente { get; set; }
        public int TotalCompras { get; set; }
        public decimal MontoAcumulado { get; set; }
        public DateTime? UltimaCompra { get; set; }
        public bool EsFrecuente { get; set; }
    }

    /// <summary>
    /// DTO para historial de compras simplificado
    /// </summary>
    public class ClienteCompraDTO
    {
        public int IdVenta { get; set; }
        public string NoFactura { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public int CantidadArticulos { get; set; }
        public bool Cancelada { get; set; }
    }
}
