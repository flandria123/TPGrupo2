using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{
    
    
        public record LoginRequestUser(
             /// <example>maria@email.com</example>
             [Required]     string Email,
             /// <example>MiPassword123!</example>
             [Required] string Password
            )
        {

        };


    
}
