
namespace Dominio.Entities;
    public class User : BaseEntity
    {
        public string UserName { get; set; }
        public string Email { get; set;}
        public string Password { get; set;}
        public ICollection<Rol> Roles { get; set; } = new HashSet<Rol>();
        public ICollection<UserRol> UserRols { get; set; }

    }
