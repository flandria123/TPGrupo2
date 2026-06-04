using System.ComponentModel.DataAnnotations;

namespace Ecommerce.App.APIS.Products.Models
{
    public class Product
    {
        /// Identificador único del producto.
        public Guid IdProduct { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Precio { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Required]
        public string Categoria { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
