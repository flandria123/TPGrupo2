namespace Users.API.DTOs
{
    public class Response
    {
        public record UserResponse(

            Guid Id,
            string Nombre,
            string Apellido,
            string Email,
            DateTime FechaRegistro,
            bool Activo


            );


    }
}
