using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Logging;
using Users.API.DTOs;
using static Users.API.DTOs.CreateIRequest;
using static Users.API.Services.UserService;

namespace Users.API.Services
{
    public class UserService
    {
        public class Userservice(ILogger<Userservice> logger)
        {
            public void CreacionDeUsuario(CreateItemRequest request, Exception ex)
            {

               string ejemploHash = "SECRET_HASH_GENERADO";
                Guid ejemploID = Guid.NewGuid();

                logger.LogTrace ("Iniciando proceso de validación de datos para {Email}", request.Email);
                logger.LogDebug("Hash de contraseña generado correctamente: {Hash}", ejemploHash);
                logger.LogInformation("Operación completada para el usuario {Email}" , request.Email);
                logger.LogWarning("Configuración no encontrada, usando default");
                logger.LogError(ex, "Error al procesar el item {Id}", ejemploID);
                logger.LogCritical("Error fatal, cerrando aplicación");


            }
        }




    }
}
