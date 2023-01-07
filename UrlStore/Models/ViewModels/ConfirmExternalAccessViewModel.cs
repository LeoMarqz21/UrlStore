using System.ComponentModel.DataAnnotations;

namespace UrlStore.Models.ViewModels
{
    public class ConfirmExternalAccessViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string UserName { get; set; }
    }
}
