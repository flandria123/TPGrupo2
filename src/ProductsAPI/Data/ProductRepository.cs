using Dapper;
using Microsoft.Data.Sqlite;
using ProductsAPI.Models;

namespace ProductsAPI.Data;

public class ProductRepository
{
    private readonly IConfiguration _config;

    public ProductRepository(IConfiguration config) => _config = config;

    private SqliteConnection CreateConnection() =>
        new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

    // ── GET ALL (Con filtros opcionales de la Sección 4.1) ── [cite: 4]
    public async Task<IEnumerable<Product>> GetAllAsync(string? categoria, string? nombre)
    {
        using var conn = CreateConnection();

        // Armamos la consulta dinámica según qué filtros vengan
        var sql = "SELECT Id, Nombre, Descripcion, Precio, Stock, Categoria, FechaCreacion FROM Products WHERE 1=1";

        if (!string.IsNullOrEmpty(categoria))
            sql += " AND Categoria = @Categoria";

        if (!string.IsNullOrEmpty(nombre))
            sql += " AND Nombre LIKE '%' || @Nombre || '%'";

        return await conn.QueryAsync<Product>(sql, new { Categoria = categoria, Nombre = nombre });
    }

    // ── GET BY ID ── [cite: 96]
    public async Task<Product?> GetByIdAsync(Guid id)
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Product>(
            "SELECT Id, Nombre, Descripcion, Precio, Stock, Categoria, FechaCreacion FROM Products WHERE Id = @Id",
            new { Id = id });
    }

    // ── CREATE ── [cite: 97]
    public async Task CreateAsync(Product product)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            INSERT INTO Products (Id, Nombre, Descripcion, Precio, Stock, Categoria, FechaCreacion)
            VALUES (@Id, @Nombre, @Descripcion, @Precio, @Stock, @Categoria, @FechaCreacion)
        """, product);
    }

    // ── UPDATE ── [cite: 98]
    public async Task UpdateAsync(Product product)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE Products 
            SET Nombre = @Nombre, 
                Descripcion = @Descripcion, 
                Precio = @Precio, 
                Stock = @Stock, 
                Categoria = @Categoria
            WHERE Id = @Id
        """, product);
    }

    // ── DELETE ── [cite: 99]
    public async Task DeleteAsync(Guid id)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("DELETE FROM Products WHERE Id = @Id", new { Id = id });
    }

    // ── VALIDACIÓN PRD-003: Verificar duplicados ── [cite: 7, 10]
    public async Task<bool> ExistsByNameAndCategoryAsync(string nombre, string categoria)
    {
        using var conn = CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Products WHERE Nombre = @Nombre AND Categoria = @Categoria",
            new { Nombre = nombre, Categoria = categoria });
        return count > 0;
    }

    
}