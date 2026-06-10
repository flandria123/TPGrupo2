using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{


    /// <summary>
    /// Objeto con las credenciales para autenticar a un usuario.
    /// </summary>
    public record LoginRequestUser
    {
        /// <summary>
        /// Correo electrónico registrado del usuario.
        /// </summary>
        /// <example>maria@email.com</example>
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        public required string Email { get; init; }

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        /// <example>MiPassword123!</example>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public required string Password { get; init; }
    }



}
