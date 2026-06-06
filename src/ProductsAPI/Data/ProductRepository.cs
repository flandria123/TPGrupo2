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

    // ── GET ALL 
    public async Task<IEnumerable<Product>> GetAllAsync(string? categoria, string? nombre)
    {
        using var conn = CreateConnection();

        
        var sql = "SELECT Id, Nombre, Descripcion, Precio, Stock, Categoria, fecha_creacion AS FechaCreacion FROM Products WHERE 1=1";

        if (!string.IsNullOrEmpty(categoria))
            sql += " AND Categoria = @Categoria";

        if (!string.IsNullOrEmpty(nombre))
            sql += " AND Nombre LIKE '%' || @Nombre || '%'";

        return await conn.QueryAsync<Product>(sql, new { Categoria = categoria, Nombre = nombre });
    }

    // ── GET BY ID ── 
    public async Task<Product?> GetByIdAsync(Guid id)
    {
        using var conn = CreateConnection();

        
        return await conn.QuerySingleOrDefaultAsync<Product>(
            "SELECT Id, Nombre, Descripcion, Precio, Stock, Categoria, fecha_creacion AS FechaCreacion FROM Products WHERE Id = @Id",
            new { Id = id });
    }

    // ── CREATE ── 
    public async Task CreateAsync(Product product)
    {
        using var conn = CreateConnection();

        
        await conn.ExecuteAsync("""
            INSERT INTO Products (Id, Nombre, Descripcion, Precio, Stock, Categoria, fecha_creacion)
            VALUES (@Id, @Nombre, @Descripcion, @Precio, @Stock, @Categoria, @FechaCreacion)
        """, product);
    }

    // ── UPDATE ── 
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

    // ── DELETE ── 
    public async Task DeleteAsync(Guid id)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("DELETE FROM Products WHERE Id = @Id", new { Id = id });
    }

    // ── VALIDACIÓN PRD-003: Verificar duplicados ── 
    public async Task<bool> ExistsByNameAndCategoryAsync(string nombre, string categoria)
    {
        using var conn = CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Products WHERE Nombre = @Nombre AND Categoria = @Categoria",
            new { Nombre = nombre, Categoria = categoria });
        return count > 0;
    }
}