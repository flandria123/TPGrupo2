
using Users.API.DTOs;

namespace Users.API.Services
{
    
    
        public interface IUserService
        {
            // Define que CUALQUIER clase que sea un UserService DEBE tener estas dos funciones.
            Task<UserResponse> RegisterAsync(CreateItemRequest request);
            Task<UserResponse> LoginAsync(LoginRequestUser request);
        }


    
}
