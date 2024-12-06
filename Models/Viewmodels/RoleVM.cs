using PatatZaak.Models.Businesslayer;

namespace PatatZaak.Models.Viewmodels
{
    public class RoleVM
    {
        public int RoleId { get; set; }
        public string? RoleName { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
