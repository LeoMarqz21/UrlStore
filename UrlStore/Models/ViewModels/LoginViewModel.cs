using System.ComponentModel.DataAnnotations;

namespace UrlStore.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Nombre de usuario")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string PasswordHash { get; set; }

        [Display(Name = "Recuerdame")]
        public bool RememberMe { get; set; }
    }
}
