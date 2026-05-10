
using Dapper;
    using Microsoft.Data.Sqlite;
using Users.API.Models;

namespace Users.API.Data
{
    public class UserRepository
    {
        private readonly IConfiguration _config;

        public UserRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

        // ── GET BY EMAIL (CRÍTICO PARA LOGIN Y REGISTRO) ─────────────────────
        public async Task<User?> GetByEmailAsync(string email)
        {
            using var conn = CreateConnection();
            // Usamos AS para mapear snake_case de DB a PascalCase de C# [8]
            return await conn.QuerySingleOrDefaultAsync<User>("""
            SELECT id, nombre, apellido, email, 
                   password_hash AS PasswordHash, 
                   fecha_registro AS FechaRegistro, 
                   activo, intentos_fallidos AS IntentosFallidos
            FROM usuarios
            WHERE email = @email
        """, new { email });
        }

        // ── CREATE (PARA REGISTRO) ───────────────────────────────────────────
        public async Task<User> CreateAsync(User user)
        {
            using var conn = CreateConnection();
            var id = await conn.ExecuteScalarAsync<int>("""
            INSERT INTO usuarios (nombre, apellido, email, password_hash, activo, intentos_fallidos)
            VALUES (@Nombre, @Apellido, @Email, @PasswordHash, @Activo, @IntentosFallidos);
            SELECT last_insert_rowid();
        """, user);

            // Retornamos la entidad completa (usando su ID si es Guid o int)
            return user;
        }

        // ── UPDATE (PARA BLOQUEO DE CUENTA Y RESET DE INTENTOS) ──────────────
        public async Task<bool> UpdateAsync(User user)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync("""
            UPDATE usuarios
            SET activo = @Activo,
                intentos_fallidos = @IntentosFallidos
            WHERE id = @Id
        """, new { user.Activo, user.IntentosFallidos, user.Id });

            return rows > 0;
        }



    }
}
