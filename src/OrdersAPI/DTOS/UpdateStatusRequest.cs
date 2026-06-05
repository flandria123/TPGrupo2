using System.ComponentModel.DataAnnotations;

namespace OrdersAPI.DTOs;

public class UpdateStatusRequest
{
    // Usamos DataAnnotations para que .NET valide automáticamente que no venga vacío
    [Required(ErrorMessage = "El campo NuevoEstado es obligatorio.")]
    public string NuevoEstado { get; set; } = string.Empty;
}