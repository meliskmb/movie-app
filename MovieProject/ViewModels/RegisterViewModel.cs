using System.ComponentModel.DataAnnotations;

namespace MovieProject.ViewModels
{
    public class RegisterViewModel
    {
        [Required, StringLength(50)]
        public string Username { get; set; } = "";

        [Required, DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = "";

        [Required, DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Şifreler uyuşmuyor.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
