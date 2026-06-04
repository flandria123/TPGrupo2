using System.ComponentModel.DataAnnotations;

namespace Users.API.Models
{
    public class User
    {

        public Guid Id { get; init; }
        [Required] public string Nombre { get; init; } = string.Empty;
        [Required] public string Apellido { get; init; } = string.Empty;
        [Required] [EmailAddress] public string Email { get; init; } = string.Empty;
        public string PasswordHash { get; init; } = string.Empty;
        public DateTime FechaRegistro { get; init; } = DateTime.UtcNow;
        public bool Activo { get; set; } = true;
        public int IntentosFallidos { get; set; } = 0;




    }

    



}

