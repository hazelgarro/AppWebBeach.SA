using AppWebBeachSA.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace AppWebBeachSA.Controllers
{
    public class ChequesController : Controller
    {
        private HotelAPI hotelAPI;

        private HttpClient client;

        /// <summary>
        /// Metodo constructor
        /// </summary>
        public ChequesController()
        {
            hotelAPI = new HotelAPI();

            client = hotelAPI.Initial();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(List<IFormFile> files, [Bind] Cheque pCheque)
        {
            pCheque.IdReservacion = await GetNumReserva();

            var agregar = client.PostAsJsonAsync<Cheque>("/Reservaciones/AgregarCheque", pCheque);
            await agregar;

            var resultado = agregar.Result;

            if (resultado.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Mensaje"] = "No se ha podido registrar el cheque.";
                return View(pCheque);
            }
        }

        public async Task<int> GetNumReserva()
        {
            int ultimoId = 0;
            List<Reservacion> listado = new List<Reservacion>();

            HttpResponseMessage response = await client.GetAsync("/Reservaciones/ListaReservas");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultados = response.Content.ReadAsStringAsync().Result;

                listado = JsonConvert.DeserializeObject<List<Reservacion>>(resultados);
            }

            foreach (var item in listado)
            {
                ultimoId = item.Id;
            }

            return ultimoId;
        }
    }
}
