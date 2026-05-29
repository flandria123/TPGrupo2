namespace Users.API.DTOs
{
    
    
        public record UserResponse(
            /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
            Guid Id,
            /// <example>María</example>
            string Nombre,
            /// <example>González</example>
            string Apellido,
            /// <example>maria@email.com</example>
            string Email,

            /// <example>2024-03-10T09:00:00Z </example>
            DateTime FechaRegistro,
            
            /// <example>true</example>
            bool Activo



            );


    
}
