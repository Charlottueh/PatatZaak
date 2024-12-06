using System.ComponentModel.DataAnnotations;

namespace PatatZaak.Models.Viewmodels
{
    public class RegisterVM
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "De wachtwoorden komen niet overeen.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public int RoleId { get; set; }
    }
}
