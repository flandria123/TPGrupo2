using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.API.Services;
using static Users.API.DTOs.CreateIRequest;
using static Users.API.Services.UserService;

namespace Users.API.Controllers
{
    [ApiController]  // [ApiController] indica que esta clase responde a peticiones web (API REST) [2].
                     // [Route] define que todos los endpoints de aquí empezarán con "api/users" [3].
    [Route("api/users")] // Ruta base según el contrato [6]
    public class UsersController: ControllerBase

    {
        private readonly IUserService _userService;
        
        
        // Inyección de Dependencias" para traer la lógica del Service [4].
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // VERBO POST: Se usa para CREAR recursos o realizar acciones de envío [3, 5].
        // En este caso, "register" crea un nuevo usuario en la base de datos.



        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <response code="201">Usuario creado con éxito.</response>
        /// <response code="409">El email ya está registrado (USR-001).</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateItemRequest request)
        {
            // El controlador NO tiene lógica de negocio [4, 7], le pasa el request al Service y espera la respuesta.
            var response = await _userService.RegisterAsync(request);

            // Retorna 201 Created según el contrato [6, 8]
            return CreatedAtAction(nameof(Register), new { id = response.Id }, response);
        }

        // VERBO POST (Acción): Aquí el POST se usa para enviar credenciales de Login.
        // No estamos creando un "recurso login", sino enviando datos para validar [9].

        /// <summary>
        /// Autentica a un usuario y maneja la regla de bloqueo.
        /// </summary>
        /// <response code="200">Login exitoso.</response>
        /// <response code="401">Credenciales incorrectas (USR-003).</response>
        /// <response code="403">Usuario bloqueado (USR-004 o USR-005).</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // El Service se encargará de contar los 3 intentos fallidos [9, 10].
            var response = await _userService.LoginAsync(request);

            // Retornamos 200 (OK) porque la operación de validación fue exitosa [11].
            return Ok(response);


        }




    }
}
