using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{
    
    
        public record LoginRequestUser(
             
            /// <example>maria@email.com</example>
             [Required(ErrorMessage = "El email es obligatorio.")]
             [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
             string Email,

             /// <example>MiPassword123!</example>
             [Required(ErrorMessage = "La contraseña es obligatoria.")]
             string Password
            )
        {

        };


    
}
