using System.ComponentModel.DataAnnotations;

namespace AppWebBeachSA.Models
{
    public class Empleado
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public string NombreCompleto { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public int TipoUsuario { get; set; }

        [Required]
        public DateTime FechaRegistro { get; set; }

        [Required]
        public char Estado { get; set; }
    }//end class
}//end namespace
