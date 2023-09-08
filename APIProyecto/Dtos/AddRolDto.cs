

using System.ComponentModel.DataAnnotations;

namespace APIProyecto.Dtos;
    public class AddRolDto
    {
        [Required] 
        public string UserName { get; set; }
        [Required] 
        public string Password { get; set; }
        [Required] 
        public string Rol { get; set; }

    }
