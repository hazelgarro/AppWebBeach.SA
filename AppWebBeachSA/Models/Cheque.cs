using System.ComponentModel.DataAnnotations;

namespace AppWebBeachSA.Models
{
    public class Cheque
    {
        [Key]
        public int NumeroCheque { get; set; }

        public string NombreBanco { get; set; }

        public int IdReservacion { get; set; }
    }
}
