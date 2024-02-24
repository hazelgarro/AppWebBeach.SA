using System.ComponentModel.DataAnnotations;

namespace AppWebBeachSA.Models
{
    public class Cliente
    {

        [Key]
        [Required]
        public string Cedula { get; set; }

        [Required]
        public string TipoCedula { get; set; }

        [Required]
        public string NombreCompleto { get; set; }

        [Required]
        public string Telefono { get; set; }

        [Required]
        public string Direccion { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public int TipoUsuario { get; set; }

        [Required]
        public int Restablecer { get; set; }

        [Required]
        public DateTime FechaRegistro { get; set; }

        [Required]
        public char Estado { get; set; }



    }
}
