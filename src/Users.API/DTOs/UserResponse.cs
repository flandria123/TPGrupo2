namespace Users.API.DTOs
{


    /// <summary>
    /// Objeto que representa los datos públicos de un usuario.
    /// </summary>
    public record UserResponse
    {
        /// <summary>Identificador único del usuario.</summary>
        /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
        public required Guid Id { get; init; }

        /// <summary>Nombre del usuario.</summary>
        /// <example>María</example>
        public required string Nombre { get; init; }

        /// <summary>Apellido del usuario.</summary>
        /// <example>González</example>
        public required string Apellido { get; init; }

        /// <summary>Correo electrónico de contacto.</summary>
        /// <example>maria@email.com</example>
        public required string Email { get; init; }

        /// <summary>Fecha y hora en que se registró el usuario.</summary>
        /// <example>2024-03-10T09:00:00Z</example>
        public required DateTime FechaRegistro { get; init; }

        /// <summary>Indica si el usuario se encuentra activo o bloqueado.</summary>
        /// <example>true</example>
        public required bool Activo { get; init; }
    }



}
