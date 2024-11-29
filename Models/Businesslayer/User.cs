using System.ComponentModel.DataAnnotations;

namespace PatatZaak.Models.Businesslayer
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? Password { get; set; }

        [Required]
        public int RoleId { get; set; }
        public Role? Role { get; set; }
    }
}
