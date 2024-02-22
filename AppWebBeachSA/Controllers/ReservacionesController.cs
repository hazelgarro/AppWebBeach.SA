using AppWebBeachSA.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace AppWebBeachSA.Controllers
{
    public class ReservacionesController : Controller
    {
        private HotelAPI hotelAPI;

        private HttpClient client;

        /// <summary>
        /// Metodo constructor
        /// </summary>
        public ReservacionesController()
        {
            hotelAPI = new HotelAPI();

            client = hotelAPI.Initial();
        }

        public async Task<IActionResult> Listado()
        {
            List<Reservacion> listado = new List<Reservacion>();

            HttpResponseMessage response = await client.GetAsync("/Reservaciones/ListaReservas");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultados = response.Content.ReadAsStringAsync().Result;

                listado = JsonConvert.DeserializeObject<List<Reservacion>>(resultados);
            }

            return View(listado);
        }

    }
}
