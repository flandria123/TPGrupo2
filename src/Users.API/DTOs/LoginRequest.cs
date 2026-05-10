using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{
    
    
        public record LoginRequestUser(
             [Required]     string Email,
             [Required] string Password
            )
        {

        };


    
}
