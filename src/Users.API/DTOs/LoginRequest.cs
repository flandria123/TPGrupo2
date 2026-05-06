using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{
    public class LoginRequest
    {
        public record LoginRequestUser(
             [Required]     string Email,
             [Required] string Password
            )
        {

        };


    }
}
