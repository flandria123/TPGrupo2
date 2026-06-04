using ProductsAPI.DTOs;
using System.Collections;


namespace ProductsAPI.Services;


public interface IProductService
{
   
    Task<IEnumerable<ProductResponse>> GetAllAsync(string? categoria, string? nombre);


    Task<ProductResponse> GetByIdAsync(Guid id);

    
    Task<ProductResponse> CreateAsync(CreateProductRequest request);

    Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request);

    
    Task DeleteAsync(Guid id);
}