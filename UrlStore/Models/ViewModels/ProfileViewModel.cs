using System.ComponentModel.DataAnnotations;
using UrlStore.Validations;

namespace UrlStore.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Display(Name = "Nombre completo")]
        public string Name { get; set; }

        [Display(Name = "Nombre de usuario")]
        public string UserName { get; set; }

        [Display(Name = "Correo electronico")]
        public string Email { get; set; }

        [Display(Name = "Fotografia")]
        public string Photo { get; set; }

        [ValidFileTypes(FileType.Image)]
        [FileSizeValidation(maximunSizeMB: 4)]
        [Display(Name = "Fotografia")]
        public IFormFile Image { get; set; }
    }
}
