using System.ComponentModel.DataAnnotations;

namespace AppWebBeachSA.Models
{
    public class SeguridadRestablecer
    {
        public string Email { get; set; }

        [Required(ErrorMessage = "Debe ingresar la contraseña enviada por email")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Debe ingresar su nueva contraseña")]
        [DataType(DataType.Password)]
        public string NuevoPassword { get; set; }

        [Required(ErrorMessage = "Confirme su nueva contraseña")]
        [DataType(DataType.Password)]
        public string Confirmar { get; set; }
    }
}
