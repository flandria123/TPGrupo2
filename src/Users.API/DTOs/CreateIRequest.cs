using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{
    
    
        public record CreateItemRequest(
            
            /// <example>María</example>
            [Required(ErrorMessage = "El nombre es obligatorio.")]
            string Nombre,
            
            /// <example>González</example>
            [Required(ErrorMessage = "El apellido es obligatorio.")]
            string Apellido,
            
            /// <example>maria@email.com</example>
            [Required(ErrorMessage = "El email es obligatorio.")]
            [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
            string Email,

            /// <example>MiPassword123!</example>
            [Required(ErrorMessage = "La contraseña es obligatoria.")]
             string Password
        );


       

    
}
