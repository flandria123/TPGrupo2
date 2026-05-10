using Users.API.Exceptions;
using Users.API.Models;
using Users.API.Data;
using Microsoft.Extensions.Logging;
using Users.API.DTOs;
using Users.API.Services;



namespace Users.API.Services
{
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(UserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        // ──────────────────────────────────────────────────────────────────────────
        // REGISTRO DE USUARIO
        // ──────────────────────────────────────────────────────────────────────────
        public async Task<UserResponse> RegisterAsync(CreateItemRequest request)
        {
            // 1. Validar si el email ya existe (Catálogo USR-001) [7, 8]
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Intento de registro con email duplicado: {Email}", request.Email);
                throw new BusinessRuleException("USR-001", $"El email '{request.Email}' ya está registrado.");
            }

            // 2. Crear la entidad (El usuario nace Activo y con 0 intentos) [7, 9]
            var newUser = new User
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // Encriptar siempre [10, 11]
                FechaRegistro = DateTime.UtcNow,
                Activo = true,
                IntentosFallidos = 0
            };

            var userCreated = await _userRepository.CreateAsync(newUser);

            // 3. Mapear a Respuesta (NUNCA devolver el PasswordHash) [7, 12]
            return MapToResponse(userCreated);
        }

        // ──────────────────────────────────────────────────────────────────────────
        // LOGIN Y REGLA DE BLOQUEO (3 INTENTOS)
        // ──────────────────────────────────────────────────────────────────────────
        public async Task<UserResponse> LoginAsync(LoginRequestUser request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            // 1. Si el usuario no existe o ya está bloqueado [7, 13, 14]
            if (user == null)
                throw new BusinessRuleException("USR-003", "Credenciales incorrectas.");

            if (!user.Activo)
            {
                // Diferenciar por qué está bloqueado según el catálogo [13-15]
                if (user.IntentosFallidos >= 3)
                    throw new BusinessRuleException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos.");

                throw new BusinessRuleException("USR-005", "Su cuenta fue suspendida por razones de seguridad.");
            }

            // 2. Validar Contraseña
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (isPasswordValid)
            {
                // LOGIN EXITOSO: Resetear intentos [9, 16]
                user.IntentosFallidos = 0;
                await _userRepository.UpdateAsync(user);
                return MapToResponse(user);
            }
            else
            {
                // LOGIN FALLIDO: Incrementar contador y bloquear si llega a 3 [7, 12, 17]
                user.IntentosFallidos++;

                if (user.IntentosFallidos >= 3)
                {
                    user.Activo = false;
                    _logger.LogCritical("Usuario {Email} BLOQUEADO por 3 intentos fallidos.", user.Email);
                }

                await _userRepository.UpdateAsync(user);

                // Siempre devolver USR-003 aunque se bloquee en este paso, por seguridad
                throw new BusinessRuleException("USR-003", "Credenciales incorrectas.");
            }
        }

        private UserResponse MapToResponse(User entity) => new(
            entity.Id,
            entity.Nombre,
            entity.Apellido,
            entity.Email,
            entity.FechaRegistro,
            entity.Activo
        );



    }
}
