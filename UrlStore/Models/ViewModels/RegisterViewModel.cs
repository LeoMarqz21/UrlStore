using System.ComponentModel.DataAnnotations;

namespace UrlStore.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(100,MinimumLength = 5)]
        [Display(Name = "Nombre completo")]
        public string Name { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Nombre de usuario")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Correo electronico")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string PasswordHash { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("PasswordHash")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmPasswordHash { get; set; }
    }
}
