using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{
    
    
        public record CreateItemRequest(
            [Required] string Nombre,
            [Required] string Apellido,
           [Required] string Email,
           [Required] string Password
        );


       

    
}
