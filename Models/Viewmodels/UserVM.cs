using PatatZaak.Models.Businesslayer;

namespace PatatZaak.Models.Viewmodels
{
    public class UserVM
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public int RoleId { get; set; }
        public Role? Role { get; set; }
    }
}
