using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{


    public record UpdateItemRequest(
    /// <summary>Nombre del usuario</summary>
    /// <example>María</example>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    string Nombre,

    /// <summary>Apellido del usuario</summary>
    /// <example>González</example>
    [Required(ErrorMessage = "El apellido es obligatorio.")]
    string Apellido,

    /// <summary>Email de contacto</summary>
    /// <example>maria@email.com</example>
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    string Email,

    /// <summary>Nueva contraseña</summary>
    /// <example>56789!</example>
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    string Password
    );





}
