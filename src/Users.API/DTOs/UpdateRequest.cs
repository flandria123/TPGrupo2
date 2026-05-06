using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{
    public class UpdateRequest
    {
        public record UpdateItemRequest(

           [Required] string Name,
           [Required] string Apellido,
          [Required] string Email,
          [Required] string Password

           );




    }
}
