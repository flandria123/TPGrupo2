using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{


    public record UpdateItemRequest(
    /// <summary>Nombre del usuario</summary>
    /// <example>María</example>
    [Required] string Nombre,

    /// <summary>Apellido del usuario</summary>
    /// <example>González</example>
    [Required] string Apellido,

    /// <summary>Email de contacto</summary>
    /// <example>maria@email.com</example>
    [Required] string Email,

    /// <summary>Nueva contraseña</summary>
    /// <example>56789!</example>
    [Required] string Password
    );





}
