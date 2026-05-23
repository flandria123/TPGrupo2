using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{
    
    
        public record CreateItemRequest(
            /// <example>María</example>
            [Required] string Nombre,
            /// <example>González</example>
            [Required] string Apellido,
            /// <example>maria@email.com</example>
            [Required] string Email,
            /// <example>MiPassword123!</example>
            [Required] string Password
        );


       

    
}
