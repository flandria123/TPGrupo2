using Microsoft.Extensions.Logging;
using Serilog.Core;
using Users.API.Data;
using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;
using Users.API.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;



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
        // MÉTODO GET BY ID (Agregado para validación de Orders.API)
        //──────────────────────────────────────────────────────────────────────────
        public async Task<UserResponse?> GetByIdAsync(Guid id)
        {
            // Buscamos el usuario en la base de datos
            var user = await _userRepository.GetByIdAsync(id);

            // Si no existe, devolvemos nulo para que el Controller devuelva 404
            if (user == null)
            {
                return null;
            }

            // Mapeamos a UserResponse (sin exponer el PasswordHash)
            return new UserResponse
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email,
                FechaRegistro = user.FechaRegistro,
                Activo = user.Activo
            };
        }


        // ──────────────────────────────────────────────────────────────────────────
        // REGISTRO DE USUARIO
        // ──────────────────────────────────────────────────────────────────────────


        public async Task<UserResponse> RegisterAsync(CreateItemRequest request)
        {
            // Validación USR-002 para Registro [1, 4]
            var errores = new List<string>();
            if (string.IsNullOrWhiteSpace(request.Nombre)) errores.Add("El campo nombre es requerido");
            if (string.IsNullOrWhiteSpace(request.Apellido)) errores.Add("El campo apellido es requerido");
            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
                errores.Add("El email es requerido y debe tener un formato válido");
            if (string.IsNullOrWhiteSpace(request.Password)) errores.Add("La contraseña es requerida");

            if (errores.Any())
            {
                
                    var msg = string.Join("; ", errores);
                    // Logueamos la advertencia de validación para auditoría [2]
                    _logger.LogWarning("Error de validación en registro: {Errores}. [USR-002]", msg);
                    throw new ValidationException("USR-002", msg);
                
            }


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
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // Encriptar siempre [10, 11]
                FechaRegistro = DateTime.UtcNow,
                Activo = true,
                IntentosFallidos = 0
            };

            var userCreated = await _userRepository.CreateAsync(newUser);

            
            _logger.LogInformation(
            "Usuario registrado correctamente: {Email}",
            newUser.Email);
            
            // 3. Mapear a Respuesta (NUNCA devolver el PasswordHash) [7, 12]
            return MapToResponse(userCreated);
        }

        // ──────────────────────────────────────────────────────────────────────────
        // LOGIN Y REGLA DE BLOQUEO (3 INTENTOS)
        // ──────────────────────────────────────────────────────────────────────────
        public async Task<UserResponse> LoginAsync(LoginRequestUser request)
        {
            // Validación USR-002 para Login[1]
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Intento de login con credenciales vacías. [USR-002]");
                throw new ValidationException("USR-002", "Email y contraseña son campos obligatorios.");
            }


            var user = await _userRepository.GetByEmailAsync(request.Email);

            // 1. Si el usuario no existe o ya está bloqueado [7, 13, 14]

            if (user == null)
            {
                _logger.LogWarning("Fallo de login: Email inexistente {Email}. [USR-003]", request.Email);
                throw new BusinessRuleException("USR-003", "Credenciales incorrectas.");
            }

            if (!user.Activo)
            {
                // Diferenciar por qué está bloqueado según el catálogo
                if (user.IntentosFallidos >= 3)
                {
                    _logger.LogWarning("Intento de acceso a cuenta bloqueada por intentos: {Email}. [USR-004]", user.Email);
                    
                    throw new BusinessRuleException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");
                }

                _logger.LogWarning("Intento de acceso a cuenta suspendida por seguridad: {Email}. [USR-005]", user.Email);
                throw new BusinessRuleException("USR-005", "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.");
            }


            // 2. Validar Contraseña
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (isPasswordValid)
            {
                // LOGIN EXITOSO: Resetear intentos [9, 16]
                user.IntentosFallidos = 0;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation(
                 "Login exitoso para usuario: {Email}",
                user.Email);
                
                return MapToResponse(user);

            }

            else
            {
                // LOGIN FALLIDO: Incrementar contador y bloquear si llega a 3 [7, 12, 17]
                user.IntentosFallidos++;

                if (user.IntentosFallidos >= 3)
                {
                    user.Activo = false;
                    _logger.LogWarning("Usuario {Email} BLOQUEADO por 3 intentos fallidos.", user.Email);
                }

                else
                {
                    _logger.LogWarning("Contraseña incorrecta para {Email}. Intento #{Intentos}. [USR-003]", user.Email, user.IntentosFallidos);
                }


                await _userRepository.UpdateAsync(user);

                // Siempre devolver USR-003 aunque se bloquee en este paso, por seguridad
                throw new BusinessRuleException("USR-003", "Credenciales incorrectas.");
            }
        }

        private UserResponse MapToResponse(User entity) => new UserResponse
        {
            Id = entity.Id,
            Nombre = entity.Nombre,
            Apellido = entity.Apellido,
            Email = entity.Email,
            FechaRegistro = entity.FechaRegistro,
            Activo = entity.Activo
        };

    }
}
