using System.ComponentModel.DataAnnotations;

namespace AppWebBeachSA.Models
{
    public class Paquete
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public string NombrePaquete { get; set; }

        [Required]
        public decimal Precio { get; set; }

        [Required]
        public decimal PorcentajePrima { get; set; }

        [Required]
        public int LimiteMeses { get; set; }

        [Required]
        public DateTime FechaRegistro { get; set; }

        [Required]
        public char Estado { get; set; }
    }//end class
}//end namespace
