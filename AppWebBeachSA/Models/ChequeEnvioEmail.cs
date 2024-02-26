namespace AppWebBeachSA.Models
{
    public class ChequeEnvioEmail
    {
        public int IdCheque { get; set; }

        public int NumeroCheque { get; set; }

        public string NombreBanco { get; set; }

        public int IdReservacion { get; set; }

        public bool EnvioEmail { get; set; }
    }
}
