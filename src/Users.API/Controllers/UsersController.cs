using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.API.Services;
using static Users.API.Services.UserService;

namespace Users.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Tags("Users")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud exitosa:
        /// 
        ///     POST /api/users/register
        ///     {
        ///        "nombre": "María",
        ///        "apellido": "González",
        ///        "email": "maria@email.com",
        ///        "password": "MiPassword123!"
        ///     }
        /// 
        /// Ejemplo de respuesta de error (Email duplicado):
        /// 
        ///     {
        ///        "type": "https://tools.ietf.org/html/rfc7231#section-6.5.9",
        ///        "title": "Conflict",
        ///        "status": 409,
        ///        "errorCode": "USR-001",
        ///        "errorMessage": "El email 'maria@email.com' ya está registrado."
        ///     }
        /// Ejemplo de respuesta de error (Datos inválidos):
        /// 
        ///     {
        ///        "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        ///        "title": "Bad Request",
        ///        "status": 400,
        ///        "detail": "La solicitud contiene campos faltantes o con formato incorrecto.",
        ///        "instance": "/api/users/register",
        ///        "errorCode": "USR-002",
        ///        "errorMessage": "Los datos del usuario son inválidos."
        ///     }    
        /// 
        /// 
        /// </remarks>
        /// <param name="request">Datos del usuario (Nombre, Apellido, Email, Password).</param>
        /// <response code="201">Usuario creado con éxito. El campo PasswordHash no se incluye en la respuesta [2].</response>
        /// <response code="400">Los datos del usuario son inválidos (USR-002) [2].</response>
        /// <response code="409">El email ya está registrado (USR-001) [2, 4].</response>
        /// <response code="500">Error interno al procesar el usuario (USR-006) [3].</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] CreateItemRequest request)
        {
            var response = await _userService.RegisterAsync(request);
            return CreatedAtAction(nameof(Register), new { id = response.Id }, response);
        }

        /// <summary>
        /// Autentica a un usuario y maneja la regla de bloqueo de 3 intentos.
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud exitosa:
        /// 
        ///     POST /api/users/login
        ///     {
        ///        "email": "maria@email.com",
        ///        "password": "MiPassword123!"
        ///     }
        /// 
        /// Ejemplo de respuesta de error (Usuario bloqueado):
        /// 
        ///     {
        ///        "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        ///        "title": "Forbidden",
        ///        "status": 403,
        ///        "errorCode": "USR-004",
        ///        "errorMessage": "Su cuenta fue bloqueada por superar el máximo de intentos fallidos."
        ///     }
        ///     
        ///  Ejemplo de respuesta de error (Credenciales incorrectas):
        /// 
        ///     {
        ///        "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
        ///        "title": "Unauthorized",
        ///        "status": 401,
        ///        "detail": "Las credenciales no son válidas.",
        ///        "instance": "/api/users/login",
        ///        "errorCode": "USR-003",
        ///        "errorMessage": "Credenciales incorrectas."
        ///     }
        ///     
        ///  Ejemplo de respuesta de error (Bloqueo por seguridad):
        /// 
        ///     {
        ///        "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        ///        "title": "Forbidden",
        ///        "status": 403,
        ///        "detail": "El acceso está prohibido.",
        ///        "instance": "/api/users/login",
        ///        "errorCode": "USR-005",
        ///        "errorMessage": "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte."
        ///     }
        /// 
        /// </remarks>
        /// <response code="200">Login exitoso.</response>
        /// <response code="400">Los datos de la petición son inválidos (USR-002) [2].</response>
        /// <response code="401">Credenciales incorrectas (USR-003) [3, 5].</response>
        /// <response code="403">Usuario bloqueado por intentos (USR-004) o seguridad (USR-005) [3, 6].</response>
        /// <response code="500">Error interno al procesar el usuario (USR-006) [3].</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequestUser request)
        {
            var response = await _userService.LoginAsync(request);
            return Ok(response);
        }
    }
}