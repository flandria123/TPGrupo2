using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{
    public class CreateIRequest
    {
        public record CreateItemRequest(
            [Required] string Name,
            [Required] string Apellido,
           [Required] string Email,
           [Required] string Password
        );


       

    }
}
