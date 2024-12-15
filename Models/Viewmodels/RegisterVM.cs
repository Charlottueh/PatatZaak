using System.ComponentModel.DataAnnotations;

namespace PatatZaak.Models.Viewmodels
{
    public class RegisterVM
    {
        [Required]
        [StringLength(100, ErrorMessage = "De gebruikersnaam moet tussen de {2} en {1} tekens bevatten.", MinimumLength = 6)]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Het wachtwoord moet minimaal {2} tekens bevatten.", MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Het wachtwoord en de bevestiging komen niet overeen.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public int RoleId { get; set; }
    }
}
