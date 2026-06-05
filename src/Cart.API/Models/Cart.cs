namespace CartAPI.Models
{
   public class Cart
    {
         public Guid UsuarioId { get; set; }

          public List<CartItem> Items { get; set; } = new();


        public DateTime FechaActualizacion { get; set; }
   }



}