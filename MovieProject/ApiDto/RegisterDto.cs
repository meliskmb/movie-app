using System.ComponentModel.DataAnnotations;

namespace MovieProject.ApiDto
{
    public class RegisterDto
    {
        [Required, StringLength(50)]
        public string Username { get; set; } = "";

        [Required, MinLength(6)]
        public string Password { get; set; } = "";
    }
}
