using System.ComponentModel.DataAnnotations;

namespace Users.API.Models
{
    public class Item
    {

        public Guid Id { get; init; }
        [Required] public string Nombre { get; init; }
        [Required] public string Apellido { get; init; }
        [Required] public string Email { get; init; }
        public string PasswordHash { get; init; }
        public DateTime FechaRegistro { get; init; }
        public bool Activo { get; set; } = true;
        public int IntentosFallidos { get; set; } = 0;




    }

    



}

