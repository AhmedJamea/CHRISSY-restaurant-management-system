using System.ComponentModel.DataAnnotations;

namespace Models.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
