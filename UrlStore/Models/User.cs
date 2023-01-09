using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace UrlStore.Models
{
    public class User : IdentityUser
    {
        [Required]
        [StringLength(100, MinimumLength = 5)]
        [Display(Name = "Nombre Completo")]
        public string Name { get; set; }

        public string Photo { get; set; }
    }
}
