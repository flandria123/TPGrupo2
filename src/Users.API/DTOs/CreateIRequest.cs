using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{


    public record CreateItemRequest
    {
        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        /// <example>María</example>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public required string Nombre { get; init; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        /// <example>González</example>
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public required string Apellido { get; init; }

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        /// <example>maria@email.com</example>
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        public required string Email { get; init; }

        /// <summary>
        /// Contraseña elegida por el usuario.
        /// </summary>
        /// <example>MiPassword123!</example>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public required string Password { get; init; }
    }





}
